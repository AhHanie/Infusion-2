using HarmonyLib;
using UnityEngine;
using Verse;
using RimWorld;
using Infusion.Comps;

namespace Infusion.Harmonize
{
    // Targets the eight-parameter virtual overload explicitly; the seven-parameter convenience
    // overload just forwards to this one, so patching it too would be redundant and ambiguous.
    [HarmonyPatch(typeof(Projectile), nameof(Projectile.Launch),
        new[] { typeof(Thing), typeof(Vector3), typeof(LocalTargetInfo), typeof(LocalTargetInfo),
            typeof(ProjectileHitFlags), typeof(bool), typeof(Thing), typeof(ThingDef) })]
    public static class Projectile_Launch_SmartInfusion
    {
        public static void Prefix(Projectile __instance, Thing equipment, LocalTargetInfo intendedTarget,
            ref LocalTargetInfo usedTarget, ref ProjectileHitFlags hitFlags, ref ThingDef targetCoverDef)
        {
            if (intendedTarget.Thing == null)
            {
                return;
            }

            if (__instance.def.thingClass != typeof(Bullet))
            {
                return;
            }

            if (!SmartInfusionHelper.ShouldForceSmartHit(equipment))
            {
                return;
            }

            usedTarget = intendedTarget;
            hitFlags = ProjectileHitFlags.IntendedTarget;
            targetCoverDef = null;
        }
    }

    [HarmonyPatch(typeof(Verb_LaunchProjectile), "WarmupComplete")]
    public static class WarmupComplete_UnstableInfusion
    {
        public static void Prefix(Verb_LaunchProjectile __instance)
        {
            ThingWithComps equipmentSource = __instance.EquipmentSource;

            if (equipmentSource == null)
            {
                return;
            }

            GameComponent_Infusion gameComp = Current.Game.GetComponent<GameComponent_Infusion>();

            // Ensure no stale projectile remains from a previous burst.
            gameComp.ClearUnstableBurstProjectile(equipmentSource);

            CompInfusion compInfusion = equipmentSource.TryGetComp<CompInfusion>();
            if (compInfusion == null)
            {
                return;
            }

            InfusionDef unstableInfusion = compInfusion.TryGetInfusionDefWithTag(InfusionTags.UNSTABLE);
            if (unstableInfusion == null)
            {
                return;
            }

            ThingDef originalProjectile = __instance.Projectile;
            if (!typeof(Bullet).IsAssignableFrom(originalProjectile.thingClass))
            {
                return;
            }

            ThingDef randomizedProjectile = Infusion.UnstableProjectileHelper.GetRandomBulletProjectile(originalProjectile);
            gameComp.SetUnstableBurstProjectile(equipmentSource, randomizedProjectile);
        }
    }

    [HarmonyPatch(typeof(Verb_LaunchProjectile), "Projectile", MethodType.Getter)]
    public static class Projectile_UnstableInfusion
    {
        public static void Postfix(Verb_LaunchProjectile __instance, ref ThingDef __result)
        {
            ThingWithComps equipmentSource = __instance.EquipmentSource;

            if (equipmentSource == null)
            {
                return;
            }

            CompInfusion compInfusion = equipmentSource.TryGetComp<CompInfusion>();
            if (compInfusion?.TryGetInfusionDefWithTag(InfusionTags.UNSTABLE) == null)
            {
                return;
            }

            GameComponent_Infusion gameComp = Current.Game.GetComponent<GameComponent_Infusion>();

            if (gameComp.TryGetUnstableBurstProjectile(equipmentSource, out ThingDef cachedProjectile))
            {
                __result = cachedProjectile;
            }
        }
    }
}
