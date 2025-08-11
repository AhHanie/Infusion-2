using RimWorld;
using Verse;

namespace Infusion.OnHitWorkers
{
    public class AddResearch: OnHitWorker
    {
        public AddResearch()
        {
        }

        public override void BulletHit(ProjectileRecord record)
        {
            AddResearchPoints(record.target, record.projectile.Launcher as Pawn);
        }

        public override void MeleeHit(VerbRecordData record)
        {
            AddResearchPoints(record.target, record.verb.CasterPawn);
        }

        private void AddResearchPoints(Thing target, Pawn caster)
        {
            ResearchProjectDef project = Find.ResearchManager.GetProject();
            if (project == null)
            {
                return;
            }
            Find.ResearchManager.ResearchPerformed(amount, caster);
            MoteMaker.ThrowText(caster.Position.ToVector3(), caster.MapHeld, "Infusion.Savant.Message".Translate());
        }
    }
}
