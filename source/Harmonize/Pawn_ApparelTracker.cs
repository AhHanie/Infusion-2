using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using Infusion.Comps;

namespace Infusion.Harmonize
{
    
    public static class Pawn_ApparelTrackerPatches
    {
        [HarmonyPatch(typeof(Pawn_ApparelTracker), "Notify_ApparelAdded")]
        public static class Notify_ApparelAdded
        {
            public static void Postfix(Apparel apparel)
            {
                CompInfusion compInfusion = apparel.TryGetComp<CompInfusion>();
                if (compInfusion != null && compInfusion.ContainsTag(InfusionTags.AEGIS))
                {
                    List<ThingComp> list = (List<ThingComp>)Constants.thingWithCompsCompsField.GetValue(apparel);
                    if (!list.Any((ThingComp x) => x is CompShield))
                    {
                        CompProperties_Shield compProperties_Shield = new CompProperties_Shield();
                        ThingComp thingComp = (ThingComp)Activator.CreateInstance(compProperties_Shield.compClass);
                        thingComp.parent = apparel;
                        list.Add(thingComp);
                        Dictionary<Type, ThingComp[]> dictionary = (Dictionary<Type, ThingComp[]>)Constants.thingWithCompsCompsByTypeField.GetValue(apparel);
                        List<ThingComp> list2 = new List<ThingComp> { thingComp };
                        dictionary.Add(compProperties_Shield.compClass, list2.ToArray());
                        thingComp.Initialize(compProperties_Shield);
                        Map currentMap = apparel.MapHeld;
                        if (currentMap == null)
                        {
                            return;
                        }
                        InfusionMapComp component = currentMap.GetComponent<InfusionMapComp>();
                        component.AddThingToTick(apparel);
                        GameComponent_Infusion gameComp = Current.Game.GetComponent<GameComponent_Infusion>();
                        gameComp.AddAegisItem(apparel);
                    }
                }
            }

            [HarmonyPatch(typeof(Pawn_ApparelTracker), "Notify_ApparelRemoved")]
            public static class Notify_ApparelRemoved
            {
                public static void Postfix(Apparel apparel)
                {
                    CompInfusion compInfusion = apparel.TryGetComp<CompInfusion>();
                    if (compInfusion != null && compInfusion.ContainsTag(InfusionTags.AEGIS))
                    {
                        List<ThingComp> list = (List<ThingComp>)Constants.thingWithCompsCompsField.GetValue(apparel);
                        list.RemoveWhere((ThingComp thingComp) => thingComp is CompShield);
                        Dictionary<Type, ThingComp[]> dictionary = (Dictionary<Type, ThingComp[]>)Constants.thingWithCompsCompsByTypeField.GetValue(apparel);
                        dictionary.Remove(typeof(CompShield));
                        InfusionMapComp component = Find.CurrentMap.GetComponent<InfusionMapComp>();
                        component.RemoveThingToTick(apparel);
                        GameComponent_Infusion gameComp = Current.Game.GetComponent<GameComponent_Infusion>();
                        gameComp.RemoveAegisItem(apparel);
                    }
                }
            }
        }
        
    }
}
