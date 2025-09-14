using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Infusion.Harmonize
{
    // InspectPaneUtility is caching labels without taking color into account
    // Weapons that have the same infusion but different tiers will all have the same color because only one string is cached
    // We have to implement our own caching for infusion labels
    [HarmonyPatch(typeof(RimWorld.InspectPaneUtility), "AdjustedLabelFor")]
    public class InspectPaneUtility
    {
        public static Dictionary<string, string> infusionLabelCache = new Dictionary<string, string>();
        public static bool Prefix(List<object> selected, Rect rect, ref string __result)
        {
            if (selected.Count == 1 && selected[0] is ThingWithComps thing && thing.TryGetComp<CompInfusion>() != null)
            {
                string fullLabel = thing.LabelCap;

                using (new TextBlock(GameFont.Medium))
                {
                    if (infusionLabelCache.TryGetValue(fullLabel, out string cachedResult))
                    {
                        __result = cachedResult;
                        return false;
                    }

                    TaggedString truncated = Truncate(fullLabel, rect.width);
                    __result = truncated.RawText; 
                    infusionLabelCache[fullLabel] = __result;
                }

                return false;
            }
            return true;
        }

        public static TaggedString Truncate(TaggedString str, float width)
        {
            if (Text.CalcSize(str.RawText.StripTags()).x < width)
            {
                return str;
            }
            TaggedString value = str;
            do
            {
                value = value.RawText.Substring(0, value.RawText.Length - 1);
            }
            while (value.RawText.StripTags().Length > 0 && Text.CalcSize(AddEllipses(value.RawText.StripTags())).x > width);
            value = AddEllipses(value);
            return value;
        }

        public static string AddEllipses(string s)
        {
            if (s.Length > 0 && s[s.Length - 1] == '.')
            {
                return s + " ...";
            }
            return s + "...";
        }
    }
}
