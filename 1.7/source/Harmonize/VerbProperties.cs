using HarmonyLib;
using Verse;

namespace Infusion.Harmonize
{
    public static class VerbPropertiesPatches
    {
        // GetHitChanceFactor ends with Mathf.Clamp(value, 0.01f, 1f); its only 1f literal is that upper bound.
        // Raising it to 2f lets accuracy overcap past 100%.
        [HarmonyPatch(typeof(VerbProperties), nameof(VerbProperties.GetHitChanceFactor))]
        public static class GetHitChanceFactor
        {
            [HarmonyInfix(1f)]
            public static float Postfix(float __result)
            {
                return 2.0f;
            }
        }
    }
}
