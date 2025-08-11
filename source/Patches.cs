using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Infusion;
using RimWorld;
using Verse;

namespace Infusion
{
    public class Patches
    {
        public static void NotifyApparelAddedPostfix(Apparel apparel)
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
                }
            }
        }

        public static void NotifyApparelRemovedPostfix(Apparel apparel)
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
            }
        }

        public static void EnergyGainPerTickPostfix(CompShield __instance, ref float __result)
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

        public static void EnergyMaxGetterPostfix(CompShield __instance, ref float __result)
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

        public static IEnumerable<CodeInstruction> GizmoOnGUITranspiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionsList = instructions.ToList();
            FieldInfo shieldField = AccessTools.Field(typeof(Gizmo_EnergyShieldStatus), "shield");
            FieldInfo parentField = AccessTools.Field(typeof(ThingComp), "parent");
            FieldInfo energyShieldEnergyMaxField = AccessTools.Field(typeof(StatDefOf), "EnergyShieldEnergyMax");
            MethodInfo getStatValueMethod = AccessTools.Method(typeof(StatExtension), "GetStatValue", new Type[4]
            {
            typeof(Thing),
            typeof(StatDef),
            typeof(bool),
            typeof(int)
            });
            MethodInfo energyMaxProperty = AccessTools.PropertyGetter(typeof(CompShield), "EnergyMax");
            int patchCount = 0;
            for (int i = 0; i < instructionsList.Count; i++)
            {
                if (i + 6 < instructionsList.Count && instructionsList[i].opcode == OpCodes.Ldarg_0 && instructionsList[i + 1].opcode == OpCodes.Ldfld && instructionsList[i + 1].operand.Equals(shieldField) && instructionsList[i + 2].opcode == OpCodes.Ldfld && instructionsList[i + 2].operand.Equals(parentField) && instructionsList[i + 3].opcode == OpCodes.Ldsfld && instructionsList[i + 3].operand.Equals(energyShieldEnergyMaxField) && instructionsList[i + 4].opcode == OpCodes.Ldc_I4_1 && instructionsList[i + 5].opcode == OpCodes.Ldc_I4_M1 && instructionsList[i + 6].opcode == OpCodes.Call && instructionsList[i + 6].operand.Equals(getStatValueMethod))
                {
                    yield return instructionsList[i];
                    yield return instructionsList[i + 1];
                    yield return new CodeInstruction(OpCodes.Callvirt, energyMaxProperty);
                    i += 6;
                    patchCount++;
                }
                else
                {
                    yield return instructionsList[i];
                }
            }
            if (patchCount == 0)
            {
                Log.Error("[Infusion 2] Failed to patch Gizmo_EnergyShieldStatus.GizmoOnGUI - no matching patterns found");
            }
        }
    }

}