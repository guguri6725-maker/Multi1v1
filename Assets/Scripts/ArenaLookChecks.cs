using System.Collections;
using System.IO;
using UnityEngine;

namespace FighterArena
{
    // 실제 입력 처리와 카메라 갱신을 통해 공격 중 회전 지연을 검사합니다.
    public static class ArenaLookChecks
    {
        public static string LastReport="Not run";
        static ArenaDiagnostics.Report report;
        static void Check(string name,bool ok){if(ok)report.passed++;else report.failed++;report.checks.Add((ok?"PASS ":"FAIL ")+name);}
        public static IEnumerator Run(ArenaSession s)
        {
            report=new ArenaDiagnostics.Report();s.match.trainingOpponentAttacks=false;
            if(!s.network.IsListening)s.StartPractice();
            yield return new WaitForSeconds(3.5f);
            var b=s.match.blue;var r=s.match.red;
            b.diagnosticControl=r.diagnosticControl=true;
            foreach(var action in new[]{CombatAction.Idle,CombatAction.LeftSlash,CombatAction.RightSlash,CombatAction.Thrust,CombatAction.Heavy,CombatAction.Whirlwind})
            {
                b.ResetFighter();r.ResetFighter();yield return null;
                b.Action.Value=action;b.ActionAt.Value=b.Now;
                b.ApplyLookInput(new Vector2(750,-200));
                Check(action+" weapon yaw changes immediately",Mathf.Abs(Mathf.DeltaAngle(b.transform.eulerAngles.y,90))<.01f);
                yield return new WaitForEndOfFrame();
                Check(action+" camera follows in same frame",Quaternion.Angle(s.viewCamera.transform.rotation,Quaternion.Euler(24,90,0))<.01f);
                yield return new WaitForFixedUpdate();
                Check(action+" server yaw has no turn-rate delay",Mathf.Abs(Mathf.DeltaAngle(b.Yaw.Value,90))<.01f);
            }
            b.ResetFighter();r.ResetFighter();yield return null;
            r.ServerCommand(2);r.ActionAt.Value=r.Now-.3;r.ReceiveHit(b,65,true);
            Check("charged heavy bypasses normal guard",r.Health.Value==185 && r.Feedback.Value==1);
            b.ResetFighter();r.ResetFighter();yield return null;
            r.ServerCommand(2);r.ReceiveHit(b,65,true);
            Check("charged heavy can be parried",r.Health.Value==250 && r.Feedback.Value==3 && b.Action.Value==CombatAction.Stunned);
            b.ResetFighter();r.ResetFighter();yield return null;
            r.ServerCommand(2);r.ActionAt.Value=r.Now-.3;r.ReceiveHit(b,65,false);
            Check("parry riposte retains ordinary blocking",r.Health.Value==237);
            b.ResetFighter();r.ResetFighter();yield return null;
            b.ApplyLookInput(new Vector2(-1500,9999));
            Check("yaw wraps and pitch remains clamped",Mathf.Abs(Mathf.DeltaAngle(b.ViewYaw,180))<.01f && b.Pitch==-65);
            b.ResetFighter();r.ResetFighter();yield return null;
            Check("round reset restores camera heading",Mathf.Abs(Mathf.DeltaAngle(b.ViewYaw,0))<.01f && b.Pitch==0);
            b.diagnosticControl=r.diagnosticControl=false;s.match.trainingOpponentAttacks=true;
            LastReport=JsonUtility.ToJson(report,true);
            File.WriteAllText(Path.Combine(Application.dataPath,"../TestResults/look-report.json"),LastReport);
            Debug.Log("ARENA_LOOK_REPORT "+LastReport);
        }
        public static IEnumerator ClientRun(ArenaSession s)
        {
            float limit=Time.realtimeSinceStartup+30;
            while((!s.match.IsSpawned || !s.match.Live.Value) && Time.realtimeSinceStartup<limit)yield return null;
            if(!s.match.IsSpawned || !s.match.Live.Value){Debug.LogError("LOOK_NET_TIMEOUT");yield break;}
            var f=s.match.blue.Local?s.match.blue:s.match.red;f.diagnosticControl=true;
            yield return null;
            float initial=f.ViewYaw;
            f.SendControl(Vector2.zero,initial,0);f.SendControl(Vector2.zero,initial,1);
            yield return new WaitForSeconds(.1f);
            f.ApplyLookInput(new Vector2(750,-100));
            float target=Mathf.Repeat(initial+90,360);
            bool immediate=Mathf.Abs(Mathf.DeltaAngle(f.transform.eulerAngles.y,target))<.01f;
            bool beforeServer=Mathf.Abs(Mathf.DeltaAngle(f.Yaw.Value,target))>30;
            yield return null;
            bool camera=Quaternion.Angle(s.viewCamera.transform.rotation,Quaternion.Euler(12,target,0))<.01f;
            f.SendControl(Vector2.zero,target,-1,12);
            yield return new WaitForSeconds(.2f);
            Debug.Log("LOOK_NET immediateWeapon="+immediate+" cameraSameFrame="+camera+" beforeServerEcho="+beforeServer+" serverMatches="+(Mathf.Abs(Mathf.DeltaAngle(f.Yaw.Value,target))<.01f));
            f.diagnosticControl=false;
        }
    }
}
