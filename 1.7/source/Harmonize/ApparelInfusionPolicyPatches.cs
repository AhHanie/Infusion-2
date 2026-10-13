using HarmonyLib;
using Infusion.Comps;
using Infusion.Helpers;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Verse;

namespace Infusion.Harmonize
{
    public static class OutfittedApparelPolicyCompat
    {
        public const string PackageId = "mitasamodel.outfitted";

        public static bool Active => ModsConfig.IsActive(PackageId);
    }

    [HarmonyPatch]
    public static class Dialog_ManagePolicies_ApparelInfusionButton
    {
        private static bool Prepare()
        {
            return !OutfittedApparelPolicyCompat.Active;
        }

        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(Dialog_ManageApparelPolicies), nameof(Dialog_ManageApparelPolicies.DoWindowContents));
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

    // DoWindowContents shrinks the policy-name box by a hard-coded 100f to make room for its header buttons.
    // Widen that reserve by one button, but only for apparel policy dialogs.
    [HarmonyPatch]
    public static class Dialog_ManagePolicies_ApparelInfusionButtonReserve
    {
        private static bool Prepare()
        {
            return !OutfittedApparelPolicyCompat.Active;
        }

        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(Dialog_ManageApparelPolicies), nameof(Dialog_ManageApparelPolicies.DoWindowContents));
        }

        [HarmonyInfix(100f)]
        public static float Postfix(float __result, [HarmonyOuter] Dialog_ManagePolicies<ApparelPolicy> __instance)
        {
            return ApparelInfusionPolicyUtility.GetHeaderButtonReserve(__result, __instance);
        }
    }

    [HarmonyPatch]
    [HarmonyAfter("rimworld.outfitted")]
    public static class Dialog_ManagePolicies_OutfittedApparelInfusionButton
    {
        private const float OutfittedButtonWidth = 150f;
        private const float OutfittedSmallGap = 6f;
        private const float ButtonSize = 28f;

        private static bool Prepare()
        {
            return OutfittedApparelPolicyCompat.Active;
        }

        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(Dialog_ManagePolicies<ApparelPolicy>), nameof(Dialog_ManagePolicies<ApparelPolicy>.DoWindowContents));
        }

        public static void Postfix(Dialog_ManagePolicies<ApparelPolicy> __instance, UnityEngine.Rect inRect)
        {
            if (!(__instance is Dialog_ManageApparelPolicies))
            {
                return;
            }

            UnityEngine.Rect buttonRect = new UnityEngine.Rect(
                inRect.xMax - OutfittedButtonWidth - OutfittedSmallGap - OutfittedButtonWidth - OutfittedSmallGap - ButtonSize,
                inRect.y,
                ButtonSize,
                ButtonSize);

            ApparelInfusionPolicyUtility.DrawManageInfusionsButtonAt(buttonRect, __instance);
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
            if (Current.ProgramState != ProgramState.Playing || !(other is ApparelPolicy sourcePolicy))
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

    // TryGiveJob asks the current apparel policy's filter about worn and candidate apparel twice;
    // layer the per-policy infusion restrictions on top of each answer.
    [HarmonyPatch(typeof(JobGiver_OptimizeApparel), "TryGiveJob")]
    public static class JobGiver_OptimizeApparel_ApparelInfusionPolicy
    {
        [HarmonyInfix(typeof(ThingFilter), nameof(ThingFilter.Allows), typeof(Thing))]
        public static bool Postfix(bool __result, Thing t, [HarmonyOuter] Pawn pawn)
        {
            return ApparelInfusionPolicyUtility.FilterAllowsThingForPolicy(__result, pawn.outfits.CurrentApparelPolicy, t);
        }
    }
}
