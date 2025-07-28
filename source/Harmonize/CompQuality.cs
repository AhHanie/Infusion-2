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

                    // All hit points of a pawn's apparels are determined *after* SetQuality() call,
                    // see: PawnGenerator.PostProcessGeneratedGear()
                    // We can blindly reset any Thing's HitPoints to its MaxHitPoints.
                    __instance.parent.HitPoints = __instance.parent.MaxHitPoints;
                }
            }
        }
    }
}