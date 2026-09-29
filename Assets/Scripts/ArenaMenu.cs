using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FighterArena
{
    // UI는 Hierarchy에 제작하고 모든 참조와 OnClick은 Inspector에서 연결합니다.
    public class ArenaMenu : MonoBehaviour
    {
        [Serializable] public class References
        {
            public ArenaSession session;
            public GameObject menu, help, equipment;
            public TMP_InputField address;
            public Button host, join, practice, resume, leave, rematch, copy;
            public Image sword, thunder, wall;
            public TMP_Text swordState, thunderState, wallState, connection, dashState, whirlState;
        }
        public References links = new References();
        ArenaFighter Local => !links.session.match.IsSpawned ? null : links.session.match.blue.Local ? links.session.match.blue : links.session.match.red.Local ? links.session.match.red : null;
        void Start() { links.address.text = ""; }
        void Update()
        {
            bool connected = links.session.network.IsListening;
            bool open = !connected || Cursor.lockState != CursorLockMode.Locked;
            links.menu.SetActive(open);
            links.session.title.gameObject.SetActive(!open);
            links.equipment.SetActive(connected);
            links.session.status.gameObject.SetActive(connected && !open);
            links.session.health.gameObject.SetActive(connected && !open);
            links.session.skills.gameObject.SetActive(connected && !open);
            links.session.crosshair.gameObject.SetActive(connected && !open);
            links.dashState.transform.parent.gameObject.SetActive(connected);
            links.whirlState.transform.parent.gameObject.SetActive(connected);
            bool busy = links.session.OnlineBusy || links.session.network.ShutdownInProgress;
            links.host.interactable = links.join.interactable = links.practice.interactable = !connected && !busy;
            links.address.interactable = !busy;
            links.address.readOnly = connected;
            if (connected && links.session.RelayRoom && links.address.text != links.session.RoomCode)
                links.address.text = links.session.RoomCode;
            links.copy.interactable = connected && links.session.RelayRoom && links.session.RoomCode.Length > 0;
            links.resume.interactable = connected && links.session.network.IsConnectedClient;
            links.leave.interactable = connected || busy;
            links.rematch.interactable = links.session.match.IsSpawned && links.session.match.Phase.Value == 4;
            links.connection.text = links.session.network.IsConnectedClient
                ? (links.session.Practice ? "OFFLINE PRACTICE" : links.session.RelayRoom ? "ROOM " + links.session.RoomCode + (links.session.network.IsHost ? " / HOST" : " / GUEST") : "DIRECT CONNECTION")
                : links.session.Notice;
            if (!open) links.help.SetActive(false);
            var f = Local;
            links.dashState.text = "E  DASH\n" + Remaining(f == null ? 0 : f.DashReady.Value, f);
            links.whirlState.text = "Q  WHIRLWIND\n" + Remaining(f == null ? 0 : f.WhirlReady.Value, f);
            Paint(links.sword, links.swordState, f, 1);
            Paint(links.thunder, links.thunderState, f, 2);
            Paint(links.wall, links.wallState, f, 3);
        }
        string Remaining(double ready, ArenaFighter f) => f == null || ready <= f.Now ? "READY" : Mathf.CeilToInt((float)(ready-f.Now)) + "s";
        void Paint(Image image, TMP_Text label, ArenaFighter f, int slot)
        {
            bool selected = f != null && f.Equipped.Value == slot;
            image.color = selected ? new Color(.22f,.34f,.44f,.98f) : new Color(.055f,.075f,.11f,.95f);
            if (f == null) { label.text = slot == 1 ? "READY" : "x 2"; return; }
            if (slot == 1) label.text = selected ? "EQUIPPED" : "SELECT";
            else
            {
                int ammo = slot == 2 ? f.throwables.ThunderAmmo.Value : f.throwables.StoneAmmo.Value;
                double remaining = f.throwables.ReadyAt.Value - f.Now;
                label.text = "x " + ammo + (ammo == 0 ? "  EMPTY" : remaining > 0 ? "  /  " + Mathf.CeilToInt((float)remaining) + "s" : selected ? "  EQUIPPED" : "  READY");
            }
        }
        // Inspector에 연결된 버튼이 Relay 방 생성/참가를 요청합니다.
        public async void Host() { await links.session.CreateRoomAsync(); }
        public async void Join() { await links.session.JoinRoomAsync(links.address.text); }
        public void CopyRoomCode() { if (links.session.RoomCode.Length > 0) GUIUtility.systemCopyBuffer = links.session.RoomCode; }
        public void Practice() { links.session.StartPractice(); }
        public void Resume() { Cursor.lockState = CursorLockMode.Locked; }
        public void Leave() { links.session.LeaveRoom(); links.address.text = ""; }
        public void Rematch() { if (links.session.match.IsSpawned) { links.session.match.RematchRpc(); Resume(); } }
        public void Help() { links.help.SetActive(!links.help.activeSelf); }
        public void Sword() { Equip(11); }
        public void Thunder() { Equip(12); }
        public void Wall() { Equip(13); }
        void Equip(int command) { var f = Local; if (f != null) f.SendControl(Vector2.zero, f.transform.eulerAngles.y, command, f.Pitch); }
        public void Dash() { var f=Local; if(f!=null) f.SendControl(Vector2.zero,f.transform.eulerAngles.y,4,f.Pitch); }
        public void Whirlwind() { var f=Local; if(f!=null) f.SendControl(Vector2.zero,f.transform.eulerAngles.y,5,f.Pitch); }
        public void Jump() { var f=Local; if(f!=null) f.SendControl(Vector2.zero,f.ViewYaw,6,f.Pitch); }
        public void Menu() { Cursor.lockState = CursorLockMode.None; }
        public void ToggleMenu()
        {
            // ESC로 메뉴를 열고 다시 닫습니다. 접속 전에는 방 선택 화면을 유지합니다.
            if (links.menu.activeSelf && links.session.network.IsConnectedClient) Resume();
            else Menu();
        }
    }
}
