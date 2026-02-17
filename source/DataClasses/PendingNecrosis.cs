using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Infusion
{
    public class PendingNecrosis : IExposable
    {
        public Apparel apparel;
        public int triggerTick;
        public int nextFxTick;
        public Corpse corpse;
        public Effecter riseEffecter;

        public PendingNecrosis()
        {
        }

        public PendingNecrosis(Corpse corpse, Apparel apparel, int triggerTick)
        {
            this.corpse = corpse;
            this.apparel = apparel;
            this.triggerTick = triggerTick;
            nextFxTick = Find.TickManager.TicksGame;
        }

        public void ExposeData()
        {
            Scribe_References.Look(ref corpse, "corpse");
            Scribe_References.Look(ref apparel, "apparel");
            Scribe_Values.Look(ref triggerTick, "triggerTick");
            Scribe_Values.Look(ref nextFxTick, "nextFxTick");
        }
    }
}
