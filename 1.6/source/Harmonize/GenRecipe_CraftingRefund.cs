using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace Infusion.Harmonize
{
    [HarmonyPatch(typeof(GenRecipe), "MakeRecipeProducts")]
    public static class GenRecipe_CraftingRefund
    {
        private const string ChanceKey = "chance";
        private const string RefundPercentKey = "refundPercent";

        public static void Postfix(RecipeDef recipeDef, Pawn worker, IEnumerable<Thing> ingredients)
        {
            if (recipeDef.workSkill != SkillDefOf.Crafting)
            {
                return;
            }

            if (worker?.apparel?.WornApparel == null)
            {
                return;
            }

            if (!TryGetBestCraftingRefundInfusion(worker, out InfusionDef infusionDef))
            {
                return;
            }

            float chance = infusionDef.GetKeyedFloatOrDefault(ChanceKey, 0f) * Settings.chanceGlobalMultiplier.Value;
            if (!Rand.Chance(chance))
            {
                return;
            }

            List<ThingDefCountClass> consumedMaterials = AggregateConsumedIngredients(ingredients);
            float refundPercent = infusionDef.GetKeyedFloatOrDefault(RefundPercentKey, 0f) * Settings.amountGlobalMultiplier.Value;
            int totalMaterialCount = consumedMaterials.Sum(item => item.count);

            int maxRefundCount = Math.Min(totalMaterialCount, (int)Math.Floor(totalMaterialCount * refundPercent));
            if (maxRefundCount <= 0)
            {
                return;
            }

            ThingDefCountClass pickedMaterial = consumedMaterials.RandomElement();
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

        private static bool TryGetBestCraftingRefundInfusion(Pawn pawn, out InfusionDef result)
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

                InfusionDef infusionDef = comp.TryGetInfusionDefWithTag(InfusionTags.CRAFTING_REFUND);
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

        private static List<ThingDefCountClass> AggregateConsumedIngredients(IEnumerable<Thing> ingredients)
        {
            Dictionary<ThingDef, int> merged = new Dictionary<ThingDef, int>();

            foreach (Thing ingredient in ingredients)
            {
                if (merged.TryGetValue(ingredient.def, out int existing))
                {
                    merged[ingredient.def] = existing + ingredient.stackCount;
                }
                else
                {
                    merged[ingredient.def] = ingredient.stackCount;
                }
            }

            return merged
                .Select(pair => new ThingDefCountClass(pair.Key, pair.Value))
                .ToList();
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
