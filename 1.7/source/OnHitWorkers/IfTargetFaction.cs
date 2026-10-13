using RimWorld;
using Verse;

namespace Infusion.OnHitWorkers
{
    public class IfTargetFaction : OnHitWorker
    {
        public FactionDef factionDef;
        public OnHitWorker value;

        public IfTargetFaction()
        {
            factionDef = null;
            value = null;
        }

        public override void BulletHit(ProjectileRecord record)
        {
            Pawn pawn = VerseTools.TryCast<Pawn>(record.target);
            if (pawn != null && pawn.Faction?.def == factionDef && CheckChance(value))
            {
                value?.BulletHit(record);
            }
        }

        public override void MeleeHit(VerbRecordData record)
        {
            Pawn pawn = VerseTools.TryCast<Pawn>(record.target);
            if (pawn != null && pawn.Faction?.def == factionDef && CheckChance(value))
            {
                value?.MeleeHit(record);
            }
        }
    }
}
