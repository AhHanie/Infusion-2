using RimWorld;
using Verse;

namespace Infusion.OnHitWorkers
{
    /// <summary>
    /// On-hit worker that reduces the severity of a specified hediff on the caster.
    /// [todo] Group as HediffBase
    /// </summary>
    public class ReduceSeverity : OnHitWorker
    {
        public bool bodySizeMatters = true;
        public HediffDef def = null;
        public StatDef severityScaleBy = null;
        public bool onMeleeCast = true;
        public bool onMeleeImpact = true;
        public bool onRangedCast = true;
        public bool onRangedImpact = true;

        public ReduceSeverity()
        {
            bodySizeMatters = true;
            def = null;
            severityScaleBy = null;
            onMeleeCast = true;
            onMeleeImpact = true;
            onRangedCast = true;
            onRangedImpact = true;
        }

        public override void AfterAttack(VerbCastedRecord record)
        {
            switch (record)
            {
                case VerbCastedRecordMelee meleeRecord when onMeleeCast:
                    ReduceSeverityBy(meleeRecord.Data.baseDamage, meleeRecord.Data.verb.Caster);
                    break;
                case VerbCastedRecordRanged rangedRecord when onRangedCast:
                    ReduceSeverityBy(rangedRecord.Data.baseDamage, rangedRecord.Data.verb.Caster);
                    break;
            }
        }

        public override void BulletHit(ProjectileRecord record)
        {
            if (onRangedImpact)
            {
                ReduceSeverityBy(record.baseDamage, record.projectile.Launcher);
            }
        }

        public override void MeleeHit(VerbRecordData record)
        {
            if (onMeleeImpact)
            {
                ReduceSeverityBy(record.baseDamage, record.verb.CasterPawn);
            }
        }

        private float CalculateSeverity(float amount, Pawn pawn)
        {
            float statScale = 1.0f;
            if (severityScaleBy != null)
            {
                statScale = pawn.GetStatValue(severityScaleBy);
            }

            float bodySizeScale = bodySizeMatters ? pawn.BodySize : 1.0f;

            return amount * statScale / bodySizeScale / 100.0f;
        }

        private void ReduceSeverityBy(float baseDamage, Thing caster)
        {
            var pawn = VerseTools.TryCast<Pawn>(caster);
            if (pawn != null && PawnUtils.IsAliveAndWell(pawn))
            {
                float amount = baseDamage * this.amount;
                var hediff = pawn.health.hediffSet.GetFirstHediffOfDef(this.def);
                if (hediff != null)
                {
                    hediff.Heal(CalculateSeverity(amount, pawn));
                }
            }
        }
    }
}