using HarmonyLib;
using Infusion.Comps;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;

namespace Infusion.Helpers
{
    public static class ApparelInfusionPolicyUtility
    {
        // Mirrors the header geometry of vanilla Dialog_ManagePolicies<T>.DoWindowContents.
        private const float HeaderButtonSize = 32f;
        private const float HeaderButtonPitch = 42f; // button width + 10px gap
        private const float HeaderRowGap = 4f;

        private static List<InfusionDef> managedInfusions;
        private static readonly Func<Dialog_ManagePolicies<ApparelPolicy>, ApparelPolicy> selectedPolicyGetter = BindSelectedPolicyGetter();

        public static List<InfusionDef> GetManagedInfusions()
        {
            if (managedInfusions == null)
            {
                managedInfusions = DefDatabase<InfusionDef>.AllDefs
                    .Where(def => !InfusionDef.ShouldRemoveItself(def))
                    .Where(def => def.matches != null && def.matches.OfType<Infusion.Matchers.EquipmentType>().Any(matcher => matcher.apparel))
                    .OrderByDescending(def => def.tier.priority)
                    .ThenBy(def => def.LabelCap.ToString())
                    .ToList();
            }

            return managedInfusions;
        }

        // Draws the "manage infusions" icon button in the dialog's header row, next to the vanilla rename / duplicate /
        // delete (and make-default) icons. contentsRect is the rect vanilla passes to DoContentsRect, which sits directly under
        // that row: DoWindowContents builds it as rect3.yMax + OffsetHeaderY + 4f, where rect3 is the 32px header row.
        // OffsetHeaderY is a protected virtual that Dialog_ManageApparelPolicies does not override, so it is 0 here.
        public static void DrawManageInfusionsButton(UnityEngine.Rect contentsRect, Dialog_ManageApparelPolicies dialog)
        {
            ApparelPolicy policy = GetSelectedPolicy(dialog);
            if (policy == null)
            {
                return;
            }

            // Vanilla header buttons, right to left: delete, duplicate, rename, then make-default (hidden for the default policy).
            // Ours takes the next free slot.
            bool isDefault = Current.Game.outfitDatabase.DefaultOutfit() == policy;
            int slot = isDefault ? 3 : 4;

            UnityEngine.Rect buttonRect = new UnityEngine.Rect(
                contentsRect.xMax - HeaderButtonSize - slot * HeaderButtonPitch,
                contentsRect.y - HeaderButtonSize - HeaderRowGap,
                HeaderButtonSize,
                HeaderButtonSize);

            // Only build the tooltip text while hovered; this runs every frame.
            if (Mouse.IsOver(buttonRect))
            {
                TooltipHandler.TipRegion(buttonRect, new TipSignal(
                    "Infusion.ApparelPolicy.ManageTip".Translate(policy.label) + "\n\n" +
                    "Infusion.ApparelPolicy.DisallowedCount".Translate(CountDisallowedInfusions(policy))));
            }

            if (Widgets.ButtonImage(buttonRect, ResourceBank.Textures.Infuser, true, null))
            {
                Find.WindowStack.Add(new Dialog_ManageApparelInfusionPolicies(policy));
            }
        }

        public static bool FilterAllowsThingForPolicy(bool filterAllows, ApparelPolicy policy, Thing thing)
        {
            if (!filterAllows || policy == null)
            {
                return filterAllows;
            }

            if (!(thing is Apparel apparel))
            {
                return true;
            }

            GameComponent_Infusion component = Current.Game.GetComponent<GameComponent_Infusion>();
            return component.AllowsApparelForPolicy(policy, apparel);
        }

        public static bool IsInfusionAllowed(ApparelPolicy policy, InfusionDef infusion)
        {
            GameComponent_Infusion component = Current.Game.GetComponent<GameComponent_Infusion>();
            return !component.IsApparelInfusionDisallowed(policy, infusion);
        }

        public static int CountDisallowedInfusions(ApparelPolicy policy)
        {
            return Current.Game.GetComponent<GameComponent_Infusion>().CountDisallowedApparelInfusions(policy);
        }

        public static void SetInfusionAllowed(ApparelPolicy policy, InfusionDef infusion, bool allowed)
        {
            Current.Game.GetComponent<GameComponent_Infusion>().SetApparelInfusionAllowed(policy, infusion, allowed);
        }

        public static void AllowAllInfusions(ApparelPolicy policy)
        {
            Current.Game.GetComponent<GameComponent_Infusion>().RemoveApparelInfusionPolicy(policy);
        }

        public static void DisallowAllInfusions(ApparelPolicy policy)
        {
            Current.Game.GetComponent<GameComponent_Infusion>().SetDisallowedApparelInfusions(policy, GetManagedInfusions());
        }

        // SelectedPolicy is a protected property of the generic base Dialog_ManagePolicies<ApparelPolicy>; read it through
        // a delegate bound once instead of reflecting on the private field every frame.
        public static ApparelPolicy GetSelectedPolicy(Dialog_ManageApparelPolicies dialog)
        {
            return selectedPolicyGetter?.Invoke(dialog);
        }

        private static Func<Dialog_ManagePolicies<ApparelPolicy>, ApparelPolicy> BindSelectedPolicyGetter()
        {
            MethodInfo getter = AccessTools.PropertyGetter(typeof(Dialog_ManagePolicies<ApparelPolicy>), "SelectedPolicy");
            if (getter == null)
            {
                Log.Error("[Infusion 2] Dialog_ManagePolicies.SelectedPolicy not found - apparel infusion policy button disabled.");
                return null;
            }

            return (Func<Dialog_ManagePolicies<ApparelPolicy>, ApparelPolicy>)Delegate.CreateDelegate(
                typeof(Func<Dialog_ManagePolicies<ApparelPolicy>, ApparelPolicy>), getter);
        }
    }
}
