using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using RimWorld;
using UnityEngine;
using Verse;

namespace Infusion
{
    public class Settings : ModSettings
    {
        public static StrongBox<bool> accuracyOvercap = new StrongBox<bool>(true);
        public static StrongBox<bool> biocodeBonus = new StrongBox<bool>(true);
        public static StrongBox<float> extractionChanceFactor = new StrongBox<float>(1.0f);
        public static StrongBox<bool> reusableInfusers = new StrongBox<bool>(false);
        public static StrongBox<float> chanceHandle = new StrongBox<float>(1.0f);
        public static StrongBox<float> muHandle = new StrongBox<float>(1.0f);
        public static StrongBox<float> sigmaHandle = new StrongBox<float>(1.5f);
        public static StrongBox<bool> bodyPartHandle = new StrongBox<bool>(true);
        public static StrongBox<bool> layerHandle = new StrongBox<bool>(true);
        public static StrongBox<int> slotAwful = new StrongBox<int>(0);
        public static StrongBox<int> slotPoor = new StrongBox<int>(0);
        public static StrongBox<int> slotNormal = new StrongBox<int>(1);
        public static StrongBox<int> slotGood = new StrongBox<int>(1);
        public static StrongBox<int> slotExcellent = new StrongBox<int>(2);
        public static StrongBox<int> slotMasterwork = new StrongBox<int>(2);
        public static StrongBox<int> slotLegendary = new StrongBox<int>(3); // 0 -> 20 Range
        public static StrongBox<bool> commonTierEnabled = new StrongBox<bool>(true);
        public static StrongBox<bool> uncommonTierEnabled = new StrongBox<bool>(true);
        public static StrongBox<bool> rareTierEnabled = new StrongBox<bool>(true);
        public static StrongBox<bool> legendaryTierEnabled = new StrongBox<bool>(true);

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref accuracyOvercap.Value, "accuracyOvercap", true);
            Scribe_Values.Look(ref biocodeBonus.Value, "biocodeBonus", true);
            Scribe_Values.Look(ref extractionChanceFactor.Value, "extractionChanceFactor", 1.0f);
            Scribe_Values.Look(ref reusableInfusers.Value, "reusableInfusers", false);
            Scribe_Values.Look(ref chanceHandle.Value, "chanceHandle", 1.0f);
            Scribe_Values.Look(ref muHandle.Value, "muHandle", 1.0f);
            Scribe_Values.Look(ref sigmaHandle.Value, "sigmaHandle", 1.5f);
            Scribe_Values.Look(ref bodyPartHandle.Value, "bodyPartHandle", true);
            Scribe_Values.Look(ref layerHandle.Value, "layerHandle", true);
            Scribe_Values.Look(ref slotAwful.Value, "slotAwful", 0);
            Scribe_Values.Look(ref slotPoor.Value, "slotPoor", 0);
            Scribe_Values.Look(ref slotNormal.Value, "slotNormal", 1);
            Scribe_Values.Look(ref slotGood.Value, "slotGood", 1);
            Scribe_Values.Look(ref slotExcellent.Value, "slotExcellent", 2);
            Scribe_Values.Look(ref slotMasterwork.Value, "slotMasterwork", 2);
            Scribe_Values.Look(ref slotLegendary.Value, "slotLegendary", 3);
            Scribe_Values.Look(ref commonTierEnabled.Value, "commonTierEnabled", true);
            Scribe_Values.Look(ref uncommonTierEnabled.Value, "uncommonTierEnabled", true);
            Scribe_Values.Look(ref rareTierEnabled.Value, "rareTierEnabled", true);
            Scribe_Values.Look(ref legendaryTierEnabled.Value, "legendaryTierEnabled", true);
        }

        public static int GetBaseSlotsFor(QualityCategory category)
        {
            switch (category)
            {
                case QualityCategory.Awful:
                    return slotAwful.Value;
                case QualityCategory.Poor:
                    return slotPoor.Value;
                case QualityCategory.Normal:
                    return slotNormal.Value;
                case QualityCategory.Good:
                    return slotGood.Value;
                case QualityCategory.Excellent:
                    return slotExcellent.Value;
                case QualityCategory.Masterwork:
                    return slotMasterwork.Value;
                case QualityCategory.Legendary:
                    return slotLegendary.Value;
            }
            return 0;
        }

        public static bool IsTierEnabled(TierDef tier)
        {
            switch (tier.defName)
            {
                case "Common":
                    return commonTierEnabled.Value;
                case "Uncommon":
                    return uncommonTierEnabled.Value;
                case "Rare":
                    return rareTierEnabled.Value;
                case "Legendary":
                    return legendaryTierEnabled.Value;
            }
            return false;
        }
    }
}