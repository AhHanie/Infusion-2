using RimWorld;
using Verse;

namespace Infusion.Matchers
{
    /// <summary>
    /// Matcher that filters for shield belts (items with energy shield capability).
    /// </summary>
    public class ShieldBelt : Matcher<InfusionDef>
    {
        public override string BuildRequirementString()
        {
            return ResourceBank.Strings.Matchers.ShieldBelt;
        }

        public override bool Match(ThingWithComps thing, InfusionDef def)
        {
            return thing.def.statBases.GetStatValueFromList(StatDefOf.EnergyShieldEnergyMax, 0.0f) > 0.0f;
        }
    }
}