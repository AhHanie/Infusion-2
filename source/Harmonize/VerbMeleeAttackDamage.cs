using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace Infusion.Harmonize
{
    [HarmonyPatch(typeof(Verb_MeleeAttackDamage), "ApplyMeleeDamageToTarget")]
    public static class ApplyMeleeDamageToTarget
    {
        public static void Prefix(LocalTargetInfo target, Verb_MeleeAttackDamage __instance)
        {
            ThingWithComps equipmentSource = __instance.EquipmentSource;
            if (equipmentSource == null || equipmentSource.def?.IsMeleeWeapon != true)
            {
                return;
            }
            (List<PreHitWorker>, CompInfusion)? tuple = CompInfusionExtensions.ForPreHitWorkers(equipmentSource);
            if (!tuple.HasValue)
            {
                return;
            }
            var (list, compInfusion) = tuple.Value;
            foreach (PreHitWorker item in list)
            {
                VerbRecordData record = new VerbRecordData(0f, compInfusion.parent, target.Thing, __instance);
                item.PreMeleeHit(record);
            }
        }

        public static void Postfix(LocalTargetInfo target, Verb_MeleeAttackDamage __instance, DamageWorker.DamageResult __result)
        {
            ThingWithComps equipmentSource = __instance.EquipmentSource;
            if (equipmentSource == null || equipmentSource.def?.IsMeleeWeapon != true)
            {
                return;
            }
            (List<OnHitWorker>, CompInfusion)? tuple = CompInfusionExtensions.ForOnHitWorkers(equipmentSource);
            if (!tuple.HasValue)
            {
                return;
            }
            var (list, compInfusion) = tuple.Value;
            foreach (OnHitWorker item in list)
            {
                VerbRecordData record = new VerbRecordData(__result.totalDamageDealt, compInfusion.parent, target.Thing, __instance, __result);
                item.MeleeHit(record);
            }
        }
    }
}