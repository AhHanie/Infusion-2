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
            public static void Postfix(Verb __instance)
            {
                var burstShotsLeft = (int)VerbReflection.burstShotsLeftField.GetValue(__instance);
                if (burstShotsLeft != 0)
                    return;

                var currentTarget = (LocalTargetInfo)VerbReflection.currentTargetField.GetValue(__instance);

                var equipmentCompSource = __instance.EquipmentCompSource;
                if (equipmentCompSource?.parent == null)
                    return;

                var onHitData = CompInfusionExtensions.ForOnHitWorkers(equipmentCompSource.parent);
                if (!onHitData.HasValue)
                    return;

                var (workers, comp) = onHitData.Value;

                foreach (var onHit in workers)
                {
                    bool isMelee = __instance.GetType().IsSubclassOf(typeof(Verb_MeleeAttack));

                    float baseDamage;
                    if (isMelee)
                    {
                        baseDamage = GetAdjustedMeleeDamage(__instance);
                    }
                    else
                    {
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

                    if (baseDamage > 0f)
                    {
                        var verbData = new VerbRecordData(
                            baseDamage: baseDamage,
                            source: comp.parent,
                            target: currentTarget.Thing,
                            verb: __instance
                        );

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