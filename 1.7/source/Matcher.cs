using Verse;

namespace Infusion
{
    /// <summary>
    /// Filter for infusion applicability.
    /// </summary>
    /// <typeparam name="T">The type parameter, constrained to inherit from Def</typeparam>
    public abstract class Matcher<T> : Editable where T : Def
    {
        public string requirementString = "";

        public Matcher()
        {
            requirementString = "";
        }
        public string RequirementString
        {
            get
            {
                if (requirementString.NullOrEmpty())
                {
                    return BuildRequirementString();
                }
                return requirementString;
            }
        }
        public virtual string BuildRequirementString()
        {
            return null;
        }

        public virtual bool Match(ThingWithComps thing, T def)
        {
            return true;
        }
    }
}