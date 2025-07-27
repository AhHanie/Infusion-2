using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Infusion
{
    public class StockGeneratorInfuser : StockGenerator
    {
        public int tierPriorityLimit = 0;

        public StockGeneratorInfuser()
        {
            tierPriorityLimit = 0;
        }

        public int RandomCountFor(ThingDef infuser)
        {
            return this.RandomCountOf(infuser);
        }

        public override IEnumerable<Thing> GenerateThings(PlanetTile forTile, Faction faction = null)
        {
            return DefDatabase<TierDef>.AllDefs
                .Where(Settings.IsTierEnabled)
                .Where(tier => tier.priority <= this.tierPriorityLimit)
                .SelectMany(tier => StockGeneratorUtility.TryMakeForStock(
                    tier.infuser,
                    this.RandomCountFor(tier.infuser),
                    faction));
        }

        public override bool HandlesThingDef(ThingDef thingDef)
        {
            return thingDef.tradeTags?.Contains("Infusion_Infuser") ?? false;
        }
    }
}