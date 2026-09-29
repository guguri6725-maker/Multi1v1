using Unity.Netcode;
using UnityEngine;

namespace FighterArena
{
    // Hierarchy에서 만든 네 기둥을 재사용하며 위치/수명/파괴 상태만 서버가 동기화합니다.
    public class ArenaStoneWall : NetworkBehaviour
    {
        [System.Serializable] public class References
        {
            public BoxCollider[] columns;
            public MeshRenderer[] visuals;
            public ArenaMatch match;
        }
        public References links = new References();
        public float lifetime = 10;
        public float columnHealth = 100;
        public NetworkVariable<Vector3> Center = new NetworkVariable<Vector3>();
        public NetworkVariable<float> Facing = new NetworkVariable<float>();
        public NetworkVariable<double> Expires = new NetworkVariable<double>();
        public NetworkVariable<int> Intact = new NetworkVariable<int>();
        public NetworkVariable<int> CapturedSlots = new NetworkVariable<int>();
        readonly float[] hp = new float[4];
        public bool Standing => IsSpawned && Intact.Value != 0 && NetworkManager.ServerTime.Time < Expires.Value;

        void Awake() { Show(false); }
        void Show(bool visible)
        {
            if (links.columns == null || links.visuals == null) return;
            for (int i = 0; i < links.columns.Length; i++)
            {
                bool alive = visible && (Intact.Value & (1 << i)) != 0;
                links.columns[i].enabled = alive;
                links.visuals[i].enabled = alive;
            }
        }
        void Update()
        {
            if (!IsSpawned) { Show(false); return; }
            if (IsServer && (!links.match.Live.Value || NetworkManager.ServerTime.Time >= Expires.Value)) Intact.Value = 0;
            transform.SetPositionAndRotation(Center.Value, Quaternion.Euler(0, Facing.Value, 0));
            Show(Standing);
            if (IsServer && CapturedSlots.Value != 0)
            {
                int captured = CapturedSlots.Value;
                if (!Standing || !Overlaps(links.match.blue)) captured &= ~1;
                if (!Standing || !Overlaps(links.match.red)) captured &= ~2;
                CapturedSlots.Value = captured;
            }
        }
        public bool Raise(Vector3 impact, float yaw)
        {
            if (!IsServer || !links.match.Live.Value) return false;
            Clear();
            // 캐릭터/다른 돌벽 위가 아니라 충돌 위치 아래의 바닥에 세웁니다.
            bool found = false; float closest = float.MaxValue; Vector3 ground = impact;
            foreach (var h in Physics.RaycastAll(impact + Vector3.up * .5f, Vector3.down, 25, ~0, QueryTriggerInteraction.Ignore))
            {
                if (h.collider is CharacterController || h.collider.GetComponentInParent<ArenaStoneWall>() != null || h.normal.y < .6f || h.distance >= closest) continue;
                closest = h.distance; ground = h.point; found = true;
            }
            if (!found) return false;
            Center.Value = ground;
            Facing.Value = yaw;
            Expires.Value = NetworkManager.ServerTime.Time + lifetime;
            transform.SetPositionAndRotation(ground, Quaternion.Euler(0, yaw, 0));
            Physics.SyncTransforms();
            int mask = 0;
            for (int i = 0; i < 4; i++)
            {
                var c = links.columns[i];
                Vector3 center = c.transform.TransformPoint(c.center);
                Vector3 half = Vector3.Scale(c.size, c.transform.lossyScale) * .48f;
                // 캐릭터와 겹치는 기둥도 생성하여 가둡니다. 기존 지형만 제외합니다.
                bool blocked = false;
                foreach (var overlap in Physics.OverlapBox(center, half, c.transform.rotation, ~0, QueryTriggerInteraction.Ignore))
                    if (!(overlap is CharacterController)) { blocked = true; break; }
                if (!blocked) { mask |= 1 << i; hp[i] = columnHealth; }
            }
            Intact.Value = mask;
            Show(Standing);
            Physics.SyncTransforms();
            // 생성 순간 실제로 몸 안에 생긴 벽만 가둡니다. 나중에 접촉한 사람은 제외합니다.
            CapturedSlots.Value = (Overlaps(links.match.blue) ? 1 : 0) | (Overlaps(links.match.red) ? 2 : 0);
            return mask != 0;
        }
        bool Overlaps(ArenaFighter fighter)
        {
            if (fighter == null || !Standing) return false;
            var motor = fighter.motor;
            Vector3 center = fighter.transform.TransformPoint(motor.center);
            float radius = Mathf.Max(.05f,motor.radius-.04f);
            float half = Mathf.Max(0,motor.height*.5f-motor.radius);
            foreach(var c in Physics.OverlapCapsule(center+Vector3.up*half,center-Vector3.up*half,radius,~0,QueryTriggerInteraction.Ignore))
                if(c.GetComponentInParent<ArenaStoneWall>()==this) return true;
            return false;
        }
        public bool Captures(ArenaFighter fighter) => Standing && (CapturedSlots.Value & (1<<fighter.slot)) != 0 && Overlaps(fighter);
        public void DamageColumn(Collider column, float amount, bool heavy = false)
        {
            // 일반 공격으로는 유지되고 강공격/패링 반격 한 번에 기둥이 부서집니다.
            if (!IsServer || !Standing || !heavy) return;
            for (int i = 0; i < 4; i++)
                if (column == links.columns[i] && (Intact.Value & (1 << i)) != 0)
                {
                    hp[i] = 0;
                    if (hp[i] <= 0) { Intact.Value &= ~(1 << i); Show(Standing); }
                    break;
                }
        }
        public void Clear()
        {
            if (IsServer) { Intact.Value = 0; Expires.Value = 0; CapturedSlots.Value = 0; }
            Show(false);
        }
    }
}
