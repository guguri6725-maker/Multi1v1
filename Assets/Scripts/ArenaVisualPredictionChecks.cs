using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FighterArena
{
    // 서버 승인 전후 매 프레임을 관찰해 모션 시계 역행과 완료된 동작의 재시작을 잡습니다.
    public static class ArenaVisualPredictionChecks
    {
        static int failures;
        static readonly List<string> checks = new List<string>();
        static IEnumerator Observe(ArenaFighter fighter, CombatAction action, float seconds)
        {
            double lastAge = -1;
            bool ended = false, seen = false, acknowledged = false;
            float until = Time.realtimeSinceStartup + seconds;
            int rewinds = 0, restarts = 0;
            while (Time.realtimeSinceStartup < until)
            {
                var current = fighter.VisualAction;
                if (current == action)
                {
                    double age = fighter.VisualActionAge;
                    if (seen && age < lastAge - .02) { rewinds++; Debug.Log("ARENA_VISUAL_REWIND " + action + " age=" + lastAge + " to=" + age + " snapshot=" + fighter.Motion.Value.action + " at=" + fighter.Motion.Value.actionAt + " ack=" + fighter.Motion.Value.commandSequence); }
                    if (ended) { restarts++; ended = false; }
                    lastAge = age; seen = true;
                }
                else if (seen) ended = true;
                acknowledged |= fighter.Motion.Value.action == action && fighter.Motion.Value.commandSequence > 0;
                yield return null;
            }
            bool passed = seen && acknowledged && rewinds == 0 && restarts == 0;
            if (!passed) failures++;
            checks.Add(action + " seen=" + seen + " acknowledged=" + acknowledged + " rewinds=" + rewinds + " restarts=" + restarts);
        }
        public static IEnumerator Run(ArenaSession s)
        {
            failures = 0; checks.Clear();
            float deadline = Time.realtimeSinceStartup + 40;
            while ((!s.match.IsSpawned || !s.match.Live.Value) && Time.realtimeSinceStartup < deadline) yield return null;
            if (!s.match.IsSpawned || !s.match.Live.Value) { Debug.LogError("ARENA_VISUAL FAIL connect timeout"); yield break; }
            var f = s.match.blue.Local ? s.match.blue : s.match.red;
            f.diagnosticControl = true;
            ArenaPredictionChecks.SetDelay(s,120);
            yield return new WaitForSeconds(.6f);
            for (int i = 0; i < 3; i++)
            {
                f.SendControl(Vector2.zero,90,0); f.SendControl(Vector2.zero,90,1);
                yield return Observe(f,ArenaCombat.Combo(i),1.15f);
            }
            yield return new WaitForSeconds(.5f);
            f.SendControl(Vector2.zero,90,0);
            yield return Observe(f,CombatAction.Charging,2.2f);
            f.SendControl(Vector2.zero,90,1);
            yield return Observe(f,CombatAction.Heavy,1.3f);
            f.SendControl(Vector2.zero,90,2);
            yield return Observe(f,CombatAction.Guard,2.2f);
            f.SendControl(Vector2.zero,90,3);
            yield return new WaitForSeconds(.6f);
            f.SendControl(Vector2.zero,90,5);
            yield return Observe(f,CombatAction.Whirlwind,3.5f);
            f.SendControl(Vector2.zero,90,12);
            yield return new WaitForSeconds(.8f);
            f.SendControl(Vector2.zero,90,0);
            yield return Observe(f,CombatAction.Throwing,1.1f);
            f.SendControl(Vector2.zero,90,1);
            Debug.Log("ARENA_VISUAL_RESULT failures=" + failures + " / " + string.Join(" | ",checks));
            ArenaPredictionChecks.SetDelay(s,0); f.diagnosticControl = false;
        }
    }
}
