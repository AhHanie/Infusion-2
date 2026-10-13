using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Infusion.Matchers
{
    public class Negate : Matcher<InfusionDef>
    {
        public Matcher<InfusionDef> value = null;

        public Negate()
        {
            value = null;
        }

        public override string BuildRequirementString()
        {
            var baseRequirement = value?.BuildRequirementString();
            if (!string.IsNullOrEmpty(baseRequirement))
            {
                return ResourceBank.Strings.Matchers.Negate(baseRequirement);
            }
            return null;
        }

        public override IEnumerable<string> ConfigErrors()
        {
            if (value == null)
            {
                yield return "no value";
            }
        }

        public override bool Match(ThingWithComps thing, InfusionDef infDef)
        {
            if (value == null)
            {
                return false;
            }
            return !value.Match(thing, infDef);
        }
    }
}