using RimWorld;

namespace Infusion
{
    [DefOf]
    public class TierDefOf
    {
        public static TierDef Common;

        public static TierDef Uncommon;

        public static TierDef Rare;

        public static TierDef Legendary;

        static TierDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(TierDefOf));
        }
    }
}
