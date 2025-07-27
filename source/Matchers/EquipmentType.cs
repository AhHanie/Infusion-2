using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Infusion.Matchers
{
    /// <summary>
    /// Matcher that filters infusions based on equipment type (apparel, melee weapon, or ranged weapon).
    /// </summary>
    public class EquipmentType : Matcher<InfusionDef>
    {
        public bool apparel = false;
        public bool melee = false;
        public bool ranged = false;

        public EquipmentType()
        {
            apparel = false;
            melee = false;
            ranged = false;
        }

        public override string BuildRequirementString()
        {
            var requirements = new List<string>();

            if (apparel)
            {
                requirements.Add(ResourceBank.Strings.Matchers.Apparel);
            }

            if (melee)
            {
                requirements.Add(ResourceBank.Strings.Matchers.Melee);
            }

            if (ranged)
            {
                requirements.Add(ResourceBank.Strings.Matchers.Ranged);
            }

            return requirements.Count > 0 ? string.Join(", ", requirements) : null;
        }

        public override bool Match(ThingWithComps thing, InfusionDef def)
        {
            if (thing.def.IsApparel)
            {
                return apparel;
            }
            else if (thing.def.IsMeleeWeapon)
            {
                return melee;
            }
            else if (thing.def.IsRangedWeapon)
            {
                return ranged;
            }
            else
            {
                return false;
            }
        }
    }
}