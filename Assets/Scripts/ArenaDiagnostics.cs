using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace FighterArena
{
    // 기존 씬에서 실행하는 검증 루틴입니다. 오브젝트/UI 생성이나 Inspector 자동 연결은 하지 않습니다.
    public static class ArenaDiagnostics
    {
        [Serializable] public class Report { public int passed; public int failed; public List<string> checks = new List<string>(); }
        public static string LastReport = "Not run";
        static void Check(Report r, string name, bool pass)
        {
            if (pass) r.passed++; else r.failed++;
            r.checks.Add((pass ? "PASS " : "FAIL ") + name);
        }
        static void Stage(ArenaSession s, float distance = 2)
        {
            var m = s.match;
            m.Live.Value = true; m.Phase.Value = 2;
            m.blue.ResetFighter(); m.red.ResetFighter();
            m.blue.motor.enabled = m.red.motor.enabled = false;
            m.blue.transform.position = new Vector3(0, .08f, -distance / 2);
            m.red.transform.position = new Vector3(0, .08f, distance / 2);
            m.blue.motor.enabled = m.red.motor.enabled = true;
            Physics.SyncTransforms();
        }
        public static IEnumerator CombatCheck(ArenaSession s)
        {
            var report = new Report();
            s.match.trainingOpponentAttacks = false;
            if (!s.network.IsListening) s.StartPractice();
            yield return new WaitForSeconds(3.5f);
            var m = s.match; var b = m.blue; var r = m.red;
            b.diagnosticControl = r.diagnosticControl = true;
            Check(report, "host and three scene NetworkObjects spawn", m.IsSpawned && b.IsSpawned && r.IsSpawned);
            Stage(s);
            b.ServerCommand(0); b.ServerCommand(1);
            Check(report, "combo begins left slash", b.Action.Value == CombatAction.LeftSlash);
            yield return new WaitForSeconds(.96f);
            Check(report, "left slash deals 34 once", Mathf.Approximately(r.Health.Value, 216));
            b.ServerCommand(0); b.ServerCommand(1);
            Check(report, "second attack right slash", b.Action.Value == CombatAction.RightSlash);
            yield return new WaitForSeconds(.96f);
            Check(report, "second slash deals 34 once", Mathf.Approximately(r.Health.Value, 182));
            b.ServerCommand(0); b.ServerCommand(1);
            Check(report, "third attack center thrust", b.Action.Value == CombatAction.Thrust);
            yield return new WaitForSeconds(1.02f);
            Check(report, "thrust deals 42 once", Mathf.Approximately(r.Health.Value, 140));
            yield return new WaitForSeconds(1.02f);
            b.ServerCommand(0); b.ServerCommand(1);
            Check(report, "expired combo resets", b.Action.Value == CombatAction.LeftSlash);
            Stage(s, 5);
            b.ServerCommand(0); b.ServerCommand(1);
            yield return new WaitForSeconds(.96f);
            Check(report, "out of range attack misses", Mathf.Approximately(r.Health.Value, 250));
            Stage(s);
            b.motor.enabled = r.motor.enabled = false;
            b.transform.position = new Vector3(0,.08f,10.7f);
            r.transform.position = new Vector3(0,.08f,13);
            b.motor.enabled = r.motor.enabled = true;
            Physics.SyncTransforms();
            b.ServerCommand(0); b.ServerCommand(1);
            yield return new WaitForSeconds(.96f);
            Check(report, "wall blocks weapon damage", r.Health.Value == 250);
            Stage(s);
            b.motor.enabled = false; b.transform.position = new Vector3(0,.08f,10); b.motor.enabled = true;
            b.ServerCommand(4);
            yield return new WaitForSeconds(.4f);
            Check(report, "dash cannot pass through arena wall", b.transform.position.z < 11.5f);
            Stage(s);
            b.ServerCommand(0); b.ServerCommand(1); double attackAt = b.ActionAt.Value;
            b.ServerCommand(0); b.ServerCommand(2);
            Check(report, "attack recovery rejects attack and guard cancellation", b.Action.Value == CombatAction.LeftSlash && b.ActionAt.Value == attackAt);
            Stage(s);
            r.ServerCommand(2); r.ActionAt.Value = r.Now - .1;
            r.ReceiveHit(b, 34, false);
            Check(report, "timed parry prevents damage and stuns attacker", r.Health.Value == 250 && b.Action.Value == CombatAction.Stunned);
            r.ServerCommand(0);
            Check(report, "parry grants immediate heavy riposte", r.Action.Value == CombatAction.Riposte);
            Stage(s);
            r.ServerCommand(2); r.ActionAt.Value = r.Now - 1.0;
            r.ReceiveHit(b, 34, false);
            Check(report, "late front guard mitigates 80 percent", Mathf.Abs(r.Health.Value - 243.2f) < .01f);
            Stage(s);
            r.ServerCommand(2); r.ActionAt.Value = r.Now - 1.0;
            r.ReceiveHit(b, 65, true);
            Check(report, "heavy breaks ordinary guard", r.Health.Value == 185 && r.Action.Value == CombatAction.Stunned);
            Stage(s);
            r.ServerCommand(2); r.ActionAt.Value = r.Now - .1; r.ReceiveHit(b, 65, true);
            Check(report, "heavy can be perfectly parried", r.Health.Value == 250 && b.Action.Value == CombatAction.Stunned);
            Stage(s);
            r.transform.rotation = Quaternion.identity;
            r.ServerCommand(2); r.ReceiveHit(b, 34, false);
            Check(report, "rear hit bypasses guard", r.Health.Value == 216);
            Stage(s);
            b.ServerCommand(0); b.ActionAt.Value = b.Now - 1.6; b.ServerCommand(1);
            Check(report, "charged release selects heavy", b.Action.Value == CombatAction.Heavy);
            yield return new WaitForSeconds(.9f);
            Check(report, "heavy lunges and hits", b.transform.position.z > -.5f && r.Health.Value == 185);
            Stage(s, 8);
            float z = b.transform.position.z;
            b.ServerCommand(4); double ready = b.DashReady.Value;
            b.ServerCommand(4);
            yield return new WaitForSeconds(.4f);
            Check(report, "dash moves and rejects cooldown reuse", b.transform.position.z > z + 3 && b.DashReady.Value == ready);
            Stage(s);
            b.ServerCommand(5); ready = b.WhirlReady.Value;
            b.ServerCommand(5);
            yield return new WaitForSeconds(3.1f);
            Check(report, "whirlwind deals four ticks and finisher", r.Health.Value == 108 && b.WhirlReady.Value == ready);
            Stage(s);
            m.BlueWins.Value = 2; m.RedWins.Value = 0;
            r.ReceiveHit(b, 300, false);
            r.ReceiveHit(b, 300, false);
            Check(report, "lethal hit ends first-to-three once", m.BlueWins.Value == 3 && m.Phase.Value == 4 && !m.Live.Value);
            m.RematchRpc();
            yield return new WaitForSeconds(.1f);
            Check(report, "rematch clears score and restores health", m.BlueWins.Value == 0 && m.RedWins.Value == 0 && r.Health.Value == 250 && m.Phase.Value == 1);
            LastReport = JsonUtility.ToJson(report, true);
            Directory.CreateDirectory(Path.Combine(Application.dataPath, "../TestResults"));
            File.WriteAllText(Path.Combine(Application.dataPath, "../TestResults/combat-report.json"), LastReport);
            Debug.Log("ARENA_COMBAT_REPORT " + LastReport);
            b.diagnosticControl = r.diagnosticControl = false;
            s.match.trainingOpponentAttacks = true;
        }

        public static IEnumerator Capture(string path)
        {
            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(path);
        }

        public static IEnumerator NetworkCheck(ArenaSession s)
        {
            float deadline = Time.realtimeSinceStartup + 30;
            while ((!s.match.IsSpawned || !s.match.Live.Value) && Time.realtimeSinceStartup < deadline) yield return null;
            if (!s.match.IsSpawned || !s.match.Live.Value) { Debug.LogError("ARENA_NETCHECK FAIL timeout"); yield break; }
            var f = s.match.blue.Local ? s.match.blue : s.match.red;
            f.diagnosticControl = true;
            double start = f.Now;
            int stage = -1;
            while (f.Now - start < 12)
            {
                float age = (float)(f.Now - start);
                f.SendControl(age < 1.6f ? Vector2.up : Vector2.zero, 180);
                int second = Mathf.FloorToInt(age);
                if (stage != second)
                {
                    stage = second;
                    if (stage == 2) f.SendControl(Vector2.zero, 180, 4);
                    if (stage == 3) f.SendControl(Vector2.zero, 180, 5);
                    Debug.Log("ARENA_NETCHECK " + JsonUtility.ToJson(new Snapshot { client = s.network.LocalClientId.ToString(), slot = f.slot, phase = s.match.Phase.Value, blueHP = s.match.blue.Health.Value, redHP = s.match.red.Health.Value, redZ = s.match.red.Position.Value.z, action = f.Action.Value.ToString() }));
                }
                yield return new WaitForSeconds(.03f);
            }
            f.diagnosticControl = false;
            Debug.Log("ARENA_NETCHECK COMPLETE");
        }
        [Serializable] public class Snapshot { public string client; public int slot; public int phase; public float blueHP, redHP, redZ; public string action; }
    }
}
