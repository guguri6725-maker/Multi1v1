using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace FighterArena
{
    // 실제 씬 구성에서 차지/방어/반격/수류탄의 서버 판정을 검증합니다.
    public static class ArenaMindgameChecks
    {
        public static string LastReport = "Not run";
        static ArenaDiagnostics.Report report;
        static void Check(string name, bool ok)
        {
            if (ok) report.passed++; else report.failed++;
            report.checks.Add((ok ? "PASS " : "FAIL ") + name);
        }
        static void Stage(ArenaSession s, float distance = 2)
        {
            s.match.Live.Value = true; s.match.Phase.Value = 2;
            s.match.blue.ResetFighter(); s.match.red.ResetFighter();
            var b = s.match.blue; var r = s.match.red;
            b.diagnosticControl = r.diagnosticControl = true;
            b.motor.enabled = r.motor.enabled = false;
            b.transform.position = new Vector3(0,.08f,-distance/2);
            r.transform.position = new Vector3(0,.08f,distance/2);
            b.motor.enabled = r.motor.enabled = true;
            Physics.SyncTransforms();
        }
        public static IEnumerator Run(ArenaSession s)
        {
            report = new ArenaDiagnostics.Report();
            s.match.trainingOpponentAttacks = false;
            if (!s.network.IsListening) s.StartPractice();
            yield return new WaitForSeconds(3.5f);
            var b = s.match.blue; var r = s.match.red;
            Stage(s, 5);
            b.ServerCommand(0);
            yield return new WaitForSeconds(.75f);
            Check("charge is still preparing after 0.75 seconds", b.Action.Value == CombatAction.Charging);
            Check("half charged sword raised above idle", b.swordPivot.localPosition.y > 1.2f && b.swordPivot.localPosition.y < 1.8f);
            b.ServerCommand(1);
            Check("early release produces normal attack", b.Action.Value == CombatAction.LeftSlash);
            Stage(s,5);
            b.ServerCommand(0); b.ActionAt.Value = b.Now - 1.49; b.ServerCommand(1);
            Check("1.49 second release remains light", b.Action.Value == CombatAction.LeftSlash);
            Stage(s,5);
            b.ServerCommand(0); b.ActionAt.Value = b.Now - 1.5; b.ServerCommand(1);
            Check("1.5 second release is heavy", b.Action.Value == CombatAction.Heavy);
            Stage(s,5);
            b.ServerCommand(0);
            yield return new WaitForSeconds(1.6f);
            Check("full charge can be held without auto attack", b.Action.Value == CombatAction.Charging && b.swordPivot.localPosition.y >= 1.84f);
            yield return new WaitForSeconds(1.48f);
            Check("3.0 seconds total triggers automatic heavy", b.Action.Value == CombatAction.Heavy);
            Stage(s);
            r.ServerCommand(2); r.ActionAt.Value = r.Now - .3;
            r.ReceiveHit(b, 34, false);
            Check("guard takes one damage after parry window", Mathf.Abs(r.Health.Value - (r.maxHealth - 1)) < .01f && b.Action.Value != CombatAction.Stunned);
            Stage(s);
            r.ServerCommand(2); r.ActionAt.Value = r.Now - .1;
            r.ReceiveHit(b, 34, false);
            yield return null;
            Check("ready guard parries and lights blade", r.Health.Value == 250 && r.links.parryGlow.enabled && r.links.parrySparks.gameObject.activeSelf);
            r.ServerCommand(0);
            Check("parry attack uses distinct overhead state", r.Action.Value == CombatAction.Riposte);
            yield return new WaitForSeconds(.25f);
            Check("riposte has golden trail", r.links.riposteTrail.emitting);
            Stage(s);
            r.ServerCommand(2); r.ActionAt.Value = r.Now - 1;
            r.ReceiveHit(b, b.AttackDamage(CombatAction.Heavy), true);
            Check("ordinary guard cannot block charged heavy", r.Health.Value == 185 && r.Action.Value == CombatAction.Stunned);
            Stage(s);
            r.ServerCommand(2); r.ActionAt.Value = r.Now - 1;
            r.ReceiveHit(b, b.AttackDamage(CombatAction.Riposte), true);
            Check("riposte pierces ordinary guard", r.Health.Value == r.maxHealth - b.AttackDamage(CombatAction.Riposte) && r.Action.Value == CombatAction.Stunned && r.Feedback.Value == 5);
            Stage(s);
            r.ServerCommand(2); r.ActionAt.Value = r.Now - .1;
            r.ReceiveHit(b, b.AttackDamage(CombatAction.Riposte), true);
            Check("riposte can be parried without attacker stun", r.Health.Value == r.maxHealth && b.Action.Value != CombatAction.Stunned);

            Stage(s, 8);
            b.ServerCommand(12);
            yield return new WaitForSeconds(.3f);
            Check("slot 2 equips thunder and hides sword", b.Equipped.Value == 2 && !b.swordPivot.gameObject.activeSelf && b.throwables.links.heldThunder.enabled);
            b.ServerCommand(2);
            Check("grenade cannot guard", b.Action.Value != CombatAction.Guard);
            b.ServerCommand(0);
            yield return new WaitForSeconds(.25f);
            Check("throw consumes one flask and starts flight", b.throwables.ThunderAmmo.Value == 1 && b.throwables.FlightKind.Value == 2);
            Check("throwable cooldown shared with wall flask", !b.throwables.CanThrow(3));
            yield return new WaitForSeconds(1.2f);
            Check("thunder lands and slows opponent", r.MoveMultiplier == .2f && r.Health.Value == 235);
            float z = r.transform.position.z;
            // 감속 만료도 서버 시각으로 판정합니다.
            r.SlowedUntil.Value = r.Now - .01;
            Check("slow expires without permanent speed change", r.MoveMultiplier == 1);
            Stage(s, 10);
            b.ServerCommand(13);
            yield return new WaitForSeconds(.3f);
            b.ServerCommand(0);
            yield return new WaitForSeconds(1.8f);
            var wall = b.throwables.links.firstWall;
            Check("wall flask creates standing stone columns", b.throwables.StoneAmmo.Value == 1 && wall.Standing && wall.Intact.Value != 0);
            Check("wall collider blocks movement and rays", Physics.Raycast(wall.transform.position + Vector3.up + wall.transform.forward * 2, -wall.transform.forward, 4));
            int mask = wall.Intact.Value;
            int index = (mask & 1) != 0 ? 0 : (mask & 2) != 0 ? 1 : (mask & 4) != 0 ? 2 : 3;
            wall.DamageColumn(wall.links.columns[index], 65, true);
            Check("wall columns can be destroyed individually", (wall.Intact.Value & (1<<index)) == 0 && !wall.links.columns[index].enabled);
            wall.Expires.Value = b.Now - .01;
            yield return null;
            Check("expired wall removes collision", !wall.Standing && !wall.links.columns[0].enabled && !wall.links.columns[3].enabled);
            Stage(s);
            Check("round reset restores ammo and clears effects", b.throwables.ThunderAmmo.Value == 2 && b.throwables.StoneAmmo.Value == 2 && b.Equipped.Value == 1 && b.MoveMultiplier == 1 && !wall.Standing);
            b.throwables.ThunderAmmo.Value = 0;
            Check("empty grenade stack rejects throw", !b.throwables.CanThrow(2));
            Stage(s,6);
            // 두 플레이어 사이의 돌벽이 실제 번개 폭발 시야도 막는지 확인합니다.
            bool built = wall.Raise(Vector3.zero, 0);
            b.ServerCommand(12);
            yield return new WaitForSeconds(.3f);
            b.ServerCommand(0);
            yield return new WaitForSeconds(1.5f);
            Check("stone wall blocks thunder splash", built && r.Health.Value == 250 && r.MoveMultiplier == 1);
            Stage(s);
            LastReport = JsonUtility.ToJson(report,true);
            Directory.CreateDirectory(Path.Combine(Application.dataPath,"../TestResults"));
            File.WriteAllText(Path.Combine(Application.dataPath,"../TestResults/mindgame-report.json"),LastReport);
            Debug.Log("ARENA_MINDGAME_REPORT " + LastReport);
            b.diagnosticControl = r.diagnosticControl = false;
            s.match.trainingOpponentAttacks = true;
        }

        [Serializable] public class NetSample
        {
            public int tick, slot, thunder, stone, wallMask;
            public string action;
            public float swordHeight, enemyHealth, slowRemaining;
            public bool goldenBlade;
        }
        public static IEnumerator NetworkRun(ArenaSession s)
        {
            float limit=Time.realtimeSinceStartup+30;
            while ((!s.match.IsSpawned || !s.match.Live.Value) && Time.realtimeSinceStartup<limit) yield return null;
            if (!s.match.IsSpawned || !s.match.Live.Value) { Debug.LogError("MINDGAME_NET_TIMEOUT"); yield break; }
            var f=s.match.blue.Local?s.match.blue:s.match.red;
            f.diagnosticControl=true;
            double start=f.Now;
            int last=-1;
            while (f.Now-start<22)
            {
                float age=(float)(f.Now-start);
                f.SendControl(Vector2.zero, f.slot==0?0:180);
                int tick=Mathf.FloorToInt(age*2);
                if (tick!=last)
                {
                    last=tick;
                    if(tick==2) f.SendControl(Vector2.zero,180,0);
                    if(tick==11) f.SendControl(Vector2.zero,180,12);
                    if(tick==12) f.SendControl(Vector2.zero,180,0);
                    if(tick==29) f.SendControl(Vector2.zero,180,13);
                    if(tick==31) f.SendControl(Vector2.zero,180,0);
                    Debug.Log("MINDGAME_NET "+JsonUtility.ToJson(new NetSample { tick=tick,slot=f.Equipped.Value,action=f.Action.Value.ToString(),swordHeight=f.swordPivot.localPosition.y,thunder=f.throwables.ThunderAmmo.Value,stone=f.throwables.StoneAmmo.Value,enemyHealth=f.opponent.Health.Value,slowRemaining=Mathf.Max(0,(float)(f.opponent.SlowedUntil.Value-f.Now)),wallMask=f.throwables.links.firstWall.Intact.Value,goldenBlade=f.opponent.links.parryGlow.enabled }));
                }
                yield return new WaitForSeconds(.03f);
            }
            f.diagnosticControl=false;
            Debug.Log("MINDGAME_NET_COMPLETE");
        }
    }
}

