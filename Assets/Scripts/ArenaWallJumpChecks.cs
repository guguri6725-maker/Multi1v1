using System.Collections;
using System.IO;
using UnityEngine;

namespace FighterArena
{
    // 벽 접근/후퇴, 생성 시 감금, 점프와 실제 타격 중단을 물리 틱으로 검증합니다.
    public static class ArenaWallJumpChecks
    {
        public static string LastReport="Not run";
        static ArenaDiagnostics.Report report;
        static void Check(string name,bool ok){if(ok)report.passed++;else report.failed++;report.checks.Add((ok?"PASS ":"FAIL ")+name);}
        static void Stage(ArenaSession s,float z=-2)
        {
            s.match.Live.Value=true;s.match.Phase.Value=2;
            s.match.blue.ResetFighter();s.match.red.ResetFighter();
            var b=s.match.blue;var r=s.match.red;b.diagnosticControl=r.diagnosticControl=true;
            b.motor.enabled=r.motor.enabled=false;b.transform.position=new Vector3(0,.08f,z);r.transform.position=new Vector3(0,.08f,3);
            b.motor.enabled=r.motor.enabled=true;Physics.SyncTransforms();
        }
        public static IEnumerator Run(ArenaSession s)
        {
            report=new ArenaDiagnostics.Report();s.match.trainingOpponentAttacks=false;
            if(!s.network.IsListening)s.StartPractice();yield return new WaitForSeconds(3.5f);
            var b=s.match.blue;var r=s.match.red;var wall=r.throwables.links.firstWall;
            Stage(s);wall.Raise(Vector3.zero,0);
            for(int i=0;i<35;i++){b.SendControl(Vector2.up,0);yield return new WaitForFixedUpdate();}
            Check("approaching existing wall never captures player",!b.Trapped && wall.CapturedSlots.Value==0);
            Check("existing wall still blocks forward movement",b.transform.position.z<-.5f);
            float touch=b.transform.position.z;
            float sideways=b.transform.position.x;
            for(int i=0;i<10;i++){b.SendControl(Vector2.right,0);yield return new WaitForFixedUpdate();}
            Check("can slide sideways along existing wall",b.transform.position.x>sideways+.5f);
            for(int i=0;i<15;i++){b.SendControl(Vector2.down,0);yield return new WaitForFixedUpdate();}
            Check("can back away after touching wall",b.transform.position.z<touch-.7f);
            Stage(s);yield return new WaitForSeconds(.25f);
            Check("ground contact recognized",b.Grounded.Value);
            float ground=b.transform.position.y,peak=ground;
            var jumpButton=GameObject.Find("JumpButton").GetComponent<UnityEngine.UI.Button>();
            Check("jump button is wired in Inspector",jumpButton.onClick.GetPersistentEventCount()==1 && jumpButton.onClick.GetPersistentMethodName(0)=="Jump");
            jumpButton.onClick.Invoke();
            for(int i=0;i<30;i++) {b.ServerCommand(6);yield return new WaitForFixedUpdate();peak=Mathf.Max(peak,b.transform.position.y);}
            Check("jump reaches about 1.25m without double jump",peak-ground>1.05f && peak-ground<1.35f);
            yield return new WaitForSeconds(.5f);
            Check("jump lands back on floor",b.Grounded.Value && Mathf.Abs(b.transform.position.y-ground)<.05f);
            b.ServerCommand(6);yield return new WaitForSeconds(.15f);
            Check("can jump again after landing",b.transform.position.y>ground+.5f);
            Stage(s);wall.Raise(b.transform.position+Vector3.up,0);yield return null;
            Check("wall created inside body still captures",b.Trapped);
            ground=b.transform.position.y;b.ServerCommand(6);yield return new WaitForSeconds(.15f);
            Check("captured player cannot jump out",Mathf.Abs(b.transform.position.y-ground)<.01f);
            b.ServerCommand(0);b.ServerCommand(1);yield return new WaitForSeconds(.52f);
            Check("normal attack inside wall recoils",b.Action.Value==CombatAction.Recoil && wall.Intact.Value==15);
            yield return new WaitForSeconds(.42f);
            b.ServerCommand(0);b.ActionAt.Value=b.Now-1.5;b.ServerCommand(1);yield return new WaitForSeconds(.45f);
            Check("heavy breaks captured columns without recoil",!b.Trapped && b.Action.Value==CombatAction.Heavy);
            for(int index=0;index<3;index++)
            {
                Stage(s,-4);
                for(int prior=0;prior<index;prior++){b.ServerCommand(0);b.ServerCommand(1);yield return new WaitForSeconds(ArenaCombat.Duration(ArenaCombat.Combo(prior))+.05f);}
                b.motor.enabled=false;b.transform.position=new Vector3(0,.08f,-1.3f);b.motor.enabled=true;
                wall.Raise(Vector3.zero,0);b.ServerCommand(0);b.ServerCommand(1);
                yield return new WaitForSeconds(ArenaCombat.HitTime(ArenaCombat.Combo(index))+.03f);
                Check("combo "+(index+1)+" bounces off stone wall",b.Action.Value==CombatAction.Recoil);
                Check("combo "+(index+1)+" cannot damage behind wall",r.Health.Value==250 && wall.Intact.Value==15);
                Check("combo "+(index+1)+" wall spark visible",b.links.blockSparks.gameObject.activeSelf);
                yield return new WaitForSeconds(.45f);
                Check("combo "+(index+1)+" recovers after recoil",b.Action.Value==CombatAction.Idle);
            }
            Stage(s,10.1f);b.ServerCommand(0);b.ServerCommand(1);yield return new WaitForSeconds(.5f);
            Check("arena boundary also interrupts normal attack",b.Action.Value==CombatAction.Recoil);
            Stage(s,-1.3f);wall.Raise(Vector3.zero,0);b.ServerCommand(0);b.ActionAt.Value=b.Now-1.5;b.ServerCommand(1);yield return new WaitForSeconds(.45f);
            Check("heavy destroys wall without rebound",wall.Intact.Value!=15 && b.Action.Value==CombatAction.Heavy);
            Stage(s);b.diagnosticControl=r.diagnosticControl=false;s.match.trainingOpponentAttacks=true;
            LastReport=JsonUtility.ToJson(report,true);File.WriteAllText(Path.Combine(Application.dataPath,"../TestResults/wall-jump-report.json"),LastReport);Debug.Log("ARENA_WALL_JUMP_REPORT "+LastReport);
        }
        public static IEnumerator ClientRun(ArenaSession s)
        {
            float timeout=Time.realtimeSinceStartup+30;
            while((!s.match.IsSpawned || !s.match.Live.Value) && Time.realtimeSinceStartup<timeout)yield return null;
            if(!s.match.IsSpawned || !s.match.Live.Value){Debug.LogError("WALL_JUMP_NET_TIMEOUT");yield break;}
            var f=s.match.blue.Local?s.match.blue:s.match.red;f.diagnosticControl=true;
            yield return new WaitForSeconds(.2f);
            float floor=f.Position.Value.y,peak=floor;f.SendControl(Vector2.zero,f.ViewYaw,6);
            float end=Time.realtimeSinceStartup+1;
            while(Time.realtimeSinceStartup<end){peak=Mathf.Max(peak,f.Position.Value.y);yield return null;}
            bool landed=f.Grounded.Value;
            f.SendControl(Vector2.zero,0);yield return new WaitForSeconds(.15f);
            f.SendControl(Vector2.zero,0,4);yield return new WaitForSeconds(.45f);
            f.SendControl(Vector2.zero,0,0);f.SendControl(Vector2.zero,0,1);
            bool recoil=false,spark=false;end=Time.realtimeSinceStartup+1;
            while(Time.realtimeSinceStartup<end){recoil |= f.Action.Value==CombatAction.Recoil;spark |= f.links.blockSparks.gameObject.activeSelf;yield return null;}
            Debug.Log("WALL_JUMP_NET jump="+(peak-floor>1)+" landed="+landed+" recoil="+recoil+" sparks="+spark);
            f.diagnosticControl=false;
        }
    }
}
