using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Infusion.Matchers
{
    public class WeaponClasses : Matcher<InfusionDef>
    {
        public List<WeaponClassDef> defs = new List<WeaponClassDef>();

        public WeaponClasses()
        {
            defs = new List<WeaponClassDef>();
        }

        public override string BuildRequirementString()
        {
            if (defs?.Count > 0)
            {
                var labels = defs.Select(def => def.label);
                return string.Join(", ", labels);
            }
            return null;
        }

        public override bool Match(ThingWithComps thing, InfusionDef def)
        {
            if (thing.def.weaponClasses == null || defs == null || defs.Count == 0)
            {
                return false;
            }

            return defs.Any(weaponClassDef => thing.def.weaponClasses.Contains(weaponClassDef));
        }
    }
}