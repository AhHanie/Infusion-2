using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Infusion
{
    public static class DefsForReading
    {
        private static IEnumerable<ThingDef> _allBuildings;
        private static List<ThingDef> _allThingsInfusable;

        private static bool ApparelOrWeapon(ThingDef def)
        {
            return ThingCategoryDefOf.Apparel.ContainedInThisOrDescendant(def) ||
                   ThingCategoryDefOf.Weapons.ContainedInThisOrDescendant(def);
        }

        private static bool HasQualityNoInfusion(ThingDef def)
        {
            return def.HasComp(typeof(CompQuality)) &&
                   !def.HasComp(typeof(CompInfusion));
        }

        private static bool IsMultiUse(ThingDef def)
        {
            if (def.thingSetMakerTags == null)
                return true;

            return !def.thingSetMakerTags.Contains("SingleUseWeapon");
        }

        public static IEnumerable<ThingDef> allBuildings
        {
            get
            {
                if (_allBuildings == null)
                {
                    _allBuildings = DefDatabase<ThingDef>.AllDefs.Where(def => def.building != null);
                }
                return _allBuildings;
            }
        }

        public static List<ThingDef> allThingsInfusable
        {
            get
            {
                if (_allThingsInfusable == null)
                {
                    _allThingsInfusable = DefDatabase<ThingDef>.AllDefs
                        .Where(def => ApparelOrWeapon(def) &&
                                    HasQualityNoInfusion(def) &&
                                    IsMultiUse(def))
                        .ToList();
                }
                return _allThingsInfusable;
            }
        }
    }
}