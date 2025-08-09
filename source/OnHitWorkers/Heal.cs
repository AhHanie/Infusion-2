using System.Collections.Generic;
using Infusion;
using Verse;

namespace Infusion.OnHitWorkers
{
    public class Heal : OnHitWorker
    {
        public bool onMeleeCast = true;

        public bool onMeleeImpact = true;

        public bool onRangedCast = true;

        public bool onRangedImpact = true;

        public bool healBasedOnDamage = true;

        public Heal()
        {
            onMeleeCast = true;
            onMeleeImpact = true;
            onRangedCast = true;
            onRangedImpact = true;
            healBasedOnDamage = true;
        }

        public override void AfterAttack(VerbCastedRecord record)
        {
            Thing thing = null;
            if (!(record is VerbCastedRecordMelee verbCastedRecordMelee))
            {
                if (record is VerbCastedRecordRanged verbCastedRecordRanged && onRangedCast)
                {
                    thing = Utils.SelectTarget(verbCastedRecordRanged, selfCast);
                    if (thing != null)
                    {
                        HealRandomInjury(verbCastedRecordRanged.Data.baseDamage, thing as Pawn);
                    }
                }
            }
            else if (onMeleeCast)
            {
                thing = Utils.SelectTarget(verbCastedRecordMelee, selfCast);
                if (thing != null)
                {
                    HealRandomInjury(verbCastedRecordMelee.Data.baseDamage, thing as Pawn);
                }
            }
        }

        public override void BulletHit(ProjectileRecord record)
        {
            if (onRangedImpact)
            {
                Thing thing = Utils.SelectTarget(record, selfCast);
                if (thing != null)
                {
                    HealRandomInjury(record.baseDamage, thing as Pawn);
                }
            }
        }

        public override void MeleeHit(VerbRecordData record)
        {
            if (onMeleeImpact)
            {
                Thing thing = Utils.SelectTarget(record, selfCast);
                if (thing != null)
                {
                    HealRandomInjury(record.baseDamage, thing as Pawn);
                }
            }
        }

        private void HealRandomInjury(float baseDamage, Thing caster)
        {
            if (!(caster is Pawn pawn) || !PawnUtils.IsAliveAndWell(pawn))
            {
                return;
            }
            float num = 0f;
            num = ((!healBasedOnDamage) ? Amount : (baseDamage * Amount));
            HediffSet hediffSet = pawn.health.hediffSet;
            if (hediffSet.HasNaturallyHealingInjury())
            {
                List<Hediff_Injury> resultHediffs = new List<Hediff_Injury>();
                hediffSet.GetHediffs(ref resultHediffs, (Hediff_Injury hediff) => hediff.CanHealNaturally());
                if (resultHediffs.Count > 0)
                {
                    Hediff_Injury hediff_Injury = resultHediffs.RandomElement();
                    hediff_Injury.Heal(num * pawn.HealthScale);
                }
            }
        }
    }
}

