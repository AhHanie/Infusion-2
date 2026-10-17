using Verse;
using RimWorld;

namespace Infusion.OnHitWorkers
{
    public class TameTarget : OnHitWorker
    {
        // Manhunting animals (raids, ambushes, Scaria packs) must not be tamed: SetFaction wipes the mental
        // state, and Scaria then forces the new colony animal straight back into BerserkPermanent.
        private static bool CanTame(Pawn pawn)
        {
            return pawn.AnimalOrWildMan()
                && pawn.Faction != Faction.OfPlayer
                && !pawn.InAggroMentalState
                && !pawn.health.hediffSet.HasHediff(HediffDefOf.Scaria);
        }

        public override void BulletHit(ProjectileRecord record)
        {
            if (record.target != null)
            {
                Thing thing = Utils.SelectTarget(record, selfCast);
                if (thing is Pawn pawn && CanTame(pawn))
                {
                    InteractionWorker_RecruitAttempt.DoRecruit(record.projectile.Launcher as Pawn, pawn);
                    DebugActionsUtility.DustPuffFrom(pawn);
                }
            }
        }

        public override void MeleeHit(VerbRecordData record)
        {
            if (record.target != null)
            {
                Thing thing = Utils.SelectTarget(record, selfCast);
                if (thing is Pawn pawn && CanTame(pawn))
                {
                    InteractionWorker_RecruitAttempt.DoRecruit(record.verb.CasterPawn, pawn);
                    DebugActionsUtility.DustPuffFrom(pawn);
                }
            }
        }
    }
}
