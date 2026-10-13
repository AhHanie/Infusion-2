using System;
using System.Collections.Generic;
using Verse;

namespace Infusion
{
    public class Extractor : ThingWithComps, IComparable
    {
        private static HashSet<Extractor> allExtractors = new HashSet<Extractor>();

        public static HashSet<Extractor> AllExtractors => allExtractors;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            allExtractors.Add(this);
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            base.DeSpawn(mode);
            allExtractors.Remove(this);
        }

        public override bool Equals(object obj)
        {
            if (obj is Thing thing)
            {
                return this.ThingID == thing.ThingID;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public int CompareTo(object obj)
        {
            if (obj is Thing thing)
            {
                return this.ThingID.CompareTo(thing.ThingID);
            }
            return 0;
        }
    }
}