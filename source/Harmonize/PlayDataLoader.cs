using HarmonyLib;

namespace Infusion.Harmonize
{
    [HarmonyPatch(typeof(Verse.PlayDataLoader), "ResetStaticDataPre")]
    public class PlayDataLoader
    {
        public static void Postfix()
        {
            InspectPaneUtility.infusionLabelCache.Clear();
        }
    }
}
