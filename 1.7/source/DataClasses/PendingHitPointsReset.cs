using Verse;

namespace Infusion
{
    public class PendingHitPointsReset : IExposable
    {
        public ThingWithComps thing;
        public int addedTick;
        public int triggerTick;

        public PendingHitPointsReset()
        {
        }

        public PendingHitPointsReset(ThingWithComps thing, int addedTick, int triggerTick)
        {
            this.thing = thing;
            this.addedTick = addedTick;
            this.triggerTick = triggerTick;
        }

        public void ExposeData()
        {
            Scribe_References.Look(ref thing, "thing");
            Scribe_Values.Look(ref addedTick, "addedTick");
            Scribe_Values.Look(ref triggerTick, "triggerTick");
        }
    }
}
