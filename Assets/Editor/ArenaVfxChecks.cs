using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Linq;
using System.IO;

namespace FighterArena
{
    // 기존 게임 명령으로 재생하고 검사합니다. 테스트용 오브젝트나 Inspector 연결을 만들지 않습니다.
    public static class ArenaVfxChecks
    {
        public static IEnumerator Preview(ArenaSession s,int stage)
        {
            s.match.trainingOpponentAttacks=false;
            var b=s.match.blue;var r=s.match.red;
            b.diagnosticControl=r.diagnosticControl=true;
            s.match.Live.Value=true;s.match.Phase.Value=2;
            b.ResetFighter();r.ResetFighter();
            b.throwables.ResetRound();r.throwables.ResetRound();
            yield return null;
            if(stage==1) {r.ServerCommand(4);yield return new WaitForSeconds(.22f);}
            if(stage==2) {r.ServerCommand(0);r.ServerCommand(1);yield return new WaitForSeconds(.49f);}
            if(stage==3)
            {
                r.throwables.links.firstWall.Raise(new Vector3(1,1,1),15);
                r.throwables.ImpactPosition.Value=new Vector3(-2,.05f,2);
                r.throwables.BurstAt.Value=r.Now;
                yield return new WaitForSeconds(.16f);
            }
            if(stage==4)
            {
                var wall=r.throwables.links.firstWall;wall.Raise(new Vector3(1,1,1),15);
                yield return new WaitForSeconds(.4f);
                wall.DamageColumn(wall.links.columns[1],100,true);
                wall.DamageColumn(wall.links.columns[2],100,true);
                yield return new WaitForSeconds(.12f);
            }
            EditorApplication.isPaused=true;
        }
        public static IEnumerator Verify(ArenaSession s)
        {
            s.match.trainingOpponentAttacks=false;
            var b=s.match.blue;var r=s.match.red;
            b.diagnosticControl=r.diagnosticControl=true;
            s.match.Live.Value=true;s.match.Phase.Value=2;
            b.ResetFighter();r.ResetFighter();b.throwables.ResetRound();r.throwables.ResetRound();
            yield return null;
            var fx=Object.FindObjectsByType<ArenaCombatVfx>(FindObjectsSortMode.None).First(x=>x.fighter==r);
            int ghosts=fx.GhostsEmitted, samples=fx.RibbonSamples;
            r.ServerCommand(4);yield return new WaitForSeconds(.45f);
            bool dash=fx.GhostsEmitted>ghosts;
            r.ServerCommand(0);r.ServerCommand(1);yield return new WaitForSeconds(1);
            bool sword=fx.RibbonSamples>samples && !fx.ribbonRenderer.enabled;
            var wall=r.throwables.links.firstWall;
            bool raised=wall.Raise(new Vector3(4,1,0),0);yield return new WaitForSeconds(.3f);
            int before=wall.Intact.Value;wall.DamageColumn(wall.links.columns[1],100,false);
            bool normalBlocked=before==wall.Intact.Value;
            wall.DamageColumn(wall.links.columns[1],100,true);yield return new WaitForSeconds(.1f);
            var wfx=wall.GetComponent<ArenaWallVfx>();
            bool broken=wfx.BreaksPlayed>0 && wall.Intact.Value!=before;
            var tfx=Object.FindObjectsByType<ArenaThrowableVfx>(FindObjectsSortMode.None).First(x=>x.source==r.throwables);
            int bursts=tfx.BurstsPlayed;
            // 실제 번개 투척 명령을 발밑으로 사용해 피해 및 둔화도 검증합니다.
            r.throwables.ReadyAt.Value=0;r.throwables.ThunderAmmo.Value=2;r.AimPitch.Value=75;
            float hp=r.Health.Value;r.throwables.Launch(2);
            yield return new WaitForSeconds(.8f);
            bool thunder=tfx.BurstsPlayed>bursts && r.Health.Value==hp-15 && Mathf.Approximately(r.MoveMultiplier,.2f);
            yield return new WaitForSeconds(1);
            bool cleanup=!tfx.shockRing.enabled && tfx.bolts.All(x=>!x.enabled) && fx.ghosts.All(x=>!x.enabled);
            int colliders=GameObject.Find("CombatVFX").GetComponentsInChildren<Collider>().Length;
            string report="dash="+dash+" sword="+sword+" wallRaise="+raised+" normalBlocked="+normalBlocked+" heavyBreak="+broken+" thunderDamageAndSlow="+thunder+" cleanup="+cleanup+" effectColliders="+colliders;
            File.WriteAllText("TestResults/vfx-report.txt",report);Debug.Log("VFX_CHECK "+report);
            b.diagnosticControl=r.diagnosticControl=false;
        }
    }
}
