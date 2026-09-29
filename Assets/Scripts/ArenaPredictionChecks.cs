using System.Collections;
using UnityEngine;
using Unity.Networking.Transport;

namespace FighterArena
{
    // 명시적으로 요청한 진단 실행에서만 네트워크 지연을 넣고 원래 설정으로 복구합니다.
    public static class ArenaPredictionChecks
    {
        public static void SetDelay(ArenaSession s, uint milliseconds)
        {
            s.transport.GetNetworkDriver().ModifyNetworkSimulatorParameters(new NetworkSimulatorParameter {
                SendDelayMS = milliseconds, SendJitterMS = milliseconds == 0 ? 0u : 20u,
                SendPacketLossPercent = milliseconds == 0 ? 0 : 1 });
        }
        public static IEnumerator ClientRun(ArenaSession s)
        {
            float deadline = Time.realtimeSinceStartup + 40;
            while ((!s.match.IsSpawned || !s.match.Live.Value) && Time.realtimeSinceStartup < deadline) yield return null;
            if (!s.match.IsSpawned || !s.match.Live.Value) { Debug.LogError("ARENA_PREDICTION FAIL connect timeout"); yield break; }
            var f = s.match.blue.Local ? s.match.blue : s.match.red;
            f.diagnosticControl = true;
            SetDelay(s, 120);
            yield return new WaitForSeconds(1);
            Vector3 start = f.transform.position;
            f.SendControl(Vector2.right, 180);
            yield return new WaitForSeconds(.06f);
            bool instantMove = Mathf.Abs(f.transform.position.x - start.x) > .02f;
            f.SendControl(Vector2.zero, 180);
            yield return new WaitForSeconds(.8f);
            float y = f.transform.position.y;
            f.SendControl(Vector2.zero, 180, 6);
            yield return new WaitForSeconds(.06f);
            bool instantJump = f.transform.position.y > y + .02f;
            yield return new WaitForSeconds(1.5f);
            f.SendControl(Vector2.zero, 180, 0);
            bool instantCharge = f.VisualAction == CombatAction.Charging;
            f.SendControl(Vector2.zero, 180, 1);
            bool instantAttack = f.VisualAction == CombatAction.LeftSlash;
            yield return new WaitForSeconds(1.3f);
            start = f.transform.position;
            f.SendControl(Vector2.zero, 180, 4);
            yield return new WaitForSeconds(.06f);
            bool instantDash = Vector3.Distance(start, f.transform.position) > .15f;
            yield return new WaitForSeconds(1.2f);
            float dashDistance = Vector3.Distance(start, f.transform.position);
            Debug.Log("ARENA_PREDICTION_DASH " + f.PredictionTrace);
            f.SendControl(Vector2.zero, 180, 0);
            yield return new WaitForSeconds(1.6f);
            start = f.transform.position;
            f.SendControl(Vector2.zero, 180, 1);
            bool instantHeavy = f.VisualAction == CombatAction.Heavy;
            yield return new WaitForSeconds(.06f);
            bool instantLunge = Vector3.Distance(start, f.transform.position) > .1f;
            yield return new WaitForSeconds(1.3f);
            float heavyDistance = Vector3.Distance(start, f.transform.position);
            Debug.Log("ARENA_PREDICTION_HEAVY " + f.PredictionTrace);
            float maxCorrection = 0;
            // 방향을 여러 번 바꿔도 서버 보정이 발산하지 않는지 확인합니다.
            for (int i = 0; i < 150; i++)
            {
                f.SendControl(i < 50 ? Vector2.right : i < 100 ? Vector2.left : Vector2.zero, 180);
                maxCorrection = Mathf.Max(maxCorrection, f.LastCorrectionDistance);
                yield return new WaitForFixedUpdate();
            }
            f.SendControl(Vector2.zero, 180);
            yield return new WaitForSeconds(1);
            float settledError = Vector3.Distance(f.transform.position, f.Position.Value);
            Debug.Log("ARENA_PREDICTION_SETTLED " + f.PredictionTrace);
            Debug.Log("ARENA_PREDICTION " + JsonUtility.ToJson(new Result { move = instantMove, jump = instantJump,
                charge = instantCharge, attack = instantAttack, dash = instantDash, heavy = instantHeavy,
                lunge = instantLunge, dashDistance = dashDistance, heavyDistance = heavyDistance,
                settledError = settledError, maxCorrection = maxCorrection, pending = f.PendingMovementInputs,
                rtt = s.transport.GetCurrentRtt(0) }));
            SetDelay(s, 0); f.diagnosticControl = false;
        }
        [System.Serializable] class Result
        {
            public bool move, jump, charge, attack, dash, heavy, lunge;
            public float dashDistance, heavyDistance, settledError, maxCorrection;
            public int pending;
            public ulong rtt;
        }
    }
}
