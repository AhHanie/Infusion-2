using System.Collections.Generic;
using Verse;

namespace Infusion.PreHitWorkers
{
    public class IfBioTargetHit : PreHitWorker
    {
        public PreHitWorker value = null;

        public IfBioTargetHit()
        {
            value = null;
        }

        public override void PreBulletHit(ProjectileRecord record)
        {
            var targetPawn = VerseTools.TryCast<Pawn>(record.target);
            if (targetPawn != null && PawnUtils.IsAliveAndWell(targetPawn))
            {
                value?.PreBulletHit(record);
            }
        }

        public override IEnumerable<string> ConfigErrors()
        {
            if (value == null)
            {
                yield return "no value";
            }
        }

        public override void PreMeleeHit(VerbRecordData record)
        {
            var targetPawn = VerseTools.TryCast<Pawn>(record.target);
            if (targetPawn != null && PawnUtils.IsAliveAndWell(targetPawn))
            {
                value?.PreMeleeHit(record);
            }
        }
    }
}