using Verse;

namespace Infusion.SpecialThingFilterWorkers
{
    /// <summary>
    /// Base class for special thing filter workers that filter based on infusion status.
    /// </summary>
    public abstract class BaseFilterWorker : SpecialThingFilterWorker
    {
        private readonly bool flag;

        protected BaseFilterWorker(bool flag)
        {
            this.flag = flag;
        }

        /// <summary>
        /// Checks if the flag matches whether the thing has infusions.
        /// </summary>
        /// <param name="flag">The flag to match against</param>
        /// <param name="thing">The thing to check</param>
        /// <returns>True if the flag matches the infusion status</returns>
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

    /// <summary>
    /// Filter worker for infused apparels.
    /// </summary>
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

    /// <summary>
    /// Filter worker for non-infused apparels.
    /// </summary>
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

    /// <summary>
    /// Filter worker for infused weapons.
    /// </summary>
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

    /// <summary>
    /// Filter worker for non-infused weapons.
    /// </summary>
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