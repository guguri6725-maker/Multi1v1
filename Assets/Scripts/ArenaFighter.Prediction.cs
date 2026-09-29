using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace FighterArena
{
    // 위치와 처리한 입력 번호를 한 묶음으로 보내 서로 다른 틱의 값이 섞이지 않게 합니다.
    public struct ArenaMotionState : INetworkSerializable, IEquatable<ArenaMotionState>
    {
        public Vector3 position, dashDirection;
        public float vertical, dashRemaining, dashSpeed;
        public uint moveSequence, commandSequence;
        public int epoch;
        public bool grounded;
        public CombatAction action;
        public double actionAt;
        public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter
        {
            s.SerializeValue(ref position); s.SerializeValue(ref dashDirection);
            s.SerializeValue(ref vertical); s.SerializeValue(ref dashRemaining); s.SerializeValue(ref dashSpeed);
            s.SerializeValue(ref moveSequence); s.SerializeValue(ref commandSequence);
            s.SerializeValue(ref epoch); s.SerializeValue(ref grounded);
            s.SerializeValue(ref action); s.SerializeValue(ref actionAt);
        }
        public bool Equals(ArenaMotionState other) => position == other.position && dashDirection == other.dashDirection
            && vertical == other.vertical && dashRemaining == other.dashRemaining && dashSpeed == other.dashSpeed
            && moveSequence == other.moveSequence && commandSequence == other.commandSequence
            && epoch == other.epoch && grounded == other.grounded && action == other.action && actionAt == other.actionAt;
    }

    public partial class ArenaFighter
    {
        public NetworkVariable<ArenaMotionState> Motion = new NetworkVariable<ArenaMotionState>();
        struct PredictedEvent
        {
            public uint command;
            public bool jump;
            public float distance, speed;
            public Vector3 direction;
        }
        struct PredictedStep
        {
            public uint sequence;
            public Vector2 input;
            public float yaw, multiplier;
            public bool stunned, trapped;
            public PredictedEvent first, second;
        }
        readonly List<PredictedStep> unconfirmedSteps = new List<PredictedStep>(128);
        uint localMoveSequence, localCommandSequence, serverMoveSequence, serverCommandSequence;
        Vector2 predictedInput;
        ArenaMotionState predictedMotion, receivedMotion;
        Vector3 renderCorrection, previousPredictedPosition;
        bool predictionReady, motionReceived;
        PredictedEvent queuedDash, queuedJump;
        CombatAction visualAction;
        double predictedDashReady, predictedComboUntil, localChargeStarted;
        double visualStartedRealtime;
        uint visualCommand;
        bool visualAcknowledged;
        double acknowledgedActionAt;
        int predictedCombo;
        float lastPredictedInput;
        public int PendingMovementInputs => unconfirmedSteps.Count;
        public float LastCorrectionDistance { get; private set; }
        public string PredictionTrace => "render=" + transform.position + " predicted=" + predictedMotion.position + " server=" + Motion.Value.position
            + " ack=" + Motion.Value.moveSequence + " command=" + Motion.Value.commandSequence + " remaining=" + predictedMotion.dashRemaining;

        bool HasVisualPrediction
        {
            get
            {
                if (IsServer || !Local || visualCommand == 0) return false;
                if (Action.Value == CombatAction.Stunned || Action.Value == CombatAction.Recoil || Health.Value <= 0 || !match.Live.Value)
                { visualCommand = 0; return false; }
                var authoritative = Motion.Value;
                if (authoritative.commandSequence >= visualCommand)
                {
                    // 승인된 동일 동작은 로컬 시작 시각을 유지합니다. 서버 시각으로 교체하면 모션이 되감깁니다.
                    if (authoritative.action != visualAction || visualAcknowledged && authoritative.actionAt != acknowledgedActionAt)
                    { visualCommand = 0; return false; }
                    acknowledgedActionAt = authoritative.actionAt;
                    visualAcknowledged = true;
                }
                else if (Time.unscaledTimeAsDouble - visualStartedRealtime > 2)
                { visualCommand = 0; return false; }
                return true;
            }
        }
        public CombatAction VisualAction
        {
            get
            {
                if (!HasVisualPrediction) return Action.Value;
                // 이미 끝난 동작은 늦은 승인으로 재생하지 않고 다음 입력을 받을 수 있게 합니다.
                double duration = ArenaCombat.IsAttack(visualAction) ? ArenaCombat.Duration(visualAction)
                    : visualAction == CombatAction.Whirlwind ? 2.9 : visualAction == CombatAction.Throwing ? .55 : double.PositiveInfinity;
                return Time.unscaledTimeAsDouble - visualStartedRealtime >= duration ? CombatAction.Idle : visualAction;
            }
        }
        // 네트워크 시각 동기화의 보정도 자기 모션을 되감지 못하도록 로컬 단조 시계를 사용합니다.
        public double VisualActionAge => HasVisualPrediction ? Math.Max(0, Time.unscaledTimeAsDouble - visualStartedRealtime)
            : Math.Max(0, Now - ActionAt.Value);

        void ResetPrediction()
        {
            unconfirmedSteps.Clear(); predictionReady = motionReceived = false;
            localMoveSequence = localCommandSequence = serverMoveSequence = serverCommandSequence = 0;
            visualCommand = 0; visualAcknowledged = false; predictedCombo = 0;
            predictedDashReady = predictedComboUntil = 0;
            renderCorrection = Vector3.zero; predictedInput = Vector2.zero;
            queuedDash = queuedJump = default;
        }
        void PublishMotion()
        {
            if (!IsServer) return;
            Motion.Value = new ArenaMotionState { position = transform.position, vertical = verticalVelocity,
                dashDirection = dashDirection, dashRemaining = dashRemaining, dashSpeed = currentDashSpeed,
                grounded = Grounded.Value, moveSequence = serverMoveSequence,
                commandSequence = serverCommandSequence, epoch = RoundEpoch.Value, action = Action.Value, actionAt = ActionAt.Value };
        }
        void ReceiveMotion(ArenaMotionState oldState, ArenaMotionState newState)
        {
            if (IsServer) return;
            receivedMotion = newState; motionReceived = true;
        }
        void SubmitMovement(Vector2 input, float yaw, float aimPitch)
        {
            predictedInput = Vector2.ClampMagnitude(input, 1);
            lastPredictedInput = Time.unscaledTime;
            if (IsServer) MoveRpc(predictedInput, yaw, aimPitch, ++localMoveSequence);
        }
        void SubmitCommand(int command)
        {
            uint sequence = ++localCommandSequence;
            if (!IsServer && match.Live.Value && Health.Value > 0) AnticipateCommand(command, sequence);
            CommandRpc(command, sequence, localYaw, pitch);
        }
        void AnticipateCommand(int command, uint sequence)
        {
            var action = VisualAction;
            bool changeVisual = false;
            var next = action;
            if (command == 0 && (action == CombatAction.Idle || action == CombatAction.Guard))
            {
                next = Equipped.Value != 1 ? CombatAction.Throwing : Now < RiposteUntil.Value ? CombatAction.Riposte : CombatAction.Charging;
                changeVisual = true;
                if (next == CombatAction.Charging) localChargeStarted = Time.unscaledTimeAsDouble;
            }
            if (command == 1 && action == CombatAction.Charging)
            {
                if (Time.unscaledTimeAsDouble - localChargeStarted >= ArenaCombat.ChargeTime)
                {
                    next = CombatAction.Heavy;
                    queuedDash = DashEvent(sequence, HeavyLungeDistance, dashSpeed * .7f);
                }
                else
                {
                    if (Now > predictedComboUntil) predictedCombo = 0;
                    next = ArenaCombat.Combo(predictedCombo++);
                    predictedComboUntil = Now + 1.3;
                }
                changeVisual = true;
            }
            if (command == 2 && Equipped.Value == 1 && (action == CombatAction.Idle || action == CombatAction.Charging))
            { next = CombatAction.Guard; changeVisual = true; }
            if (command == 3 && action == CombatAction.Guard) { next = CombatAction.Idle; changeVisual = true; }
            if (command == 4 && Now >= Math.Max(DashReady.Value, predictedDashReady)
                && action != CombatAction.Stunned && action != CombatAction.Recoil && action != CombatAction.Throwing && action != CombatAction.Switching)
            {
                queuedDash = DashEvent(sequence, dashSpeed * dashDuration, dashSpeed);
                predictedDashReady = Now + dashCooldown;
            }
            if (command == 6 && action != CombatAction.Stunned && !Trapped && (predictionReady ? predictedMotion.grounded : Grounded.Value))
                queuedJump = new PredictedEvent { command = sequence, jump = true };
            if (command == 5 && Equipped.Value == 1 && Now >= WhirlReady.Value && (action == CombatAction.Idle || action == CombatAction.Guard))
            { next = CombatAction.Whirlwind; changeVisual = true; }
            if (changeVisual) { visualAction = next; visualStartedRealtime = Time.unscaledTimeAsDouble; visualCommand = sequence; visualAcknowledged = false; }
        }
        PredictedEvent DashEvent(uint command, float distance, float speedValue) => new PredictedEvent {
            command = command, distance = distance, speed = speedValue,
            direction = Quaternion.Euler(0, localYaw, 0) * Vector3.forward };

        void EnsurePrediction()
        {
            if (predictionReady && predictedMotion.epoch == Motion.Value.epoch) return;
            predictedMotion = Motion.Value;
            previousPredictedPosition = predictedMotion.position;
            receivedMotion = predictedMotion;
            unconfirmedSteps.Clear(); renderCorrection = Vector3.zero;
            visualCommand = 0; visualAcknowledged = false; predictedCombo = 0;
            queuedDash = queuedJump = default;
            predictionReady = true; motionReceived = false;
            motor.enabled = false; transform.position = predictedMotion.position; motor.enabled = true;
        }
        void PredictMovement()
        {
            EnsurePrediction();
            ReconcileMovement();
            if (!match.Live.Value || Health.Value <= 0)
            {
                predictedInput = Vector2.zero; unconfirmedSteps.Clear();
                predictedMotion = Motion.Value; renderCorrection = Vector3.zero;
                queuedDash = queuedJump = default; return;
            }
            if (Time.unscaledTime - lastPredictedInput > .35f) predictedInput = Vector2.zero;
            var action = VisualAction;
            float movementMultiplier = action == CombatAction.Guard ? .45f : action == CombatAction.Charging ? .55f
                : ArenaCombat.IsAttack(action) ? .65f : action == CombatAction.Whirlwind ? .5f : 1;
            var step = new PredictedStep { sequence = ++localMoveSequence, input = predictedInput, yaw = localYaw,
                multiplier = movementMultiplier * MoveMultiplier, stunned = action == CombatAction.Stunned,
                trapped = Trapped, first = queuedDash, second = queuedJump };
            queuedDash = queuedJump = default;
            MoveRpc(step.input, step.yaw, pitch, step.sequence);
            previousPredictedPosition = predictedMotion.position;
            SimulatePredictedStep(step, 0);
            unconfirmedSteps.Add(step);
            // 긴 연결 중단 때 무한히 예측하지 않고 서버 위치로 돌아갑니다.
            if (unconfirmedSteps.Count > 100) { unconfirmedSteps.Clear(); predictedMotion = Motion.Value; renderCorrection = Vector3.zero; }
        }
        void ApplyPredictedEvent(PredictedEvent e, uint acknowledgedCommand)
        {
            if (e.command == 0 || e.command <= acknowledgedCommand) return;
            if (e.jump && predictedMotion.grounded && predictedMotion.vertical <= 0)
                predictedMotion.vertical = Mathf.Sqrt(2 * gravity * jumpHeight);
            if (e.distance > 0)
            { predictedMotion.dashRemaining = e.distance; predictedMotion.dashSpeed = e.speed; predictedMotion.dashDirection = e.direction; }
        }
        void SimulatePredictedStep(PredictedStep step, uint acknowledgedCommand)
        {
            // CharacterController의 내부 위치도 되돌려 재생 중 이전 이동이 누적되지 않게 합니다.
            motor.enabled = false;
            transform.position = predictedMotion.position;
            transform.rotation = Quaternion.Euler(0, step.yaw, 0);
            motor.enabled = true;
            ApplyPredictedEvent(step.first, acknowledgedCommand); ApplyPredictedEvent(step.second, acknowledgedCommand);
            Vector3 velocity = (transform.right * step.input.x + transform.forward * step.input.y) * speed * step.multiplier;
            if (predictedMotion.dashRemaining > 0)
            {
                float distance = Mathf.Min(predictedMotion.dashRemaining, predictedMotion.dashSpeed * Time.fixedDeltaTime);
                predictedMotion.dashRemaining -= distance;
                velocity = predictedMotion.dashDirection * (distance / Time.fixedDeltaTime);
            }
            if (step.stunned) velocity = Vector3.zero;
            if (step.trapped) predictedMotion.vertical = 0;
            else
            {
                if (predictedMotion.grounded && predictedMotion.vertical < 0) predictedMotion.vertical = -2;
                predictedMotion.vertical -= gravity * Time.fixedDeltaTime;
                var collisions = motor.Move((velocity + Vector3.up * predictedMotion.vertical) * Time.fixedDeltaTime);
                predictedMotion.grounded = (collisions & CollisionFlags.Below) != 0;
                if ((collisions & CollisionFlags.Above) != 0 && predictedMotion.vertical > 0) predictedMotion.vertical = 0;
            }
            predictedMotion.position = transform.position;
        }
        void ReconcileMovement()
        {
            if (!motionReceived) return;
            motionReceived = false;
            Vector3 oldPosition = predictedMotion.position + renderCorrection;
            Vector3 oldSimulation = predictedMotion.position;
            predictedMotion = receivedMotion;
            unconfirmedSteps.RemoveAll(step => step.sequence <= receivedMotion.moveSequence);
            // 서버가 아직 처리하지 않은 입력만 재생합니다. 클라이언트 위치는 서버에 보내지 않습니다.
            foreach (var step in unconfirmedSteps) SimulatePredictedStep(step, receivedMotion.commandSequence);
            previousPredictedPosition += predictedMotion.position - oldSimulation;
            Vector3 error = oldPosition - predictedMotion.position;
            LastCorrectionDistance = error.magnitude;
            renderCorrection = error.magnitude < 1.5f && !Trapped ? error : Vector3.zero;
        }
        void RenderPrediction()
        {
            EnsurePrediction();
            renderCorrection *= Mathf.Exp(-18 * Time.deltaTime);
            // 물리 틱 사이의 표시도 부드럽게 보정하되 충돌 판정은 예측 위치에서 수행합니다.
            float fraction = Mathf.Clamp01((Time.time - Time.fixedTime) / Time.fixedDeltaTime);
            transform.position = Vector3.Lerp(previousPredictedPosition, predictedMotion.position, fraction) + renderCorrection;
        }
    }
}
