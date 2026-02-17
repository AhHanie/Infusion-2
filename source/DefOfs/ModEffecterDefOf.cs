using RimWorld;
using Verse;

namespace Infusion
{
    [DefOf]
    public class ModEffecterDefOf
    {
        [MayRequireAnomaly]
        public static EffecterDef DeadlifeReleasing;

        static ModEffecterDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(EffecterDef));
        }
    }
}
