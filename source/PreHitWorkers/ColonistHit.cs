using Infusion;
using Verse;

namespace Infusion.PreHitWorkers
{
    public class ColonistHit : PreHitWorker
    {
        public PreHitWorker value = null;

        public ColonistHit()
        {
            value = null;
        }

        public override void PreBulletHit(ProjectileRecord record)
        {
            Pawn pawn = VerseTools.TryCast<Pawn>(record.target);
            if (pawn != null && pawn.IsColonist && Rand.Chance(value.chance))
            {
                value?.PreBulletHit(record);
            }
        }

        public override void PreMeleeHit(VerbRecordData record)
        {
            Pawn pawn = VerseTools.TryCast<Pawn>(record.target);
            if (pawn != null && pawn.IsColonist && Rand.Chance(value.chance))
            {
                value?.PreMeleeHit(record);
            }
        }
    }
}
