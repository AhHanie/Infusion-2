using HarmonyLib;
using System;
using System.Reflection;
using Verse;

namespace Infusion
{
    public class Constants
    {
        public static Type temporaryAllyCompType = typeof(TemporaryAllyComp);

        public static FieldInfo thingWithCompsCompsField;

        public static FieldInfo thingWithCompsCompsByTypeField;

        public static void Init()
        {
            thingWithCompsCompsField = AccessTools.Field(typeof(ThingWithComps), "comps");
            thingWithCompsCompsByTypeField = AccessTools.Field(typeof(ThingWithComps), "compsByType");
        }
    }
}
