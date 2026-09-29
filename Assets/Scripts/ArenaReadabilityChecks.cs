using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

namespace FighterArena
{
    // 준비 모션의 무피해 구간, 실제 3연타 패링, 이동 거리와 화면 피드백을 검증합니다.
    public static class ArenaReadabilityChecks
    {
        public static string LastReport = "Not run";
        static ArenaDiagnostics.Report report;
        static void Check(string label,bool ok) { if(ok)report.passed++;else report.failed++;report.checks.Add((ok?"PASS ":"FAIL ")+label); }
        static void Stage(ArenaSession s,float distance=2)
        {
            s.match.Live.Value=true;s.match.Phase.Value=2;
            var b=s.match.blue;var r=s.match.red;
            b.ResetFighter();r.ResetFighter();b.diagnosticControl=r.diagnosticControl=true;
            b.motor.enabled=r.motor.enabled=false;
            b.transform.position=new Vector3(0,.08f,-distance/2);r.transform.position=new Vector3(0,.08f,distance/2);
            b.motor.enabled=r.motor.enabled=true;Physics.SyncTransforms();
        }
        public static IEnumerator Run(ArenaSession s)
        {
            report=new ArenaDiagnostics.Report();s.match.trainingOpponentAttacks=false;
            if(!s.network.IsListening)s.StartPractice();
            yield return new WaitForSeconds(3.5f);
            var b=s.match.blue;var r=s.match.red;
            // 앞선 타격의 회복을 기다려 실제 콤보 입력으로 원하는 타수까지 진행합니다.
            for(int index=0;index<3;index++)
            {
                Stage(s,6);
                for(int prior=0;prior<index;prior++)
                {
                    b.ServerCommand(0);b.ServerCommand(1);
                    yield return new WaitForSeconds(ArenaCombat.Duration(ArenaCombat.Combo(prior))+.04f);
                }
                b.motor.enabled=r.motor.enabled=false;
                b.transform.position=new Vector3(0,.08f,-1);r.transform.position=new Vector3(0,.08f,1);
                b.motor.enabled=r.motor.enabled=true;Physics.SyncTransforms();
                b.ServerCommand(0);b.ServerCommand(1);
                var expected=ArenaCombat.Combo(index);
                Check("combo telegraph "+(index+1)+" correct action",b.Action.Value==expected);
                yield return new WaitForSeconds(ArenaCombat.Windup(expected)-.1f);
                Check("combo "+(index+1)+" windup causes no damage",r.Health.Value==250);
                Check("combo "+(index+1)+" preparation pose visible",Vector3.Distance(b.swordPivot.localPosition,new Vector3(.45f,.95f,.95f))>.3f);
                r.ServerCommand(2);
                yield return new WaitForSeconds(.19f);
                Check("combo "+(index+1)+" can be reactively parried",r.Health.Value==250 && b.Action.Value==CombatAction.Stunned && r.Feedback.Value==3);
            }
            Stage(s,16);
            Vector3 start=b.transform.position;b.ServerCommand(4);
            yield return new WaitForSeconds(.4f);
            float dash=Vector3.Distance(new Vector3(start.x,0,start.z),new Vector3(b.transform.position.x,0,b.transform.position.z));
            Stage(s,16);start=b.transform.position;
            b.ServerCommand(0);b.ActionAt.Value=b.Now-1.5;b.ServerCommand(1);
            yield return new WaitForSeconds(.4f);
            float heavy=Vector3.Distance(new Vector3(start.x,0,start.z),new Vector3(b.transform.position.x,0,b.transform.position.z));
            Check("E dash travels 4.16m",Mathf.Abs(dash-4.16f)<.025f);
            Check("heavy lunge is 70 percent of E",Mathf.Abs(heavy/dash-.7f)<.01f);
            Stage(s);yield return null;
            Check("all nine local anatomy renderers hidden",b.links.anatomy.Length==9 && b.links.anatomy.All(x=>!x.enabled));
            Check("all opponent anatomy remains visible",r.links.anatomy.All(x=>x.enabled));
            Check("local sword still visible",b.swordPivot.gameObject.activeSelf);
            r.Health.Value=123;yield return null;
            Check("HUD shows only local HP",s.health.text=="YOUR HP  250 / 250" && !s.health.text.Contains("123"));
            r.ReceiveHit(b,34,false);yield return new WaitForSeconds(.08f);
            var block=new MaterialPropertyBlock();r.body.GetPropertyBlock(block);
            Check("opponent damage creates visible body flash",block.GetColor("_BaseColor").r>1);
            Check("hit feedback has no text",s.crosshair.text=="+" && !s.skills.text.Contains("HIT"));
            yield return new WaitForSeconds(.3f);r.body.GetPropertyBlock(block);
            Check("body flash restores normal material",block.isEmpty);
            Stage(s);r.ServerCommand(2);r.ActionAt.Value=r.Now-.3;r.ReceiveHit(b,34,false);yield return null;
            Check("ordinary block displays blue sparks",r.Feedback.Value==2 && r.links.blockSparks.gameObject.activeSelf);
            Check("block feedback has no text",s.crosshair.text=="+" && !s.skills.text.Contains("BLOCK"));
            yield return new WaitForSeconds(.3f);
            Check("block sparks expire",!r.links.blockSparks.gameObject.activeSelf);
            Stage(s);Cursor.lockState=CursorLockMode.Locked;b.ReceiveHit(r,34,false);yield return new WaitForSeconds(.06f);
            Check("local damage displays red screen edges",s.links.impactEdges.All(x=>x.color.a>.1f && x.color.r>.9f));
            Check("local flash never reveals own body",b.links.anatomy.All(x=>!x.enabled));
            yield return new WaitForSeconds(.35f);
            Check("screen edges clear after damage",s.links.impactEdges.All(x=>x.color.a==0));
            Stage(s);b.diagnosticControl=r.diagnosticControl=false;s.match.trainingOpponentAttacks=true;
            LastReport=JsonUtility.ToJson(report,true);
            File.WriteAllText(Path.Combine(Application.dataPath,"../TestResults/readability-report.json"),LastReport);
            Debug.Log("ARENA_READABILITY_REPORT "+LastReport);
        }

        public static IEnumerator ObserveNetwork(ArenaSession s)
        {
            float limit=Time.realtimeSinceStartup+30;
            while((!s.match.IsSpawned || !s.match.Live.Value) && Time.realtimeSinceStartup<limit) yield return null;
            if(!s.match.IsSpawned || !s.match.Live.Value){Debug.LogError("READABILITY_NET_TIMEOUT");yield break;}
            var f=s.match.blue.Local?s.match.blue:s.match.red;
            bool flashed=false,blocked=false,edges=false;
            var properties=new MaterialPropertyBlock();
            float until=Time.realtimeSinceStartup+10;
            while(Time.realtimeSinceStartup<until)
            {
                f.opponent.body.GetPropertyBlock(properties);
                flashed |= properties.GetColor("_BaseColor").r>1;
                blocked |= f.opponent.links.blockSparks.gameObject.activeSelf;
                edges |= s.links.impactEdges.Any(x=>x.color.a>.1f);
                yield return null;
            }
            Debug.Log("READABILITY_NET "+"hidden="+f.links.anatomy.All(x=>!x.enabled)+" opponentVisible="+f.opponent.links.anatomy.All(x=>x.enabled)+" damageFlash="+flashed+" blockSparks="+blocked+" localEdges="+edges+" localOnlyHP="+(s.health.text=="YOUR HP  "+Mathf.CeilToInt(f.Health.Value)+" / 250")+" crosshair="+s.crosshair.text);
        }
        public static IEnumerator NetworkStimulus(ArenaSession s)
        {
            while(!s.match.IsSpawned || !s.match.Live.Value)yield return null;
            var b=s.match.blue;var r=s.match.red;
            yield return new WaitForSeconds(1);
            b.ReceiveHit(r,34,false);
            yield return new WaitForSeconds(2);
            b.ServerCommand(2);b.ActionAt.Value=b.Now-.3;b.ReceiveHit(r,34,false);
            yield return new WaitForSeconds(2);
            r.ReceiveHit(b,34,false);
        }
    }
}
