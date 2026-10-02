using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

namespace FighterArena
{
    // 씬의 캐릭터/무기 참조는 Inspector에서 연결하며 런타임에 오브젝트를 생성하지 않습니다.
    public partial class ArenaFighter : NetworkBehaviour
    {
        [System.Serializable] public class References
        {
            public CharacterController motor;
            public Transform swordPivot;
            public Renderer body, helmet, visor;
            public ArenaMatch match;
            public ArenaFighter opponent;
            public ArenaThrowables throwables;
            public MeshRenderer parryGlow;
            public Transform parrySparks;
            public TrailRenderer riposteTrail;
            public Renderer[] anatomy;
            public Transform blockSparks;
        }
        public References links = new References();
        public CharacterController motor => links.motor;
        public Transform swordPivot => links.swordPivot;
        public Renderer body => links.body;
        public Renderer helmet => links.helmet;
        public Renderer visor => links.visor;
        public ArenaMatch match => links.match;
        public ArenaFighter opponent => links.opponent;
        public ArenaThrowables throwables => links.throwables;
        public int slot;
        public float speed = 4.2f;
        public float dashSpeed = 13f;
        public float dashDuration = .32f;
        public float dashCooldown = 12f;
        public float whirlwindCooldown = 40f;
        public float maxHealth = 250f;
        public float jumpHeight = 1.25f;
        public float gravity = 22f;
        [Header("공격 피해량")]
        [Min(0), InspectorName("일반 공격 1타 (왼쪽 베기)")] public float lightOneDamage = 34f;
        [Min(0), InspectorName("일반 공격 2타 (오른쪽 베기)")] public float lightTwoDamage = 34f;
        [Min(0), InspectorName("일반 공격 3타 (찌르기)")] public float lightThreeDamage = 42f;
        [Min(0), InspectorName("강공격")] public float heavyDamage = 65f;
        [Min(0), InspectorName("패링 반격 (내려찍기)")] public float riposteDamage = 65f;
        [System.NonSerialized] public bool diagnosticControl;
        public NetworkVariable<ulong> Driver = new NetworkVariable<ulong>(ulong.MaxValue);
        public NetworkVariable<float> Health = new NetworkVariable<float>(250);
        public NetworkVariable<CombatAction> Action = new NetworkVariable<CombatAction>();
        public NetworkVariable<double> ActionAt = new NetworkVariable<double>();
        public NetworkVariable<double> DashReady = new NetworkVariable<double>();
        public NetworkVariable<double> WhirlReady = new NetworkVariable<double>();
        public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();
        public NetworkVariable<float> Yaw = new NetworkVariable<float>();
        public NetworkVariable<int> HitSerial = new NetworkVariable<int>();
        public NetworkVariable<int> Feedback = new NetworkVariable<int>();
        public NetworkVariable<int> RoundEpoch = new NetworkVariable<int>();
        public NetworkVariable<int> Equipped = new NetworkVariable<int>(1);
        public NetworkVariable<float> AimPitch = new NetworkVariable<float>();
        public NetworkVariable<double> SlowedUntil = new NetworkVariable<double>();
        public NetworkVariable<double> RiposteUntil = new NetworkVariable<double>();
        public NetworkVariable<double> ParryAt = new NetworkVariable<double>(-100);
        public NetworkVariable<double> FeedbackAt = new NetworkVariable<double>(-100);
        public NetworkVariable<bool> Grounded = new NetworkVariable<bool>();
        public NetworkVariable<Vector3> RecoilPosition = new NetworkVariable<Vector3>();
        public NetworkVariable<Quaternion> RecoilRotation = new NetworkVariable<Quaternion>(Quaternion.identity);
        public NetworkVariable<Vector3> WallContact = new NetworkVariable<Vector3>();
        public NetworkVariable<double> WallContactAt = new NetworkVariable<double>(-100);
        public float HeavyLungeDistance => dashSpeed * dashDuration * .7f;
        // 번개 둔화 중에는 기본 이동속도의 20%만 남습니다.
        public float MoveMultiplier => Now < SlowedUntil.Value ? .2f : 1f;
        public bool Trapped
        {
            get
            {
                return throwables.links.firstWall.Captures(this) || throwables.links.secondWall.Captures(this)
                    || opponent.throwables.links.firstWall.Captures(this) || opponent.throwables.links.secondWall.Captures(this);
            }
        }
        public float Pitch { get; private set; }
        public float ViewYaw => Local ? localYaw : Yaw.Value;
        public bool Local => IsSpawned && Driver.Value == NetworkManager.LocalClientId;
        public double Now => NetworkManager.ServerTime.Time;
        Vector2 move;
        float desiredYaw, localYaw, pitch, sendAt;
        double lastInput, dashUntil, nextGuard, comboUntil;
        Vector3 dashDirection;
        float dashRemaining, currentDashSpeed;
        float verticalVelocity;
        float stunDuration = .65f;
        bool jumpQueued;
        MaterialPropertyBlock flash;
        int combo, spinTick, seenEpoch = -1;
        bool didHit, heldAttack, heldGuard, chargeReleased, wasLocal;

        public override void OnNetworkSpawn()
        {
            ResetPrediction();
            if (IsServer) ResetFighter();
            motor.enabled = IsServer;
            Motion.OnValueChanged += ReceiveMotion;
        }
        public override void OnNetworkDespawn()
        {
            Motion.OnValueChanged -= ReceiveMotion;
            ResetPrediction();
            motor.enabled = false;
        }

        public void ResetFighter()
        {
            if (!IsServer) return;
            motor.enabled = false;
            transform.SetPositionAndRotation(new Vector3(0, .08f, slot == 0 ? -5 : 5), Quaternion.Euler(0, slot == 0 ? 0 : 180, 0));
            motor.enabled = true;
            Position.Value = transform.position;
            Yaw.Value = desiredYaw = slot == 0 ? 0 : 180;
            RoundEpoch.Value++;
            Health.Value = maxHealth;
            DashReady.Value = WhirlReady.Value = 0;
            SlowedUntil.Value = 0;
            Equipped.Value = 1;
            AimPitch.Value = 0;
            if (throwables != null) throwables.ResetRound();
            dashUntil = nextGuard = comboUntil = 0;
            dashRemaining = 0;
            FeedbackAt.Value = -100;
            WallContactAt.Value = -100; verticalVelocity = 0; jumpQueued = false; Grounded.Value = false;
            RiposteUntil.Value = 0; ParryAt.Value = -100;
            combo = spinTick = 0;
            heldAttack = heldGuard = chargeReleased = false;
            move = Vector2.zero;
            SetAction(CombatAction.Idle);
            PublishMotion();
        }

        void Update()
        {
            if (!IsSpawned) return;
            if (Local)
            {
                if (!wasLocal || seenEpoch != RoundEpoch.Value)
                {
                    // 라운드 재시작 시 이전 라운드의 시선 입력을 지웁니다.
                    localYaw = Yaw.Value; pitch = Pitch = 0; seenEpoch = RoundEpoch.Value;
                }
                if (!diagnosticControl) ReadInput();
            }
            wasLocal = Local;
            if (!IsServer && Local)
            {
                RenderPrediction();
            }
            else if (!IsServer)
            {
                float blend = 1 - Mathf.Exp(-25 * Time.deltaTime);
                transform.position = Vector3.Distance(transform.position, Position.Value) > 3 ? Position.Value : Vector3.Lerp(transform.position, Position.Value, blend);
                if (!Local) transform.rotation = Quaternion.Euler(0, Yaw.Value, 0);
            }
            // 자기 무기와 시점은 서버 왕복이나 공격 중 회전 제한을 기다리지 않습니다.
            if (Local) transform.rotation = Quaternion.Euler(0, localYaw, 0);
            AnimateSword();
            swordPivot.gameObject.SetActive(Equipped.Value == 1);
            // 팔/다리/신발까지 전부 숨기고 대검과 장비만 1인칭으로 표시합니다.
            UpdateFeedback();
        }

        void ReadInput()
        {
            var k = Keyboard.current;
            var m = Mouse.current;
            if (k == null || m == null) return;
            bool control = Cursor.lockState == CursorLockMode.Locked && match.Live.Value;
            if (control)
            {
                ApplyLookInput(m.delta.ReadValue());
            }
            if (!IsServer || Time.unscaledTime >= sendAt)
            {
                sendAt = Time.unscaledTime + 1f / 30;
                Vector2 input = control ? new Vector2((k.dKey.isPressed ? 1 : 0) - (k.aKey.isPressed ? 1 : 0), (k.wKey.isPressed ? 1 : 0) - (k.sKey.isPressed ? 1 : 0)) : Vector2.zero;
                SubmitMovement(input, localYaw, pitch);
            }
            if (!control) { if (m.leftButton.wasReleasedThisFrame) SubmitCommand(1); if (m.rightButton.wasReleasedThisFrame) SubmitCommand(3); return; }
            if (m.leftButton.wasPressedThisFrame) SubmitCommand(0);
            if (m.leftButton.wasReleasedThisFrame) SubmitCommand(1);
            if (m.rightButton.wasPressedThisFrame) SubmitCommand(2);
            if (m.rightButton.wasReleasedThisFrame) SubmitCommand(3);
            if (k.eKey.wasPressedThisFrame) SubmitCommand(4);
            if (k.qKey.wasPressedThisFrame) SubmitCommand(5);
            if (k.spaceKey.wasPressedThisFrame) SubmitCommand(6);
            if (k.digit1Key.wasPressedThisFrame) SubmitCommand(11);
            if (k.digit2Key.wasPressedThisFrame) SubmitCommand(12);
            if (k.digit3Key.wasPressedThisFrame) SubmitCommand(13);
        }

        public void ApplyLookInput(Vector2 delta)
        {
            if (!Local || !ArenaCombat.Finite(delta.x) || !ArenaCombat.Finite(delta.y)) return;
            localYaw = Mathf.Repeat(localYaw + delta.x * .12f,360);
            pitch = Mathf.Clamp(pitch - delta.y * .12f,-65,65);
            Pitch = pitch;
            transform.rotation = Quaternion.Euler(0,localYaw,0);
            // 호스트 자신의 회전도 다음 물리 틱에서 이전 입력으로 되돌아가지 않게 합니다.
            if (IsServer) desiredYaw = localYaw;
        }

        // 이동은 손실 허용 패킷, 공격 입력은 신뢰성 있는 RPC로 전달합니다.
        [Rpc(SendTo.Server, Delivery = RpcDelivery.Unreliable)]
        void MoveRpc(Vector2 input, float yaw, float aimPitch, uint sequence, RpcParams rpc = default)
        {
            if (rpc.Receive.SenderClientId != Driver.Value || !ArenaCombat.Finite(yaw) || !ArenaCombat.Finite(aimPitch) || !ArenaCombat.Finite(input.x) || !ArenaCombat.Finite(input.y)) return;
            if (sequence <= serverMoveSequence) return;
            serverMoveSequence = sequence;
            move = Vector2.ClampMagnitude(input, 1);
            desiredYaw = Mathf.Repeat(yaw, 360);
            AimPitch.Value = Mathf.Clamp(aimPitch, -65, 65);
            lastInput = Now;
        }
        [Rpc(SendTo.Server)]
        void CommandRpc(int command, uint sequence, float yaw, float aimPitch, RpcParams rpc = default)
        {
            if (rpc.Receive.SenderClientId != Driver.Value || sequence <= serverCommandSequence || !ArenaCombat.Finite(yaw) || !ArenaCombat.Finite(aimPitch)) return;
            serverCommandSequence = sequence;
            desiredYaw = Mathf.Repeat(yaw, 360);
            transform.rotation = Quaternion.Euler(0, desiredYaw, 0);
            AimPitch.Value = Mathf.Clamp(aimPitch, -65, 65);
            ServerCommand(command);
        }

        public void ServerCommand(int command)
        {
            if (!IsServer || !match.Live.Value || Health.Value <= 0) return;
            var a = Action.Value;
            if (command == 6)
            {
                // 서버의 접지 판정으로 공중 연속 점프와 벽 감금 탈출을 막습니다.
                if (Grounded.Value && verticalVelocity <= 0 && !Trapped && a != CombatAction.Stunned) jumpQueued = true;
                return;
            }
            if (command >= 11 && command <= 13)
            {
                if (a != CombatAction.Idle && a != CombatAction.Guard && a != CombatAction.Charging) return;
                if (Equipped.Value == command - 10) return;
                heldAttack = heldGuard = false;
                Equipped.Value = command - 10;
                SetAction(CombatAction.Switching);
                return;
            }
            if (command == 1)
            {
                heldAttack = false;
                if (a == CombatAction.Charging) ReleaseAttack();
                return;
            }
            if (command == 3)
            {
                heldGuard = false;
                if (a == CombatAction.Guard) { SetAction(CombatAction.Idle); nextGuard = Now + .12; }
                return;
            }
            if (command == 4 && a != CombatAction.Stunned && a != CombatAction.Recoil && a != CombatAction.Throwing && a != CombatAction.Switching && Now >= DashReady.Value)
            {
                dashDirection = transform.forward;
                dashUntil = Now + dashDuration;
                dashRemaining = dashSpeed * dashDuration;
                currentDashSpeed = dashSpeed;
                DashReady.Value = Now + dashCooldown;
                return;
            }
            if (command == 5 && Equipped.Value == 1 && (a == CombatAction.Idle || a == CombatAction.Guard) && Now >= WhirlReady.Value)
            {
                heldGuard = false;
                WhirlReady.Value = Now + whirlwindCooldown;
                spinTick = 0;
                SetAction(CombatAction.Whirlwind);
                return;
            }
            if (command == 2 && Equipped.Value == 1 && Now >= nextGuard && (a == CombatAction.Idle || a == CombatAction.Charging))
            {
                heldAttack = false;
                heldGuard = true;
                SetAction(CombatAction.Guard);
                return;
            }
            if (command == 0 && (a == CombatAction.Idle || a == CombatAction.Guard))
            {
                heldGuard = false;
                if (Equipped.Value != 1)
                {
                    if (throwables.CanThrow(Equipped.Value)) { didHit = false; SetAction(CombatAction.Throwing); }
                    return;
                }
                heldAttack = true;
                if (Now < RiposteUntil.Value) { RiposteUntil.Value = 0; BeginAttack(CombatAction.Riposte); }
                else { chargeReleased = false; SetAction(CombatAction.Charging); }
            }
        }

        void ReleaseAttack()
        {
            if (Now - ActionAt.Value >= ArenaCombat.ChargeTime) BeginAttack(CombatAction.Heavy);
            else
            {
                if (Now > comboUntil) combo = 0;
                BeginAttack(ArenaCombat.Combo(combo));
                combo = (combo + 1) % 3;
                comboUntil = Now + 1.3;
            }
        }

        void BeginAttack(CombatAction action)
        {
            didHit = false;
            chargeReleased = true;
            SetAction(action);
            if (action == CombatAction.Heavy)
            {
                // E와 같은 시간 동안 70% 속도로 전진합니다. 마지막 물리 틱의 거리도 제한합니다.
                dashDirection = transform.forward; dashUntil = Now + dashDuration;
                dashRemaining = HeavyLungeDistance; currentDashSpeed = dashSpeed * .7f;
            }
        }
        void SetAction(CombatAction action) { Action.Value = action; ActionAt.Value = Now; if (action == CombatAction.Stunned) stunDuration = .65f; }

        // 검증 드라이버도 실제 RPC 경로로 입력을 전달하여 접속자 권한 검사를 거칩니다.
        public void SendControl(Vector2 input, float yaw, int command = -1, float aimPitch = 0)
        {
            if (!Local || !ArenaCombat.Finite(yaw) || !ArenaCombat.Finite(aimPitch)) return;
            localYaw = Mathf.Repeat(yaw,360); pitch = Pitch = Mathf.Clamp(aimPitch,-65,65);
            transform.rotation = Quaternion.Euler(0,localYaw,0);
            SubmitMovement(input, yaw, aimPitch);
            if (command >= 0) SubmitCommand(command);
        }

        void FixedUpdate()
        {
            if (!IsSpawned) return;
            if (!IsServer) { if (Local) PredictMovement(); return; }
            if (!match.Live.Value || Health.Value <= 0) { move = Vector2.zero; return; }
            // 입력 방향을 먼저 적용하여 같은 틱의 명중/방어 판정과 시선이 일치합니다.
            transform.rotation = Quaternion.Euler(0,desiredYaw,0);
            var a = Action.Value;
            double age = Now - ActionAt.Value;
            if (Now - lastInput > .35) move = Vector2.zero;
            // 일시적 이동 패킷 손실로 공격을 멋대로 발사하지 않고 준비 동작만 취소합니다.
            if (Now - lastInput > 2 && !diagnosticControl && (a == CombatAction.Charging || a == CombatAction.Guard))
            { heldAttack = heldGuard = false; SetAction(CombatAction.Idle); a = Action.Value; }
            if (a == CombatAction.Guard && !heldGuard) SetAction(CombatAction.Idle);
            if (a == CombatAction.Charging && ((!heldAttack && !chargeReleased) || age >= ArenaCombat.ChargeTime + ArenaCombat.FullChargeHold)) ReleaseAttack();
            if (a == CombatAction.Switching && age >= .25) SetAction(CombatAction.Idle);
            if (a == CombatAction.Throwing)
            {
                if (!didHit && age >= .2) { didHit = true; throwables.Launch(Equipped.Value); }
                if (age >= .55) SetAction(CombatAction.Idle);
            }
            if (a == CombatAction.Stunned && age >= stunDuration) SetAction(CombatAction.Idle);
            if (a == CombatAction.Recoil && age >= .38) SetAction(CombatAction.Idle);
            if (ArenaCombat.IsAttack(a))
            {
                float duration = ArenaCombat.Duration(a);
                // 준비 동작 뒤 검의 이동 경로를 먼저 검사하여 벽을 통과하기 전에 튕깁니다.
                if (ArenaCombat.IsLight(a) && age >= ArenaCombat.Windup(a) && age <= ArenaCombat.Windup(a)+.14f+Time.fixedDeltaTime && SweepBlade(a,(float)age,out var wallPoint)) WallBounce(wallPoint);
                if (Action.Value == a && !didHit && age >= ArenaCombat.HitTime(a)) { didHit = true; TryHit(AttackDamage(a), ArenaCombat.PiercesGuard(a), a == CombatAction.Thrust ? 23 : 65); }
                if (Action.Value == a && age >= duration) SetAction(CombatAction.Idle);
            }
            if (a == CombatAction.Whirlwind)
            {
                if (spinTick < 5 && age >= .35 + spinTick * .5) { spinTick++; TryHit(spinTick == 5 ? 38 : 26, false, 180); }
                if (Action.Value == a && age >= 2.9) SetAction(CombatAction.Idle);
            }
            float multiplier = a == CombatAction.Guard ? .45f : a == CombatAction.Charging ? .55f : ArenaCombat.IsAttack(a) ? .65f : a == CombatAction.Whirlwind ? .5f : 1;
            // 둔화는 조작 이동에만 적용하고, 강공격/E 돌진의 속도와 거리는 보존합니다.
            Vector3 velocity = (transform.right * move.x + transform.forward * move.y) * speed * multiplier * MoveMultiplier;
            if (dashRemaining > 0)
            {
                float step = Mathf.Min(dashRemaining, currentDashSpeed * Time.fixedDeltaTime);
                dashRemaining -= step;
                velocity = dashDirection * (step / Time.fixedDeltaTime);
            }
            if (a == CombatAction.Stunned) velocity = Vector3.zero;
            // 돌벽과 몸이 겹치면 밀려 나오지 않고 갇힙니다. 시점/공격은 유지합니다.
            if (Trapped) { verticalVelocity = 0; jumpQueued = false; }
            else
            {
                if (Grounded.Value && verticalVelocity < 0) verticalVelocity = -2;
                if (jumpQueued) { verticalVelocity = Mathf.Sqrt(2*gravity*jumpHeight); jumpQueued = false; }
                verticalVelocity -= gravity*Time.fixedDeltaTime;
                var collisions = motor.Move((velocity + Vector3.up*verticalVelocity)*Time.fixedDeltaTime);
                Grounded.Value = (collisions & CollisionFlags.Below) != 0;
                if ((collisions & CollisionFlags.Above) != 0 && verticalVelocity > 0) verticalVelocity = 0;
            }
            Position.Value = transform.position;
            Yaw.Value = transform.eulerAngles.y;
            PublishMotion();
        }

        // 공격자 Inspector의 피해량을 서버 판정에 사용합니다.
        public float AttackDamage(CombatAction action)
        {
            float damage = action == CombatAction.LeftSlash ? lightOneDamage
                : action == CombatAction.RightSlash ? lightTwoDamage
                : action == CombatAction.Thrust ? lightThreeDamage
                : action == CombatAction.Heavy ? heavyDamage
                : action == CombatAction.Riposte ? riposteDamage : 0;
            return ArenaCombat.Finite(damage) ? Mathf.Max(0, damage) : 0;
        }
        // 각 타격마다 거리/방향/벽 가림을 서버에서 검사하고 한 번만 피해를 줍니다.
        void TryHit(float damage, bool heavy, float arc)
        {
            bool wallBreaker = heavy || Action.Value == CombatAction.Riposte;
            // 벽 내부에서 시작한 광선은 벽을 못 찾으므로 몸에 겹친 기둥부터 처리합니다.
            bool insideWall = false;
            foreach (var collider in Physics.OverlapCapsule(transform.position + Vector3.up * .5f, transform.position + Vector3.up * 1.5f, .4f))
            {
                var enclosing = collider.GetComponentInParent<ArenaStoneWall>();
                if (enclosing == null || !enclosing.Standing) continue;
                insideWall = true;
                enclosing.DamageColumn(collider, damage, wallBreaker);
            }
            if (insideWall) { if (!wallBreaker) WallBounce(transform.position+Vector3.up*1.3f+transform.forward*.3f); return; }
            if (Physics.Raycast(transform.position + Vector3.up * 1.15f, transform.forward, out var wallHit, ArenaCombat.Reach, ~0, QueryTriggerInteraction.Ignore))
            {
                var wall = wallHit.collider.GetComponentInParent<ArenaStoneWall>();
                if (wall != null) wall.DamageColumn(wallHit.collider, damage, wallBreaker);
                if (wall != null || IsWall(wallHit.collider))
                {
                    if (!wallBreaker) WallBounce(wallHit.point);
                    return;
                }
            }
            if (opponent.Health.Value <= 0 || !match.Live.Value) return;
            Vector3 delta = opponent.transform.position - transform.position;
            delta.y = 0;
            if (delta.magnitude > ArenaCombat.Reach || Vector3.Angle(transform.forward, delta) > arc) return;
            Vector3 origin = transform.position + Vector3.up * 1.15f + delta.normalized * .5f;
            if (Physics.Raycast(origin, delta.normalized, out var hit, Mathf.Max(0, delta.magnitude - .5f), ~0, QueryTriggerInteraction.Ignore) && hit.collider != opponent.motor)
            {
                if (!wallBreaker && IsWall(hit.collider)) WallBounce(hit.point);
                return;
            }
            opponent.ReceiveHit(this, damage, heavy);
        }

        static bool IsWall(Collider c) => c != null && !c.isTrigger && !(c is CharacterController) && c.GetComponentInParent<ArenaFighter>() == null && c.bounds.size.y > .4f;
        bool SweepBlade(CombatAction action,float age,out Vector3 contact)
        {
            contact = Vector3.zero;
            float previous = Mathf.Max(ArenaCombat.Windup(action),age-Time.fixedDeltaTime);
            LightPose(action,previous,out var before,out var beforeRotation);
            LightPose(action,age,out var after,out var afterRotation);
            // 검의 뿌리/중앙/끝을 두께 있게 검사해 옆 벽에 닿는 베기도 멈춥니다.
            foreach(float along in new[]{.2f,.75f,1.3f})
            {
                Vector3 from = transform.TransformPoint(before+beforeRotation*Vector3.up*along);
                Vector3 to = transform.TransformPoint(after+afterRotation*Vector3.up*along);
                Vector3 step = to-from;
                foreach(var overlap in Physics.OverlapSphere(from,.07f,~0,QueryTriggerInteraction.Ignore))
                    if(IsWall(overlap)){contact=overlap.ClosestPoint(from);return true;}
                if(step.sqrMagnitude < .000001f) continue;
                foreach(var hit in Physics.SphereCastAll(from,.07f,step.normalized,step.magnitude,~0,QueryTriggerInteraction.Ignore))
                    if(IsWall(hit.collider)){contact=hit.point;return true;}
            }
            return false;
        }
        void WallBounce(Vector3 contact)
        {

            // 마지막 표시 자세에서 검을 되당겨 충돌 뒤 공격이 끝까지 진행되지 않게 합니다.
            RecoilPosition.Value = swordPivot.localPosition;
            RecoilRotation.Value = swordPivot.localRotation;
            WallContact.Value = contact; WallContactAt.Value = Now;
            didHit = true; heldAttack = heldGuard = false; combo = 0; comboUntil = 0; dashRemaining = 0;
            SetAction(CombatAction.Recoil);
        }

        public void ReceiveHit(ArenaFighter attacker, float damage, bool heavy)
        {
            if (!IsServer || !match.Live.Value || Health.Value <= 0) return;
            Vector3 toward = attacker.transform.position - transform.position;
            bool facing = Vector3.Angle(transform.forward, toward) <= ArenaCombat.GuardArc;
            float dealt = ArenaCombat.DamageAfterGuard(damage, Action.Value == CombatAction.Guard, facing, (float)(Now - ActionAt.Value), heavy, out bool parried);
            bool guardBroken = heavy && facing && Action.Value == CombatAction.Guard && !parried;
            HitSerial.Value++;
            FeedbackAt.Value = Now;
            // 5번 피드백은 방어 관통 전용이며 양쪽 클라이언트에 함께 전달됩니다.
            Feedback.Value = parried ? 3 : guardBroken ? 5 : dealt < damage ? 2 : 1;
            if (parried)
            {
                // 일반 공격만 패링 경직 0.8초. 강공격/패링 반격은 피해만 막습니다.
                if (!heavy)
                {
                    attacker.SetAction(CombatAction.Stunned);
                    attacker.stunDuration = .8f;
                    attacker.dashUntil = 0;
                    attacker.dashRemaining = 0;
                }
                RiposteUntil.Value = Now + 1.5;
                ParryAt.Value = Now;
                heldGuard = false;
                SetAction(CombatAction.Idle);
                nextGuard = Now + .25;
                return;
            }
            Health.Value = Mathf.Max(0, Health.Value - dealt);
            if (Health.Value <= 0) { SetAction(CombatAction.Dead); match.Defeated(slot); }
            else if (heavy && Action.Value == CombatAction.Guard) { heldGuard = false; SetAction(CombatAction.Stunned); }
        }

        public void ReceiveThunder(float damage, float duration, bool finishRound = true)
        {
            if (!IsServer || !match.Live.Value || Health.Value <= 0) return;
            SlowedUntil.Value = System.Math.Max(SlowedUntil.Value, Now + duration);
            Health.Value = Mathf.Max(0, Health.Value - damage);
            HitSerial.Value++; Feedback.Value = 4;
            FeedbackAt.Value = Now;
            if (Health.Value <= 0) { SetAction(CombatAction.Dead); if (finishRound) match.Defeated(slot); }
        }

        // 준비/타격/회수 자세를 애니메이션과 벽 충돌 검사에서 함께 사용합니다.
        static void LightPose(CombatAction a,float t,out Vector3 p,out Quaternion r)
        {
            p = new Vector3(.45f,.95f,.95f); r = Quaternion.Euler(35,0,-18);
                float windup = ArenaCombat.Windup(a);
                bool thrust = a == CombatAction.Thrust;
                float side = a == CombatAction.RightSlash ? -1 : 1;
                Vector3 readyP = thrust ? new Vector3(.25f, 1.2f, .28f) : new Vector3(side * .85f, 1.45f, .5f);
                Quaternion readyR = thrust ? Quaternion.Euler(85,0,0) : Quaternion.Euler(-15,side * -55,side * -75);
                Vector3 endP = thrust ? new Vector3(.12f,1.2f,1.8f) : new Vector3(-side * .75f,1.05f,1.05f);
                Quaternion endR = thrust ? readyR : Quaternion.Euler(80,side * 60,side * 80);
                if (t < windup)
                {
                    float pull = Mathf.SmoothStep(0,1,Mathf.Clamp01(t / (windup * .65f)));
                    p = Vector3.Lerp(p,readyP,pull); r = Quaternion.Slerp(r,readyR,pull);
                }
                else if (t < windup + .14f)
                {
                    float strike = Mathf.Clamp01((t-windup)/.14f);
                    p = Vector3.Lerp(readyP,endP,strike); r = Quaternion.Slerp(readyR,endR,strike);
                }
                else
                {
                    float recover = Mathf.SmoothStep(0,1,Mathf.InverseLerp(windup+.14f,ArenaCombat.Duration(a),t));
                    p = Vector3.Lerp(endP,p,recover); r = Quaternion.Slerp(endR,r,recover);
                }
        }

        void AnimateSword()
        {
            float t = (float)VisualActionAge;
            Vector3 p = new Vector3(.45f, .95f, .95f);
            Quaternion r = Quaternion.Euler(35, 0, -18);
            var a = VisualAction;
            if (a == CombatAction.Guard)
            {
                float raise = 1f;
                p = Vector3.Lerp(p, new Vector3(.35f, 1.25f, .75f), raise);
                r = Quaternion.Slerp(r, Quaternion.Euler(15, 0, 65), raise);
            }
            if (a == CombatAction.Charging)
            {
                // 동기화된 시작 시각으로 모든 클라이언트가 같은 차지 준비 동작을 봅니다.
                float charge = Mathf.SmoothStep(0, 1, Mathf.Clamp01(t / ArenaCombat.ChargeTime));
                p = Vector3.Lerp(p, new Vector3(.36f, 1.85f, .65f), charge);
                r = Quaternion.Slerp(r, Quaternion.Euler(-55, -20, -65), charge);
            }
            if (ArenaCombat.IsLight(a)) LightPose(a,t,out p,out r);
            else if (ArenaCombat.IsAttack(a))
            {
                float f = Mathf.Clamp01(t / ArenaCombat.Duration(a));
                float swing = Mathf.Sin(f * Mathf.PI);
                if (a == CombatAction.Thrust) { p.z += swing * .9f; r = Quaternion.Euler(85, 0, 0); }
                else { p.x = Mathf.Lerp(a == CombatAction.RightSlash ? -.5f : .5f, a == CombatAction.RightSlash ? .5f : -.5f, f); r = Quaternion.Euler(25 + swing * 70, Mathf.Lerp(-60, 60, f), (a == CombatAction.RightSlash ? -1 : 1) * Mathf.Lerp(-85, 85, f)); }
            }
            if (a == CombatAction.Riposte)
            {
                // 패링 반격은 일반 차지 베기와 구별되는 수직 내려찍기입니다.
                float f = Mathf.Clamp01(t / ArenaCombat.Duration(a));
                float strike = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.18f, .68f, f));
                p = Vector3.Lerp(new Vector3(.18f, 1.95f, .6f), new Vector3(.1f, .85f, 1.25f), strike);
                r = Quaternion.Euler(Mathf.Lerp(-75, 135, strike), 0, 0);
            }
            if (a == CombatAction.Whirlwind) { float angle = t * 720 * Mathf.Deg2Rad; p = new Vector3(Mathf.Sin(angle) * .7f, 1.1f, Mathf.Cos(angle) * .7f); r = Quaternion.Euler(85, t * 720, 0); }
            if (a == CombatAction.Stunned) r = Quaternion.Euler(70, 0, 75);
            if (a == CombatAction.Recoil)
            {
                float recover = Mathf.SmoothStep(0,1,Mathf.Clamp01(t/.38f));
                p = Vector3.Lerp(RecoilPosition.Value,p,recover) - Vector3.forward * (Mathf.Sin(recover*Mathf.PI)*.15f);
                r = Quaternion.Slerp(RecoilRotation.Value,r,recover);
            }
            swordPivot.localPosition = p;
            swordPivot.localRotation = r;
            // 미리 만든 금빛 외곽/불꽃/잔상을 성공 시각에 맞춰 두 플레이어에게 표시합니다.
            bool glow = Now < RiposteUntil.Value || a == CombatAction.Riposte;
            if (links.parryGlow != null) links.parryGlow.enabled = glow;
            if (links.parrySparks != null)
            {
                float age = (float)(Now - ParryAt.Value);
                links.parrySparks.gameObject.SetActive(age >= 0 && age < .45f);
                links.parrySparks.localScale = Vector3.one * (1 + Mathf.Clamp01(age / .45f));
            }
            if (links.riposteTrail != null) links.riposteTrail.emitting = a == CombatAction.Riposte && t > .12f && t < .6f;
        }

        void UpdateFeedback()
        {
            float age = (float)(Now-FeedbackAt.Value);
            float pulse = age >= 0 && age < .3f ? Mathf.Sin(Mathf.Clamp01(age/.3f)*Mathf.PI) : 0;
            bool hurt = Feedback.Value == 1 || Feedback.Value == 4 || Feedback.Value == 5;
            if (flash == null) flash = new MaterialPropertyBlock();
            foreach (var part in links.anatomy)
            {
                if (part == null) continue;
                part.enabled = !Local;
                if (!Local && hurt && pulse > 0)
                {
                    Color normal = part.sharedMaterial.HasProperty("_BaseColor") ? part.sharedMaterial.GetColor("_BaseColor") : Color.white;
                    flash.Clear(); flash.SetColor("_BaseColor",Color.Lerp(normal,new Color(2.5f,.3f,.15f),pulse));
                    part.SetPropertyBlock(flash);
                }
                else part.SetPropertyBlock(null);
            }
            // 방어는 청색 금속 불꽃, 패링은 기존 금빛 불꽃으로 구분합니다.
            if (links.blockSparks != null)
            {
                float wallAge = (float)(Now-WallContactAt.Value);
                bool wallSpark = wallAge >= 0 && wallAge < .22f;
                links.blockSparks.gameObject.SetActive(wallSpark || Feedback.Value == 2 && age >= 0 && age < .25f);
                if (wallSpark) links.blockSparks.position = WallContact.Value;
                else links.blockSparks.localPosition = new Vector3(.05f,1.35f,.85f);
                links.blockSparks.localScale = Vector3.one * (1 + Mathf.Clamp01((wallSpark?wallAge:age)/.25f));
            }
        }
    }
}

