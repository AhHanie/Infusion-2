using Verse;

namespace Infusion.OnHitWorkers
{
    public class DamageBase : OnHitWorker
    {
        public float armorPenetration = -1.0f;
        public DamageDef def = null;

        public DamageBase()
        {
            armorPenetration = -1.0f;
            def = null;
        }

        public float MeleeArmorPen(Verb verb)
        {
            if (armorPenetration < 0.0f)
            {
                return verb.verbProps.AdjustedArmorPenetration(verb, verb.CasterPawn);
            }
            else
            {
                return armorPenetration;
            }
        }

        public float RangedArmorPen(Projectile projectile)
        {
            if (armorPenetration < 0.0f)
            {
                return projectile.ArmorPenetration;
            }
            else
            {
                return armorPenetration;
            }
        }
    }
}