using Infusion.Helpers;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Infusion
{
    public class Dialog_ManageApparelInfusionPolicies : Window
    {
        private const float RowHeight = 32f;

        private readonly ApparelPolicy policy;
        private readonly QuickSearchWidget quickSearch = new QuickSearchWidget();
        private Vector2 scrollPosition = Vector2.zero;

        public override Vector2 InitialSize => new Vector2(760f, 720f);

        public Dialog_ManageApparelInfusionPolicies(ApparelPolicy policy)
        {
            this.policy = policy;
            forcePause = true;
            doCloseX = true;
            doCloseButton = true;
            closeOnClickedOutside = true;
            absorbInputAroundWindow = true;
            optionalTitle = "Infusion.ApparelPolicy.ManageWindowTitle".Translate(policy?.label ?? "Unknown");
        }

        public override void PreOpen()
        {
            base.PreOpen();
            quickSearch.Reset();
            Current.Game?.GetComponent<Comps.GameComponent_Infusion>()?.PruneMissingApparelInfusionPolicies();
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;

            Rect summaryRect = new Rect(inRect.x, inRect.y, inRect.width, 48f);
            Widgets.Label(summaryRect, "Infusion.ApparelPolicy.ManageDescription".Translate());

            Rect searchRect = new Rect(inRect.x, summaryRect.yMax + 6f, inRect.width, 24f);
            quickSearch.OnGUI(searchRect, null, null);

            Rect allowAllRect = new Rect(inRect.x, searchRect.yMax + 10f, 160f, 32f);
            Rect disallowAllRect = new Rect(allowAllRect.xMax + 10f, allowAllRect.y, 160f, 32f);
            Rect countRect = new Rect(disallowAllRect.xMax + 10f, allowAllRect.y, inRect.width - disallowAllRect.xMax - 10f, 32f);

            if (Widgets.ButtonText(allowAllRect, "Infusion.ApparelPolicy.AllowAll".Translate()))
            {
                ApparelInfusionPolicyUtility.AllowAllInfusions(policy);
            }

            if (Widgets.ButtonText(disallowAllRect, "Infusion.ApparelPolicy.DisallowAll".Translate()))
            {
                ApparelInfusionPolicyUtility.DisallowAllInfusions(policy);
            }

            Text.Anchor = TextAnchor.MiddleRight;
            Widgets.Label(countRect, "Infusion.ApparelPolicy.DisallowedCount".Translate(ApparelInfusionPolicyUtility.CountDisallowedInfusions(policy)));
            Text.Anchor = TextAnchor.UpperLeft;

            Rect listRect = new Rect(
                inRect.x,
                allowAllRect.yMax + 10f,
                inRect.width,
                inRect.yMax - allowAllRect.yMax - Window.CloseButSize.y - 20f);
            Widgets.DrawMenuSection(listRect);

            List<InfusionDef> infusions = GetFilteredInfusions();
            if (infusions.Count == 0)
            {
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(listRect, "Infusion.ApparelPolicy.NoInfusions".Translate());
                Text.Anchor = TextAnchor.UpperLeft;
                return;
            }

            Rect outRect = listRect.ContractedBy(6f);
            Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, infusions.Count * RowHeight);
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);

            for (int i = 0; i < infusions.Count; i++)
            {
                DrawInfusionRow(viewRect, infusions[i], i);
            }

            Widgets.EndScrollView();
        }

        private List<InfusionDef> GetFilteredInfusions()
        {
            IEnumerable<InfusionDef> infusions = ApparelInfusionPolicyUtility.GetManagedInfusions();
            return infusions
                .Where(infusion => quickSearch.filter.Matches(BuildSearchLabel(infusion)))
                .ToList();
        }

        private string BuildSearchLabel(InfusionDef infusion)
        {
            return $"{infusion.LabelCap} {infusion.LabelShort} {infusion.tier.LabelCap}";
        }

        private void DrawInfusionRow(Rect viewRect, InfusionDef infusion, int rowIndex)
        {
            bool allowed = ApparelInfusionPolicyUtility.IsInfusionAllowed(policy, infusion);

            Rect rowRect = new Rect(0f, rowIndex * RowHeight, viewRect.width, RowHeight);
            Rect iconRect = new Rect(rowRect.x + 4f, rowRect.y + 2f, 28f, 28f);
            Rect textRect = new Rect(iconRect.xMax + 8f, rowRect.y, rowRect.width - 160f, rowRect.height);
            Rect stateRect = new Rect(rowRect.xMax - 110f, rowRect.y, 100f, rowRect.height);

            if (Mouse.IsOver(rowRect))
            {
                Widgets.DrawHighlight(rowRect);
            }
            else if (rowIndex % 2 == 1)
            {
                Widgets.DrawLightHighlight(rowRect);
            }

            GUI.DrawTexture(iconRect, allowed ? ResourceBank.Textures.Infuser : ResourceBank.Textures.InfuserEmpty);
            Widgets.Label(textRect, $"{infusion.LabelCap} ({infusion.tier.LabelCap})".Colorize(Utils.GetTierColor(infusion.tier)));

            Text.Anchor = TextAnchor.MiddleRight;
            string stateText = allowed
                ? "Infusion.ApparelPolicy.Allowed".Translate().ToString()
                : "Infusion.ApparelPolicy.Disallowed".Translate().ToString();
            Widgets.Label(
                stateRect,
                stateText.Colorize(allowed ? new Color(0.55f, 0.9f, 0.55f) : new Color(0.95f, 0.45f, 0.45f)));
            Text.Anchor = TextAnchor.UpperLeft;

            TooltipHandler.TipRegion(rowRect, new TipSignal(InfusionDef.MakeDescriptionString(infusion)));
            if (Widgets.ButtonInvisible(rowRect))
            {
                ApparelInfusionPolicyUtility.SetInfusionAllowed(policy, infusion, !allowed);
                (allowed ? SoundDefOf.Checkbox_TurnedOff : SoundDefOf.Checkbox_TurnedOn).PlayOneShotOnCamera();
            }
        }
    }
}
