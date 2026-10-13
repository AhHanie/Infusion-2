using Infusion;
using Verse;

namespace Infusion.OnHitWorkers
{
    public class RemoveHediff : OnHitWorker
    {
        public HediffDef def = null;

        public bool onMeleeCast = true;

        public bool onMeleeImpact = true;

        public RemoveHediff()
        {
            def = null;
            onMeleeCast = true;
            onMeleeImpact = true;
        }

        public override void AfterAttack(VerbCastedRecord record)
        {
            if (!(record is VerbCastedRecordMelee verbCastedRecordMelee))
            {
                if (record is VerbCastedRecordRanged verbCastedRecordRanged)
                {
                    if (selfCast)
                    {
                        AddHediff(verbCastedRecordRanged.Data.baseDamage, verbCastedRecordRanged.Data.verb.CasterPawn);
                    }
                    else if (verbCastedRecordRanged.Data.target is Pawn pawn)
                    {
                        AddHediff(verbCastedRecordRanged.Data.baseDamage, pawn);
                    }
                }
            }
            else if (onMeleeCast)
            {
                if (selfCast)
                {
                    AddHediff(verbCastedRecordMelee.Data.baseDamage, verbCastedRecordMelee.Data.verb.CasterPawn);
                }
                else if (verbCastedRecordMelee.Data.target is Pawn pawn2)
                {
                    AddHediff(verbCastedRecordMelee.Data.baseDamage, pawn2);
                }
            }
        }

        public override void BulletHit(ProjectileRecord record)
        {
            if (selfCast)
            {
                if (record.projectile.Launcher is Pawn pawn)
                {
                    AddHediff(record.baseDamage, pawn);
                }
            }
            else if (record.target is Pawn pawn2)
            {
                AddHediff(record.baseDamage, pawn2);
            }
        }

        public override void MeleeHit(VerbRecordData record)
        {
            if (onMeleeImpact)
            {
                if (selfCast)
                {
                    AddHediff(record.baseDamage, record.verb.CasterPawn);
                }
                else if (record.target is Pawn pawn)
                {
                    AddHediff(record.baseDamage, pawn);
                }
            }
        }

        private void AddHediff(float baseDamage, Pawn pawn)
        {
            if (PawnUtils.IsAliveAndWell(pawn))
            {
                Hediff hediff = HediffMaker.MakeHediff(def, pawn);
                pawn.health.RemoveHediff(hediff);
            }
        }
    }
}