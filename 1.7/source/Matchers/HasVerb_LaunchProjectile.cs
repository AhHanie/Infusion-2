using System.Linq;
using Verse;

namespace Infusion.Matchers
{
    public class HasVerb_LaunchProjectile : Matcher<InfusionDef>
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
                return firstVerb?.verbClass?.IsSubclassOf(typeof(Verb_LaunchProjectile)) == true;
            }
        }
    }
}