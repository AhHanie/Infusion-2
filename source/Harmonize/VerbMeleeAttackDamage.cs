using HarmonyLib;
using RimWorld;
using Verse;

namespace Infusion.Harmonize
{
    [HarmonyPatch(typeof(Verb_MeleeAttackDamage), "ApplyMeleeDamageToTarget")]
    public static class VerbMeleeAttackDamagePatches
    {
        /// <summary>
        /// Applies on hit effects for melee attacks.
        /// </summary>
        public static class ApplyMeleeDamageToTarget
        {
            /// <summary>
            /// Processes on-hit effects after melee damage is applied to target.
            /// </summary>
            public static void Postfix(LocalTargetInfo target, Verb_MeleeAttackDamage __instance)
            {
                // Get the weapon equipment source
                var equipmentSource = __instance.EquipmentSource;
                if (equipmentSource?.def?.IsMeleeWeapon != true)
                    return;

                // Get on-hit workers from the equipment's infusion component
                var onHitData = CompInfusionExtensions.ForOnHitWorkers(equipmentSource);
                if (!onHitData.HasValue)
                    return;

                var (workers, comp) = onHitData.Value;

                // Execute MeleeHit for each on-hit worker
                foreach (var onHit in workers)
                {
                    var verbData = new VerbRecordData(
                        baseDamage: GetAdjustedMeleeDamage(__instance),
                        source: comp.parent,
                        target: target.Thing,
                        verb: __instance
                    );

                    onHit.MeleeHit(verbData);
                }
            }
        }

        /// <summary>
        /// Gets the adjusted melee damage for a verb.
        /// This mirrors the F# Verb.getAdjustedMeleeDamage function.
        /// </summary>
        private static float GetAdjustedMeleeDamage(Verb_MeleeAttackDamage verb)
        {
            // This would need to be implemented based on your mod's damage calculation logic
            // For now, returning a basic damage amount from the verb properties
            if (verb.verbProps?.meleeDamageDef != null)
            {
                return verb.verbProps.meleeDamageDef.defaultDamage;
            }
            return 0f;
        }
    }
}