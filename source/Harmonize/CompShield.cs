using HarmonyLib;
using RimWorld;
using Verse;

namespace Infusion.Harmonize
{
    public static class CompShieldPatches
    {
        [HarmonyPatch(typeof(CompShield), "EnergyGainPerTick", MethodType.Getter)]
        public static class EnergyGainPerTick
        {
            public static void Postfix(CompShield __instance, ref float __result)
            {
                CompInfusion compInfusion = __instance.parent.TryGetComp<CompInfusion>();
                if (compInfusion != null)
                {
                    InfusionDef infusionDef = compInfusion.TryGetInfusionDefWithTag(InfusionTags.AEGIS);
                    if (infusionDef != null)
                    {
                        __result = infusionDef.keyedFloats[KeyedDataHelper.ConvertToString(KeyedData.ENERGY_SHIELD_RECHARGE_RATE)];
                    }
                }
            }
        }

        [HarmonyPatch(typeof(CompShield), "EnergyMax", MethodType.Getter)]
        public static class EnergyMax
        {
            public static void Postfix(CompShield __instance, ref float __result)
            {
                CompInfusion compInfusion = __instance.parent.TryGetComp<CompInfusion>();
                if (compInfusion != null)
                {
                    InfusionDef infusionDef = compInfusion.TryGetInfusionDefWithTag(InfusionTags.AEGIS);
                    if (infusionDef != null)
                    {
                        __result = infusionDef.keyedFloats[KeyedDataHelper.ConvertToString(KeyedData.ENERGY_SHIELD_MAX_ENERGY)];
                    }
                }
            }
        }
    }
}