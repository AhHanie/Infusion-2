using HarmonyLib;
using RimWorld;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine.Profiling;
using Verse;

namespace Infusion.Harmonize
{
    public static class VerbPatches
    {
        private static class VerbReflection
        {
            public static readonly FieldInfo burstShotsLeftField = AccessTools.Field(typeof(Verb), "burstShotsLeft");
            public static readonly FieldInfo currentTargetField = AccessTools.Field(typeof(Verb), "currentTarget");
        }

        /// <summary>
        /// Gets the adjusted melee damage for a verb.
        /// </summary>
        private static float GetAdjustedMeleeDamage(Verb verb)
        {
            // This would need to be implemented based on your mod's damage calculation logic
            // For now, returning a basic damage amount
            if (verb.verbProps?.meleeDamageDef != null)
            {
                return verb.verbProps.meleeDamageDef.defaultDamage;
            }
            return 0f;
        }

        [HarmonyPatch(typeof(Verb), "TryCastNextBurstShot")]
        public static class TryCastNextBurstShot
        {
            /// <summary>
            /// Handles on-hit effects when a burst is completed (last shot fired).
            /// </summary>
            public static void Postfix(Verb __instance)
            {
                // Check if this was the last shot in the burst
                var burstShotsLeft = (int)VerbReflection.burstShotsLeftField.GetValue(__instance);
                if (burstShotsLeft != 0)
                    return;

                // Get the current target
                var currentTarget = (LocalTargetInfo)VerbReflection.currentTargetField.GetValue(__instance);

                // Get the equipment comp source and check for on-hit workers
                var equipmentCompSource = __instance.EquipmentCompSource;
                if (equipmentCompSource?.parent == null)
                    return;

                var onHitData = CompInfusionExtensions.ForOnHitWorkers(equipmentCompSource.parent);
                if (!onHitData.HasValue)
                    return;

                var (workers, comp) = onHitData.Value;

                foreach (var onHit in workers)
                {
                    // Determine if this is a melee attack
                    bool isMelee = __instance.GetType().IsSubclassOf(typeof(Verb_MeleeAttack));

                    // Calculate base damage
                    float baseDamage;
                    if (isMelee)
                    {
                        baseDamage = GetAdjustedMeleeDamage(__instance);
                    }
                    else
                    {
                        // Ranged damage calculation
                        var defaultProjectile = __instance.verbProps?.defaultProjectile;
                        if (defaultProjectile?.projectile != null)
                        {
                            var projectileDamage = (float)defaultProjectile.projectile.GetDamageAmount(comp.parent);
                            var burstMultiplier = Math.Max(1f,
                                Math.Round((float)__instance.verbProps.burstShotCount / 5f));
                            baseDamage = (float)projectileDamage * (float)burstMultiplier;
                        }
                        else
                        {
                            baseDamage = 0f;
                        }
                    }

                    // Only proceed if there's actual damage
                    if (baseDamage > 0f)
                    {
                        var verbData = new VerbRecordData(
                            baseDamage: baseDamage,
                            source: comp.parent,
                            target: currentTarget.Thing,
                            verb: __instance
                        );

                        // Create the appropriate record type and call AfterAttack
                        VerbCastedRecord record = null;

                        if (isMelee) 
                        {
                            record = new VerbCastedRecordMelee(verbData);
                        }
                        else
                        {
                            new VerbCastedRecordRanged(verbData);
                        }
                        onHit.AfterAttack(record);
                    }
                }
            }
        }
    }
}