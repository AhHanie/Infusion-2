using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Infusion.Harmonize
{
    [HarmonyPatch(typeof(Verb_MeleeAttackDamage), "ApplyMeleeDamageToTarget")]
    public static class ApplyMeleeDamageToTarget
    {
        public static void Postfix(LocalTargetInfo target, Verb_MeleeAttackDamage __instance, DamageWorker.DamageResult __result)
        {
            var equipmentSource = __instance.EquipmentSource;
            if (equipmentSource?.def?.IsMeleeWeapon != true)
                return;

            var onHitData = CompInfusionExtensions.ForOnHitWorkers(equipmentSource);
            if (!onHitData.HasValue)
                return;

            var (workers, comp) = onHitData.Value;

            foreach (var onHit in workers)
            {
                var verbData = new VerbRecordData(
                    baseDamage: __result.totalDamageDealt,
                    source: comp.parent,
                    target: target.Thing,
                    verb: __instance,
                    damageResult: __result
                );

                onHit.MeleeHit(verbData);
            }
        }
    }
}