using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
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

        [HarmonyPatch(typeof(Verb), "TryCastNextBurstShot")]
        public static class TryCastNextBurstShot
        {
            public static void Postfix(Verb __instance)
            {
                if ((int)VerbReflection.burstShotsLeftField.GetValue(__instance) != 0)
                {
                    return;
                }
                LocalTargetInfo localTargetInfo = (LocalTargetInfo)VerbReflection.currentTargetField.GetValue(__instance);
                CompEquippable equipmentCompSource = __instance.EquipmentCompSource;
                if (equipmentCompSource?.parent == null)
                {
                    return;
                }
                (List<OnHitWorker>, CompInfusion)? tuple = CompInfusionExtensions.ForOnHitWorkers(equipmentCompSource.parent);
                if (!tuple.HasValue)
                {
                    return;
                }
                var (list, compInfusion) = tuple.Value;
                foreach (OnHitWorker item in list)
                {
                    bool flag = __instance.GetType().IsSubclassOf(typeof(Verb_MeleeAttack));
                    VerbRecordData data = new VerbRecordData(0f, compInfusion.parent, localTargetInfo.Thing, __instance);
                    VerbCastedRecord verbCastedRecord = null;
                    verbCastedRecord = ((!flag) ? ((VerbCastedRecord)new VerbCastedRecordRanged(data)) : ((VerbCastedRecord)new VerbCastedRecordMelee(data)));
                    item.AfterAttack(verbCastedRecord);
                }
            }
        }
    }
}