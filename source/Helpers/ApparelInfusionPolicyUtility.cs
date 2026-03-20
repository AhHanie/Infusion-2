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
        private static List<InfusionDef> managedInfusions;
        private static string lastOpenedTitleKey;

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

        public static float GetHeaderButtonReserve(float currentReserve, object dialog)
        {
            return IsApparelPolicyDialog(dialog) ? currentReserve + 42f : currentReserve;
        }

        public static void DrawManageInfusionsButton(Rect referenceRect, object dialog)
        {
            if (!IsApparelPolicyDialog(dialog))
            {
                return;
            }

            ApparelPolicy policy = GetSelectedPolicy(dialog);
            if (policy == null)
            {
                return;
            }

            Rect buttonRect = new Rect(referenceRect.x - referenceRect.width - 10f, referenceRect.y, referenceRect.width, referenceRect.height);

            TooltipHandler.TipRegion(buttonRect, new TipSignal("Infusion.ApparelPolicy.ManageTip".Translate(policy.label)));
            if (Widgets.ButtonImage(buttonRect, ResourceBank.Textures.Infuser, true, null))
            {
                Find.WindowStack.Add(new Dialog_ManageApparelInfusionPolicies(policy));
            }
        }

        public static bool FilterAllowsThingForPolicy(bool filterAllows, ApparelPolicy policy, Thing thing)
        {
            if (!filterAllows)
            {
                return false;
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

        public static bool IsApparelPolicyDialog(object dialog)
        {
            return string.Equals(lastOpenedTitleKey, "ApparelPolicyTitle", StringComparison.Ordinal);
        }

        public static void CacheDialogTitleKey(object dialog)
        {
            PropertyInfo titleKeyProperty = AccessTools.Property(dialog.GetType(), "TitleKey");
            lastOpenedTitleKey = titleKeyProperty?.GetValue(dialog, null) as string;
        }

        public static ApparelPolicy GetSelectedPolicy(object dialog)
        {
            FieldInfo policyField = AccessTools.Field(dialog.GetType(), "policyInt");
            return policyField.GetValue(dialog) as ApparelPolicy;
        }
    }
}
