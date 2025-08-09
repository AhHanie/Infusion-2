using System;
using RimWorld;
using Verse;

namespace Infusion.PreHitWorkers
{
    public class ApplyHediff : PreHitWorker
    {
        public bool bodySizeMatters = true;

        public HediffDef def = null;

        public bool inverseStatScaling = false;

        public StatDef severityScaleBy = null;

        public bool onMeleeCast = true;

        public bool onMeleeImpact = true;

        public ApplyHediff()
        {
            bodySizeMatters = true;
            def = null;
            inverseStatScaling = false;
            severityScaleBy = null;
            onMeleeCast = true;
            onMeleeImpact = true;
        }

        public override void PreBulletHit(ProjectileRecord record)
        {
            Thing thing = Utils.SelectTarget(record, selfCast);
            if (thing != null)
            {
                AddHediff(record.baseDamage, thing as Pawn);
            }
        }

        public override void PreMeleeHit(VerbRecordData record)
        {
            if (onMeleeImpact)
            {
                Thing thing = Utils.SelectTarget(record, selfCast);
                if (thing != null)
                {
                    AddHediff(record.baseDamage, thing as Pawn);
                }
            }
        }

        private void AddHediff(float baseDamage, Pawn pawn)
        {
            if (PawnUtils.IsAliveAndWell(pawn))
            {
                float numSeconds = baseDamage * amount;
                Hediff hediff = HediffMaker.MakeHediff(def, pawn);
                HediffComp_Disappears hediffComp_Disappears = hediff.TryGetComp<HediffComp_Disappears>();
                if (hediffComp_Disappears != null)
                {
                    hediffComp_Disappears.ticksToDisappear = numSeconds.SecondsToTicks();
                }
                else
                {
                    hediff.Severity = CalculateSeverity(numSeconds, pawn);
                }
                pawn.health.AddHediff(hediff);
            }
        }

        private float CalculateSeverity(float amount, Pawn pawn)
        {
            float num = 1f;
            if (severityScaleBy != null)
            {
                float statValue = pawn.GetStatValue(severityScaleBy);
                num = ((!inverseStatScaling) ? statValue : Math.Max(0f, 1f - statValue));
            }
            float num2 = (bodySizeMatters ? pawn.BodySize : 1f);
            return amount * num / num2 / 100f;
        }
    }
}