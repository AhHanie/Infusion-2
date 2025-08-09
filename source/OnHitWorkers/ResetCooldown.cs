using Infusion;
using Verse;

namespace Infusion.OnHitWorkers
{
    public class ResetCooldown : OnHitWorker
    {
        public bool onMeleeCast = true;

        public bool onMeleeImpact = true;

        public ResetCooldown()
        {
            onMeleeCast = true;
            onMeleeImpact = true;
        }

        public override void AfterAttack(VerbCastedRecord record)
        {
            if (!(record is VerbCastedRecordMelee verbCastedRecordMelee))
            {
                if (record is VerbCastedRecordRanged verbCastedRecordRanged)
                {
                    ResetCooldownFor(verbCastedRecordRanged.Data.verb.Caster, verbCastedRecordRanged.Data.verb, verbCastedRecordRanged.Data.target, isMelee: false);
                }
            }
            else if (onMeleeCast)
            {
                ResetCooldownFor(verbCastedRecordMelee.Data.verb.Caster, verbCastedRecordMelee.Data.verb, verbCastedRecordMelee.Data.target, isMelee: true);
            }
        }

        public override void MeleeHit(VerbRecordData record)
        {
            if (onMeleeImpact)
            {
                ResetCooldownFor(record.verb.Caster, record.verb, record.target, isMelee: true);
            }
        }

        private void ResetCooldownFor(Thing caster, Verb verb, LocalTargetInfo target, bool isMelee)
        {
            Pawn pawn = caster as Pawn;
            if (!isMelee)
            {
                if (pawn?.stances?.curStance is Stance_Cooldown stance_Cooldown)
                {
                    stance_Cooldown.ticksLeft = 1;
                }
                verb.TryStartCastOn(target);
                if (pawn?.stances?.curStance is Stance_Warmup stance_Warmup)
                {
                    stance_Warmup.ticksLeft = 1;
                }
            }
            else if (pawn?.stances?.curStance is Stance_Cooldown stance_Cooldown2)
            {
                Log.Message("Fired twice");
                stance_Cooldown2.ticksLeft = 10;
            }
        }
    }
}