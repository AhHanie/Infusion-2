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
    // Optional integration with 1trickPwnyta's Defaults, which adds a "save as default" icon to the header row of the
    // in-game policy editors. Detected by package id only; there is no reference to its assembly.
    // Provisional: the Defaults release available when this was written has no RimWorld 1.7 build, so its 1.7 icon position
    // is assumed to match its 1.6 one (the slot right where vanilla's make-default icon is, or the one left of it).
    public static class DefaultsPolicyCompat
    {
        public const string PackageId = "defaults.1trickPwnyta";

        private static bool? active;

        public static bool Active => active ?? (active = ModsConfig.IsActive(PackageId)).Value;
    }

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
            if (!TryGetGamePolicy(dialog, out ApparelPolicy policy))
            {
                return;
            }

            // Vanilla header buttons, right to left: delete, duplicate, rename, then make-default (hidden for the default policy).
            // Ours takes the next free slot. Defaults' "save as default" icon sits in the make-default slot for the default
            // policy and in the slot left of it otherwise, so with Defaults active ours moves one slot further left.
            bool isDefault = Current.Game.outfitDatabase.DefaultOutfit() == policy;
            int slot = (isDefault ? 3 : 4) + (DefaultsPolicyCompat.Active ? 1 : 0);

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

        // Only a policy editor opened by the game itself manages policies of the current game. Defaults also embeds a
        // Dialog_ManageApparelPolicies subclass in its settings window and draws it by hand; that one is not in the window
        // stack and edits templates that do not belong to the game, so it gets no infusion button.
        public static bool TryGetGamePolicy(Dialog_ManageApparelPolicies dialog, out ApparelPolicy policy)
        {
            policy = null;

            if (dialog == null || Current.Game == null || !Find.WindowStack.Windows.Contains(dialog))
            {
                return false;
            }

            policy = GetSelectedPolicy(dialog);
            if (policy == null || !Current.Game.outfitDatabase.AllOutfits.Contains(policy))
            {
                policy = null;
                return false;
            }

            return true;
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
