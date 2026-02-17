using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Infusion
{
    public class AegisShieldCompData : IExposable
    {
        private float energy;
        private int ticksToReset;
        private int lastKeepDisplayTick;

        public float Energy { get => energy; set => energy = value; }
        public int TicksToReset { get => ticksToReset; set => ticksToReset = value; }
        public int LastKeepDisplayTick { get => lastKeepDisplayTick; set => lastKeepDisplayTick = value; }

        public AegisShieldCompData() { }

        public AegisShieldCompData(float energy, int ticksToReset, int lastKeepDisplayTick)
        {
            this.energy = energy;
            this.ticksToReset = ticksToReset;
            this.lastKeepDisplayTick = lastKeepDisplayTick;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref energy, "energy");
            Scribe_Values.Look(ref ticksToReset, "ticksToReset");
            Scribe_Values.Look(ref lastKeepDisplayTick, "lastKeepDisplayTick");
        }
    }
}
