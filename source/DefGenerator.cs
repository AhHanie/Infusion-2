using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Infusion
{
    public static class DefGenerator
    {
        public static IEnumerable<ThingDef> makeInfuserDefs()
        {
            return DefDatabase<TierDef>.AllDefs.Select(makeInfuserDef);
        }

        private static ThingDef makeInfuserDef(TierDef tier)
        {
            var comps = new List<CompProperties>
            {
                new CompInfuserProperties(tier),
                new CompProperties_Forbiddable()
            };

            var stats = new List<StatModifier>
            {
                new StatModifier { stat = StatDefOf.MaxHitPoints, value = 80.0f },
                new StatModifier { stat = StatDefOf.Mass, value = 0.5f },
                new StatModifier { stat = StatDefOf.MarketValue, value = tier.infuserValue },
                new StatModifier { stat = StatDefOf.SellPriceFactor, value = 0.2f }
            };

            var infuser = new ThingDef
            {
                alwaysHaulable = true,
                category = ThingCategory.Item,
                comps = new List<CompProperties>(comps),
                defName = "Infusion_Infuser_" + tier.defName,
                description = "Infusion.Infuser.Description".Translate(),
                drawGUIOverlay = true,
                graphicData = new GraphicData
                {
                    texPath = "Things/Infuser",
                    graphicClass = typeof(Graphic_Single)
                },
                label = "Infusion.Infuser.Label".Translate(tier.label),
                pathCost = 15,
                rotatable = false,
                selectable = true,
                statBases = new List<StatModifier>(stats),
                techLevel = TechLevel.Ultra,
                thingCategories = new List<ThingCategoryDef> { ThingCategoryDef.Named("Infusion_Infusers") },
                thingClass = typeof(Infuser),
                tradeability = Tradeability.Buyable,
                tradeTags = new List<string> { "Infusion_Infuser" }
            };

            tier.infuser = infuser;
            return infuser;
        }
    }
}