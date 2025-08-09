using Infusion;
using Verse;

namespace Infusion
{
    public abstract class PreHitWorker : Editable
    {
        public float chance = 1f;

        public float amount = 1f;

        public bool selfCast = false;

        public virtual float Chance => chance;

        public PreHitWorker()
        {
            amount = 0f;
            chance = 1f;
            selfCast = false;
        }

        public virtual void PreBulletHit(ProjectileRecord record)
        {
        }

        public virtual void PreMeleeHit(VerbRecordData record)
        {
        }

        public static bool CheckChance(PreHitWorker worker)
        {
            return Rand.Chance(worker.Chance);
        }
    }

}