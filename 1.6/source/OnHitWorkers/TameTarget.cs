using Verse;
using RimWorld;

namespace Infusion.OnHitWorkers
{
    public class TameTarget : OnHitWorker
    {
        public override void BulletHit(ProjectileRecord record)
        {
            if (record.target != null)
            {
                Thing thing = Utils.SelectTarget(record, selfCast);
                if (thing is Pawn pawn && pawn.AnimalOrWildMan() && pawn.Faction != Faction.OfPlayer)
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
                if (thing is Pawn pawn && pawn.AnimalOrWildMan() && pawn.Faction != Faction.OfPlayer)
                {
                    InteractionWorker_RecruitAttempt.DoRecruit(record.verb.CasterPawn, pawn);
                    DebugActionsUtility.DustPuffFrom(pawn);
                }
            }
        }
    }
}
