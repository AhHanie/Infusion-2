using Infusion;
using Verse;

namespace Infusion.OnHitWorkers
{
    public class ColonistHit : OnHitWorker
    {
        public OnHitWorker value = null;

        public ColonistHit()
        {
            value = null;
        }

        public override void BulletHit(ProjectileRecord record)
        {
            Pawn pawn = VerseTools.TryCast<Pawn>(record.target);
            if (pawn != null && pawn.IsColonist && Rand.Chance(value.chance))
            {
                value?.BulletHit(record);
            }
        }

        public override void MeleeHit(VerbRecordData record)
        {
            Pawn pawn = VerseTools.TryCast<Pawn>(record.target);
            if (pawn != null && pawn.IsColonist && Rand.Chance(value.chance))
            {
                value?.MeleeHit(record);
            }
        }
    }
}
