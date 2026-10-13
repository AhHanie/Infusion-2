using RimWorld;

namespace Infusion
{
    [DefOf]
    public class ThingSetMakerDefOf
    {
        public static ThingSetMakerDef Infusion_MiningBonusLoot;

        static ThingSetMakerDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ThingSetMakerDefOf));
        }
    }
}
