using LessUI;
using RimWorld;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;

namespace Infusion
{
    public class ModSettingsWindow
    {
        public static StrongBox<Vector2> scrollPosition = new StrongBox<Vector2>(Vector2.zero);
        public static StrongBox<InfusionDef> selectedInfusionDef = new StrongBox<InfusionDef>(ResourceBank.allInfusionDefs.First());
        public static StrongBox<TierDef> selectedTierDef = new StrongBox<TierDef>(ResourceBank.allTierDefs.First());
        public static StrongBox<Color> selectedColor = new StrongBox<Color>(Settings.tierColorOverride.ContainsKey(ResourceBank.allTierDefs.First()) ? Settings.tierColorOverride[ResourceBank.allTierDefs.First()] : ResourceBank.allTierDefs.First().color);
        public static TierDef previousSelectedTierDef = selectedTierDef.Value;
        public static void Draw(Rect parent)
        {
            ScrollCanvas canvas = new ScrollCanvas(rect: parent, scrollPositionBox: scrollPosition);
            Stack stack = new Stack(widthMode: SizeMode.Fill, verticalSpacing: 10f, heightMode: SizeMode.Content); 

            int gridRows = 15;
            if (ModsConfig.OdysseyActive)
            {
                gridRows++;
            }
            Grid grid = new Grid(2, gridRows, widthMode: SizeMode.Fill, heightMode: SizeMode.Content, padding: 5f);
            Text.Font = GameFont.Small;

            Label restartGameInfoLabel = new Label(text: "Infusion.Settings.Label.RestartGameInfo".Translate(), alignment: Align.MiddleLeft);
            Label generalSettingsTitleLabel = new Label("Infusion.Settings.General.Title".Translate(), alignment: Align.MiddleLeft);
            Label accuracyOvercappingLabel = new Label("Infusion.Settings.AccuracyOvercapping".Translate(), alignment: Align.MiddleLeft, tooltip: "Infusion.Settings.AccuracyOvercapping.Description".Translate());
            Label bonusToBiocodeLabel = new Label("Infusion.Settings.BiocodeBonus".Translate(), alignment: Align.MiddleLeft, tooltip: "Infusion.Settings.BiocodeBonus.Description".Translate());
            Label infusionsFromCraftingLabel = new Label("Infusion.Settings.InfusionsFromCrafting".Translate(), alignment: Align.MiddleLeft, tooltip: "Infusion.Settings.InfusionsFromCrafting.Description".Translate());
            Label infuseUniqueWeaponsLabel = new Label("Infusion.Settings.InfuseUniqueWeapons".Translate(), alignment: Align.MiddleLeft);
            Label extractionSuccessFactorLabel = new Label("Infusion.Settings.ExtractionChanceFactor".Translate(), alignment: Align.MiddleLeft, tooltip: "Infusion.Settings.ExtractionChanceFactor.Description".Translate());
            Label reusableInfusersLabel = new Label("Infusion.Settings.ReusableInfusers".Translate(), alignment: Align.MiddleLeft, tooltip: "Infusion.Settings.ReusableInfusers.Description".Translate());
            Label infusionChanceLabel = new Label("Infusion.Settings.ChanceFactor".Translate(), alignment: Align.MiddleLeft, tooltip: "Infusion.Settings.ChanceFactor.Description".Translate());
            Label muLabel = new Label("Infusion.Settings.Mu".Translate(), alignment: Align.MiddleLeft, tooltip: "Infusion.Settings.Mu.Description".Translate());
            Label sigmaLabel = new Label("Infusion.Settings.Sigma".Translate(), alignment: Align.MiddleLeft, tooltip: "Infusion.Settings.Sigma.Description".Translate());
            Label bodyPartLimitLabel = new Label("Infusion.Settings.BodyPartLimit".Translate(), alignment: Align.MiddleLeft, tooltip: "Infusion.Settings.BodyPartLimit.Description".Translate());
            Label morePerLayerLabel = new Label("Infusion.Settings.LayerBonuses".Translate(), alignment: Align.MiddleLeft, tooltip: "Infusion.Settings.LayerBonuses.Description".Translate());
            Label statsGlobalMultiplierLabel = new Label("Infusion.Settings.StatsGlobalMultiplier".Translate(), alignment: Align.MiddleLeft, tooltip: "Infusion.Settings.StatsGlobalMultiplier.Description".Translate());
            Label chanceGlobalMultiplierLabel = new Label("Infusion.Settings.ChanceGlobalMultiplier".Translate(), alignment: Align.MiddleLeft, tooltip: "Infusion.Settings.ChanceGlobalMultiplier.Description".Translate());
            Label amountGlobalMultiplierLabel = new Label("Infusion.Settings.AmountGlobalMultiplier".Translate(), alignment: Align.MiddleLeft, tooltip: "Infusion.Settings.AmountGlobalMultiplier.Description".Translate());

            Checkbox accuracyOvercappingCheckbox = new Checkbox(isChecked: Settings.accuracyOvercap, alignment: Align.MiddleLeft);
            Checkbox bonusToBiocodeCheckbox = new Checkbox(isChecked: Settings.biocodeBonus, alignment: Align.MiddleLeft);
            Checkbox infusionsFromCraftingCheckbox = new Checkbox(isChecked: Settings.infusionsFromCrafting, alignment: Align.MiddleLeft);
            Checkbox infuseUniqueWeaponsCheckbox = new Checkbox(isChecked: Settings.infuseUniqueWeapons, alignment: Align.MiddleLeft);
            Checkbox reusableInfusersCheckbox = new Checkbox(isChecked: Settings.reusableInfusers, alignment: Align.MiddleLeft);
            Checkbox bodyPartLimitCheckbox = new Checkbox(isChecked: Settings.bodyPartHandle, alignment: Align.MiddleLeft);
            Checkbox morePerLayerCheckbox = new Checkbox(isChecked: Settings.layerHandle, alignment: Align.MiddleLeft);

            Slider extractionSuccessFactorSlider = new Slider(label: $"{Settings.extractionChanceFactor.Value.ToString()}x", value: Settings.extractionChanceFactor, min: 1f, max: 100f, roundTo: 0.5f, alignment: Align.UpperLeft, widthMode: SizeMode.Fill);
            Slider infusionChanceSlider = new Slider(label: Settings.chanceHandle.Value.ToString(), value: Settings.chanceHandle, min: 0f, max: 10f, roundTo: 0.1f, alignment: Align.UpperLeft, widthMode: SizeMode.Fill);
            Slider muSlider = new Slider(label: Settings.muHandle.Value.ToString(), value: Settings.muHandle, min: 0.5f, max: 10f, roundTo: 0.1f, alignment: Align.UpperLeft, widthMode: SizeMode.Fill);
            Slider sigmaSlider = new Slider(label: Settings.sigmaHandle.Value.ToString(), value: Settings.sigmaHandle, min: 0.5f, max: 10f, roundTo: 0.1f, alignment: Align.UpperLeft, widthMode: SizeMode.Fill);
            Slider statsGlobalMultiplierSlider = new Slider(label: Settings.statsGlobalMultiplier.Value.ToString(), value: Settings.statsGlobalMultiplier, min: 0.1f, max: 5f, roundTo: 0.1f, alignment: Align.UpperLeft, widthMode: SizeMode.Fill);
            Slider chanceGlobalMultiplierSlider = new Slider(label: Settings.chanceGlobalMultiplier.Value.ToString(), value: Settings.chanceGlobalMultiplier, min: 0.1f, max: 5f, roundTo: 0.1f, alignment: Align.UpperLeft, widthMode: SizeMode.Fill);
            Slider amountGlobalMultiplierSlider = new Slider(label: Settings.amountGlobalMultiplier.Value.ToString(), value: Settings.amountGlobalMultiplier, min: 0.1f, max: 5f, roundTo: 0.1f, alignment: Align.UpperLeft, widthMode: SizeMode.Fill);

            grid.AddChild(restartGameInfoLabel)
            .AddChild(new Empty())
            .AddChild(generalSettingsTitleLabel)
            .AddChild(new Empty())
            .AddChild(accuracyOvercappingLabel)
            .AddChild(accuracyOvercappingCheckbox)
            .AddChild(bonusToBiocodeLabel)
            .AddChild(bonusToBiocodeCheckbox)
            .AddChild(infusionsFromCraftingLabel)
            .AddChild(infusionsFromCraftingCheckbox);

            if (ModsConfig.OdysseyActive)
            {
                grid.AddChild(infuseUniqueWeaponsLabel)
                .AddChild(infuseUniqueWeaponsCheckbox);
            }


            grid.AddChild(extractionSuccessFactorLabel)
            .AddChild(extractionSuccessFactorSlider)
            .AddChild(reusableInfusersLabel)
            .AddChild(reusableInfusersCheckbox)
            .AddChild(infusionChanceLabel)
            .AddChild(infusionChanceSlider)
            .AddChild(muLabel)
            .AddChild(muSlider)
            .AddChild(sigmaLabel)
            .AddChild(sigmaSlider)
            .AddChild(bodyPartLimitLabel)
            .AddChild(bodyPartLimitCheckbox)
            .AddChild(morePerLayerLabel)
            .AddChild(morePerLayerCheckbox)
            .AddChild(statsGlobalMultiplierLabel)
            .AddChild(statsGlobalMultiplierSlider)
            .AddChild(chanceGlobalMultiplierLabel)
            .AddChild(chanceGlobalMultiplierSlider)
            .AddChild(amountGlobalMultiplierLabel)
            .AddChild(amountGlobalMultiplierSlider);


            Grid grid1 = new Grid(2, 11, widthMode: SizeMode.Fill, padding: 5f, heightMode: SizeMode.Content);

            Label infusionSettingsLabel = new Label("Infusion.Settings.Infusions.Title".Translate(), alignment: Align.MiddleLeft);
            Label infusionSlotSettingsLabel = new Label("Infusion.Settings.Infusions.Slots.Title".Translate(), alignment: Align.MiddleLeft);
            Label awfulSlotsLabel = new Label("Awful", alignment: Align.MiddleLeft);
            Label poorSlotsLabel = new Label("Poor", alignment: Align.MiddleLeft);
            Label normalSlotsLabel = new Label("Normal", alignment: Align.MiddleLeft);
            Label goodSlotsLabel = new Label("Good", alignment: Align.MiddleLeft);
            Label excellentSlotsLabel = new Label("Excellent", alignment: Align.MiddleLeft);
            Label masterworkSlotsLabel = new Label("Masterwork", alignment: Align.MiddleLeft);
            Label legendarySlotsLabel = new Label("Legendary", alignment: Align.MiddleLeft);
            Label infusionDefsControlLabel = new Label("Infusion.Settings.Infusions.Defs.Title".Translate(), alignment: Align.MiddleLeft);

            SliderInt awfulSlotsSlider = new SliderInt(label: Settings.slotAwful.Value.ToString(), value: Settings.slotAwful, min: 0, max: 20, alignment: Align.UpperLeft, widthMode: SizeMode.Fill);
            SliderInt poorSlotsSlider = new SliderInt(label: Settings.slotPoor.Value.ToString(), value: Settings.slotPoor, min: 0, max: 20, alignment: Align.UpperLeft, widthMode: SizeMode.Fill);
            SliderInt normalSlotsSlider = new SliderInt(label: Settings.slotNormal.Value.ToString(), value: Settings.slotNormal, min: 0, alignment: Align.UpperLeft, widthMode: SizeMode.Fill);
            SliderInt goodSlotsSlider = new SliderInt(label: Settings.slotGood.Value.ToString(), value: Settings.slotGood, min: 0, max: 20, alignment: Align.UpperLeft, widthMode: SizeMode.Fill);
            SliderInt excellentSlotsSlider = new SliderInt(label: Settings.slotExcellent.Value.ToString(), value: Settings.slotExcellent, min: 0, max: 20, alignment: Align.UpperLeft, widthMode: SizeMode.Fill);
            SliderInt masterworkSlotsSlider = new SliderInt(label: Settings.slotMasterwork.Value.ToString(), value: Settings.slotMasterwork, min: 0, max: 20, alignment: Align.UpperLeft, widthMode: SizeMode.Fill);
            SliderInt legendarySlotsSlider = new SliderInt(label: Settings.slotLegendary.Value.ToString(), value: Settings.slotLegendary, min: 0, max: 20, alignment: Align.UpperLeft, widthMode: SizeMode.Fill);

            Dropdown<InfusionDef> infusionDefsDropdown = new Dropdown<InfusionDef>(ResourceBank.allInfusionDefs, selectedInfusionDef, (def) => def.defName, alignment: Align.MiddleLeft);

            Button infusionDefsEnableDisableButton;
            if (Settings.infusionDefsDisabledMap.ContainsKey(selectedInfusionDef.Value) && Settings.infusionDefsDisabledMap[selectedInfusionDef.Value].Value)
            {
                infusionDefsEnableDisableButton = new Button("Infusion.Settings.Infusions.Defs.EnableButton".Translate(), alignment: Align.MiddleLeft);
            }
            else
            {
                infusionDefsEnableDisableButton = new Button("Infusion.Settings.Infusions.Defs.DisableButton".Translate(), alignment: Align.MiddleLeft);
            }

            grid1.AddChild(infusionSettingsLabel)
            .AddChild(new Empty())
            .AddChild(infusionSlotSettingsLabel)
            .AddChild(new Empty())
            .AddChild(awfulSlotsLabel)
            .AddChild(awfulSlotsSlider)
            .AddChild(poorSlotsLabel)
            .AddChild(poorSlotsSlider)
            .AddChild(normalSlotsLabel)
            .AddChild(normalSlotsSlider)
            .AddChild(goodSlotsLabel)
            .AddChild(goodSlotsSlider)
            .AddChild(excellentSlotsLabel)
            .AddChild(excellentSlotsSlider)
            .AddChild(masterworkSlotsLabel)
            .AddChild(masterworkSlotsSlider)
            .AddChild(legendarySlotsLabel)
            .AddChild(legendarySlotsSlider)
            .AddChild(infusionDefsControlLabel)
            .AddChild(new Empty())
            .AddChild(infusionDefsDropdown)
            .AddChild(infusionDefsEnableDisableButton);

            Grid grid2 = new Grid(3, 7, widthMode: SizeMode.Fill, padding: 5f, heightMode: SizeMode.Content);

            Label tiersTitleLabel = new Label("Infusion.Settings.Tiers.Title".Translate(), alignment: Align.MiddleLeft);
            Label commonTierLabel = new Label("Common", alignment: Align.MiddleLeft);
            Label uncommonTierLabel = new Label("Uncommon", alignment: Align.MiddleLeft);
            Label rareTierLabel = new Label("Rare", alignment: Align.MiddleLeft);
            Label legendaryTierLabel = new Label("Legendary", alignment: Align.MiddleLeft);
            Label tierColorLabel = new Label("Infusion.Settings.Tiers.Color.Title".Translate(), alignment: Align.MiddleLeft);

            Checkbox commonTierCheckbox = new Checkbox(isChecked: Settings.commonTierEnabled, alignment: Align.MiddleLeft);
            Checkbox uncommonTierCheckbox = new Checkbox(isChecked: Settings.uncommonTierEnabled, alignment: Align.MiddleLeft);
            Checkbox rareTierCheckbox = new Checkbox(isChecked: Settings.rareTierEnabled, alignment: Align.MiddleLeft);
            Checkbox legendaryTierCheckbox = new Checkbox(isChecked: Settings.legendaryTierEnabled, alignment: Align.MiddleLeft);

            Dropdown<TierDef> tierDefsDropdown = new Dropdown<TierDef>(ResourceBank.allTierDefs, selectedTierDef, (def) => def.defName, alignment: Align.MiddleLeft);

            ColorPicker picker = new ColorPicker(selectedColor.Value, selectedColor, alignment: Align.MiddleLeft);

            Button saveColorButton = new Button("Infusion.Settings.Tiers.SaveColorButton.Label".Translate(), alignment: Align.MiddleLeft);

            grid2.AddChild(tiersTitleLabel)
            .AddChild(new Empty())
            .AddChild(new Empty())
            .AddChild(commonTierLabel)
            .AddChild(commonTierCheckbox)
            .AddChild(new Empty())
            .AddChild(uncommonTierLabel)
            .AddChild(uncommonTierCheckbox)
            .AddChild(new Empty())
            .AddChild(rareTierLabel)
            .AddChild(rareTierCheckbox)
            .AddChild(new Empty())
            .AddChild(legendaryTierLabel)
            .AddChild(legendaryTierCheckbox)
            .AddChild(new Empty())
            .AddChild(tierColorLabel)
            .AddChild(new Empty())
            .AddChild(new Empty())
            .AddChild(tierDefsDropdown)
            .AddChild(picker)
            .AddChild(saveColorButton);

            Line line = new Line(LineType.Horizontal, widthMode: SizeMode.Fill);
            Line line1 = new Line(LineType.Horizontal, widthMode: SizeMode.Fill);

            stack.AddChild(grid)
            .AddChild(line)
            .AddChild(grid1)
            .AddChild(line1)
            .AddChild(grid2)
            .AddChild(new Empty());

            canvas.AddChild(stack);

            canvas.Render();

            if (infusionDefsEnableDisableButton.Clicked)
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

            if (tierDefsDropdown.SelectedItem != previousSelectedTierDef)
            {
                selectedColor.Value = Settings.tierColorOverride.ContainsKey(tierDefsDropdown.SelectedItem) ? Settings.tierColorOverride[tierDefsDropdown.SelectedItem] : tierDefsDropdown.SelectedItem.color;
                previousSelectedTierDef = tierDefsDropdown.SelectedItem;
            }

            if (saveColorButton.Clicked)
            {
                if (selectedColor.Value == selectedTierDef.Value.color)
                {
                    if (Settings.tierColorOverride.ContainsKey(selectedTierDef.Value))
                    {
                        Settings.tierColorOverride.Remove(selectedTierDef.Value);
                        Messages.Message("Infusion.Settings.Tiers.ColorResetMessage".Translate(selectedTierDef.Value.defName), MessageTypeDefOf.NeutralEvent);
                    }
                    else
                    {
                        Messages.Message("Infusion.Settings.Tiers.ColorAlreadyDefaultMessage".Translate(selectedTierDef.Value.defName), MessageTypeDefOf.NeutralEvent);
                    }
                }
                else
                {
                    Settings.tierColorOverride[selectedTierDef.Value] = selectedColor.Value;
                    Messages.Message("Infusion.Settings.Tiers.ColorSavedMessage".Translate(selectedTierDef.Value.defName), MessageTypeDefOf.NeutralEvent);
                }
            }
        }
    }
}
