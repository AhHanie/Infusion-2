using RimWorld;
using Verse;

namespace Infusion.OnHitWorkers
{
    /// <summary>
    /// On-hit worker that spawns blood filth when hitting living targets.
    /// </summary>
    public class SpawnBlood : OnHitWorker
    {
        public override void BulletHit(ProjectileRecord record)
        {
            var targetPawn = VerseTools.TryCast<Pawn>(record.target);
            if (targetPawn != null && PawnUtils.IsAliveAndWell(targetPawn))
            {
                FilthMaker.TryMakeFilth(
                    targetPawn.Position,
                    targetPawn.Map,
                    targetPawn.RaceProps.BloodDef,
                    targetPawn.LabelIndefinite()
                );
            }
        }

        public override void MeleeHit(VerbRecordData record)
        {
            var targetPawn = VerseTools.TryCast<Pawn>(record.target);
            if (targetPawn != null && PawnUtils.IsAliveAndWell(targetPawn))
            {
                FilthMaker.TryMakeFilth(
                    targetPawn.Position,
                    targetPawn.Map,
                    targetPawn.RaceProps.BloodDef,
                    targetPawn.LabelIndefinite()
                );
            }
        }
    }
}