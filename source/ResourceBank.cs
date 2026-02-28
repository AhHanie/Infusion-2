using UnityEngine;
using Verse;
using System.Collections.Generic;
using System.Linq;

namespace Infusion
{
    [StaticConstructorOnStartup]
    public static class ResourceBank
    {
        public static List<InfusionDef> allInfusionDefs = DefDatabase<InfusionDef>.AllDefs.ToList();
        public static List<TierDef> allTierDefs = DefDatabase<TierDef>.AllDefs.ToList();
        [StaticConstructorOnStartup]
        public static class Textures
        {
            private static readonly Texture2D flame = ContentFinder<Texture2D>.Get("Things/Special/Fire/FireA");

            public static Texture2D Flame => flame;
        }

        public static class Strings
        {
            public static class Matchers
            {
                public static string Apparel => "Infusion.Matchers.Apparel".Translate();
                public static string Melee => "Infusion.Matchers.Melee".Translate();
                public static string Ranged => "Infusion.Matchers.Ranged".Translate();
                public static string NotUtility => "Infusion.Matchers.NotUtility".Translate();
                public static string ShieldBelt => "Infusion.Matchers.ShieldBelt".Translate();

                public static string Negate(string str)
                {
                    return "Infusion.Matchers.Negate".Translate(str);
                }
            }

            public static class OnHitWorkers
            {
                public static string Sacrificed(string str)
                {
                    return "Infusion.Sacrificed".Translate(str);
                }
            }

            public static class Gizmo
            {
                public static string label => "Infusion.EffectGizmo".Translate();
                public static string desc => "Infusion.EffectGizmo.Description".Translate();
            }

            public static class ITab
            {
                public static string Hint => "Infusion.ITab.Hint".Translate();
                public static string NoSlot => "Infusion.ITab.NoSlot".Translate();

                // marks
                public static string MarkExtraction(string extractionChance)
                {
                    return "Infusion.ITab.MarkForExtraction".Translate(extractionChance);
                }

                public static string MarkRemoval(string extractionChance)
                {
                    return "Infusion.ITab.MarkForRemoval".Translate(extractionChance);
                }

                public static string Unmark => "Infusion.ITab.Unmark".Translate();

                // applying infusers
                public static string ApplyInfuser => "Infusion.ITab.ApplyInfuser".Translate();
                public static string CancelInfuser => "Infusion.ITab.CancelInfuser".Translate();
                public static string ApplyInfuserDesc => "Infusion.ITab.ApplyInfuser.Description".Translate();
                public static string CantApplySlotsFull => "Infusion.ITab.ApplyInfuser.SlotsFull".Translate();
                public static string CantApplyNoSuitable => "Infusion.ITab.ApplyInfuser.NoSuitableInfuser".Translate();
            }
        }
    }
}