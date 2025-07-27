using System.Collections.Generic;
using Verse;

namespace Infusion.OnHitWorkers
{
    /// <summary>
    /// On-hit worker that only executes its wrapped worker if the target is a living Pawn (biological target).
    /// </summary>
    public class IfBioTargetHit : OnHitWorker
    {
        public OnHitWorker value = null;

        public IfBioTargetHit()
        {
            value = null;
        }

        public override void BulletHit(ProjectileRecord record)
        {
            var targetPawn = VerseTools.TryCast<Pawn>(record.target);
            if (targetPawn != null && PawnUtils.IsAliveAndWell(targetPawn))
            {
                value?.BulletHit(record);
            }
        }

        public override IEnumerable<string> ConfigErrors()
        {
            if (value == null)
            {
                yield return "no value";
            }
        }

        public override void MeleeHit(VerbRecordData record)
        {
            var targetPawn = VerseTools.TryCast<Pawn>(record.target);
            if (targetPawn != null && PawnUtils.IsAliveAndWell(targetPawn))
            {
                value?.MeleeHit(record);
            }
        }
    }
}