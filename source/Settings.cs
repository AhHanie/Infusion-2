using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Verse;
using UnityEngine;

namespace Infusion
{
    public class Settings : ModSettings
    {
        public static StrongBox<float> statsGlobalMultiplier = new StrongBox<float>(1.0f);
        public static StrongBox<float> amountGlobalMultiplier = new StrongBox<float>(1.0f);
        public static StrongBox<float> chanceGlobalMultiplier = new StrongBox<float>(1.0f);
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
        public static Dictionary<TierDef, StrongBox<bool>> tiersEnabled = ResourceBank.allTierDefs.Where(item => item != null).ToDictionary(
            tier=>tier,
            tier => new StrongBox<bool>(true)
        );

        public static Dictionary<TierDef, Color> tierColorOverride = new Dictionary<TierDef, Color>();
        public static StrongBox<bool> infuseUniqueWeapons = new StrongBox<bool>(false);
        public static StrongBox<bool> infusionsFromCrafting = new StrongBox<bool>(true);
        public static Dictionary<InfusionDef, StrongBox<bool>> infusionDefsDisabledMap = new Dictionary<InfusionDef, StrongBox<bool>>();
        public static List<InfusionDef> infusionDefsDisabledList = new List<InfusionDef>();
        public static List<bool> infusionDefsDisabledList1 = new List<bool>();
        public static StrongBox<bool> disableItemInfusion = new StrongBox<bool>(false);
        public static StrongBox<bool> update1_11_1Applied = new StrongBox<bool>(false);

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
            foreach (TierDef tier in tiersEnabled.Keys) {
                Scribe_Values.Look(ref tiersEnabled[tier].Value, tier.defName.ToLower()+"TierEnabled", true);
            }

            Scribe_Values.Look(ref statsGlobalMultiplier.Value, "statsGlobalMultiplier", 1.0f);
            Scribe_Values.Look(ref chanceGlobalMultiplier.Value, "chanceGlobalMultiplier", 1.0f);
            Scribe_Values.Look(ref amountGlobalMultiplier.Value, "amountGlobalMultiplier", 1.0f);
            Scribe_Values.Look(ref infuseUniqueWeapons.Value, "infuseUniqueWeapons", false);
            Scribe_Values.Look(ref infusionsFromCrafting.Value, "infusionsFromCrafting", true);
            Scribe_Collections.Look(ref tierColorOverride, "tierColorOverride");
            Scribe_Values.Look(ref disableItemInfusion.Value, "disableItemInfusion", false);
            Scribe_Values.Look(ref update1_11_1Applied.Value, "update1_11_1Applied", false);

            if (Scribe.mode == LoadSaveMode.Saving)
            {
                infusionDefsDisabledList = infusionDefsDisabledMap.Keys.ToList();
                infusionDefsDisabledList1 = infusionDefsDisabledMap.Values.Select(value => value.Value).ToList();
            }

            Scribe_Collections.Look(ref infusionDefsDisabledList, "infusionDefsEnabledList", LookMode.Def);
            Scribe_Collections.Look(ref infusionDefsDisabledList1, "infusionDefsEnabledList1", LookMode.Value);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                infusionDefsDisabledMap.Clear();

                if (infusionDefsDisabledList != null && infusionDefsDisabledList1 != null)
                {
                    foreach (var pair in infusionDefsDisabledList.Zip(infusionDefsDisabledList1, (key, value) => (key, value)))
                    {
                        infusionDefsDisabledMap.Add(pair.key, new StrongBox<bool>(pair.value));
                    }
                }

                if (tierColorOverride == null)
                {
                    tierColorOverride = new Dictionary<TierDef, Color>();
                }

                if (tiersEnabled == null)
                {
                    tiersEnabled = ResourceBank.allTierDefs.Where(item => item != null).ToDictionary(
                        tier => tier,
                        tier => new StrongBox<bool>(true)
                    );
                }
            }
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
            if (tiersEnabled.ContainsKey(tier))
                return tiersEnabled[tier].Value;
            return false;
        }
    }
}