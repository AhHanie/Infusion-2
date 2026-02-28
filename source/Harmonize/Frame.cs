using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Infusion.Harmonize
{
    [HarmonyPatch(typeof(Frame), "CompleteConstruction")]
    public static class Frame_ConstructionRefund
    {
        private const string ChanceKey = "chance";
        private const string RefundPercentKey = "refundPercent";

        public static void Postfix(Frame __instance, Pawn worker)
        {
            if (worker?.apparel?.WornApparel == null)
            {
                return;
            }

            if (!TryGetBestConstructionRefundInfusion(worker, out InfusionDef infusionDef))
            {
                return;
            }

            float chance = infusionDef.GetKeyedFloatOrDefault(ChanceKey, 0f) * Settings.chanceGlobalMultiplier.Value;
            if (!Rand.Chance(chance))
            {
                return;
            }

            List<ThingDefCountClass> materials = __instance.TotalMaterialCost();

            float refundPercent = infusionDef.GetKeyedFloatOrDefault(RefundPercentKey, 0f) * Settings.amountGlobalMultiplier.Value;
            if (refundPercent <= 0f)
            {
                return;
            }

            int totalMaterialCount = materials.Sum(item => item.count);
            if (totalMaterialCount <= 0)
            {
                return;
            }

            int maxRefundCount = Math.Min(totalMaterialCount, (int)Math.Floor(totalMaterialCount * refundPercent));
            if (maxRefundCount <= 0)
            {
                return;
            }

            ThingDefCountClass pickedMaterial = materials.RandomElement();
            int maxTakeForPicked = Math.Min(maxRefundCount, pickedMaterial.count);
            if (maxTakeForPicked <= 0)
            {
                return;
            }

            int amountToRefund = Rand.RangeInclusive(1, maxTakeForPicked);
            if (!TrySpawnRefundStacks(pickedMaterial.thingDef, amountToRefund, worker.PositionHeld, worker.MapHeld))
            {
                return;
            }

            MoteMaker.ThrowText(worker.DrawPos, worker.MapHeld, "Infusion.Refunding.Message".Translate(), 4f);
        }

        private static bool TryGetBestConstructionRefundInfusion(Pawn pawn, out InfusionDef result)
        {
            result = null;
            int bestPriority = int.MinValue;

            foreach (Apparel apparel in pawn.apparel.WornApparel)
            {
                CompInfusion comp = apparel.TryGetComp<CompInfusion>();
                if (comp == null)
                {
                    continue;
                }

                InfusionDef infusionDef = comp.TryGetInfusionDefWithTag(InfusionTags.CONSTRUCTION_REFUND);
                if (infusionDef == null)
                {
                    continue;
                }

                int priority = infusionDef.tier.priority;
                if (priority > bestPriority)
                {
                    bestPriority = priority;
                    result = infusionDef;
                }
            }

            return result != null;
        }

        private static bool TrySpawnRefundStacks(ThingDef thingDef, int count, IntVec3 position, Map map)
        {
            bool spawnedAny = false;
            int remaining = count;

            while (remaining > 0)
            {
                Thing refundThing = ThingMaker.MakeThing(thingDef);
                int stackLimit = Math.Max(1, refundThing.def.stackLimit);
                int stackCount = Math.Min(remaining, stackLimit);

                refundThing.stackCount = stackCount;
                GenPlace.TryPlaceThing(refundThing, position, map, ThingPlaceMode.Near);

                remaining -= stackCount;
                spawnedAny = true;
            }

            return spawnedAny;
        }
    }
}
