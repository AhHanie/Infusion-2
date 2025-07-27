using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;

namespace Infusion.Harmonize
{
    public static class VerbPropertiesPatches
    {
        [HarmonyPatch(typeof(VerbProperties), "GetHitChanceFactor")]
        public static class GetHitChanceFactor
        {
            /// <summary>
            /// Changes max accuracy to 200%.
            /// </summary>
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var insts = instructions.ToArray();

                // Find: Mathf.Clamp(value, 0.01f, 1f)
                //                                 ~~
                int targetOpCodePos = -1;

                for (int i = 0; i < insts.Length; i++)
                {
                    var code = insts[i];
                    if (code.opcode == OpCodes.Ldc_R4 &&
                        Convert.ToSingle(code.operand) == 1.0f)
                    {
                        targetOpCodePos = i;
                        break;
                    }
                }

                if (targetOpCodePos >= 0)
                {
                    insts[targetOpCodePos].operand = 2.0f;
                }
                else
                {
                    Log.Warning(
                        "[Infusion 2] Couldn't find matching opCode for VerbProperties.GetHitChanceFactor(). Can't apply accuracy overcapping."
                    );
                }

                return insts;
            }
        }
    }
}