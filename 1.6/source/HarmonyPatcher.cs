using System.Reflection;
using HarmonyLib;
using RimWorld;

namespace Infusion
{
    public class HarmonyPatcher
    {
        public static void PatchAllVanillaMethods(Harmony instance)
        {
            MethodInfo original = AccessTools.Method(typeof(Pawn_ApparelTracker), "Notify_ApparelAdded");
            HarmonyMethod postfix = new HarmonyMethod(typeof(Patches).GetMethod("NotifyApparelAddedPostfix"));
            instance.Patch(original, null, postfix);

            if (!Compat.ToggleableShields.Enabled)
            {
                MethodInfo original2 = AccessTools.PropertyGetter(typeof(CompShield), "EnergyGainPerTick");
                HarmonyMethod postfix2 = new HarmonyMethod(typeof(Patches).GetMethod("EnergyGainPerTickPostfix"));
                instance.Patch(original2, null, postfix2);
                MethodInfo original3 = AccessTools.PropertyGetter(typeof(CompShield), "EnergyMax");
                HarmonyMethod postfix3 = new HarmonyMethod(typeof(Patches).GetMethod("EnergyMaxGetterPostfix"));
                instance.Patch(original3, null, postfix3);
                MethodInfo original5 = AccessTools.Method(typeof(Gizmo_EnergyShieldStatus), "GizmoOnGUI");
                HarmonyMethod transpiler = new HarmonyMethod(typeof(Patches).GetMethod("GizmoOnGUITranspiler"));
                instance.Patch(original5, null, null, transpiler);
            }
            MethodInfo original4 = AccessTools.Method(typeof(Pawn_ApparelTracker), "Notify_ApparelRemoved");
            HarmonyMethod postfix4 = new HarmonyMethod(typeof(Patches).GetMethod("NotifyApparelRemovedPostfix"));
            instance.Patch(original4, null, postfix4);
           
        }
    }
}

