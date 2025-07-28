using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Infusion
{
    public static class VerseTools
    {
        // Because StatDef doesn't implement IComparable,
        // defs can't be used directly for Sets.
        // Thus we use their defNames instead.

        public static readonly HashSet<string> AccuracyStats = new HashSet<string>
        {
            StatDefOf.AccuracyTouch.defName,
            StatDefOf.AccuracyShort.defName,
            StatDefOf.AccuracyMedium.defName,
            StatDefOf.AccuracyLong.defName
        };

        public static readonly HashSet<string> ArmorStats = new HashSet<string>
        {
            StatDefOf.ArmorRating_Blunt.defName,
            StatDefOf.ArmorRating_Heat.defName,
            StatDefOf.ArmorRating_Sharp.defName
        };

        public static readonly HashSet<string> PawnStatCategories = new HashSet<string>
        {
            StatCategoryDefOf.BasicsPawn.defName,
            StatCategoryDefOf.BasicsPawnImportant.defName,
            StatCategoryDefOf.PawnCombat.defName,
            StatCategoryDefOf.PawnMisc.defName,
            StatCategoryDefOf.PawnSocial.defName,
            StatCategoryDefOf.PawnWork.defName
        };

        public static void ResetHP<T>(T thing) where T : Thing
        {
            thing.HitPoints = thing.MaxHitPoints;
        }

        public static Thing UpcastToThing(object obj)
        {
            return obj as Thing;
        }

        public static T TryCast<T>(object obj) where T : class
        {
            return obj as T;
        }
    }

    public static class PawnUtils
    {
        public static bool IsAliveAndWell(Thing thing)
        {
            if (thing.Destroyed)
            {
                return false;
            }

            var pawn = VerseTools.TryCast<Pawn>(thing);
            if (pawn != null)
            {
                return !pawn.Dead;
            }

            return false;
        }
    }

    public static class VerbUtils
    {
        public static float GetAdjustedMeleeDamage(Verb verb)
        {
            return verb.verbProps.AdjustedMeleeDamageAmount(verb, verb.CasterPawn);
        }
    }
}