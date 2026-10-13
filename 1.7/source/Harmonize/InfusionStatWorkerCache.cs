using RimWorld;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Verse;

namespace Infusion.Harmonize
{
    public static class InfusionStatWorkerCache
    {
        private static readonly Dictionary<StatDef, StatEligibility> statsEligibilityMap = new Dictionary<StatDef, StatEligibility>();
        private const int StatRequestCacheTtlTicks = 300;
        private const int StatRequestFastCacheSize = 65536;
        private const int StatRequestCacheStatsReportIntervalTicks = 60;

        private static readonly Thing[] statRequestCacheGearSlots = new Thing[StatRequestFastCacheSize];
        private static readonly StatDef[] statRequestCacheStatSlots = new StatDef[StatRequestFastCacheSize];
        private static readonly float[] statRequestCacheOffsetSlots = new float[StatRequestFastCacheSize];
        private static readonly int[] statRequestCacheTickSlots = new int[StatRequestFastCacheSize];
        private static int statRequestCacheStatsWindowStartTick = -1;
        private static int statRequestCacheStatsHitCount;
        private static int statRequestCacheStatsMissCount;
        private static readonly HashSet<Thing> gearWithInfusions = new HashSet<Thing>();
        private static readonly HashSet<Thing> gearWithNoInfusions = new HashSet<Thing>();

        public static void PopulateStatsEligibilityMap(HashSet<string> pawnStatCategories, HashSet<string> armorStats)
        {
            statsEligibilityMap.Clear();

            foreach (var stat in DefDatabase<StatDef>.AllDefs)
            {
                StatEligibility eligibility;

                if (stat.category == null)
                {
                    eligibility = StatEligibility.Ineligible;
                }
                else if (pawnStatCategories.Contains(stat.category.defName))
                {
                    eligibility = StatEligibility.PawnStat;
                }
                else if (armorStats.Contains(stat.defName))
                {
                    eligibility = StatEligibility.ArmorStat;
                }
                else
                {
                    eligibility = StatEligibility.Ineligible;
                }

                statsEligibilityMap[stat] = eligibility;
            }
        }

        public static bool TryGetStatEligibility(StatDef stat, out StatEligibility statEligibility)
        {
            return statsEligibilityMap.TryGetValue(stat, out statEligibility);
        }

        private static int GetStatRequestCacheSlot(Thing gear, StatDef stat)
        {
            unchecked
            {
                var hash = (RuntimeHelpers.GetHashCode(gear) * 397) ^ RuntimeHelpers.GetHashCode(stat);
                return hash & (StatRequestFastCacheSize - 1);
            }
        }

        public static void ResetStatRequestCache()
        {
            for (var i = 0; i < StatRequestFastCacheSize; i++)
            {
                statRequestCacheGearSlots[i] = null;
                statRequestCacheStatSlots[i] = null;
                statRequestCacheOffsetSlots[i] = 0.0f;
                statRequestCacheTickSlots[i] = -1;
            }

            gearWithInfusions.Clear();
            gearWithNoInfusions.Clear();
            statRequestCacheStatsWindowStartTick = -1;
            statRequestCacheStatsHitCount = 0;
            statRequestCacheStatsMissCount = 0;
        }

        public static void ClearGearCache()
        {
            gearWithNoInfusions.Clear();
            gearWithInfusions.Clear();
        }

        public static bool EnsureGearHasInfusions(Thing gear)
        {
            if (gearWithNoInfusions.Contains(gear))
            {
                return false;
            }

            if (gearWithInfusions.Contains(gear))
            {
                return true;
            }

            var compInf = gear.TryGetComp<CompInfusion>();
            if (compInf == null || compInf.infusionsCount == 0)
            {
                gearWithNoInfusions.Add(gear);
                return false;
            }

            gearWithInfusions.Add(gear);
            return true;
        }

        public static bool TryGetStatRequestCache(Thing gear, StatDef stat, int currentTick, out float infusionOffset)
        {
            infusionOffset = 0.0f;

            var slot = GetStatRequestCacheSlot(gear, stat);
            if (!ReferenceEquals(statRequestCacheGearSlots[slot], gear) ||
                !ReferenceEquals(statRequestCacheStatSlots[slot], stat))
            {
                return false;
            }

            var cachedAtTick = statRequestCacheTickSlots[slot];
            if (cachedAtTick >= 0 && currentTick - cachedAtTick <= StatRequestCacheTtlTicks)
            {
                infusionOffset = statRequestCacheOffsetSlots[slot];
                return true;
            }

            return false;
        }

        public static void SetStatRequestCache(Thing gear, StatDef stat, int currentTick, float infusionOffset)
        {
            var slot = GetStatRequestCacheSlot(gear, stat);
            statRequestCacheGearSlots[slot] = gear;
            statRequestCacheStatSlots[slot] = stat;
            statRequestCacheOffsetSlots[slot] = infusionOffset;
            statRequestCacheTickSlots[slot] = currentTick;
        }

        [Conditional("DEBUG")]
        public static void RecordStatRequestCacheStats(int currentTick, bool cacheHit)
        {
            if (currentTick < 0)
            {
                return;
            }

            if (statRequestCacheStatsWindowStartTick < 0 || currentTick < statRequestCacheStatsWindowStartTick)
            {
                statRequestCacheStatsWindowStartTick = currentTick;
                statRequestCacheStatsHitCount = 0;
                statRequestCacheStatsMissCount = 0;
            }

            if (cacheHit)
            {
                statRequestCacheStatsHitCount++;
            }
            else
            {
                statRequestCacheStatsMissCount++;
            }

            if (currentTick - statRequestCacheStatsWindowStartTick < StatRequestCacheStatsReportIntervalTicks)
            {
                return;
            }

            var totalRequests = statRequestCacheStatsHitCount + statRequestCacheStatsMissCount;
            if (totalRequests > 0)
            {
                var hitPercent = (statRequestCacheStatsHitCount * 100f) / totalRequests;
                var missPercent = 100f - hitPercent;
                Log.Message(
                    $"[Infusion] StatOffsetFromGear cache: hit={hitPercent:F1}% ({statRequestCacheStatsHitCount}/{totalRequests}), miss={missPercent:F1}% ({statRequestCacheStatsMissCount}/{totalRequests})");
            }

            statRequestCacheStatsWindowStartTick = currentTick;
            statRequestCacheStatsHitCount = 0;
            statRequestCacheStatsMissCount = 0;
        }
    }
}
