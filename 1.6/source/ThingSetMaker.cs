using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Infusion
{
    public class ThingSetMakerInfuser : ThingSetMaker
    {
        private Thing GenerateOne()
        {
            var enabledTiers = DefDatabase<TierDef>.AllDefs
                .Where(Settings.IsTierEnabled)
                .ToList();

            if (enabledTiers.Count == 0)
                return null;

            var randomTier = enabledTiers.RandomElement();
            return ThingMaker.MakeThing(randomTier.infuser);
        }

        protected override void Generate(ThingSetMakerParams parms, List<Thing> outThings)
        {
            var count = parms.countRange?.RandomInRange ?? 1;

            var infusers = Enumerable.Range(0, count)
                .Select(_ => GenerateOne())
                .Where(infuser => infuser != null);

            outThings.AddRange(infusers);
        }

        protected override IEnumerable<ThingDef> AllGeneratableThingsDebugSub(ThingSetMakerParams parms)
        {
            return DefDatabase<TierDef>.AllDefs
                .Where(Settings.IsTierEnabled)
                .Select(tier => tier.infuser);
        }
    }
}