using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace Infusion.Harmonize
{
    public class MineablePatches
    {
        [HarmonyPatch(typeof(Mineable), "TrySpawnYield", new Type[] { typeof(Map), typeof(bool), typeof(Pawn) })]
        public static class TrySpawnYield
        {
            public static void Postfix(Mineable __instance, Map map, bool moteOnWaste, Pawn pawn)
            {
                if (pawn?.apparel?.WornApparel == null)
                {
                    return;
                }

                InfusionDef infusionDef = TryGetBestProspectorInfusion(pawn);
                if (infusionDef == null)
                {
                    return;
                }

                float chance = __instance.def.building.isResourceRock
                    ? infusionDef.GetKeyedFloatOrDefault(KeyedData.MINING_LOOT_CHANCE, 0f) * Settings.chanceGlobalMultiplier.Value
                    : infusionDef.GetKeyedFloatOrDefault(KeyedData.MINING_LOOT_RARE_CHANCE, 0f) * Settings.chanceGlobalMultiplier.Value;
                if (!Rand.Chance(chance))
                {
                    return;
                }

                float baseRolls = infusionDef.GetKeyedFloatOrDefault(KeyedData.MINING_LOOT_ROLLS, 1f) * Settings.amountGlobalMultiplier.Value;
                int rolls = Math.Max(1, GenMath.RoundRandom(baseRolls));

                ThingSetMakerDef thingSetMakerDef = ThingSetMakerDefOf.Infusion_MiningBonusLoot;
                if (thingSetMakerDef?.root == null)
                {
                    return;
                }

                IntVec3 spawnPos = __instance.PositionHeld;
                if (!spawnPos.IsValid)
                {
                    return;
                }

                for (int i = 0; i < rolls; i++)
                {
                    IEnumerable<Thing> generated = thingSetMakerDef.root.Generate(default(ThingSetMakerParams));
                    if (generated == null)
                    {
                        continue;
                    }

                    foreach (Thing thing in generated)
                    {
                        if (thing == null)
                        {
                            continue;
                        }

                        GenPlace.TryPlaceThing(thing, spawnPos, map, ThingPlaceMode.Near);
                    }
                }

                MoteMaker.ThrowText(pawn.DrawPos, pawn.MapHeld, "Infusion.Prospecting.Message".Translate(), 4f);
            }
        }

        private static InfusionDef TryGetBestProspectorInfusion(Pawn pawn)
        {
            InfusionDef result = null;
            int bestPriority = int.MinValue;

            foreach (Apparel apparel in pawn.apparel.WornApparel)
            {
                CompInfusion comp = apparel.TryGetComp<CompInfusion>();
                if (comp == null)
                {
                    continue;
                }

                InfusionDef prospectorDef = comp.TryGetInfusionDefWithTag(InfusionTags.PROSPECTOR);
                if (prospectorDef == null)
                {
                    continue;
                }

                int priority = prospectorDef.tier.priority;
                if (priority > bestPriority)
                {
                    bestPriority = priority;
                    result = prospectorDef;
                }
            }

            return result;
        }
    }
}
