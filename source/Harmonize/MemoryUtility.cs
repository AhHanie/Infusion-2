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
            Infuser.AllInfusers.Clear();
        }
    }

    [HarmonyPatch(typeof(Root_Entry), "Start")]
    public static class RootEntryStart
    {
        public static void Postfix()
        {
            CompInfusion.ClearCaches();
            Infuser.AllInfusers.Clear();
        }
    }

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
            Infuser.AllInfusers.Clear();
        }
    }
}
