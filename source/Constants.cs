using HarmonyLib;
using System;
using System.Reflection;
using Verse;
using RimWorld;

namespace Infusion
{
    public static class Constants
    {
        public static Type temporaryAllyCompType = typeof(TemporaryAllyComp);
        public static FieldInfo thingWithCompsCompsField;
        public static FieldInfo thingWithCompsCompsByTypeField;
        public static FieldInfo energyField;
        public static FieldInfo ticksToResetField;
        public static FieldInfo lastKeepDisplayTickField;
        public const int ONE_MINUTE_IN_TICKS = 3600;

        public static void Init()
        {
            thingWithCompsCompsField = AccessTools.Field(typeof(ThingWithComps), "comps");
            thingWithCompsCompsByTypeField = AccessTools.Field(typeof(ThingWithComps), "compsByType");
            Type compShieldType = typeof(CompShield);
            energyField = compShieldType.GetField("energy", BindingFlags.NonPublic | BindingFlags.Instance);
            ticksToResetField = compShieldType.GetField("ticksToReset", BindingFlags.NonPublic | BindingFlags.Instance);
            lastKeepDisplayTickField = compShieldType.GetField("lastKeepDisplayTick", BindingFlags.NonPublic | BindingFlags.Instance);
        }
    }
}
