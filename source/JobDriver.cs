using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace Infusion
{
    public static class JobDriverUtilities
    {
        public static JobCondition IncompletableOnSetEmpty<T>(HashSet<T> set)
        {
            return set.Count == 0 ? JobCondition.Incompletable : JobCondition.Ongoing;
        }

        public static JobCondition ErrorOnNone<T>(T option) where T : class
        {
            return option == null ? JobCondition.Errored : JobCondition.Ongoing;
        }

        public static CompInfusion CompInfusionOfTarget(JobDriver jobDriver, TargetIndex target)
        {
            var thing = jobDriver.job.GetTarget(target).Thing;
            return thing?.TryGetComp<CompInfusion>();
        }

        public static Toil WaitFor(int delay, TargetIndex target)
        {
            return Toils_General.WaitWith(target, delay, true);
        }

        public static IEnumerable<Toil> CarryBToA(Job job)
        {
            yield return Toils_General.DoAtomic(() => job.count = 1);

            yield return Toils_Goto
                .GotoThing(TargetIndex.B, PathEndMode.Touch)
                .FailOnSomeonePhysicallyInteracting(TargetIndex.B);

            yield return Toils_Haul
                .StartCarryThing(TargetIndex.B)
                .FailOnDestroyedNullOrForbidden(TargetIndex.B);

            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
        }

        public static void DestroyCarriedThing(Pawn pawn)
        {
            pawn.carryTracker.CarriedThing?.Destroy();
        }

        public static JobDriver FailOnNoWantingSet(this JobDriver jobDriver, TargetIndex target)
        {
            jobDriver.AddEndCondition(() =>
            {
                var comp = CompInfusionOfTarget(jobDriver, target);
                var wantingSet = comp?.WantingSet ?? new HashSet<InfusionDef>();
                return IncompletableOnSetEmpty(wantingSet);
            });
            return jobDriver;
        }

        public static JobDriver FailOnNoMatchingInfuser(this JobDriver jobDriver, TargetIndex target)
        {
            jobDriver.AddEndCondition(() =>
            {
                var comp = CompInfusionOfTarget(jobDriver, target);
                var firstWanting = comp?.FirstWanting;
                if (firstWanting != null && Infuser.AllInfusersByDef.ContainsKey(firstWanting))
                {
                    return JobCondition.Ongoing;
                }
                return JobCondition.Errored;
            });
            return jobDriver;
        }

        public static JobDriver FailOnNoExtractionSet(this JobDriver jobDriver, TargetIndex target)
        {
            jobDriver.AddEndCondition(() =>
            {
                var comp = CompInfusionOfTarget(jobDriver, target);
                var extractionSet = comp?.ExtractionSet ?? new HashSet<InfusionDef>();
                return IncompletableOnSetEmpty(extractionSet);
            });
            return jobDriver;
        }

        public static JobDriver FailOnNoRemovalSet(this JobDriver jobDriver, TargetIndex target)
        {
            jobDriver.AddEndCondition(() =>
            {
                var comp = CompInfusionOfTarget(jobDriver, target);
                var removalSet = comp?.RemovalSet ?? new HashSet<InfusionDef>();
                return IncompletableOnSetEmpty(removalSet);
            });
            return jobDriver;
        }

        public static Toil AddFailOnDestroyedNullOrForbidden(this Toil toil, TargetIndex target)
        {
            return toil.FailOnDestroyedNullOrForbidden(target);
        }

        public static Toil AddEndOn(this Toil toil, Func<JobCondition> endCondition)
        {
            toil.AddEndCondition(endCondition);
            return toil;
        }

        public static Thing PlaceThingNear(Thing thing, IntVec3 position, Map map)
        {
            if (GenPlace.TryPlaceThing(thing, position, map, ThingPlaceMode.Near))
            {
                return thing;
            }
            return null;
        }
    }

    public class JobDriverApplyInfuser : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed) &&
                   pawn.Reserve(job.targetB, job, 1, 1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            var target = TargetThingA;
            var targetComp = target?.TryGetComp<CompInfusion>();

            // Must be an Infuser
            var infuser = TargetThingB as Infuser;

            this.FailOnDestroyedNullOrForbidden(TargetIndex.A)
                .FailOnDestroyedNullOrForbidden(TargetIndex.B)
                .FailOnNoWantingSet(TargetIndex.A);

            // Carry infuser to target
            foreach (var toil in JobDriverUtilities.CarryBToA(job))
            {
                yield return toil;
            }

            // Wait and apply infuser
            yield return JobDriverUtilities.WaitFor(300, TargetIndex.A)
                .AddFailOnDestroyedNullOrForbidden(TargetIndex.A)
                .AddEndOn(() =>
                {
                    if (targetComp == null)
                        return JobCondition.Errored;
                    return JobDriverUtilities.IncompletableOnSetEmpty(targetComp.WantingSet);
                });

            yield return Toils_General.Do(() =>
            {
                if (targetComp != null && infuser?.Content != null)
                {
                    targetComp.AddInfusion(infuser.Content);

                    // If reusable infusers are enabled, place a new empty infuser
                    if (Settings.reusableInfusers.Value)
                    {
                        var emptyInfuser = ThingMaker.MakeThing(ThingDef.Named("Infusion_InfuserEmpty"));
                        JobDriverUtilities.PlaceThingNear(emptyInfuser, pawn.Position, pawn.Map);
                    }

                    JobDriverUtilities.DestroyCarriedThing(pawn);
                }
            });
        }
    }

    public class JobDriverExtractInfusion : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed) &&
                   pawn.Reserve(job.targetB, job, 10, 1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            var target = TargetThingA;
            var targetComp = target?.TryGetComp<CompInfusion>();
            var extractor = TargetThingB;

            this.FailOnDestroyedNullOrForbidden(TargetIndex.A)
                .FailOnDestroyedNullOrForbidden(TargetIndex.B)
                .FailOnNoExtractionSet(TargetIndex.A);

            // Carry extractor to target
            foreach (var toil in JobDriverUtilities.CarryBToA(job))
            {
                yield return toil;
            }

            // Wait and extract infusion
            yield return JobDriverUtilities.WaitFor(300, TargetIndex.A)
                .AddFailOnDestroyedNullOrForbidden(TargetIndex.A)
                .AddEndOn(() =>
                {
                    if (targetComp == null)
                        return JobCondition.Errored;
                    return JobDriverUtilities.IncompletableOnSetEmpty(targetComp.ExtractionSet);
                });

            yield return Toils_General.Do(() =>
            {
                if (targetComp?.FirstExtraction != null)
                {
                    var inf = targetComp.FirstExtraction;
                    var baseExtractionChance = inf.tier.extractionChance * Settings.extractionChanceFactor.Value;
                    var successChance = targetComp.Biocoder != null
                        ? baseExtractionChance * 0.5f
                        : baseExtractionChance;

                    if (Rand.Chance(successChance))
                    {
                        // Successful extraction
                        var infuser = ThingMaker.MakeThing(ThingDef.Named("Infusion_Infuser_" + inf.tier.defName)) as Infuser;

                        targetComp.RemoveInfusion(inf);
                        infuser.SetContent(inf);

                        if (JobDriverUtilities.PlaceThingNear(infuser, pawn.Position, pawn.Map) != null)
                        {
                            JobDriverUtilities.DestroyCarriedThing(pawn);
                        }
                    }
                    else
                    {
                        // Extraction failed
                        var chance = Rand.Value;
                        string failureMessage;

                        if (chance >= 0.5f)
                        {
                            targetComp.RemoveInfusion(inf);
                            failureMessage = "Infusion.Job.Message.ExtractionFailureInfusion";
                        }
                        else if (chance >= 0.2f)
                        {
                            JobDriverUtilities.DestroyCarriedThing(pawn);
                            failureMessage = "Infusion.Job.Message.ExtractionFailureInfuser";
                        }
                        else
                        {
                            targetComp.RemoveInfusion(inf);
                            JobDriverUtilities.DestroyCarriedThing(pawn);
                            failureMessage = "Infusion.Job.Message.ExtractionFailure";
                        }

                        Messages.Message(
                            failureMessage.Translate(inf.label, target.def.label),
                            new LookTargets(extractor),
                            MessageTypeDefOf.NegativeEvent
                        );
                    }
                }
            });
        }
    }

    public class JobDriverRemoveInfusions : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            var target = TargetThingA;
            var targetComp = target?.TryGetComp<CompInfusion>();

            this.FailOnDestroyedNullOrForbidden(TargetIndex.A)
                .FailOnNoRemovalSet(TargetIndex.A);

            // Go to target
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);

            // Wait and remove infusions
            yield return JobDriverUtilities.WaitFor(1000, TargetIndex.A)
                .AddFailOnDestroyedNullOrForbidden(TargetIndex.A)
                .AddEndOn(() =>
                {
                    if (targetComp == null)
                        return JobCondition.Errored;
                    return JobDriverUtilities.IncompletableOnSetEmpty(targetComp.RemovalSet);
                });

            yield return Toils_General.Do(() =>
            {
                targetComp?.RemoveMarkedInfusions();
            });
        }
    }
}