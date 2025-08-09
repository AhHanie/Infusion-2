using Verse;

namespace Infusion
{
    public class TemporaryAlly : IExposable
    {
        private Pawn pawn;

        private int totalTicksToLive;

        private int currentTicksAlive;

        private ThingWithComps allyOwner;

        public Pawn Pawn => pawn;

        public int CurrentTicksAlive => currentTicksAlive;

        public int TotalTicksToLive => totalTicksToLive;

        public bool ShouldBeDestroyed => currentTicksAlive >= totalTicksToLive;

        public bool Dead => pawn.Dead;

        public bool Destroyed => pawn.DestroyedOrNull();

        public ThingWithComps Owner => allyOwner;

        public TemporaryAlly()
        {
        }

        public TemporaryAlly(Pawn ally, int totalTicksToLive, ThingWithComps allyOwner)
        {
            pawn = ally;
            this.totalTicksToLive = totalTicksToLive;
            this.allyOwner = allyOwner;
            currentTicksAlive = 0;
        }

        public void ExposeData()
        {
            Scribe_References.Look(ref pawn, "ally");
            Scribe_References.Look(ref allyOwner, "allyOwner");
            Scribe_Values.Look(ref currentTicksAlive, "currentTicksAlive", 0);
            Scribe_Values.Look(ref totalTicksToLive, "totalTicksToLive", 0);
        }

        public void AddTicks(int ticks)
        {
            currentTicksAlive += ticks;
        }
    }

}