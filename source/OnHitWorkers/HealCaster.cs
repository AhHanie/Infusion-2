using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Infusion.OnHitWorkers
{
    public class HealCaster : OnHitWorker
    {
        public bool onMeleeCast = true;
        public bool onMeleeImpact = true;
        public bool onRangedCast = true;
        public bool onRangedImpact = true;

        public HealCaster()
        {
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
                    HealRandomInjury(meleeRecord.Data.baseDamage, meleeRecord.Data.verb.Caster);
                    break;
                case VerbCastedRecordRanged rangedRecord when onRangedCast:
                    HealRandomInjury(rangedRecord.Data.baseDamage, rangedRecord.Data.verb.Caster);
                    break;
            }
        }

        public override void BulletHit(ProjectileRecord record)
        {
            if (onRangedImpact)
            {
                HealRandomInjury(record.baseDamage, record.projectile.Launcher);
            }
        }

        public override void MeleeHit(VerbRecordData record)
        {
            if (onMeleeImpact)
            {
                HealRandomInjury(record.baseDamage, record.verb.CasterPawn);
            }
        }

        private void HealRandomInjury(float baseDamage, Thing caster)
        {
            var pawn = caster as Pawn;
            if (pawn != null && PawnUtils.IsAliveAndWell(pawn))
            {
                var amount = baseDamage * this.amount;
                var hediffSet = pawn.health.hediffSet;

                if (hediffSet.HasNaturallyHealingInjury())
                {
                    var injuries = new List<Hediff_Injury>();
                    hediffSet.GetHediffs<Hediff_Injury>(ref injuries, hediff => hediff.CanHealNaturally());

                    if (injuries.Count > 0)
                    {
                        var randomInjury = injuries.RandomElement();
                        randomInjury.Heal(amount * pawn.HealthScale);
                    }
                }
            }
        }
    }
}