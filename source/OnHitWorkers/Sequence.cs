using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Infusion.OnHitWorkers
{
    public class Sequence : OnHitWorker
    {
        public List<OnHitWorker> value = null;
        public bool onMeleeCast = true;
        public bool onMeleeImpact = true;
        public bool onRangedCast = true;
        public bool onRangedImpact = true;

        public Sequence()
        {
            value = null;
            onMeleeCast = true;
            onMeleeImpact = true;
            onRangedCast = true;
            onRangedImpact = true;
        }

        public override void AfterAttack(VerbCastedRecord record)
        {
            if (value == null) return;

            switch (record)
            {
                case VerbCastedRecordMelee _ when onMeleeCast:
                    ExecuteWorkersAfterAttack(record);
                    break;
                case VerbCastedRecordRanged _ when onRangedCast:
                    ExecuteWorkersAfterAttack(record);
                    break;
            }
        }

        public override void BulletHit(ProjectileRecord record)
        {
            if (onRangedImpact && value != null)
            {
                foreach (var worker in value)
                {
                    if (ShouldExecuteWorker(worker))
                    {
                        worker.BulletHit(record);
                    }
                }
            }
        }

        public override IEnumerable<string> ConfigErrors()
        {
            if (value == null)
            {
                yield return "no value";
            }
        }

        public override void MeleeHit(VerbRecordData record)
        {
            if (onMeleeImpact && value != null)
            {
                foreach (var worker in value)
                {
                    if (ShouldExecuteWorker(worker))
                    {
                        worker.MeleeHit(record);
                    }
                }
            }
        }

        public override bool WearerDowned(Pawn pawn, Apparel apparel)
        {
            if (value == null) return true;

            // Execute workers until one returns false (runUntilFalseFrom equivalent)
            foreach (var worker in value)
            {
                if (!worker.WearerDowned(pawn, apparel))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Executes workers for AfterAttack based on their individual chances.
        /// </summary>
        private void ExecuteWorkersAfterAttack(VerbCastedRecord record)
        {
            foreach (var worker in value)
            {
                if (ShouldExecuteWorker(worker))
                {
                    worker.AfterAttack(record);
                }
            }
        }

        /// <summary>
        /// Determines if a worker should be executed based on its chance.
        /// </summary>
        private static bool ShouldExecuteWorker(OnHitWorker worker)
        {
            return worker.chance >= 1.0f || Rand.Chance(worker.chance);
        }
    }
}