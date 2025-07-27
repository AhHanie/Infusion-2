using HarmonyLib;
using RimWorld;
using Verse;

namespace Infusion.Harmonize
{
    [HarmonyPatch(typeof(Bullet), "Impact")]
    public static class BulletPatches
    {
        [HarmonyPatch(typeof(Bullet), "Impact")]
        /// <summary>
        /// Applies on hit effects for bullet impacts.
        /// </summary>
        public static class Impact
        {
            /// <summary>
            /// Stores the map reference before the impact for use in postfix.
            /// </summary>
            public static void Prefix(Thing hitThing, Bullet __instance, out Map __state)
            {
                __state = __instance.Map;
            }

            /// <summary>
            /// Processes on-hit effects after bullet impact.
            /// </summary>
            public static void Postfix(Thing hitThing, Bullet __instance, Map __state)
            {
                var baseDamage = (float)__instance.DamageAmount;

                // Try to get the launcher as a Pawn
                if (__instance.Launcher is Pawn pawn)
                {
                    // Get primary equipment
                    var primaryEquipment = pawn.equipment?.Primary;
                    if (primaryEquipment != null)
                    {
                        // Get on-hit workers from the equipment's infusion component
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

                            // Execute BulletHit for each on-hit worker
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