using System.Linq;
using RimWorld;
using Verse;

namespace Infusion.Matchers
{
    /// <summary>
    /// Matcher that filters for weapons that use Bullet projectiles.
    /// </summary>
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