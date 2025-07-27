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

        /// <summary>
        /// Gets the requirement string, building it if not already set.
        /// </summary>
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

        /// <summary>
        /// Builds the requirement string for this matcher.
        /// </summary>
        /// <returns>The requirement string, or null if none</returns>
        public virtual string BuildRequirementString()
        {
            return null;
        }

        /// <summary>
        /// Checks if the given thing matches the criteria defined by this matcher.
        /// </summary>
        /// <param name="thing">The thing to check</param>
        /// <param name="def">The definition to match against</param>
        /// <returns>True if the thing matches, false otherwise</returns>
        public virtual bool Match(ThingWithComps thing, T def)
        {
            return true;
        }
    }
}