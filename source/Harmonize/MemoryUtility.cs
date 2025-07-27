using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;
using Verse.Profile;

namespace Infusion.Harmonize
{
    [HarmonyPatch(typeof(MemoryUtility), "ClearAllMapsAndWorld")]
    public static class MemoryUtilityPatches
    {
        public static void Postfix()
        {
            CompInfusion.ClearCaches();
        }
    }

    // Optional: Also clear when returning to main menu
    [HarmonyPatch(typeof(Root_Entry), "Start")]
    public static class RootEntryStart
    {
        public static void Postfix()
        {
            CompInfusion.ClearCaches();
        }
    }

    // Covers loading saved games - targets all LoadGame methods
    [HarmonyPatch]
    public static class GameDataSaveLoaderLoadGame
    {
        public static IEnumerable<MethodBase> TargetMethods()
        {
            return AccessTools.GetDeclaredMethods(typeof(GameDataSaveLoader))
                .Where(method => method.Name == "LoadGame");
        }

        public static void Prefix()
        {
            CompInfusion.ClearCaches();
        }
    }
}
