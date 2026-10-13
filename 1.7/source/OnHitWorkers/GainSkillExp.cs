using RimWorld;
using Verse;


namespace Infusion.OnHitWorkers
{
    public class GainSkillExp : OnHitWorker
    {
        public SkillDef skill;
        public override void BulletHit(ProjectileRecord record)
        {
            Thing target = Utils.SelectTarget(record, selfCast);
            if (target is Pawn pawn && pawn.skills != null)
            {
                pawn.skills.Learn(skill, amount, true, true);
            }
        }

        public override void MeleeHit(VerbRecordData record)
        {
            Thing target = Utils.SelectTarget(record, selfCast);
            if (target is Pawn pawn && pawn.skills != null)
            {
                pawn.skills.Learn(skill, amount, true, true);
            }
        }
    }
}