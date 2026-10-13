using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace Infusion.Harmonize
{
    public enum StatEligibility
    {
        PawnStat,
        ArmorStat,
        Ineligible
    }

    public static class StatWorker
    {
        // These would need to be defined based on your mod's requirements
        private static readonly HashSet<string> pawnStatCategories = new HashSet<string>
        {
            "PawnCombat",
            "PawnSocial",
            "PawnMisc",
            "PawnWork",
            "PawnFood",
            "PawnResistances",
            "BasicsPawn",
            "PawnPsyfocus",
            "Mechanitor"
        };

        private static readonly HashSet<string> armorStats = new HashSet<string>
        {
            "ArmorRating_Blunt",
            "ArmorRating_Sharp",
            "ArmorRating_Heat"
        };

        private static IEnumerable<Thing> ApparelsAndEquipments(Pawn pawn)
        {
            var things = new List<Thing>();

            // Add apparels
            if (pawn.apparel?.WornApparel != null)
            {
                things.AddRange(pawn.apparel.WornApparel.Cast<Thing>());
            }

            // Add equipment
            if (pawn.equipment?.Primary != null)
            {
                things.Add(pawn.equipment.Primary);
            }

            if (pawn.equipment?.AllEquipmentListForReading != null)
            {
                things.AddRange(pawn.equipment.AllEquipmentListForReading.Cast<Thing>());
            }

            return things;
        }

        public static void PopulateStatsEligibilityMap()
        {
            InfusionStatWorkerCache.PopulateStatsEligibilityMap(pawnStatCategories, armorStats);
        }

        public static void ResetStatRequestCache()
        {
            InfusionStatWorkerCache.ResetStatRequestCache();
        }

        [HarmonyPatch(typeof(RimWorld.StatWorker), "RelevantGear")]
        public static class RelevantGear
        {
            public static IEnumerable<Thing> Postfix(IEnumerable<Thing> __result, Pawn pawn, StatDef stat)
            {
                // Just skip animals
                if (!pawn.def.race.Humanlike)
                {
                    return __result;
                }

                var isPawnStat = pawnStatCategories.Contains(stat.category?.defName);
                var isArmorStat = armorStats.Contains(stat.defName);

                if (!isPawnStat && !isArmorStat)
                {
                    return __result;
                }

                var thingMap = new Dictionary<string, Thing>();

                // Add existing results
                foreach (var thing in __result)
                {
                    thingMap[thing.ThingID] = thing;
                }

                IEnumerable<Thing> thingsToConsider;
                if (isPawnStat)
                {
                    thingsToConsider = ApparelsAndEquipments(pawn);
                }
                else
                {
                    // armor stats = only weapons, no need to calculate
                    var equipments = new List<Thing>();
                    if (pawn.equipment?.Primary != null)
                    {
                        equipments.Add(pawn.equipment.Primary);
                    }
                    if (pawn.equipment?.AllEquipmentListForReading != null)
                    {
                        equipments.AddRange(pawn.equipment.AllEquipmentListForReading.Cast<Thing>());
                    }
                    thingsToConsider = equipments;
                }

                foreach (var thing in thingsToConsider)
                {
                    var comp = thing.TryGetComp<CompInfusion>();
                    if (comp?.HasInfusionForStat(stat) == true)
                    {
                        thingMap[thing.ThingID] = thing;
                    }
                }

                return thingMap.Values;
            }
        }

        [HarmonyPatch(typeof(RimWorld.StatWorker), "StatOffsetFromGear")]
        public static class StatOffsetFromGear
        {
            public static float Postfix(float __result, Thing gear, StatDef stat)
            {
                if (!InfusionStatWorkerCache.TryGetStatEligibility(stat, out var statType) ||
                    statType == StatEligibility.Ineligible)
                {
                    return __result;
                }

                if (!InfusionStatWorkerCache.EnsureGearHasInfusions(gear))
                {
                    return __result;
                }

                var currentTick = Comps.GameComponent_Infusion.currentGameTick;
                if (InfusionStatWorkerCache.TryGetStatRequestCache(gear, stat, currentTick, out var cachedInfusionOffset))
                {
                    InfusionStatWorkerCache.RecordStatRequestCacheStats(currentTick, true);
                    return __result + cachedInfusionOffset;
                }

                float infusionOffset = 0.0f;
                if (statType == StatEligibility.PawnStat ||
                     (statType == StatEligibility.ArmorStat && gear.def.IsWeapon))
                {
                    // for general stats, considers both weapons and apparels into account
                    // for armor stats, only checks weapons
                    CompInfusion compInf = gear.TryGetComp<CompInfusion>();
                    infusionOffset = compInf.GetModForStat(stat).offset;
                }

                InfusionStatWorkerCache.SetStatRequestCache(gear, stat, currentTick, infusionOffset);

                InfusionStatWorkerCache.RecordStatRequestCacheStats(currentTick, false);
                return __result + infusionOffset;
            }
        }

        [HarmonyPatch(typeof(RimWorld.StatWorker), "InfoTextLineFromGear")]
        public static class InfoTextLineFromGear
        {
            public static bool Prefix(Thing gear, StatDef stat, ref string __result)
            {
                var baseMod = new StatMod(
                    gear.def.equippedStatOffsets.GetStatOffsetFromList(stat),
                    0.0f);

                var fromInfusion = gear.TryGetComp<CompInfusion>()?.GetModForStat(stat) ?? StatMod.Empty;

                __result = "    " + gear.LabelCap + ": " + (baseMod + fromInfusion).StringForStat(stat);

                return false;
            }
        }

        [HarmonyPatch(typeof(RimWorld.StatWorker), "GetExplanationUnfinalized")]
        public static class GetExplanationUnfinalized
        {
            public static bool GearOrInfusionAffectsStat(Thing gear, StatDef stat)
            {
                if (gear.def.equippedStatOffsets.GetStatOffsetFromList(stat) > 0.0f)
                {
                    return true;
                }

                var comp = gear.TryGetComp<CompInfusion>();
                return comp?.HasInfusionForStat(stat) ?? false;
            }

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var instructionsList = instructions.ToList();

                var statField = AccessTools.Field(typeof(Thing), "def");
                var gearAffectsStatMethod = AccessTools.Method(typeof(RimWorld.StatWorker), "GearAffectsStat");
                var gearOrInfusionAffectsStatMethod = AccessTools.Method(typeof(GetExplanationUnfinalized), nameof(GearOrInfusionAffectsStat));

                for (int i = 0; i < instructionsList.Count; i++)
                {
                    var instruction = instructionsList[i];

                    if (instruction.opcode == OpCodes.Ldfld &&
                        instruction.operand is FieldInfo field &&
                        field == statField)
                    {
                        instruction.opcode = OpCodes.Nop;
                    }
                    else if (instruction.opcode == OpCodes.Call &&
                             instruction.operand is MethodInfo method &&
                             method == gearAffectsStatMethod)
                    {
                        instruction.operand = gearOrInfusionAffectsStatMethod;
                    }
                }

                return instructionsList;
            }
        }
    }
}
