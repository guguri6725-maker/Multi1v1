using Unity.Netcode;
using UnityEngine;

namespace FighterArena
{
    // 투사체, 손에 든 병, 폭발 표시, 돌벽은 미리 제작한 씬 오브젝트 풀을 사용합니다.
    public class ArenaThrowables : NetworkBehaviour
    {
        [System.Serializable] public class References
        {
            public ArenaFighter fighter;
            public Transform projectile;
            public MeshRenderer projectileRenderer;
            public MeshRenderer heldThunder, heldStone;
            public Transform burst;
            public MeshRenderer burstRenderer;
            public ArenaStoneWall firstWall, secondWall;
            public Material thunderMaterial, stoneMaterial;
            public Transform aimMarker;
            public MeshRenderer aimRenderer;
        }
        public References links = new References();
        public int flasksPerType = 2;
        public float sharedCooldown = 8;
        public float throwSpeed = 13;
        public float thunderRadius = 4;
        public float thunderDamage = 15;
        public float slowDuration = 6;
        public NetworkVariable<int> ThunderAmmo = new NetworkVariable<int>(2);
        public NetworkVariable<int> StoneAmmo = new NetworkVariable<int>(2);
        public NetworkVariable<double> ReadyAt = new NetworkVariable<double>();
        public NetworkVariable<int> FlightKind = new NetworkVariable<int>();
        public NetworkVariable<Vector3> FlightPosition = new NetworkVariable<Vector3>();
        public NetworkVariable<Vector3> ImpactPosition = new NetworkVariable<Vector3>();
        public NetworkVariable<double> BurstAt = new NetworkVariable<double>(-100);
        Vector3 velocity;
        float thrownYaw;
        double launchedAt;
        bool nextWall;
        ArenaFighter Fighter => links.fighter;
        double Now => NetworkManager.ServerTime.Time;

        public bool CanThrow(int slot) => IsSpawned && FlightKind.Value == 0 && Now >= ReadyAt.Value && (slot == 2 ? ThunderAmmo.Value > 0 : slot == 3 && StoneAmmo.Value > 0);
        public void ResetRound()
        {
            if (!IsServer) return;
            ThunderAmmo.Value = StoneAmmo.Value = flasksPerType;
            ReadyAt.Value = 0; FlightKind.Value = 0; BurstAt.Value = -100;
            nextWall = false;
            if (links.firstWall != null) links.firstWall.Clear();
            if (links.secondWall != null) links.secondWall.Clear();
        }
        void Awake()
        {
            if (links.projectileRenderer != null) links.projectileRenderer.enabled = false;
            if (links.burstRenderer != null) links.burstRenderer.enabled = false;
            if (links.aimRenderer != null) links.aimRenderer.enabled = false;
            if (links.heldThunder != null) links.heldThunder.enabled = false;
            if (links.heldStone != null) links.heldStone.enabled = false;
        }
        Vector3 LaunchVelocity() => Quaternion.Euler(Fighter.AimPitch.Value, Fighter.transform.eulerAngles.y, 0) * Vector3.forward * throwSpeed + Vector3.up * 2;
        public void Launch(int kind)
        {
            if (!IsServer || !Fighter.match.Live.Value || !CanThrow(kind)) return;
            if (kind == 2) ThunderAmmo.Value--; else StoneAmmo.Value--;
            ReadyAt.Value = Now + sharedCooldown;
            FlightPosition.Value = Fighter.transform.position + Vector3.up * 1.5f;
            velocity = LaunchVelocity();
            thrownYaw = Fighter.transform.eulerAngles.y;
            launchedAt = Now;
            FlightKind.Value = kind;
        }

        bool Sweep(Vector3 start, Vector3 step, out RaycastHit closestHit)
        {
            closestHit = default;
            float closest = float.MaxValue;
            foreach (var hit in Physics.SphereCastAll(start, .12f, step.normalized, step.magnitude, ~0, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider == Fighter.motor || hit.distance >= closest) continue;
                closestHit = hit; closest = hit.distance;
            }
            return closest < float.MaxValue;
        }
        void FixedUpdate()
        {
            if (!IsSpawned || !IsServer || FlightKind.Value == 0) return;
            if (!Fighter.match.Live.Value) { FlightKind.Value = 0; return; }
            velocity += Physics.gravity * Time.fixedDeltaTime;
            Vector3 step = velocity * Time.fixedDeltaTime;
            if (Sweep(FlightPosition.Value, step, out var hit))
            {
                Vector3 point = hit.distance <= .001f ? FlightPosition.Value : hit.point + hit.normal * .14f;
                // 벽 수류탄이 사람을 맞히면 발밑에 세워 실제로 가둡니다.
                if (FlightKind.Value == 3 && hit.collider is CharacterController) point = hit.collider.transform.position + Vector3.up;
                Impact(point);
            }
            else
            {
                FlightPosition.Value += step;
                if (Now - launchedAt > 5 || FlightPosition.Value.y < -5) FlightKind.Value = 0;
            }
        }
        void Impact(Vector3 point)
        {
            int kind = FlightKind.Value;
            FlightKind.Value = 0;
            FlightPosition.Value = ImpactPosition.Value = point;
            if (kind == 2)
            {
                BurstAt.Value = Now;
                // 자신도 같은 폭발 피해와 둔화를 받아 근거리 투척에 위험이 따릅니다.
                ApplyThunder(Fighter, point);
                ApplyThunder(Fighter.opponent, point);
                Fighter.match.ResolveExplosionDeaths();
            }
            else
            {
                var wall = nextWall ? links.secondWall : links.firstWall;
                nextWall = !nextWall;
                wall.Raise(point, thrownYaw);
            }
        }
        void ApplyThunder(ArenaFighter target, Vector3 center)
        {
            Vector3 targetPoint = target.transform.position + Vector3.up;
            Vector3 delta = targetPoint - center;
            if (delta.magnitude > thunderRadius || target.Health.Value <= 0) return;
            // 지형이나 돌벽 뒤에 있는 대상에게 둔화가 관통하지 않습니다.
            foreach (var hit in Physics.RaycastAll(center, delta.normalized, delta.magnitude, ~0, QueryTriggerInteraction.Ignore))
                if (hit.collider != target.motor && hit.collider != Fighter.motor) return;
            target.ReceiveThunder(thunderDamage, slowDuration, false);
        }
        void Update()
        {
            if (!IsSpawned) return;
            bool alive = Fighter.Health.Value > 0;
            links.heldThunder.enabled = alive && Fighter.Equipped.Value == 2;
            links.heldStone.enabled = alive && Fighter.Equipped.Value == 3;
            links.projectileRenderer.enabled = FlightKind.Value != 0;
            if (FlightKind.Value != 0)
            {
                links.projectile.position = FlightPosition.Value;
                links.projectile.Rotate(0, 180 * Time.deltaTime, 120 * Time.deltaTime);
                links.projectileRenderer.sharedMaterial = FlightKind.Value == 2 ? links.thunderMaterial : links.stoneMaterial;
            }
            float age = (float)(Now - BurstAt.Value);
            links.burstRenderer.enabled = age >= 0 && age < .4f;
            if (links.burstRenderer.enabled)
            {
                links.burst.position = ImpactPosition.Value + Vector3.up * .03f;
                float diameter = Mathf.Lerp(.2f, thunderRadius * 2, age / .4f);
                links.burst.localScale = new Vector3(diameter, .06f, diameter);
            }
            bool aiming = Fighter.Local && Fighter.match.Live.Value && Fighter.Equipped.Value != 1 && Fighter.Action.Value != CombatAction.Throwing;
            links.aimRenderer.enabled = false;
            if (aiming)
            {
                Vector3 pos = Fighter.transform.position + Vector3.up * 1.5f;
                Vector3 vel = LaunchVelocity();
                // 투척 전 예상 낙하지점을 표시합니다. 서버 투사체와 같은 중력을 사용합니다.
                for (int i = 0; i < 100; i++)
                {
                    vel += Physics.gravity * .04f;
                    Vector3 step = vel * .04f;
                    if (Sweep(pos, step, out var hit))
                    {
                        links.aimMarker.position = hit.point + hit.normal * .04f;
                        links.aimRenderer.enabled = true;
                        break;
                    }
                    pos += step;
                }
            }
        }
    }
}
