using System;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;

namespace FighterArena
{
    // 씬에 배치한 네트워크/카메라/HUD를 Inspector로 연결합니다.
    public class ArenaSession : MonoBehaviour
    {
        [Serializable] public class References
        {
            public NetworkManager network;
            public UnityTransport transport;
            public ArenaMatch match;
            public Camera viewCamera;
            public TMP_Text title, status, health, skills, controls, crosshair, loadout;
            public UnityEngine.UI.Image[] impactEdges;
        }
        public References links = new References();
        public NetworkManager network => links.network;
        public UnityTransport transport => links.transport;
        public ArenaMatch match => links.match;
        public Camera viewCamera => links.viewCamera;
        public TMP_Text title => links.title;
        public TMP_Text status => links.status;
        public TMP_Text health => links.health;
        public TMP_Text skills => links.skills;
        public TMP_Text controls => links.controls;
        public TMP_Text crosshair => links.crosshair;
        public string address = "127.0.0.1";
        public ushort port = 7777;
        public bool Practice { get; private set; }
        public string Notice { get; private set; } = "";
        public bool OnlineBusy { get; private set; }
        public bool RelayRoom { get; private set; }
        public string RoomCode { get; private set; } = "";
        int roomAttempt;
        Task authentication;
        bool started, captureNextFrame;
        float connectionStarted;

        void Start()
        {
            Application.runInBackground = true;
            network.ConnectionApprovalCallback = Approve;
            network.OnClientDisconnectCallback += Disconnected;
            network.OnClientConnectedCallback += Connected;
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--arena-address" && i + 1 < args.Length) address = args[++i];
            }
            int roomArgument = Array.IndexOf(args, "--arena-room");
            if (Array.IndexOf(args, "--arena-room-host") >= 0) _ = CreateRoomAsync();
            else if (roomArgument >= 0 && roomArgument + 1 < args.Length) _ = JoinRoomAsync(args[roomArgument + 1]);
            else if (Array.IndexOf(args, "--arena-host") >= 0) StartHost();
            else if (Array.IndexOf(args, "--arena-client") >= 0) StartClient();
            else if (Array.IndexOf(args, "--arena-practice") >= 0) StartPractice();
            if (Array.IndexOf(args, "--arena-netcheck") >= 0) StartCoroutine(ArenaDiagnostics.NetworkCheck(this));
            if (Array.IndexOf(args, "--arena-mindcheck") >= 0) StartCoroutine(ArenaMindgameChecks.NetworkRun(this));
            if (Array.IndexOf(args, "--arena-readcheck") >= 0) StartCoroutine(ArenaReadabilityChecks.ObserveNetwork(this));
            if (Array.IndexOf(args, "--arena-lookcheck") >= 0) StartCoroutine(ArenaLookChecks.ClientRun(this));
            if (Array.IndexOf(args, "--arena-walljumpcheck") >= 0) StartCoroutine(ArenaWallJumpChecks.ClientRun(this));
            if (Array.IndexOf(args, "--arena-predictcheck") >= 0) StartCoroutine(ArenaPredictionChecks.ClientRun(this));
            if (Array.IndexOf(args, "--arena-visualcheck") >= 0) StartCoroutine(ArenaVisualPredictionChecks.Run(this));
        }
        void Approve(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            response.Approved = !Practice && network.ConnectedClientsIds.Count < 2 || request.ClientNetworkId == NetworkManager.ServerClientId;
            response.CreatePlayerObject = false;
            response.Pending = false;
            response.Reason = response.Approved ? "" : "This duel already has two players (or is a practice session).";
        }
        public void StartHost() { Begin(true); }
        public void StartPractice() { Begin(true, true); }
        public void StartClient() { Begin(false); }
        public void LeaveRoom()
        {
            // 방을 나간 뒤 같은 실행에서 새 방 생성/재접속이 가능합니다.
            roomAttempt++; OnlineBusy = false; RelayRoom = false; RoomCode = "";
            network.Shutdown(); started = false; Practice = false; captureNextFrame = false;
            Notice = "Room closed. Choose a mode.";
            Cursor.lockState = CursorLockMode.None;
        }
        void Begin(bool host, bool practice = false)
        {
            if (started || OnlineBusy || network.IsListening || network.ShutdownInProgress) return;
            Practice = practice;
            RelayRoom = false; RoomCode = "";
            transport.SetConnectionData(address, port, "0.0.0.0");
            bool ok = host ? network.StartHost() : network.StartClient();
            started = ok;
            connectionStarted = Time.unscaledTime;
            Notice = ok ? host ? "Host ready. Waiting for opponent..." : "Connecting..." : "Connection could not start. Check address / port.";
            captureNextFrame = ok;
        }
        void Disconnected(ulong id)
        {
            if (!started) return;
            Notice = network.IsServer ? "Opponent disconnected. Waiting for a new challenger." : "Disconnected. You can create or join a room again. " + network.DisconnectReason;
            if (!network.IsServer) { started = false; RelayRoom = false; RoomCode = ""; }
            Cursor.lockState = CursorLockMode.None;
        }
        void Connected(ulong id)
        {
            if (!network.IsHost || network.ConnectedClientsIds.Count == 2) captureNextFrame = true;
        }

        // Relay 인증은 한 번 공유하고, 취소된 요청은 새 방 상태를 덮어쓰지 않습니다.
        async Task Authenticate()
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
            {
                var options = new InitializationOptions();
                string profile = Application.isEditor ? "editor" : "player";
                string[] args = Environment.GetCommandLineArgs();
                int p = Array.IndexOf(args, "--arena-profile");
                if (p >= 0 && p + 1 < args.Length) profile = args[p + 1];
                options.SetProfile(profile);
                await UnityServices.InitializeAsync(options);
            }
            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        async Task<T> WithTimeout<T>(Task<T> operation)
        {
            if (await Task.WhenAny(operation, Task.Delay(20000)) != operation)
            {
                // 늦게 실패하는 서비스 요청도 관찰하여 예외가 남지 않게 합니다.
                _ = operation.ContinueWith(t => { var ignored = t.Exception; }, TaskContinuationOptions.OnlyOnFaulted);
                throw new TimeoutException();
            }
            return await operation;
        }
        async Task<bool> Ready()
        {
            if (authentication == null || authentication.IsFaulted || authentication.IsCanceled) authentication = Authenticate();
            await authentication;
            return true;
        }
        public Task CreateRoomAsync() => OpenRelay(true, "");
        public Task JoinRoomAsync(string code) => OpenRelay(false, (code ?? "").Trim().ToUpperInvariant());
        async Task OpenRelay(bool host, string code)
        {
            if (OnlineBusy || started || network.IsListening || network.ShutdownInProgress) return;
            if (!host && (code.Length < 4 || code.Length > 12 || !System.Text.RegularExpressions.Regex.IsMatch(code, "^[A-Z0-9]+$")))
            { Notice = "Enter the room code shared by the host."; return; }
            int attempt = ++roomAttempt;
            OnlineBusy = true; Practice = false;
            Notice = host ? "Creating online room..." : "Joining online room...";
            try
            {
                await WithTimeout(Ready());
                if (this == null || attempt != roomAttempt) return;
                if (host)
                {
                    var allocation = await WithTimeout(RelayService.Instance.CreateAllocationAsync(1));
                    if (this == null || attempt != roomAttempt) return;
                    code = await WithTimeout(RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId));
                    if (this == null || attempt != roomAttempt) return;
                    transport.SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, "dtls"));
                }
                else
                {
                    var allocation = await WithTimeout(RelayService.Instance.JoinAllocationAsync(code));
                    if (this == null || attempt != roomAttempt) return;
                    transport.SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, "dtls"));
                }
                RelayRoom = true;
                started = host ? network.StartHost() : network.StartClient();
                if (!started) throw new InvalidOperationException("Transport could not start.");
                RoomCode = code; connectionStarted = Time.unscaledTime;
                Notice = host ? "ROOM " + code + " / Share code. Waiting for guest." : "Connecting to room " + code + "...";
                Cursor.lockState = CursorLockMode.None;
            }
            catch (Exception error)
            {
                if (this == null || attempt != roomAttempt) return;
                started = false; network.Shutdown(); RelayRoom = false; RoomCode = "";
                Notice = error is TimeoutException ? "Request timed out. Check internet and retry."
                    : host ? "Room creation failed. Check internet / Unity Relay service."
                    : "Cannot join. Check code, room capacity and host connection.";
                var serviceError = error as RequestFailedException;
                Debug.LogWarning("[ArenaRelay] " + error.GetType().Name + (serviceError == null ? "" : " code=" + serviceError.ErrorCode));
            }
            finally { if (this != null && attempt == roomAttempt) OnlineBusy = false; }
        }
        void Update()
        {
            var k = Keyboard.current;
            if (captureNextFrame) { captureNextFrame = false; Cursor.lockState = CursorLockMode.Locked; }
            if (k != null)
            {
                if (k.escapeKey.wasPressedThisFrame) GetComponent<ArenaMenu>().ToggleMenu();
                if (k.f1Key.wasPressedThisFrame && started) Cursor.lockState = CursorLockMode.Locked;
                if (k.rKey.wasPressedThisFrame && match.IsSpawned) match.RematchRpc();
            }
            Cursor.visible = Cursor.lockState != CursorLockMode.Locked;
            if (started && !network.IsConnectedClient && Time.unscaledTime - connectionStarted > 20)
            { LeaveRoom(); Notice = "Connection timed out. Check the room code and retry."; }
            title.text = "IRON DUEL";
            controls.text = "";
            if (!match.IsSpawned)
            {
                PaintImpact(null);
                status.text = started ? Notice : "ONE ARENA. TWO FIGHTERS. FIRST TO THREE.";
                health.text = "250 HP   /   GREATSWORD";
                skills.text = "";
                crosshair.text = "+";
                if (links.loadout != null) links.loadout.text = "";
                return;
            }
            var local = match.blue.Local ? match.blue : match.red.Local ? match.red : null;
            status.text = "BLUE  " + match.BlueWins.Value + "  :  " + match.RedWins.Value + "  RED\n" + PhaseText();
            // 상대 체력은 UI에 표시하지 않습니다.
            health.text = local == null ? "" : "YOUR HP  " + Mathf.CeilToInt(local.Health.Value) + " / " + Mathf.CeilToInt(local.maxHealth);
            PaintImpact(local);
            if (local != null)
            {
                string state = "";
                float age = (float)(local.Now - local.ActionAt.Value);
                if (local.Action.Value == CombatAction.Charging)
                    state = age < ArenaCombat.ChargeTime ? "CHARGING " + Mathf.FloorToInt(age / ArenaCombat.ChargeTime * 100) + "%" : "FULL CHARGE  /  AUTO IN " + Mathf.Max(0, 3 - age).ToString("F1") + "s";
                if (local.Now < local.SlowedUntil.Value) state += " / SLOWED " + Mathf.CeilToInt((float)(local.SlowedUntil.Value-local.Now)) + "s";
                skills.text = state + (local.Trapped ? "  /  TRAPPED - HEAVY TO BREAK" : "");
                if (links.loadout != null)
                {
                    string a = local.Equipped.Value == 1 ? "[1 SWORD]" : "1 SWORD";
                    string b = (local.Equipped.Value == 2 ? "[2 THUNDER " : "2 THUNDER ") + local.throwables.ThunderAmmo.Value + (local.Equipped.Value == 2 ? "]" : "");
                    string c = (local.Equipped.Value == 3 ? "[3 WALL " : "3 WALL ") + local.throwables.StoneAmmo.Value + (local.Equipped.Value == 3 ? "]" : "");
                    links.loadout.text = "";
                }
            }
            crosshair.text = "+";
        }
        void PaintImpact(ArenaFighter local)
        {
            // Hierarchy에 만든 화면 가장자리 표시만 갱신하며 중앙 시야는 가리지 않습니다.
            float age = local == null ? 100 : (float)(local.Now-local.FeedbackAt.Value);
            float alpha = age >= 0 && age < .32f && Cursor.lockState == CursorLockMode.Locked ? .55f*(1-age/.32f) : 0;
            Color color = local != null && local.Feedback.Value == 2 ? new Color(.2f,.7f,1,alpha) : local != null && local.Feedback.Value == 3 ? new Color(1,.7f,.15f,alpha) : new Color(1,.12f,.04f,alpha);
            foreach (var edge in links.impactEdges) if (edge != null) edge.color = color;
        }
        string Cooldown(double ready) => ready <= network.ServerTime.Time ? "READY" : Mathf.CeilToInt((float)(ready - network.ServerTime.Time)) + "s";
        string PhaseText()
        {
            switch (match.Phase.Value)
            {
                case 0: return Notice;
                case 1: return "GET READY   " + Mathf.Max(1, Mathf.CeilToInt((float)(match.Deadline.Value - network.ServerTime.Time)));
                case 2: return Practice ? "PRACTICE  /  APPROACH THE RED FIGHTER" : "FIGHT";
                case 3: return "ROUND OVER";
                case 4: return (match.BlueWins.Value > match.RedWins.Value ? "BLUE" : "RED") + " WINS THE DUEL   /   R TO REMATCH";
                default: return "";
            }
        }
        void LateUpdate()
        {
            if (!match.IsSpawned) return;
            var f = match.blue.Local ? match.blue : match.red.Local ? match.red : null;
            if (f == null) return;
            Quaternion look = Quaternion.Euler(f.Pitch,f.ViewYaw,0);
            viewCamera.transform.position = f.transform.position + Vector3.up * 1.7f + Quaternion.Euler(0,f.ViewYaw,0) * Vector3.forward * .07f;
            viewCamera.transform.rotation = look;
        }
        void OnDestroy()
        {
            roomAttempt++;
            if (network != null) network.OnClientConnectedCallback -= Connected;
            if (network != null) network.OnClientDisconnectCallback -= Disconnected;
            Cursor.lockState = CursorLockMode.None;
        }
    }
}
