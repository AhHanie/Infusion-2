using LessUI;
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
        public static void Draw(Rect parent)
        {
            Canvas canvas = new Canvas(parent);
            FillGrid grid = new FillGrid(2, 20)
            {
                Padding = 10f
            };
            grid.HeightMode = SizeMode.Fixed;
            grid.Height = 25f;
            Text.Font = GameFont.Small;

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

            LabeledSlider extractionSuccessFactorSlider = new LabeledSlider(Settings.extractionChanceFactor.Value.ToStringPercent(), Settings.extractionChanceFactor.Value, 0.01f, 1f, (val) => Settings.extractionChanceFactor.Value = val);
            extractionSuccessFactorSlider.RoundTo = 0.01f;
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

            canvas.AddChild(grid);
            canvas.Render();
        }
    }
}
