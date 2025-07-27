using Verse;

namespace Infusion.Matchers
{
    /// <summary>
    /// Matcher that filters out utility apparel (items that only cover the waist body part group).
    /// </summary>
    public class NotUtility : Matcher<InfusionDef>
    {
        public override string BuildRequirementString()
        {
            return ResourceBank.Strings.Matchers.NotUtility;
        }

        public override bool Match(ThingWithComps thing, InfusionDef def)
        {
            if (!thing.def.IsApparel)
            {
                return false;
            }

            if (thing.def.apparel.bodyPartGroups.Count == 0)
            {
                return false;
            }

            // If it covers only a single group, it can't be an utility slot.
            return !(thing.def.apparel.bodyPartGroups.Count == 1
                    && thing.def.apparel.bodyPartGroups[0].defName == "Waist");
        }
    }
}