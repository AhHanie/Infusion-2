using HarmonyLib;
using RimWorld;
using Verse;

namespace Infusion.Harmonize
{
    public static class CompQualityPatches
    {
        [HarmonyPatch(typeof(CompQuality), "SetQuality")]
        public static class SetQuality
        {
            public static void Postfix(CompQuality __instance)
            {
                var compInfusion = __instance.parent.TryGetComp<CompInfusion>();
                if (compInfusion != null)
                {
                    compInfusion.Quality = __instance.Quality;
                    var newInfusions = compInfusion.PickInfusions(__instance.Quality);
                    compInfusion.SetInfusions(newInfusions, false);
                }
            }
        }
    }
}