using System;
using UnityEngine;
using Verse;

namespace Infusion
{
    public class TierDef : HashEqualDef, IComparable
    {
        public Color color;
        public QualityMap chances;
        public QualityMap weights;

        /// <summary>
        /// Used for sorting infusions, higher being higher.
        /// </summary>
        public int priority;

        public float infuserValue;

        public float extractionChance;

        public ThingDef infuser;

        public TierDef()
        {
            color = Color.white;
            chances = new QualityMap();
            weights = new QualityMap();
            priority = 0;
            infuserValue = 100.0f;
            extractionChance = 1.0f;
            infuser = null;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public int CompareTo(object obj)
        {
            if (obj is TierDef def)
            {
                return this.defName.CompareTo(def.defName);
            }
            return 0;
        }

        public static bool IsEmpty(TierDef tier)
        {
            return tier.defName == "UnnamedDef";
        }

        public static int Priority(TierDef tier)
        {
            return tier.priority;
        }

        public static TierDef Empty => new TierDef();
    }
}