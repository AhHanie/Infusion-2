using HarmonyLib;
using RimWorld;
using Verse;

namespace Infusion.Harmonize
{
    public static class GizmoEnergyShieldStatusPatches
    {
        // The gizmo reads the shield's max energy straight from the stat, bypassing CompShield.EnergyMax
        // (and with it the Aegis override). Wrap each of those stat reads instead of rewriting the IL.
        [HarmonyPatch(typeof(Gizmo_EnergyShieldStatus), nameof(Gizmo_EnergyShieldStatus.GizmoOnGUI))]
        public static class GizmoOnGUI
        {
            public static bool Prepare()
            {
                return !Compat.ToggleableShields.Enabled;
            }

            [HarmonyInfix(typeof(StatExtension), nameof(StatExtension.GetStatValue),
                typeof(Thing), typeof(StatDef), typeof(bool), typeof(int))]
            public static float Postfix(float __result, Thing thing, StatDef stat)
            {
                if (stat != StatDefOf.EnergyShieldEnergyMax)
                {
                    return __result;
                }

                return CompShieldPatches.EnergyMax.Apply(thing, __result);
            }
        }
    }
}
