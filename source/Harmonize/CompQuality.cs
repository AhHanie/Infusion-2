﻿using HarmonyLib;
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
                if (compInfusion != null)
                {
                    compInfusion.Quality = __instance.Quality;
                    var newInfusions = compInfusion.PickInfusions(__instance.Quality);
                    compInfusion.SetInfusions(newInfusions, false);
                    compInfusion.TryUpdateMaxHitpoints();
                }
            }
        }
    }
}