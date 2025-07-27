using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace Infusion
{
    /// <summary>
    /// Utility methods for WorkGiver classes.
    /// </summary>
    public static class WorkGiverUtilities
    {
        /// <summary>
        /// Chooses components that are on the same map as the target, returning their parent Things.
        /// </summary>
        /// <typeparam name="T">The component type</typeparam>
        /// <param name="target">The target thing to compare maps with</param>
        /// <param name="comps">The collection of components to filter</param>
        /// <returns>Things that are on the same map as the target</returns>
        public static IEnumerable<Thing> ChooseSameMapOnly<T>(Thing target, IEnumerable<T> comps)
            where T : ThingComp
        {
            return comps
                .Select(comp => comp.parent)
                .Where(thing => thing.MapHeld == target.MapHeld)
                .Cast<Thing>();
        }
    }

    /// <summary>
    /// WorkGiver for applying infusers to items that want infusions.
    /// </summary>
    public class WorkGiverApplyInfuser : WorkGiver_Scanner
    {
        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            return WorkGiverUtilities.ChooseSameMapOnly(pawn, CompInfusion.WantingCandidates);
        }

        public override Job JobOnThing(Pawn pawn, Thing thing, bool forced)
        {
            var thingTarget = new LocalTargetInfo(thing);

            // Get the infusion component and check what it wants
            var compInfusion = thing.TryGetComp<CompInfusion>();
            if (compInfusion == null)
                return null;

            InfusionDef toInfuse = null;
            if (compInfusion.FirstWanting != null)
            {
                toInfuse = compInfusion.FirstWanting;
            }
            else
            {
                // No longer wanting anything, unregister
                CompInfusion.UnregisterWantingCandidates(compInfusion);
                return null;
            }

            // Find matching infuser
            if (!Infuser.AllInfusersByDef.TryGetValue(toInfuse, out var infuser))
            {
                JobFailReason.Is("Infusion.Job.FailReason.NoMatchingInfuser".Translate());
                return null;
            }

            var infuserTarget = new LocalTargetInfo(infuser);

            // Check if pawn can reserve both targets
            if (pawn.CanReserve(thingTarget, 1, -1, null, forced) &&
                pawn.CanReserve(infuserTarget, 1, 1, null, forced))
            {
                var jobDef = DefDatabase<JobDef>.GetNamed("Infusion_ApplyInfuser");
                return JobMaker.MakeJob(jobDef, thingTarget, infuserTarget);
            }

            JobFailReason.Is("Infusion.Job.FailReason.NoMatchingInfuser".Translate());
            return null;
        }
    }

    /// <summary>
    /// WorkGiver for extracting infusions from items.
    /// </summary>
    public class WorkGiverExtractInfusion : WorkGiver_Scanner
    {
        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            return WorkGiverUtilities.ChooseSameMapOnly(pawn, CompInfusion.ExtractionCandidates);
        }

        public override Job JobOnThing(Pawn pawn, Thing thing, bool forced)
        {
            var thingTarget = new LocalTargetInfo(thing);

            // Find the nearest available extractor
            var nearestExtractor = GenClosest.ClosestThing_Global_Reachable(
                pawn.Position,
                pawn.Map,
                Extractor.AllExtractors.Cast<Thing>(),
                PathEndMode.Touch,
                TraverseParms.For(pawn),
                validator: extractorThing =>
                    !extractorThing.IsForbidden(pawn) &&
                    pawn.CanReserve(new LocalTargetInfo(extractorThing), 10, 1)
            );

            if (nearestExtractor == null)
            {
                JobFailReason.Is("Infusion.Job.FailReason.NoExtractor".Translate());
                return null;
            }

            var extractorTarget = new LocalTargetInfo(nearestExtractor);

            // Check if pawn can reserve the target thing
            if (pawn.CanReserve(thingTarget, 1, -1, null, forced))
            {
                var jobDef = DefDatabase<JobDef>.GetNamed("Infusion_ExtractInfusion");
                return JobMaker.MakeJob(jobDef, thingTarget, extractorTarget);
            }

            JobFailReason.Is("Infusion.Job.FailReason.NoExtractor".Translate());
            return null;
        }
    }

    /// <summary>
    /// WorkGiver for removing infusions from items.
    /// </summary>
    public class WorkGiverRemoveInfusions : WorkGiver_Scanner
    {
        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            return WorkGiverUtilities.ChooseSameMapOnly(pawn, CompInfusion.RemovalCandidates);
        }

        public override Job JobOnThing(Pawn pawn, Thing thing, bool forced)
        {
            var thingTarget = new LocalTargetInfo(thing);

            // Check if pawn can reserve the target
            if (pawn.CanReserve(thingTarget, 1, -1, null, forced))
            {
                var jobDef = DefDatabase<JobDef>.GetNamed("Infusion_RemoveInfusions");
                return JobMaker.MakeJob(jobDef, thingTarget);
            }

            return null;
        }
    }
}