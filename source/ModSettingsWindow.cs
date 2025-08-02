using LessUI;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Infusion
{
    public class ModSettingsWindow
    {
        public static StrongBox<string> accuracyOvercappingLabelRef = new StrongBox<string>("Infusion.Settings.AccuracyOvercapping".Translate());
        public static StrongBox<string> bonusToBiocodeLabelRef = new StrongBox<string>("Infusion.Settings.BiocodeBonus".Translate());
        public static StrongBox<string> extractionSuccessFactorLabelRef = new StrongBox<string>("Infusion.Settings.ExtractionChanceFactor".Translate());
        public static StrongBox<string> reusableInfusersLabelRef = new StrongBox<string>("Infusion.Settings.ReusableInfusers".Translate());
        public static StrongBox<string> infusionChanceLabelRef = new StrongBox<string>("Infusion.Settings.ChanceFactor".Translate());
        public static StrongBox<string> muLabelRef = new StrongBox<string>("Infusion.Settings.Mu".Translate());
        public static StrongBox<string> sigmaLabelRef = new StrongBox<string>("Infusion.Settings.Sigma".Translate());
        public static StrongBox<string> bodyPartLimitLabelRef = new StrongBox<string>("Infusion.Settings.BodyPartLimit".Translate());
        public static StrongBox<string> morePerLayerLabelRef = new StrongBox<string>("Infusion.Settings.LayerBonuses".Translate());
        public static StrongBox<string> awfulLabelRef = new StrongBox<string>("Awful");
        public static StrongBox<string> poorLabelRef = new StrongBox<string>("Poor");
        public static StrongBox<string> normalLabelRef = new StrongBox<string>("Normal");
        public static StrongBox<string> goodLabelRef = new StrongBox<string>("Good");
        public static StrongBox<string> excellentLabelRef = new StrongBox<string>("Excellent");
        public static StrongBox<string> masterworkLabelRef = new StrongBox<string>("Masterwork");
        public static StrongBox<string> legendaryLabelRef = new StrongBox<string>("Legendary");
        public static StrongBox<string> infusionSlotsSettingsLabelRef = new StrongBox<string>("Infusion.Settings.Infusions.Slots.Title".Translate());
        public static StrongBox<string> commonLabelRef = new StrongBox<string>("Common");
        public static StrongBox<string> uncommonLabelRef = new StrongBox<string>("Uncommon");
        public static StrongBox<string> rareLabelRef = new StrongBox<string>("Rare");
        public static StrongBox<string> tiersTitleLabelRef = new StrongBox<string>("Infusion.Settings.Tiers.Title".Translate());
        public static StrongBox<string> generalSettingsTitleLabelRef = new StrongBox<string>("Infusion.Settings.General.Title".Translate());
        public static StrongBox<string> statsGlobalMultiplierLabelRef = new StrongBox<string>("Infusion.Settings.StatsGlobalMultiplier".Translate());
        public static StrongBox<string> chanceGlobalMultiplierLabelRef = new StrongBox<string>("Infusion.Settings.ChanceGlobalMultiplier".Translate());
        public static StrongBox<string> amountGlobalMultiplierLabelRef = new StrongBox<string>("Infusion.Settings.AmountGlobalMultiplier".Translate());
        public static StrongBox<string> infusionSettingsLabelRef = new StrongBox<string>("Infusion.Settings.Infusions.Title".Translate());
        public static StrongBox<string> infusionDefsControlLabelRef = new StrongBox<string>("Infusion.Settings.Infusions.Defs.Title".Translate());
        public static StrongBox<Vector2> scrollPosition = new StrongBox<Vector2>(Vector2.zero);
        public static StrongBox<InfusionDef> selectedInfusionDef = new StrongBox<InfusionDef>(ResourceBank.allInfusionDefs.First());
        public static StrongBox<bool> infusionDefsEnableDisableButtonClicked = new StrongBox<bool>(false);
        public static void Draw(Rect parent)
        {
            ScrollCanvas canvas = new ScrollCanvas(parent, () => scrollPosition.Value, (vec2) => scrollPosition.Value = vec2);
            Stack stack = new Stack();
            stack.WidthMode = SizeMode.Fill;
            stack.VerticalSpacing = 5f;

            FillGrid grid = new FillGrid(2, 13)
            {
                Padding = 10f
            };
            grid.HeightMode = SizeMode.Content;
            Text.Font = GameFont.Small;

            Label generalSettingsTitleLabel = new Label(generalSettingsTitleLabelRef);
            generalSettingsTitleLabel.Alignment = Align.MiddleLeft;

            grid.AddChild(generalSettingsTitleLabel);
            grid.AddChild(new Empty());

            Label accuracyOvercappingLabel = new Label(accuracyOvercappingLabelRef);
            accuracyOvercappingLabel.Alignment = Align.MiddleLeft;
            accuracyOvercappingLabel.Tooltip = "Infusion.Settings.AccuracyOvercapping.Description".Translate();

            grid.AddChild(accuracyOvercappingLabel);

            Checkbox accuracyOvercappingCheckbox = new Checkbox(Settings.accuracyOvercap);
            accuracyOvercappingCheckbox.Alignment = Align.MiddleLeft;

            grid.AddChild(accuracyOvercappingCheckbox);

            Label bonusToBiocodeLabel = new Label(bonusToBiocodeLabelRef);
            bonusToBiocodeLabel.Alignment = Align.MiddleLeft;
            bonusToBiocodeLabel.Tooltip = "Infusion.Settings.BiocodeBonus.Description".Translate();

            grid.AddChild(bonusToBiocodeLabel);

            Checkbox bonusToBiocodeCheckbox = new Checkbox(Settings.biocodeBonus);
            bonusToBiocodeCheckbox.Alignment = Align.MiddleLeft;

            grid.AddChild(bonusToBiocodeCheckbox);

            Label extractionSuccessFactorLabel = new Label(extractionSuccessFactorLabelRef);
            extractionSuccessFactorLabel.Alignment = Align.MiddleLeft;
            extractionSuccessFactorLabel.Tooltip = "Infusion.Settings.ExtractionChanceFactor.Description".Translate();

            grid.AddChild(extractionSuccessFactorLabel);

            LabeledSlider extractionSuccessFactorSlider = new LabeledSlider($"{Settings.extractionChanceFactor.Value.ToString()}x", Settings.extractionChanceFactor.Value, 1f, 100f, (val) => Settings.extractionChanceFactor.Value = val);
            extractionSuccessFactorSlider.RoundTo = 0.5f;
            extractionSuccessFactorSlider.Alignment = Align.MiddleLeft;
            extractionSuccessFactorSlider.WidthMode = SizeMode.Fill;

            grid.AddChild(extractionSuccessFactorSlider);

            Label reusableInfusersLabel = new Label(reusableInfusersLabelRef);
            reusableInfusersLabel.Alignment = Align.MiddleLeft;
            reusableInfusersLabel.Tooltip = "Infusion.Settings.ReusableInfusers.Description".Translate();

            grid.AddChild(reusableInfusersLabel);

            Checkbox reusableInfusersCheckbox = new Checkbox(Settings.reusableInfusers);
            reusableInfusersCheckbox.Alignment = Align.MiddleLeft;

            grid.AddChild(reusableInfusersCheckbox);

            Label infusionChanceLabel = new Label(infusionChanceLabelRef);
            infusionChanceLabel.Alignment = Align.MiddleLeft;
            infusionChanceLabel.Tooltip = "Infusion.Settings.ChanceFactor.Description".Translate();

            grid.AddChild(infusionChanceLabel);

            LabeledSlider infusionChanceSlider = new LabeledSlider(Settings.chanceHandle.Value.ToString(), Settings.chanceHandle.Value, 0f, 100f, (val) => Settings.chanceHandle.Value = val);
            infusionChanceSlider.RoundTo = 1f;
            infusionChanceSlider.Alignment = Align.MiddleLeft;
            infusionChanceSlider.WidthMode = SizeMode.Fill;

            grid.AddChild(infusionChanceSlider);

            Label muLabel = new Label(muLabelRef);
            muLabel.Alignment = Align.MiddleLeft;
            muLabel.Tooltip = "Infusion.Settings.Mu.Description".Translate();

            grid.AddChild(muLabel);

            LabeledSlider muSlider = new LabeledSlider(Settings.muHandle.Value.ToString(), Settings.muHandle.Value, 0.5f, 10f, (val) => Settings.muHandle.Value = val);
            muSlider.RoundTo = 0.1f;
            muSlider.Alignment = Align.MiddleLeft;
            muSlider.WidthMode = SizeMode.Fill;

            grid.AddChild(muSlider);

            Label sigmaLabel = new Label(sigmaLabelRef);
            sigmaLabel.Alignment = Align.MiddleLeft;
            sigmaLabel.Tooltip = "Infusion.Settings.Sigma.Description".Translate();

            grid.AddChild(sigmaLabel);

            LabeledSlider sigmaSlider = new LabeledSlider(Settings.sigmaHandle.Value.ToString(), Settings.sigmaHandle.Value, 0.5f, 10f, (val) => Settings.sigmaHandle.Value = val);
            sigmaSlider.RoundTo = 0.1f;
            sigmaSlider.Alignment = Align.MiddleLeft;
            sigmaSlider.WidthMode = SizeMode.Fill;

            grid.AddChild(sigmaSlider);

            Label bodyPartLimitLabel = new Label(bodyPartLimitLabelRef);
            bodyPartLimitLabel.Alignment = Align.MiddleLeft;
            bodyPartLimitLabel.Tooltip = "Infusion.Settings.BodyPartLimit.Description".Translate();

            grid.AddChild(bodyPartLimitLabel);

            Checkbox bodyPartLimitCheckbox = new Checkbox(Settings.bodyPartHandle);
            bodyPartLimitCheckbox.Alignment = Align.MiddleLeft;

            grid.AddChild(bodyPartLimitCheckbox);

            Label morePerLayerLabel = new Label(morePerLayerLabelRef);
            morePerLayerLabel.Alignment = Align.MiddleLeft;
            morePerLayerLabel.Tooltip = "Infusion.Settings.LayerBonuses.Description".Translate();

            grid.AddChild(morePerLayerLabel);

            Checkbox morePerLayerCheckbox = new Checkbox(Settings.layerHandle);
            morePerLayerCheckbox.Alignment = Align.MiddleLeft;

            grid.AddChild(morePerLayerCheckbox);

            Label statsGlobalMultiplierLabel = new Label(statsGlobalMultiplierLabelRef);
            statsGlobalMultiplierLabel.Alignment = Align.MiddleLeft;
            statsGlobalMultiplierLabel.Tooltip = "Infusion.Settings.StatsGlobalMultiplier.Description".Translate();

            grid.AddChild(statsGlobalMultiplierLabel);

            LabeledSlider statsGlobalMultiplierSlider = new LabeledSlider(Settings.statsGlobalMultiplier.Value.ToString(), Settings.statsGlobalMultiplier.Value, 0.1f, 5f, (val) => Settings.statsGlobalMultiplier.Value = val);
            statsGlobalMultiplierSlider.RoundTo = 0.1f;
            statsGlobalMultiplierSlider.Alignment = Align.MiddleLeft;
            statsGlobalMultiplierSlider.WidthMode = SizeMode.Fill;

            grid.AddChild(statsGlobalMultiplierSlider);

            Label chanceGlobalMultiplierLabel = new Label(chanceGlobalMultiplierLabelRef);
            chanceGlobalMultiplierLabel.Alignment = Align.MiddleLeft;
            chanceGlobalMultiplierLabel.Tooltip = "Infusion.Settings.ChanceGlobalMultiplier.Description".Translate();

            grid.AddChild(chanceGlobalMultiplierLabel);

            LabeledSlider chanceGlobalMultiplierSlider = new LabeledSlider(Settings.chanceGlobalMultiplier.Value.ToString(), Settings.chanceGlobalMultiplier.Value, 0.1f, 5f, (val) => Settings.chanceGlobalMultiplier.Value = val);
            chanceGlobalMultiplierSlider.RoundTo = 0.1f;
            chanceGlobalMultiplierSlider.Alignment = Align.MiddleLeft;
            chanceGlobalMultiplierSlider.WidthMode = SizeMode.Fill;

            grid.AddChild(chanceGlobalMultiplierSlider);

            Label amountGlobalMultiplierLabel = new Label(amountGlobalMultiplierLabelRef);
            amountGlobalMultiplierLabel.Alignment = Align.MiddleLeft;
            amountGlobalMultiplierLabel.Tooltip = "Infusion.Settings.AmountGlobalMultiplier.Description".Translate();

            grid.AddChild(amountGlobalMultiplierLabel);

            LabeledSlider amountGlobalMultiplierSlider = new LabeledSlider(Settings.amountGlobalMultiplier.Value.ToString(), Settings.amountGlobalMultiplier.Value, 0.1f, 5f, (val) => Settings.amountGlobalMultiplier.Value = val);
            amountGlobalMultiplierSlider.RoundTo = 0.1f;
            amountGlobalMultiplierSlider.Alignment = Align.MiddleLeft;
            amountGlobalMultiplierSlider.WidthMode = SizeMode.Fill;

            grid.AddChild(amountGlobalMultiplierSlider);

            stack.AddChild(grid);

            Line line = new Line(LineType.Horizontal);
            line.WidthMode = SizeMode.Fill;
            stack.AddChild(line);

            FillGrid grid1 = new FillGrid(2, 11)
            {
                Padding = 10f
            };
            grid1.HeightMode = SizeMode.Content;

            Label infusionSettingsLabel = new Label(infusionSettingsLabelRef);
            infusionSettingsLabel.Alignment = Align.MiddleLeft;

            grid1.AddChild(infusionSettingsLabel);
            grid1.AddChild(new Empty());

            Label infusionSlotSettingsLabel = new Label(infusionSlotsSettingsLabelRef);
            infusionSlotSettingsLabel.Alignment = Align.MiddleLeft;

            grid1.AddChild(infusionSlotSettingsLabel);
            grid1.AddChild(new Empty());

            Label awfulSlotsLabel = new Label(awfulLabelRef);
            awfulSlotsLabel.Alignment = Align.MiddleLeft;

            grid1.AddChild(awfulSlotsLabel);

            LabeledSlider awfulSlotsSlider = new LabeledSlider(Settings.slotAwful.Value.ToString(), Settings.slotAwful.Value, 0f, 20f, (val) => Settings.slotAwful.Value = (int)val);
            awfulSlotsSlider.RoundTo = 1f;
            awfulSlotsSlider.Alignment = Align.MiddleLeft;
            awfulSlotsSlider.WidthMode = SizeMode.Fill;

            grid1.AddChild(awfulSlotsSlider);

            Label poorSlotsLabel = new Label(poorLabelRef);
            poorSlotsLabel.Alignment = Align.MiddleLeft;

            grid1.AddChild(poorSlotsLabel);

            LabeledSlider poorSlotsSlider = new LabeledSlider(Settings.slotPoor.Value.ToString(), Settings.slotPoor.Value, 0f, 20f, (val) => Settings.slotPoor.Value = (int)val);
            poorSlotsSlider.RoundTo = 1f;
            poorSlotsSlider.Alignment = Align.MiddleLeft;
            poorSlotsSlider.WidthMode = SizeMode.Fill;

            grid1.AddChild(poorSlotsSlider);

            Label normalSlotsLabel = new Label(normalLabelRef);
            normalSlotsLabel.Alignment = Align.MiddleLeft;

            grid1.AddChild(normalSlotsLabel);

            LabeledSlider normalSlotsSlider = new LabeledSlider(Settings.slotNormal.Value.ToString(), Settings.slotNormal.Value, 0f, 20f, (val) => Settings.slotNormal.Value = (int)val);
            normalSlotsSlider.RoundTo = 1f;
            normalSlotsSlider.Alignment = Align.MiddleLeft;
            normalSlotsSlider.WidthMode = SizeMode.Fill;

            grid1.AddChild(normalSlotsSlider);

            Label goodSlotsLabel = new Label(goodLabelRef);
            goodSlotsLabel.Alignment = Align.MiddleLeft;

            grid1.AddChild(goodSlotsLabel);

            LabeledSlider goodSlotsSlider = new LabeledSlider(Settings.slotGood.Value.ToString(), Settings.slotGood.Value, 0f, 20f, (val) => Settings.slotGood.Value = (int)val);
            goodSlotsSlider.RoundTo = 1f;
            goodSlotsSlider.Alignment = Align.MiddleLeft;
            goodSlotsSlider.WidthMode = SizeMode.Fill;

            grid1.AddChild(goodSlotsSlider);

            Label excellentSlotsLabel = new Label(excellentLabelRef);
            excellentSlotsLabel.Alignment = Align.MiddleLeft;

            grid1.AddChild(excellentSlotsLabel);

            LabeledSlider excellentSlotsSlider = new LabeledSlider(Settings.slotExcellent.Value.ToString(), Settings.slotExcellent.Value, 0f, 20f, (val) => Settings.slotExcellent.Value = (int)val);
            excellentSlotsSlider.RoundTo = 1f;
            excellentSlotsSlider.Alignment = Align.MiddleLeft;
            excellentSlotsSlider.WidthMode = SizeMode.Fill;

            grid1.AddChild(excellentSlotsSlider);

            Label masterworkSlotsLabel = new Label(masterworkLabelRef);
            masterworkSlotsLabel.Alignment = Align.MiddleLeft;

            grid1.AddChild(masterworkSlotsLabel);

            LabeledSlider masterworkSlotsSlider = new LabeledSlider(Settings.slotMasterwork.Value.ToString(), Settings.slotMasterwork.Value, 0f, 20f, (val) => Settings.slotMasterwork.Value = (int)val);
            masterworkSlotsSlider.RoundTo = 1f;
            masterworkSlotsSlider.Alignment = Align.MiddleLeft;
            masterworkSlotsSlider.WidthMode = SizeMode.Fill;

            grid1.AddChild(masterworkSlotsSlider);

            Label legendarySlotsLabel = new Label(legendaryLabelRef);
            legendarySlotsLabel.Alignment = Align.MiddleLeft;

            grid1.AddChild(legendarySlotsLabel);

            LabeledSlider legendarySlotsSlider = new LabeledSlider(Settings.slotLegendary.Value.ToString(), Settings.slotLegendary.Value, 0f, 20f, (val) => Settings.slotLegendary.Value = (int)val);
            legendarySlotsSlider.RoundTo = 1f;
            legendarySlotsSlider.Alignment = Align.MiddleLeft;
            legendarySlotsSlider.WidthMode = SizeMode.Fill;

            grid1.AddChild(legendarySlotsSlider);

            Label infusionDefsControlLabel = new Label(infusionDefsControlLabelRef);
            infusionDefsControlLabel.Alignment = Align.MiddleLeft;

            grid1.AddChild(infusionDefsControlLabel);
            grid1.AddChild(new Empty());

            Dropdown<InfusionDef> infusionDefsDropdown = new Dropdown<InfusionDef>(ResourceBank.allInfusionDefs, selectedInfusionDef, (def) => def.defName);
            infusionDefsDropdown.Alignment = Align.MiddleLeft;

            grid1.AddChild(infusionDefsDropdown);

            Button infusionDefsEnableDisableButton;
            if (Settings.infusionDefsDisabledMap.ContainsKey(selectedInfusionDef.Value) && Settings.infusionDefsDisabledMap[selectedInfusionDef.Value].Value)
            {
                infusionDefsEnableDisableButton = new Button("Infusion.Settings.Infusions.Defs.EnableButton".Translate(), infusionDefsEnableDisableButtonClicked);
            }
            else
            {
                infusionDefsEnableDisableButton = new Button("Infusion.Settings.Infusions.Defs.DisableButton".Translate(), infusionDefsEnableDisableButtonClicked);
            }

            if (infusionDefsEnableDisableButtonClicked.Value)
            {
                if (Settings.infusionDefsDisabledMap.ContainsKey(selectedInfusionDef.Value) && Settings.infusionDefsDisabledMap[selectedInfusionDef.Value].Value)
                {
                    Settings.infusionDefsDisabledMap.Remove(selectedInfusionDef.Value);
                    Messages.Message("Infusion.Settings.Infusions.Defs.EnableMessage".Translate(selectedInfusionDef.Value.defName), MessageTypeDefOf.NeutralEvent);
                }
                else
                {
                    Settings.infusionDefsDisabledMap[selectedInfusionDef.Value] = new StrongBox<bool>(true);
                    Messages.Message("Infusion.Settings.Infusions.Defs.DisableMessage".Translate(selectedInfusionDef.Value.defName), MessageTypeDefOf.NeutralEvent);
                }
            }

            grid1.AddChild(infusionDefsEnableDisableButton);

            stack.AddChild(grid1);

            Line line1 = new Line(LineType.Horizontal);
            line1.WidthMode = SizeMode.Fill;
            stack.AddChild(line1);

            FillGrid grid2 = new FillGrid(2, 5)
            {
                Padding = 10f
            };
            grid2.HeightMode = SizeMode.Content;

            Label tiersTitleLabel = new Label(tiersTitleLabelRef);
            tiersTitleLabel.Alignment = Align.MiddleLeft;

            grid2.AddChild(tiersTitleLabel);
            grid2.AddChild(new Empty());

            Label commonTierLabel = new Label(commonLabelRef);
            commonTierLabel.Alignment = Align.MiddleLeft;

            grid2.AddChild(commonTierLabel);

            Checkbox commonTierCheckbox = new Checkbox(Settings.commonTierEnabled);
            commonTierCheckbox.Alignment = Align.MiddleLeft;

            grid2.AddChild(commonTierCheckbox);

            Label uncommonTierLabel = new Label(uncommonLabelRef);
            uncommonTierLabel.Alignment = Align.MiddleLeft;

            grid2.AddChild(uncommonTierLabel);

            Checkbox uncommonTierCheckbox = new Checkbox(Settings.uncommonTierEnabled);
            uncommonTierCheckbox.Alignment = Align.MiddleLeft;

            grid2.AddChild(uncommonTierCheckbox);

            Label rareTierLabel = new Label(rareLabelRef);
            rareTierLabel.Alignment = Align.MiddleLeft;

            grid2.AddChild(rareTierLabel);

            Checkbox rareTierCheckbox = new Checkbox(Settings.rareTierEnabled);
            rareTierCheckbox.Alignment = Align.MiddleLeft;

            grid2.AddChild(rareTierCheckbox);

            Label legendaryTierLabel = new Label(legendaryLabelRef);
            legendaryTierLabel.Alignment = Align.MiddleLeft;

            grid2.AddChild(legendaryTierLabel);

            Checkbox legendaryTierCheckbox = new Checkbox(Settings.legendaryTierEnabled);
            legendaryTierCheckbox.Alignment = Align.MiddleLeft;

            grid2.AddChild(legendaryTierCheckbox);

            stack.AddChild(grid2);

            canvas.AddChild(stack);

            canvas.Render();
        }
    }
}
