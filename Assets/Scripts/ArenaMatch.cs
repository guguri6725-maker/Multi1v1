using UnityEngine;
using Unity.Netcode;

namespace FighterArena
{
    // 두 개의 씬 캐릭터를 접속자에게 배정하고 서버에서 라운드 흐름을 관리합니다.
    public class ArenaMatch : NetworkBehaviour
    {
        [System.Serializable] public class References { public ArenaFighter blue, red; public ArenaSession session; }
        public References links = new References();
        public ArenaFighter blue => links.blue;
        public ArenaFighter red => links.red;
        public ArenaSession session => links.session;
        public bool trainingOpponentAttacks = true;
        public NetworkVariable<bool> Live = new NetworkVariable<bool>();
        public NetworkVariable<int> BlueWins = new NetworkVariable<int>();
        public NetworkVariable<int> RedWins = new NetworkVariable<int>();
        public NetworkVariable<int> Phase = new NetworkVariable<int>(); // 0 대기, 1 카운트다운, 2 전투, 3 결과, 4 경기 종료
        public NetworkVariable<double> Deadline = new NetworkVariable<double>();
        double nextBot;
        bool assigned;
        public override void OnNetworkSpawn()
        {
            // 같은 실행에서 방을 다시 만들 때 이전 참가자 배정을 초기화합니다.
            assigned = false; nextBot = 0;
            if (IsServer) { Live.Value = false; Phase.Value = 0; }
        }

        void Update()
        {
            if (!IsSpawned || !IsServer) return;
            var ids = NetworkManager.ConnectedClientsIds;
            bool enough = ids.Count == 2 || session.Practice;
            if (!enough)
            {
                Live.Value = false;
                Phase.Value = 0;
                assigned = false;
                return;
            }
            if (!assigned)
            {
                blue.Driver.Value = ids[0];
                red.Driver.Value = session.Practice ? ulong.MaxValue : ids[1];
                BlueWins.Value = RedWins.Value = 0;
                assigned = true;
                StartRound();
            }
            double now = NetworkManager.ServerTime.Time;
            if (Phase.Value == 1 && now >= Deadline.Value) { Live.Value = true; Phase.Value = 2; }
            if (Phase.Value == 3 && now >= Deadline.Value) StartRound();
            if (session.Practice && trainingOpponentAttacks && Live.Value && now >= nextBot)
            {
                nextBot = now + 1.1;
                // 연습 상대는 패링/피격 확인용으로 제자리에서 공격합니다.
                red.ServerCommand(3);
                red.ServerCommand(0);
                red.ServerCommand(1);
            }
        }
        public void StartRound()
        {
            if (!IsServer) return;
            Live.Value = false;
            blue.ResetFighter(); red.ResetFighter();
            Phase.Value = 1;
            Deadline.Value = NetworkManager.ServerTime.Time + 3;
        }
        // 같은 폭발의 피해를 모두 적용한 뒤 승패를 정해 처리 순서에 따른 이득을 막습니다.
        public void ResolveExplosionDeaths()
        {
            if (!IsServer || !Live.Value) return;
            if (blue.Health.Value <= 0 && red.Health.Value <= 0)
            {
                Live.Value = false;
                Phase.Value = 3; // 동시 사망은 점수 없이 다음 라운드로 진행합니다.
                Deadline.Value = NetworkManager.ServerTime.Time + 3;
            }
            else if (blue.Health.Value <= 0) Defeated(0);
            else if (red.Health.Value <= 0) Defeated(1);
        }
        public void Defeated(int slot)
        {
            if (!IsServer || !Live.Value) return;
            Live.Value = false;
            if (slot == 0) RedWins.Value++; else BlueWins.Value++;
            Phase.Value = BlueWins.Value >= 3 || RedWins.Value >= 3 ? 4 : 3;
            Deadline.Value = NetworkManager.ServerTime.Time + 3;
        }
        [Rpc(SendTo.Server)]
        public void RematchRpc(RpcParams rpc = default)
        {
            if (Phase.Value != 4 || (rpc.Receive.SenderClientId != blue.Driver.Value && rpc.Receive.SenderClientId != red.Driver.Value)) return;
            BlueWins.Value = RedWins.Value = 0;
            StartRound();
        }
    }
}
