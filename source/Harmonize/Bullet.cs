using HarmonyLib;
using RimWorld;
using Verse;

namespace Infusion.Harmonize
{
    [HarmonyPatch(typeof(Bullet), "Impact")]
    public static class BulletPatches
    {
        [HarmonyPatch(typeof(Bullet), "Impact")]
        public static class Impact
        {
            public static void Prefix(Thing hitThing, Bullet __instance, out Map __state)
            {
                __state = __instance.Map;
            }
            public static void Postfix(Thing hitThing, Bullet __instance, Map __state)
            {
                var baseDamage = (float)__instance.DamageAmount;

                if (__instance.Launcher is Pawn pawn)
                {
                    var primaryEquipment = pawn.equipment?.Primary;
                    if (primaryEquipment != null)
                    {
                        var onHitData = CompInfusionExtensions.ForOnHitWorkers(primaryEquipment);
                        if (onHitData.HasValue)
                        {
                            var (workers, comp) = onHitData.Value;

                            var projectileRecord = new ProjectileRecord(
                                baseDamage: baseDamage,
                                map: __state,
                                projectile: __instance,
                                source: comp.parent,
                                target: hitThing
                            );

                            foreach (var worker in workers)
                            {
                                worker.BulletHit(projectileRecord);
                            }
                        }
                    }
                }
            }
        }
    }
}