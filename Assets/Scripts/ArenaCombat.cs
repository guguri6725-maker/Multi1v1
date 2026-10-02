using System;
using UnityEngine;

namespace FighterArena
{
    public enum CombatAction { Idle, Charging, LeftSlash, RightSlash, Thrust, Heavy, Guard, Stunned, Whirlwind, Dead, Switching, Throwing, Riposte, Recoil }

    // 서버와 테스트가 공유하는 전투 규칙. 원작의 정확한 수치가 아닌 프로토타입 밸런스입니다.
    public static class ArenaCombat
    {
        public const float ChargeTime = 1.5f;
        public const float FullChargeHold = 1.5f;
        public const float GuardRaiseTime = 0f;
        public const float ParryWindow = .2f;
        public const float GuardArc = 65f;
        public const float Reach = 2.7f;
        public static bool IsAttack(CombatAction a) => a >= CombatAction.LeftSlash && a <= CombatAction.Heavy || a == CombatAction.Riposte;
        public static bool IsLight(CombatAction a) => a >= CombatAction.LeftSlash && a <= CombatAction.Thrust;
        public static bool PiercesGuard(CombatAction a) => a == CombatAction.Heavy || a == CombatAction.Riposte;
        public static float Windup(CombatAction a) => a == CombatAction.Thrust ? .46f : .4f;
        public static float HitTime(CombatAction a) => IsLight(a) ? Windup(a) + .06f : Duration(a) * .42f;
        public static float Duration(CombatAction a) => a == CombatAction.Heavy ? .85f : a == CombatAction.Riposte ? .8f : a == CombatAction.Thrust ? .96f : .9f;
        public static float DamageAfterGuard(float damage, bool guarding, bool facing, float guardAge, bool heavy, out bool parried)
        {
            // 누르는 즉시 방어하고 최초 0.2초는 패링으로 판정합니다.
            bool ready = guarding && facing && guardAge >= GuardRaiseTime;
            parried = ready && guardAge <= GuardRaiseTime + ParryWindow;
            if (parried) return 0;
            // 차징 강공격과 패링 반격은 일반 방어를 관통하며 패링만 가능합니다.
            if (heavy) return damage;
            return ready ? Mathf.Min(1f, damage) : damage;
        }
        public static CombatAction Combo(int index) => (CombatAction)((int)CombatAction.LeftSlash + index % 3);
        public static bool Finite(float f) => !float.IsNaN(f) && !float.IsInfinity(f);
    }
}
