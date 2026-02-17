using RimWorld;
using Verse;

namespace Infusion
{
    [DefOf]
    public class ModSoundDefOf
    {
        [MayRequireAnomaly]
        public static SoundDef Pawn_Ghoul_Call;

        static ModSoundDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(SoundDef));
        }
    }
}
