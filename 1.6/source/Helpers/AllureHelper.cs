using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Infusion.Comps
{
    public static class AllureHelper
    {
        private const string ChanceKey = "chance";
        private const string MaxMoneyKey = "maxMoney";
        private const int MaxStealIterations = 16;
        private const float MinValue = 0.001f;

        public static void TryTriggerAfterTrade(
            Pawn negotiator,
            ITrader trader)
        {
            if (negotiator.apparel.WornApparel == null)
            {
                return;
            }

            if (!TryGetBestAllure(negotiator, out InfusionDef allureDef))
            {
                return;
            }

            float chance = GetKeyedFloatOrDefault(allureDef, ChanceKey, 0f);
            if (!Rand.Chance(chance))
            {
                return;
            }

            float maxMoney = GetKeyedFloatOrDefault(allureDef, MaxMoneyKey, 0f);

            TryStealFromTrader(trader, negotiator, maxMoney);
        }

        private static bool TryGetBestAllure(Pawn pawn, out InfusionDef result)
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

                InfusionDef allureInf = comp.TryGetInfusionDefWithTag(InfusionTags.ALLURE);

                if (allureInf == null)
                {
                    continue;
                }

                int priority = allureInf.tier.priority;
                if (priority > bestPriority)
                {
                    bestPriority = priority;
                    result = allureInf;
                }
            }

            return result != null;
        }

        private static void TryStealFromTrader(ITrader trader, Pawn negotiator, float maxMoney)
        {
            float remaining = maxMoney;
            List<Thing> candidates = trader.Goods
                .Distinct()
                .ToList();
            List<string> stolenLabels = new List<string>();

            int iterations = 0;
            while (remaining > MinValue && candidates.Count > 0 && iterations++ < MaxStealIterations)
            {
                int index = Rand.Range(0, candidates.Count);
                Thing candidate = candidates[index];

                float totalValue = GetTotalMarketValue(candidate);
                if (totalValue <= MinValue)
                {
                    candidates.RemoveAt(index);
                    continue;
                }

                int countToTake = GetTakeCount(candidate, remaining);
                if (countToTake <= 0)
                {
                    candidates.RemoveAt(index);
                    continue;
                }

                Thing toGive = candidate;
                if (countToTake < candidate.stackCount)
                {
                    toGive = candidate.SplitOff(countToTake);
                }

                int giveCount = toGive.stackCount;
                trader.GiveSoldThingToPlayer(toGive, giveCount, negotiator);

                remaining -= GetTotalMarketValue(toGive);
                stolenLabels.Add(toGive.LabelCap);
            }

            if (stolenLabels.Count > 0)
            {
                Messages.Message(
                    "Infusion.Allure.Success".Translate(negotiator.LabelShortCap, stolenLabels.ToCommaList(useAnd: true)),
                    MessageTypeDefOf.PositiveEvent);
            }
        }

        private static int GetTakeCount(Thing thing, float remainingValue)
        {
            float totalValue = GetTotalMarketValue(thing);
            if (totalValue <= remainingValue)
            {
                return thing.stackCount;
            }

            float unitValue = GetUnitMarketValue(thing);
            if (unitValue <= MinValue)
            {
                return 0;
            }

            int maxAffordable = (int)(remainingValue / unitValue);
            if (maxAffordable <= 0)
            {
                return 0;
            }

            return maxAffordable;
        }

        private static float GetTotalMarketValue(Thing thing)
        {
            return thing.MarketValue;
        }

        private static float GetUnitMarketValue(Thing thing)
        {
            return thing.MarketValue / thing.stackCount;
        }

        private static float GetKeyedFloatOrDefault(InfusionDef def, string key, float fallback)
        {
            if (def.keyedFloats.TryGetValue(key, out float value))
            {
                return value;
            }

            return fallback;
        }
    }
}
