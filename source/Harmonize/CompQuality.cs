using HarmonyLib;
using Infusion.Comps;
using RimWorld;
using System.Reflection;
using Verse;

namespace Infusion.Harmonize
{
    public static class CompQualityPatches
    {
        [HarmonyPatch(typeof(CompQuality), "SetQuality")]
        public static class SetQuality
        {
            public static bool Prepare(MethodBase original)
            {
                return !Settings.disableItemInfusion.Value;
            }

            public static void Postfix(CompQuality __instance)
            {
                var compInfusion = __instance.parent.TryGetComp<CompInfusion>();
                if (compInfusion == null || (!Settings.infusionsFromCrafting.Value && StaticFlags.IsFinalizingBillProduct))
                {
                    return;
                }

                var newInfusions = compInfusion.PickInfusions(__instance.Quality);
                compInfusion.SetInfusions(newInfusions, false);

                // We do this for comaptibility with mods modifying HP
                if (newInfusions.Count > 0)
                {
                    Current.Game.GetComponent<GameComponent_Infusion>().QueueHitPointReset(__instance.parent, 10);
                }
            }
        }
    }
}
