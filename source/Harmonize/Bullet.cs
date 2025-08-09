using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Infusion.Harmonize
{
    public class BulletImpactPatchState
    {
        public Map map = null;

        public bool canRunOnHitWorkers = false;
    }

    [HarmonyPatch(typeof(Bullet), "Impact")]
    public static class Impact
    {
        public static void Prefix(Thing hitThing, Bullet __instance, out BulletImpactPatchState __state)
        {
            __state = new BulletImpactPatchState();
            __state.map = __instance.Map;
            if (!(__instance.Launcher is Pawn pawn))
            {
                return;
            }
            ThingWithComps thingWithComps = pawn.equipment?.Primary;
            if (thingWithComps == null)
            {
                return;
            }
            __state.canRunOnHitWorkers = true;
            (List<PreHitWorker>, CompInfusion)? tuple = CompInfusionExtensions.ForPreHitWorkers(thingWithComps);
            if (!tuple.HasValue)
            {
                return;
            }
            (List<PreHitWorker>, CompInfusion) value = tuple.Value;
            List<PreHitWorker> item = value.Item1;
            CompInfusion item2 = value.Item2;
            float baseDamage = __instance.DamageAmount;
            ProjectileRecord record = new ProjectileRecord(baseDamage, __state.map, __instance, item2.parent, hitThing);
            foreach (PreHitWorker item3 in item)
            {
                item3.PreBulletHit(record);
            }
        }

        public static void Postfix(Thing hitThing, Bullet __instance, BulletImpactPatchState __state)
        {
            if (!__state.canRunOnHitWorkers)
            {
                return;
            }
            float baseDamage = __instance.DamageAmount;
            ThingWithComps primary = (__instance.Launcher as Pawn).equipment.Primary;
            (List<OnHitWorker>, CompInfusion)? tuple = CompInfusionExtensions.ForOnHitWorkers(primary);
            if (!tuple.HasValue)
            {
                return;
            }
            (List<OnHitWorker>, CompInfusion) value = tuple.Value;
            List<OnHitWorker> item = value.Item1;
            CompInfusion item2 = value.Item2;
            ProjectileRecord record = new ProjectileRecord(baseDamage, __state.map, __instance, item2.parent, hitThing);
            foreach (OnHitWorker item3 in item)
            {
                item3.BulletHit(record);
            }
        }
    }
}