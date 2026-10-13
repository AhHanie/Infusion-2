using Verse;

namespace Infusion.SpecialThingFilterWorkers
{
    public abstract class BaseFilterWorker : SpecialThingFilterWorker
    {
        private readonly bool flag;

        protected BaseFilterWorker(bool flag)
        {
            this.flag = flag;
        }

        private bool MatchAgainst(bool flag, Thing thing)
        {
            var comp = thing.TryGetComp<CompInfusion>();
            bool hasInfusions = comp?.Size > 0;
            return flag == hasInfusions;
        }

        public override bool Matches(Thing thing)
        {
            return this.CanEverMatch(thing.def) && MatchAgainst(flag, thing);
        }
    }
    public class InfusedApparels : BaseFilterWorker
    {
        public InfusedApparels() : base(true)
        {
        }

        public override bool CanEverMatch(ThingDef thingDef)
        {
            return thingDef.IsApparel;
        }
    }
    public class NonInfusedApparels : BaseFilterWorker
    {
        public NonInfusedApparels() : base(false)
        {
        }

        public override bool CanEverMatch(ThingDef thingDef)
        {
            return thingDef.IsApparel;
        }
    }
    public class InfusedWeapons : BaseFilterWorker
    {
        public InfusedWeapons() : base(true)
        {
        }

        public override bool CanEverMatch(ThingDef thingDef)
        {
            return thingDef.IsWeapon;
        }
    }
    public class NonInfusedWeapons : BaseFilterWorker
    {
        public NonInfusedWeapons() : base(false)
        {
        }

        public override bool CanEverMatch(ThingDef thingDef)
        {
            return thingDef.IsWeapon;
        }
    }
}