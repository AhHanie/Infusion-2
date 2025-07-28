using RimWorld;
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
            switch (record)
            {
                case VerbCastedRecordMelee meleeRecord when onMeleeCast:
                    ResetCooldownFor(meleeRecord.Data.verb.Caster);
                    break;
                case VerbCastedRecordRanged rangedRecord:
                    ResetCooldownFor(rangedRecord.Data.verb.Caster);
                    break;
            }
        }

        public override void MeleeHit(VerbRecordData record)
        {
            if (onMeleeImpact)
            {
                ResetCooldownFor(record.verb.Caster);
            }
        }

        private void ResetCooldownFor(Thing caster)
        {
            var pawn = caster as Pawn;
            if (pawn?.stances?.curStance is Stance_Cooldown cooldownStance)
            {
                cooldownStance.ticksLeft = 0;
            }
        }
    }
}