using HarmonyLib;
using System.Reflection;
using Verse;

namespace Infusion.Harmonize
{
    public static class GenRecipePatches
    {
        [HarmonyPatch(typeof(GenRecipe), "PostProcessProduct")]
        public static class PostProcessProduct
        {
            public static bool Prepare(MethodBase original)
            {
                return !Settings.infusionsFromCrafting.Value;
            }

            public static void Prefix()
            {
                StaticFlags.IsFinalizingBillProduct = true;
            }

            public static void Postfix()
            {
                StaticFlags.IsFinalizingBillProduct = false;
            }
        }
    }
}
