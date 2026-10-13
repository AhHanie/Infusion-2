using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Infusion.Harmonize
{
    [HarmonyPatch(typeof(Pawn_HealthTracker), "MakeDowned")]
    public static class PawnHealthTrackerPatches
    {
        private static readonly FieldInfo pawnField = AccessTools.Field(typeof(Pawn_HealthTracker), "pawn");

        public static void Postfix(DamageInfo? dinfo, Hediff hediff, Pawn_HealthTracker __instance)
        {
            // Only for downs from damages
            if (!dinfo.HasValue)
                return;

            var pawn = pawnField.GetValue(__instance) as Pawn;
            if (pawn?.apparel?.WornApparel == null)
                return;

            // Get all apparels with infusion components that have on-hit workers
            var apparelWorkerSets = pawn.apparel.WornApparel
                .Select(apparel => new
                {
                    Apparel = apparel,
                    CompInfusion = apparel.TryGetComp<CompInfusion>()
                })
                .Where(x => x.CompInfusion != null && x.CompInfusion.OnHits.Count > 0)
                .Select(x => new
                {
                    x.Apparel,
                    OnHits = x.CompInfusion.OnHits
                })
                .ToList();

            // Run until false for each apparel and its on-hit workers
            foreach (var apparelWorkerSet in apparelWorkerSets)
            {
                bool shouldContinue = true;
                foreach (var onHit in apparelWorkerSet.OnHits)
                {
                    if (!onHit.WearerDowned(pawn, apparelWorkerSet.Apparel))
                    {
                        shouldContinue = false;
                        break;
                    }
                }

                if (!shouldContinue)
                    break;
            }
        }
    }
}