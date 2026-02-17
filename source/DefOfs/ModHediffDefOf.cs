using RimWorld;
using Verse;

namespace Infusion
{
    [DefOf]
    public class ModHediffDefOf
    {
        [MayRequireAnomaly]
        public static HediffDef Ghoul;

        static ModHediffDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(HediffDef));
        }
    }
}
