using RimWorld;
using Verse;

namespace Infusion.OnHitWorkers
{
    public class AddResearch: OnHitWorker
    {
        public bool anomaly;
        public KnowledgeCategoryDef knowledgeCategory;
        public AddResearch()
        {
            anomaly = false;
            knowledgeCategory = null;
        }

        public override void BulletHit(ProjectileRecord record)
        {
            AddResearchPoints(record.projectile.Launcher as Pawn);
        }

        public override void MeleeHit(VerbRecordData record)
        {
            AddResearchPoints(record.verb.CasterPawn);
        }

        private void AddResearchPoints(Pawn caster)
        {
            if (anomaly)
            {
                MoteMaker.ThrowText(caster.DrawPos, caster.MapHeld, $"{knowledgeCategory.LabelCap} +{amount:0.00}", 3f);
                Find.ResearchManager.ApplyKnowledge(knowledgeCategory, amount);
                return;
            }
            ResearchProjectDef project = Find.ResearchManager.GetProject();
            if (project == null)
            {
                return;
            }
            Find.ResearchManager.AddProgress(project, amount, caster);
            MoteMaker.ThrowText(caster.DrawPos, caster.MapHeld, "Infusion.Savant.Message".Translate(amount));
        }
    }
}
