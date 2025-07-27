using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Infusion
{
    public class InfusedTab : ITab
    {
        private static readonly Color gray = new Color(0.61f, 0.61f, 0.61f);
        private Vector2 scrollPos = Vector2.zero;

        public InfusedTab()
        {
            size = new Vector2(460.0f, 550.0f);
            labelKey = "Infusion.ITab";
        }

        private CompInfusion CompInf => SelThing?.TryGetComp<CompInfusion>();

        public override bool IsVisible => CompInf != null;

        private static void ResetStyles()
        {
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
        }

        protected override void FillTab()
        {
            var container = new Rect(0.0f, 0.0f, size.x, size.y).ContractedBy(16.0f);

            // label
            var labelView = new Rect(container.xMin, container.yMin, container.xMax * 0.85f, container.yMax);
            var labelHeight = DrawLabel(labelView) + 4.0f;

            // subLabel
            var subLabelView = new Rect(
                container.xMin,
                labelView.yMin + labelHeight,
                container.width,
                container.yMax - labelHeight
            );
            var subLabelHeight = DrawSubLabel(subLabelView) + 12.0f;

            // body
            var bodyView = new Rect(
                container.xMin,
                subLabelView.yMin + subLabelHeight,
                container.xMax - 6.0f,
                container.yMax - subLabelView.yMin - subLabelHeight - 56.0f
            );

            if (CompInf.SlotCount > 0)
            {
                DrawInfusionList(bodyView);
            }
            else
            {
                GUI.color = gray;
                Text.Font = GameFont.Small;
                Widgets.Label(bodyView, ResourceBank.Strings.ITab.NoSlot);
                GUI.color = Color.white;
            }

            // infuser button
            var infuserView = new Rect(
                new Vector2(container.center.x - 100.0f, container.yMax - 40.0f),
                new Vector2(200.0f, 36.0f)
            );

            DrawApplyButton(infuserView);
        }

        private float DrawLabel(Rect parentRect)
        {
            var compInf = CompInf;
            var label = compInf.InspectionLabel;

            Text.Font = GameFont.Medium;

            GUI.color = compInf.BestInfusion?.tier.color ?? gray;

            Widgets.Label(parentRect, label);

            var labelHeight = Text.CalcHeight(label, parentRect.width);
            ResetStyles();

            return labelHeight;
        }

        private float DrawSubLabel(Rect parentRect)
        {
            var compInf = CompInf;
            var hasAnySlot = compInf.SlotCount > 0;
            var compQuality = SelThing?.TryGetComp<CompQuality>();

            var subLabelSB = new StringBuilder();

            if (compQuality != null)
            {
                subLabelSB.Append(compQuality.Quality.GetLabel()).Append(" ");
            }

            if (compInf.parent.Stuff != null)
            {
                subLabelSB.Append(compInf.parent.Stuff.LabelAsStuff).Append(" ");
            }

            if (hasAnySlot)
            {
                subLabelSB
                    .Append(compInf.parent.def.label)
                    .Append(" (")
                    .Append(compInf.Size)
                    .Append("/")
                    .Append(compInf.SlotCount)
                    .Append(")");
            }

            var subLabel = subLabelSB.ToString().CapitalizeFirst();
            Widgets.Label(parentRect, subLabel);

            var subLabelHeight = Text.CalcHeight(subLabel, parentRect.width);

            Text.Font = GameFont.Tiny;
            GUI.color = Color.gray;

            const float hintMarginTop = 4.0f;

            if (hasAnySlot)
            {
                var hint = ResourceBank.Strings.ITab.Hint;
                var hintHeight = Text.CalcHeight(hint, parentRect.width);

                var hintView = new Rect(
                    parentRect.xMin,
                    parentRect.yMin + subLabelHeight + hintMarginTop,
                    parentRect.width,
                    hintHeight
                );

                Widgets.Label(hintView, hint);
                ResetStyles();

                return subLabelHeight + hintHeight + hintMarginTop;
            }

            ResetStyles();
            return subLabelHeight + hintMarginTop;
        }

        private Rect DrawBaseInfusion(Rect parentRect, float yOffset, InfusionDef infDef)
        {
            var description = InfusionDef.MakeDescriptionString(infDef);
            var contentsHeight = Text.CalcHeight(description, parentRect.width - 16.0f) + 16.0f;

            var container = new Rect(parentRect.x, yOffset, parentRect.width, contentsHeight);
            var body = new Rect(
                container.x + 8.0f,
                container.y + 8.0f,
                container.xMax - 8.0f,
                container.yMax - 8.0f
            );

            var hovered = Mouse.IsOver(container);

            // hover highlight
            if (hovered)
            {
                GUI.DrawTexture(container, TexUI.HighlightTex);
            }

            Widgets.Label(body, description);

            return container;
        }

        private Rect DrawInfusion(Rect parentRect, float yOffset, InfusionDef infDef)
        {
            var comp = CompInf;
            var container = DrawBaseInfusion(parentRect, yOffset, infDef);

            // extraction/removal highlight
            var markedForExtraction = comp.ExtractionSet.Contains(infDef);
            var markedForRemoval = comp.RemovalSet.Contains(infDef);

            if (markedForExtraction)
            {
                GUI.color = new Color(1.0f, 1.0f, 0.0f, 0.85f);
                GUI.DrawTexture(container, TexUI.HighlightTex);
            }
            else if (markedForRemoval)
            {
                GUI.color = new Color(1.0f, 0.0f, 0.0f, 0.85f);
                GUI.DrawTexture(container, TexUI.HighlightTex);
            }

            GUI.color = Color.white;

            var baseExtractionChance = Mathf.Min(1.0f,
                infDef.tier.extractionChance * Settings.extractionChanceFactor.Value);

            var successChance = comp.Biocoder != null
                ? baseExtractionChance * 0.5f
                : baseExtractionChance;

            var successChancePercent = GenText.ToStringPercent(successChance);

            string tooltipStringKey;
            if (markedForExtraction)
            {
                tooltipStringKey = ResourceBank.Strings.ITab.MarkRemoval(successChance);
            }
            else if (markedForRemoval)
            {
                tooltipStringKey = ResourceBank.Strings.ITab.Unmark;
            }
            else
            {
                tooltipStringKey = ResourceBank.Strings.ITab.MarkExtraction(successChance);
            }

            TooltipHandler.TipRegion(container, new TipSignal(tooltipStringKey));

            if (Widgets.ButtonInvisible(container))
            {
                if (markedForExtraction)
                {
                    comp.MarkForRemoval(infDef);
                    SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera();
                }
                else if (markedForRemoval)
                {
                    comp.UnmarkForRemoval(infDef);
                    SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera();
                }
                else
                {
                    comp.MarkForExtractor(infDef);
                    SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera();
                }
            }

            return container;
        }

        private Rect DrawPendingInfusion(Rect parentRect, float yOffset, InfusionDef infDef)
        {
            var compInf = CompInf;
            var container = DrawBaseInfusion(parentRect, yOffset, infDef);

            // pending highlight
            GUI.color = new Color(0.0f, 1.0f, 0.0f, 0.85f);
            GUI.DrawTexture(container, TexUI.HighlightTex);
            GUI.color = Color.white;

            TooltipHandler.TipRegion(container, new TipSignal(ResourceBank.Strings.ITab.CancelInfuser));

            if (Widgets.ButtonInvisible(container))
            {
                compInf.UnmarkForInfuser(infDef);
                SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera();
            }

            return container;
        }

        private void DrawInfusionList(Rect parentRect)
        {
            var comp = CompInf;
            var scrollerWidth = parentRect.width - 24.0f;

            float CalculateInfusionsHeight(IEnumerable<InfusionDef> infusions)
            {
                return infusions.Sum(inf =>
                    Text.CalcHeight(InfusionDef.MakeDescriptionString(inf), scrollerWidth) + 24.0f);
            }

            var totalHeight = CalculateInfusionsHeight(comp.Infusions) +
                             CalculateInfusionsHeight(comp.WantingSet);

            var scroller = new Rect(0.0f, 0.0f, scrollerWidth, totalHeight);

            Widgets.BeginScrollView(parentRect, ref scrollPos, scroller);

            var currentY = scroller.y;

            // Draw existing infusions
            foreach (var infusion in comp.Infusions)
            {
                var drawnRect = DrawInfusion(scroller, currentY, infusion);
                currentY += drawnRect.height + 8.0f;
            }

            // Draw pending infusions
            foreach (var pendingInfusion in comp.WantingSet)
            {
                var drawnRect = DrawPendingInfusion(scroller, currentY, pendingInfusion);
                currentY += drawnRect.height;
            }

            Widgets.EndScrollView();
            ResetStyles();
        }

        private void DrawApplyButton(Rect parentRect)
        {
            var comp = CompInf;

            void DrawUnavailableReason(string reason)
            {
                GUI.color = gray;
                Text.Anchor = TextAnchor.MiddleCenter;
                Text.Font = GameFont.Tiny;

                Widgets.Label(parentRect, reason);

                GUI.color = Color.white;
                Text.Anchor = TextAnchor.UpperLeft;
                Text.Font = GameFont.Small;
            }

            void DrawButton(IEnumerable<KeyValuePair<InfusionDef, Infuser>> infPairs)
            {
                var buttonView = new Rect(
                    parentRect.xMin + 30.0f,
                    parentRect.yMin,
                    140.0f,
                    parentRect.height
                );

                TooltipHandler.TipRegion(parentRect, new TipSignal(ResourceBank.Strings.ITab.ApplyInfuserDesc));

                if (Widgets.ButtonText(buttonView, ResourceBank.Strings.ITab.ApplyInfuser))
                {
                    var floatMenuOptions = infPairs.Select(infPair =>
                        new FloatMenuOption(infPair.Key.LabelCap, () => comp.MarkForInfuser(infPair.Key))
                    ).ToList();

                    Find.WindowStack.Add(new FloatMenu(floatMenuOptions));
                }
            }

            if (comp.SlotCount <= comp.InfusionsRaw.Count)
            {
                DrawUnavailableReason(ResourceBank.Strings.ITab.CantApplySlotsFull);
                return;
            }

            // Get all available infusers that match criteria
            var availableInfusers = Infuser.AllInfusersByDef
                .Where(kv => !comp.InfusionsRaw.Contains(kv.Key) &&
                            InfusionDef.MatchesAll(kv.Key, comp.parent, comp.Quality))
                .ToList();

            if (!availableInfusers.Any())
            {
                DrawUnavailableReason(ResourceBank.Strings.ITab.CantApplyNoSuitable);
                return;
            }

            DrawButton(availableInfusers);
        }
    }
}