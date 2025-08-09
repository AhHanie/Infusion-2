using System.Collections.Generic;
using Verse;

namespace Infusion.PreHitWorkers
{
    public class Sequence : PreHitWorker
    {
        public List<PreHitWorker> value = null;
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

        public override void PreBulletHit(ProjectileRecord record)
        {
            if (onRangedImpact && value != null)
            {
                foreach (var worker in value)
                {
                    if (ShouldExecuteWorker(worker))
                    {
                        worker.PreBulletHit(record);
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

        public override void PreMeleeHit(VerbRecordData record)
        {
            if (onMeleeImpact && value != null)
            {
                foreach (var worker in value)
                {
                    if (ShouldExecuteWorker(worker))
                    {
                        worker.PreMeleeHit(record);
                    }
                }
            }
        }

        /// <summary>
        /// Determines if a worker should be executed based on its chance.
        /// </summary>
        private static bool ShouldExecuteWorker(PreHitWorker worker)
        {
            return Rand.Chance(worker.Chance);
        }
    }
}