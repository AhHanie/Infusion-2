using HarmonyLib;
using Infusion.Comps;
using Infusion.Helpers;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace Infusion.Harmonize
{
    [HarmonyPatch]
    public static class Dialog_ManagePolicies_ApparelInfusionButton
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(Dialog_ManageApparelPolicies), nameof(Dialog_ManageApparelPolicies.DoWindowContents));
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = instructions.ToList();
            MethodInfo reserveMethod = AccessTools.Method(typeof(ApparelInfusionPolicyUtility), nameof(ApparelInfusionPolicyUtility.GetHeaderButtonReserve));

            if (reserveMethod == null)
            {
                Log.Error("[Infusion 2] Failed to patch Dialog_ManagePolicies<ApparelPolicy>.DoWindowContents - required reflection targets were not found.");
                return codes;
            }

            bool reservePatched = false;

            for (int i = 0; i < codes.Count; i++)
            {
                if (!reservePatched
                    && codes[i].opcode == OpCodes.Ldc_R4
                    && codes[i].operand is float reserve
                    && reserve == 100f)
                {
                    codes.InsertRange(i + 1, new[]
                    {
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Call, reserveMethod)
                    });
                    reservePatched = true;
                    i += 2;
                    continue;
                }
            }

            if (!reservePatched)
            {
                Log.Error("[Infusion 2] Failed to patch Dialog_ManagePolicies<ApparelPolicy>.DoWindowContents - header reserve injection failed.");
            }

            return codes;
        }

        public static void Postfix(Dialog_ManageApparelPolicies __instance, UnityEngine.Rect inRect)
        {
            UnityEngine.Rect rect = inRect;
            rect.height = 32f;

            UnityEngine.Rect rect2 = rect;
            rect2.y = rect.yMax;

            UnityEngine.Rect rect3 = rect2;
            rect3.y = rect2.yMax + 10f;
            rect3.height = 32f;
            rect3.width = 200f;

            UnityEngine.Rect rect4 = rect3;
            rect4.x = rect3.xMax + 10f;
            rect4.xMax = inRect.xMax;

            UnityEngine.Rect rect6 = rect4;
            rect6.x = rect4.xMax - rect4.height;
            rect6.width = rect4.height;

            UnityEngine.Rect rect7 = rect6;
            rect7.x = rect6.x - rect4.height - 10f;

            UnityEngine.Rect rect8 = rect7;
            rect8.x = rect7.x - rect4.height - 10f;

            UnityEngine.Rect rect9 = rect8;
            rect9.x = rect8.x - rect4.height - 10f;

            ApparelInfusionPolicyUtility.DrawManageInfusionsButton(rect9, __instance);
        }
    }

    [HarmonyPatch(typeof(Dialog_ManageApparelPolicies), nameof(Dialog_ManageApparelPolicies.PreOpen))]
    public static class Dialog_ManagePolicies_CacheTitleKey
    {
        public static void Postfix(Dialog_ManageApparelPolicies __instance)
        {
            ApparelInfusionPolicyUtility.CacheDialogTitleKey(__instance);
        }
    }

    [HarmonyPatch(typeof(OutfitDatabase), nameof(OutfitDatabase.TryDelete))]
    public static class OutfitDatabase_RemoveApparelInfusionPolicySettings
    {
        public static void Postfix(ApparelPolicy apparelPolicy, AcceptanceReport __result)
        {
            if (!__result.Accepted)
            {
                return;
            }

            Current.Game.GetComponent<GameComponent_Infusion>().RemoveApparelInfusionPolicy(apparelPolicy);
        }
    }

    [HarmonyPatch(typeof(ApparelPolicy), nameof(ApparelPolicy.CopyFrom))]
    public static class ApparelPolicy_CopyFromInfusionPolicySettings
    {
        public static void Postfix(ApparelPolicy __instance, Policy other)
        {
            if (!(other is ApparelPolicy sourcePolicy))
            {
                return;
            }

            GameComponent_Infusion component = Current.Game.GetComponent<GameComponent_Infusion>();

            if (component.HasCustomApparelInfusionRestrictions(sourcePolicy))
            {
                component.SetDisallowedApparelInfusions(__instance, component.GetDisallowedApparelInfusions(sourcePolicy).ToList());
            }
            else
            {
                component.RemoveApparelInfusionPolicy(__instance);
            }
        }
    }

    [HarmonyPatch(typeof(JobGiver_OptimizeApparel), "TryGiveJob")]
    public static class JobGiver_OptimizeApparel_ApparelInfusionPolicy
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = instructions.ToList();
            MethodInfo allowsMethod = AccessTools.Method(typeof(ThingFilter), nameof(ThingFilter.Allows), new[] { typeof(Thing) });
            FieldInfo apparelFilterField = AccessTools.Field(typeof(ApparelPolicy), nameof(ApparelPolicy.filter));
            MethodInfo helperMethod = AccessTools.Method(typeof(ApparelInfusionPolicyUtility), nameof(ApparelInfusionPolicyUtility.FilterAllowsThingForPolicy));

            if (allowsMethod == null || apparelFilterField == null || helperMethod == null)
            {
                Log.Error("[Infusion 2] Failed to patch JobGiver_OptimizeApparel.TryGiveJob - required reflection targets were not found.");
                return codes;
            }

            int patchedChecks = 0;

            for (int i = 0; i < codes.Count; i++)
            {
                if (!codes[i].Calls(allowsMethod) || i < 3 || !codes[i - 2].LoadsField(apparelFilterField))
                {
                    continue;
                }

                codes.InsertRange(i + 1, new[]
                {
                    new CodeInstruction(codes[i - 3].opcode, codes[i - 3].operand),
                    new CodeInstruction(codes[i - 1].opcode, codes[i - 1].operand),
                    new CodeInstruction(OpCodes.Call, helperMethod)
                });

                patchedChecks++;
                i += 3;
            }

            if (patchedChecks != 2)
            {
                Log.Error($"[Infusion 2] Failed to patch JobGiver_OptimizeApparel.TryGiveJob - expected 2 apparel infusion policy checks but patched {patchedChecks}.");
            }

            return codes;
        }
    }
}
