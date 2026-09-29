using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FighterArena
{
    // 실제 씬의 버튼 이벤트와 서버 물리를 함께 검사합니다. UI 생성/연결 코드는 없습니다.
    public static class ArenaRevisionChecks
    {
        public static string LastReport = "Not run";
        static ArenaDiagnostics.Report report;
        static void Check(string label, bool ok) { if(ok) report.passed++; else report.failed++; report.checks.Add((ok?"PASS ":"FAIL ")+label); }
        static void Click(string name)
        {
            var button = GameObject.Find(name).GetComponent<Button>();
            var data = new PointerEventData(EventSystem.current) { button=PointerEventData.InputButton.Left, position=RectTransformUtility.WorldToScreenPoint(null,button.transform.position) };
            var hits = new List<RaycastResult>(); EventSystem.current.RaycastAll(data,hits);
            Check(name+" receives UI raycast",hits.Count>0 && hits[0].gameObject==button.gameObject);
            ExecuteEvents.Execute(button.gameObject,data,ExecuteEvents.pointerClickHandler);
        }
        public static IEnumerator Run(ArenaSession s)
        {
            report=new ArenaDiagnostics.Report();
            var menu=s.GetComponent<ArenaMenu>();
            yield return new WaitForSeconds(.15f);
            Click("HelpButton"); yield return null;
            Check("help button opens instructions",menu.links.help.activeSelf);
            Click("HelpButton");
            Click("PracticeButton"); yield return new WaitForSeconds(3.6f);
            Check("practice button starts live match",s.network.IsHost && s.match.Live.Value && s.Practice);
            s.match.trainingOpponentAttacks=false;
            var b=s.match.blue; var r=s.match.red;
            b.diagnosticControl=r.diagnosticControl=true;
            b.ResetFighter(); r.ResetFighter();
            b.ServerCommand(2); b.ReceiveHit(r,34,false);
            Check("guard parries immediately",b.Health.Value==250 && r.Action.Value==CombatAction.Stunned);
            b.ResetFighter(); r.ResetFighter();
            var wall=r.throwables.links.firstWall;
            Check("wall builds through player",wall.Raise(b.transform.position+Vector3.up,0));
            yield return null;
            Check("player is trapped inside wall",b.Trapped && wall.Intact.Value==15);
            Vector3 start=b.transform.position;
            for(int i=0;i<12;i++) { b.SendControl(Vector2.up,0); yield return new WaitForFixedUpdate(); }
            Check("trapped movement cannot escape",Vector3.Distance(start,b.transform.position)<.01f);
            b.ServerCommand(0); b.ServerCommand(1); yield return new WaitForSeconds(.96f);
            Check("light attack cannot destroy trap",b.Trapped && wall.Intact.Value==15);
            b.ServerCommand(0); b.ActionAt.Value=b.Now-1.5; b.ServerCommand(1);
            yield return new WaitForSeconds(.45f);
            Check("heavy breaks overlapping columns",!b.Trapped && wall.Intact.Value!=15);
            start=b.transform.position;
            for(int i=0;i<15;i++) { b.SendControl(Vector2.up,0); yield return new WaitForFixedUpdate(); }
            Check("movement resumes after breaking trap",Vector3.Distance(start,b.transform.position)>.1f);
            b.ResetFighter(); r.ResetFighter();
            b.motor.enabled=r.motor.enabled=false;
            b.transform.position=new Vector3(0,.08f,-4); r.transform.position=new Vector3(0,.08f,4);
            b.motor.enabled=r.motor.enabled=true; Physics.SyncTransforms();
            b.ServerCommand(13); yield return new WaitForSeconds(.3f); b.ServerCommand(0);
            yield return new WaitForSeconds(1.1f);
            Check("thrown wall grenade traps struck opponent",r.Trapped && b.throwables.StoneAmmo.Value==1);
            b.ResetFighter(); r.ResetFighter();
            Cursor.lockState=CursorLockMode.None; yield return new WaitForSeconds(.15f);
            Click("ThunderSlot"); yield return new WaitForSeconds(.3f);
            Check("equipment button selects thunder",b.Equipped.Value==2 && menu.links.thunderState.text.Contains("EQUIPPED"));
            Click("SwordSlot"); yield return new WaitForSeconds(.3f);
            Click("DashButton"); yield return null;
            Check("dash button sends gameplay command",b.DashReady.Value>b.Now);
            s.match.Phase.Value=4; s.match.Live.Value=false;
            yield return null; Click("RematchButton"); yield return new WaitForSeconds(.1f);
            Check("rematch button resets match",s.match.Phase.Value==1 && s.match.BlueWins.Value==0);
            Cursor.lockState=CursorLockMode.None; yield return new WaitForSeconds(.15f); Click("LeaveButton");
            yield return new WaitForSeconds(.5f);
            Check("leave returns to lobby",!s.network.IsListening && menu.links.host.interactable);
            Click("HostButton");
            // 방 생성 버튼은 비동기 Relay 요청이므로 서비스 응답까지 기다립니다.
            float roomDeadline = Time.realtimeSinceStartup + 65;
            while (s.OnlineBusy && Time.realtimeSinceStartup < roomDeadline) yield return null;
            Check("create Relay room works after leave",s.network.IsHost && s.RelayRoom && s.RoomCode.Length>0 && !s.Practice && s.match.Phase.Value==0);
            Cursor.lockState=CursorLockMode.None; yield return new WaitForSeconds(.15f); Click("LeaveButton");
            yield return new WaitForSeconds(.5f);
            s.match.trainingOpponentAttacks=true;
            b.diagnosticControl=r.diagnosticControl=false;
            LastReport=JsonUtility.ToJson(report,true);
            File.WriteAllText(Path.Combine(Application.dataPath,"../TestResults/revision-report.json"),LastReport);
            Debug.Log("ARENA_REVISION_REPORT "+LastReport);
        }
    }
}
