using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Infusion.Harmonize
{
    public static class GizmoEnergyShieldStatusPatches
    {
        [HarmonyPatch(typeof(Gizmo_EnergyShieldStatus), "GizmoOnGUI")]
        public static class GizmoOnGUI
        {
            public static bool Prepare(MethodBase original)
            {
                return !Compat.ToggleableShields.Enabled;
            }
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
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
                    if (i + 6 < instructionsList.Count &&
                        instructionsList[i].opcode == OpCodes.Ldarg_0 &&
                        instructionsList[i + 1].opcode == OpCodes.Ldfld &&
                        instructionsList[i + 1].operand.Equals(shieldField) &&
                        instructionsList[i + 2].opcode == OpCodes.Ldfld &&
                        instructionsList[i + 2].operand.Equals(parentField) &&
                        instructionsList[i + 3].opcode == OpCodes.Ldsfld &&
                        instructionsList[i + 3].operand.Equals(energyShieldEnergyMaxField) &&
                        instructionsList[i + 4].opcode == OpCodes.Ldc_I4_1 &&
                        instructionsList[i + 5].opcode == OpCodes.Ldc_I4_M1 &&
                        instructionsList[i + 6].opcode == OpCodes.Call &&
                        instructionsList[i + 6].operand.Equals(getStatValueMethod))
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
}