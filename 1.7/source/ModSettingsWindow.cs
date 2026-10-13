using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;

namespace Infusion
{
    public class ModSettingsWindow
    {
        private const float RowHeight = 30f;
        private const float LabelPct = 0.5f;
        private const float ValuePct = 0.12f;
        private const float ColorWheelSize = 160f;
        private const float ScrollbarWidth = 16f;

        // UI-only state; never saved. Settings live in Settings.* and keep their Scribe keys.
        private static Vector2 scrollPosition = Vector2.zero;
        private static float viewHeight = 1600f;
        private static bool initialized;
        private static List<TierDef> tiers;
        private static List<InfusionDef> infusionDefs;
        private static InfusionDef selectedInfusionDef;
        private static TierDef selectedTierDef;
        private static Color selectedColor;
        private static bool colorWheelDragging;

        private static string GetInfusionBaseName(string defName)
        {
            string[] tierPrefixes = { "Legendary_", "Rare_", "Uncommon_", "Common_" };
            foreach (string prefix in tierPrefixes)
            {
                if (defName.StartsWith(prefix))
                {
                    return defName.Substring(prefix.Length);
                }
            }
            return defName;
        }

        // Every tier variant of the selected infusion (Common_X, Rare_X, ...).
        private static List<InfusionDef> GetAllTiersOfInfusion(InfusionDef infusionDef)
        {
            string baseName = GetInfusionBaseName(infusionDef.defName);
            return infusionDefs.Where(def => GetInfusionBaseName(def.defName) == baseName).ToList();
        }

        private static bool IsInfusionDisabled(InfusionDef def)
        {
            return Settings.infusionDefsDisabledMap.TryGetValue(def, out StrongBox<bool> box) && box.Value;
        }

        // Defs are not available when this class loads, so the selection is built on first draw.
        private static void EnsureInitialized()
        {
            if (initialized)
            {
                return;
            }
            initialized = true;

            tiers = ResourceBank.allTierDefs.Where(tier => tier != null).ToList();
            infusionDefs = ResourceBank.allInfusionDefs
                .Where(def => def != null && !InfusionDef.ShouldRemoveItself(def))
                .OrderBy(def => def.defName)
                .ToList();

            selectedInfusionDef = infusionDefs.FirstOrDefault();
            SelectTier(tiers.FirstOrDefault());
        }

        private static void SelectTier(TierDef tier)
        {
            selectedTierDef = tier;
            colorWheelDragging = false;
            if (tier != null)
            {
                selectedColor = Settings.tierColorOverride.TryGetValue(tier, out Color overrideColor) ? overrideColor : tier.color;
            }
        }

        public static void Draw(Rect parent)
        {
            EnsureInitialized();
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;

            // viewHeight is measured from the previous frame's content, so the whole list stays reachable
            // at any UI scale and when optional sections (Odyssey, multi-tier families) appear.
            Rect viewRect = new Rect(0f, 0f, parent.width - ScrollbarWidth, viewHeight);
            Widgets.BeginScrollView(parent, ref scrollPosition, viewRect);

            // One column only: otherwise content taller than the guessed view height wraps sideways
            // and the measured height collapses, hiding everything below the wrap.
            Listing_Standard listing = new Listing_Standard { maxOneColumn = true };
            listing.Begin(viewRect);

            DrawGeneral(listing);
            listing.GapLine();
            DrawSlots(listing);
            listing.GapLine();
            DrawTiers(listing);

            if (Prefs.DevMode)
            {
                listing.GapLine();
                DrawDebug(listing);
            }

            viewHeight = listing.CurHeight + 10f;
            listing.End();
            Widgets.EndScrollView();
        }

        private static void DrawDebug(Listing_Standard listing)
        {
            listing.Label("Infusion.Settings.Debug.Title".Translate());
            CheckboxRow(listing, "Infusion.Settings.Debug.ForceChances", "Infusion.Settings.Debug.ForceChances.Description", Settings.debugForceChances);
        }

        private static void DrawGeneral(Listing_Standard listing)
        {
            listing.Label("Infusion.Settings.Label.RestartGameInfo".Translate());
            listing.Label("Infusion.Settings.General.Title".Translate());

            CheckboxRow(listing, "Infusion.Settings.Label.DisableItemInfusion", "Infusion.Settings.Tooltip.DisableItemInfusion", Settings.disableItemInfusion);
            CheckboxRow(listing, "Infusion.Settings.AccuracyOvercapping", "Infusion.Settings.AccuracyOvercapping.Description", Settings.accuracyOvercap);
            CheckboxRow(listing, "Infusion.Settings.BiocodeBonus", "Infusion.Settings.BiocodeBonus.Description", Settings.biocodeBonus);
            CheckboxRow(listing, "Infusion.Settings.InfusionsFromCrafting", "Infusion.Settings.InfusionsFromCrafting.Description", Settings.infusionsFromCrafting);

            if (ModsConfig.OdysseyActive)
            {
                CheckboxRow(listing, "Infusion.Settings.InfuseUniqueWeapons", null, Settings.infuseUniqueWeapons);
            }

            SliderRow(listing, "Infusion.Settings.ExtractionChanceFactor", "Infusion.Settings.ExtractionChanceFactor.Description", Settings.extractionChanceFactor, 1f, 100f, 0.5f, "x");
            CheckboxRow(listing, "Infusion.Settings.ReusableInfusers", "Infusion.Settings.ReusableInfusers.Description", Settings.reusableInfusers);
            SliderRow(listing, "Infusion.Settings.ChanceFactor", "Infusion.Settings.ChanceFactor.Description", Settings.chanceHandle, 0f, 10f, 0.1f);
            SliderRow(listing, "Infusion.Settings.Mu", "Infusion.Settings.Mu.Description", Settings.muHandle, 0.5f, 10f, 0.1f);
            SliderRow(listing, "Infusion.Settings.Sigma", "Infusion.Settings.Sigma.Description", Settings.sigmaHandle, 0.5f, 10f, 0.1f);
            CheckboxRow(listing, "Infusion.Settings.BodyPartLimit", "Infusion.Settings.BodyPartLimit.Description", Settings.bodyPartHandle);
            CheckboxRow(listing, "Infusion.Settings.LayerBonuses", "Infusion.Settings.LayerBonuses.Description", Settings.layerHandle);
            SliderRow(listing, "Infusion.Settings.StatsGlobalMultiplier", "Infusion.Settings.StatsGlobalMultiplier.Description", Settings.statsGlobalMultiplier, 0.1f, 5f, 0.1f);
            SliderRow(listing, "Infusion.Settings.ChanceGlobalMultiplier", "Infusion.Settings.ChanceGlobalMultiplier.Description", Settings.chanceGlobalMultiplier, 0.1f, 5f, 0.1f);
            SliderRow(listing, "Infusion.Settings.AmountGlobalMultiplier", "Infusion.Settings.AmountGlobalMultiplier.Description", Settings.amountGlobalMultiplier, 0.1f, 5f, 0.1f);
        }

        private static void DrawSlots(Listing_Standard listing)
        {
            listing.Label("Infusion.Settings.Infusions.Title".Translate());
            listing.Label("Infusion.Settings.Infusions.Slots.Title".Translate());

            IntSliderRow(listing, "Awful", Settings.slotAwful, 0, 20);
            IntSliderRow(listing, "Poor", Settings.slotPoor, 0, 20);
            IntSliderRow(listing, "Normal", Settings.slotNormal, 0, 100);
            IntSliderRow(listing, "Good", Settings.slotGood, 0, 20);
            IntSliderRow(listing, "Excellent", Settings.slotExcellent, 0, 20);
            IntSliderRow(listing, "Masterwork", Settings.slotMasterwork, 0, 20);
            IntSliderRow(listing, "Legendary", Settings.slotLegendary, 0, 20);
        }

        private static void DrawTiers(Listing_Standard listing)
        {
            listing.Label("Infusion.Settings.Tiers.Title".Translate());
            foreach (TierDef tier in tiers)
            {
                if (Settings.tiersEnabled.TryGetValue(tier, out StrongBox<bool> enabled))
                {
                    listing.CheckboxLabeled(tier.defName, ref enabled.Value, null, RowHeight, LabelPct);
                }
            }

            listing.Gap();
            listing.Label("Infusion.Settings.Tiers.Color.Title".Translate());
            DrawTierColor(listing);

            listing.Gap();
            listing.Label("Infusion.Settings.Infusions.Defs.Title".Translate());
            DrawInfusionToggles(listing);
        }

        private static void DrawTierColor(Listing_Standard listing)
        {
            if (selectedTierDef == null)
            {
                return;
            }

            Rect dropdownRow = listing.GetRect(RowHeight);
            if (Widgets.ButtonText(dropdownRow.LeftPart(LabelPct), selectedTierDef.defName))
            {
                List<FloatMenuOption> options = tiers
                    .Select(tier => new FloatMenuOption(tier.defName, () => SelectTier(tier)))
                    .ToList();
                Find.WindowStack.Add(new FloatMenu(options));
            }
            listing.Gap(listing.verticalSpacing);

            Rect pickerRow = listing.GetRect(ColorWheelSize);
            Rect wheelRect = new Rect(pickerRow.x, pickerRow.y, ColorWheelSize, ColorWheelSize);
            Widgets.HSVColorWheel(wheelRect, ref selectedColor, ref colorWheelDragging);

            float controlsX = wheelRect.xMax + 20f;
            float controlsWidth = Mathf.Min(260f, pickerRow.xMax - controlsX);

            Rect swatchRect = new Rect(controlsX, pickerRow.y, controlsWidth, RowHeight);
            Widgets.DrawBoxSolidWithOutline(swatchRect, selectedColor, Color.gray);

            Color.RGBToHSV(selectedColor, out float hue, out float saturation, out float brightness);
            Rect brightnessRect = new Rect(controlsX, swatchRect.yMax + 10f, controlsWidth, RowHeight);
            float newBrightness = Widgets.HorizontalSlider(brightnessRect, brightness, 0.05f, 1f, true, null, null, null, 0.01f);
            if (newBrightness != brightness)
            {
                float alpha = selectedColor.a;
                selectedColor = Color.HSVToRGB(hue, saturation, newBrightness);
                selectedColor.a = alpha;
            }

            Rect saveRect = new Rect(controlsX, brightnessRect.yMax + 10f, controlsWidth, RowHeight);
            if (Widgets.ButtonText(saveRect, "Infusion.Settings.Tiers.SaveColorButton.Label".Translate()))
            {
                SaveSelectedColor();
            }
            listing.Gap(listing.verticalSpacing);
        }

        // Saving a color equal to the tier's default clears the override, as before.
        private static void SaveSelectedColor()
        {
            if (selectedColor == selectedTierDef.color)
            {
                if (Settings.tierColorOverride.Remove(selectedTierDef))
                {
                    Messages.Message("Infusion.Settings.Tiers.ColorResetMessage".Translate(selectedTierDef.defName), MessageTypeDefOf.NeutralEvent);
                }
                else
                {
                    Messages.Message("Infusion.Settings.Tiers.ColorAlreadyDefaultMessage".Translate(selectedTierDef.defName), MessageTypeDefOf.NeutralEvent);
                }
            }
            else
            {
                Settings.tierColorOverride[selectedTierDef] = selectedColor;
                Messages.Message("Infusion.Settings.Tiers.ColorSavedMessage".Translate(selectedTierDef.defName), MessageTypeDefOf.NeutralEvent);
            }
        }

        private static void DrawInfusionToggles(Listing_Standard listing)
        {
            if (selectedInfusionDef == null)
            {
                return;
            }

            Rect row = listing.GetRect(RowHeight);
            if (Widgets.ButtonText(row.LeftPart(LabelPct), selectedInfusionDef.defName))
            {
                List<FloatMenuOption> options = infusionDefs
                    .Select(def => new FloatMenuOption(def.defName, () => selectedInfusionDef = def))
                    .ToList();
                Find.WindowStack.Add(new FloatMenu(options));
            }

            string toggleLabel = Settings.infusionDefsDisabledMap.ContainsKey(selectedInfusionDef)
                ? "Infusion.Settings.Infusions.Defs.EnableButton".Translate()
                : "Infusion.Settings.Infusions.Defs.DisableButton".Translate();
            if (Widgets.ButtonText(row.RightPart(1f - LabelPct), toggleLabel))
            {
                ToggleSelectedInfusion();
            }
            listing.Gap(listing.verticalSpacing);

            List<InfusionDef> family = GetAllTiersOfInfusion(selectedInfusionDef);
            if (family.Count <= 1)
            {
                return;
            }

            // Derived every frame so it always reflects individual toggles and family changes.
            bool anyEnabled = family.Any(def => !IsInfusionDisabled(def));
            bool enableAll = anyEnabled;
            listing.CheckboxLabeled("Infusion.Settings.Infusions.Defs.EnableDisableAllTiers".Translate(), ref enableAll, null, RowHeight, LabelPct);
            if (enableAll != anyEnabled)
            {
                SetFamilyEnabled(family, enableAll);
            }
        }

        private static void ToggleSelectedInfusion()
        {
            if (IsInfusionDisabled(selectedInfusionDef))
            {
                Settings.infusionDefsDisabledMap.Remove(selectedInfusionDef);
                Messages.Message("Infusion.Settings.Infusions.Defs.EnableMessage".Translate(selectedInfusionDef.defName), MessageTypeDefOf.NeutralEvent);
            }
            else
            {
                Settings.infusionDefsDisabledMap[selectedInfusionDef] = new StrongBox<bool>(true);
                Messages.Message("Infusion.Settings.Infusions.Defs.DisableMessage".Translate(selectedInfusionDef.defName), MessageTypeDefOf.NeutralEvent);
            }
        }

        private static void SetFamilyEnabled(List<InfusionDef> family, bool enable)
        {
            foreach (InfusionDef def in family)
            {
                if (enable)
                {
                    Settings.infusionDefsDisabledMap.Remove(def);
                }
                else
                {
                    Settings.infusionDefsDisabledMap[def] = new StrongBox<bool>(true);
                }
            }

            string baseName = GetInfusionBaseName(selectedInfusionDef.defName);
            Messages.Message(
                (enable ? "Infusion.Settings.Infusions.Defs.EnableAllTiersMessage" : "Infusion.Settings.Infusions.Defs.DisableAllTiersMessage").Translate(baseName),
                MessageTypeDefOf.NeutralEvent);
        }

        private static void CheckboxRow(Listing_Standard listing, string labelKey, string tooltipKey, StrongBox<bool> box)
        {
            string tooltip = tooltipKey == null ? null : tooltipKey.Translate().Resolve();
            listing.CheckboxLabeled(labelKey.Translate(), ref box.Value, tooltip, RowHeight, LabelPct);
        }

        private static void SliderRow(Listing_Standard listing, string labelKey, string tooltipKey, StrongBox<float> box, float min, float max, float roundTo, string valueSuffix = "")
        {
            Rect row = listing.GetRect(RowHeight);
            Rect labelRect = row.LeftPart(LabelPct);
            Rect valueRect = row.RightPart(ValuePct);
            Rect sliderRect = new Rect(labelRect.xMax, row.y, row.width * (1f - LabelPct - ValuePct) - 10f, row.height);

            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(labelRect, labelKey.Translate());
            Widgets.Label(valueRect, box.Value.ToString("0.##") + valueSuffix);
            Text.Anchor = TextAnchor.UpperLeft;

            TooltipHandler.TipRegion(labelRect, tooltipKey.Translate().Resolve());
            box.Value = Widgets.HorizontalSlider(sliderRect, box.Value, min, max, true, null, null, null, roundTo);
            listing.Gap(listing.verticalSpacing);
        }

        private static void IntSliderRow(Listing_Standard listing, string label, StrongBox<int> box, int min, int max)
        {
            Rect row = listing.GetRect(RowHeight);
            Rect labelRect = row.LeftPart(LabelPct);
            Rect valueRect = row.RightPart(ValuePct);
            Rect sliderRect = new Rect(labelRect.xMax, row.y, row.width * (1f - LabelPct - ValuePct) - 10f, row.height);

            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(labelRect, label);
            Widgets.Label(valueRect, box.Value.ToString());
            Text.Anchor = TextAnchor.UpperLeft;

            box.Value = Mathf.RoundToInt(Widgets.HorizontalSlider(sliderRect, box.Value, min, max, true, null, null, null, 1f));
            listing.Gap(listing.verticalSpacing);
        }
    }
}
