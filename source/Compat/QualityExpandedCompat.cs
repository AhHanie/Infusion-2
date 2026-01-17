using HarmonyLib;
using RimWorld;
using System;
using System.Reflection;
using UnityEngine;
using Verse;

namespace Infusion.Compat
{
    [HarmonyPatch]
    public static class QualityExpandedCompat
    {
        public static bool Enabled => ModsConfig.IsActive("cozarkian.qualityexpanded");
        private static readonly Type QeType = AccessTools.TypeByName("QualityExpanded.Quality_HitPoints");
        private static readonly MethodInfo QeGetQualityFactor = QeType != null
            ? AccessTools.Method(QeType, "GetQualityFactor")
            : null;
        private static readonly float[] QualityFactorCache = new float[Enum.GetValues(typeof(QualityCategory)).Length];
        private static readonly bool[] QualityFactorCached = new bool[Enum.GetValues(typeof(QualityCategory)).Length];

        [HarmonyPrepare]
        public static bool Prepare()
        {
            return Enabled && QeGetQualityFactor != null;
        }

        private static bool TryGetQualityFactor(Thing thing, out float factor)
        {
            factor = 1f;
            if (!Enabled || QeGetQualityFactor == null || thing == null)
            {
                return false;
            }

            CompQuality comp = thing.TryGetComp<CompQuality>();
            if (comp == null)
            {
                return false;
            }

            int index = (int)comp.Quality;
            if (index >= 0 && index < QualityFactorCached.Length && QualityFactorCached[index])
            {
                factor = QualityFactorCache[index];
                return true;
            }


            try
            {
                factor = (float)QeGetQualityFactor.Invoke(null, new object[] { comp.Quality });
                if (index >= 0 && index < QualityFactorCached.Length)
                {
                    QualityFactorCache[index] = factor;
                    QualityFactorCached[index] = true;
                }
                return true;
            }
            catch
            {
                factor = 1f;
                return false;
            }
        }

        private static void AdjustHitPointsForQualityExpanded(Thing thing)
        {
            if (!TryGetQualityFactor(thing, out float factor))
            {
                return;
            }

            float statMax = thing.GetStatValue(StatDefOf.MaxHitPoints);
            if (statMax <= 0f)
            {
                return;
            }

            int targetMax = Mathf.RoundToInt(statMax * factor);
            if (targetMax <= 0)
            {
                return;
            }

            float ratio = thing.HitPoints / statMax;
            int newHp = Mathf.Clamp(Mathf.RoundToInt(ratio * targetMax), 1, targetMax);
            thing.HitPoints = newHp;
        }

        [HarmonyPatch(typeof(CompInfusion), nameof(CompInfusion.TryUpdateMaxHitpoints), new Type[0])]
        [HarmonyPostfix]
        public static void TryUpdateMaxHitpoints_Postfix(CompInfusion __instance)
        {
            AdjustHitPointsForQualityExpanded(__instance.parent);
        }

        [HarmonyPatch(typeof(CompInfusion), "TryUpdateMaxHitpoints", new[] { typeof(int) })]
        [HarmonyPostfix]
        public static void TryUpdateMaxHitpoints_WithPrevious_Postfix(CompInfusion __instance)
        {
            AdjustHitPointsForQualityExpanded(__instance.parent);
        }

        [HarmonyPatch(typeof(CompQuality), "SetQuality")]
        [HarmonyPostfix]
        [HarmonyAfter("QualityExpanded")]
        [HarmonyPriority(Priority.VeryLow)]
        public static void CompQuality_SetQuality_Postfix(CompQuality __instance)
        {
            if (__instance.parent.TryGetComp<CompInfusion>() == null)
            {
                return;
            }

            AdjustHitPointsForQualityExpanded(__instance.parent);
        }
    }
}
