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
    // Draws the "manage infusions" icon button in the header row of the Apparel policies dialog (next to rename/duplicate/delete).
    //
    // This deliberately patches the concrete, non-generic Dialog_ManageApparelPolicies.DoContentsRect and NOT
    // Dialog_ManagePolicies<ApparelPolicy>.DoWindowContents: on the 1.7 runtime a detour on the shared-generic base method is
    // lost once the JIT tiers it up (~30 calls, a fraction of a second after the dialog opens), so a button drawn from there
    // vanishes right after the window opens. Detours on the non-generic method survive. DoContentsRect receives the rect
    // directly under the header row, so the header button position is derived from it (see DrawManageInfusionsButton).
    [HarmonyPatch]
    public static class Dialog_ManageApparelPolicies_InfusionButton
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.DeclaredMethod(typeof(Dialog_ManageApparelPolicies), "DoContentsRect");
        }

        public static void Prefix(Dialog_ManageApparelPolicies __instance, UnityEngine.Rect rect)
        {
            ApparelInfusionPolicyUtility.DrawManageInfusionsButton(rect, __instance);
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
