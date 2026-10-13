using System.Linq;
using RimWorld;
using Verse;

namespace Infusion.Matchers
{
    public class ProjectileBullet : Matcher<InfusionDef>
    {
        public override bool Match(ThingWithComps thing, InfusionDef _)
        {
            if (!thing.def.IsRangedWeapon)
            {
                return true;
            }
            else
            {
                var firstVerb = thing.def.Verbs?.FirstOrDefault();
                var defaultProjectile = firstVerb?.defaultProjectile;
                return defaultProjectile?.thingClass == typeof(Bullet);
            }
        }
    }
}