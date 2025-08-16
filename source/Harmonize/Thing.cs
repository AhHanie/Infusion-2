using HarmonyLib;
using System;
using Verse;

namespace Infusion.Harmonize
{
    public class ThingPatches
    {
        [HarmonyPatch(typeof(Thing), "TakeDamage")]
        public static class TakeDamage
        {
            public static Nullable<DamageInfo> damageTakenDuringBulletImpact;
            public static void Prefix(DamageInfo dinfo)
            {
                if (StaticFlags.DuringBulletImpact || StaticFlags.DuringApplyMeleeDamageToTarget && !damageTakenDuringBulletImpact.HasValue)
                {
                    damageTakenDuringBulletImpact = dinfo;
                }
            }
        }
    }
}
