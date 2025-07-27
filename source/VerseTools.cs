using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Infusion
{
    /// <summary>
    /// Utility tools for working with Verse and RimWorld objects.
    /// </summary>
    public static class VerseTools
    {
        // Because StatDef doesn't implement IComparable,
        // defs can't be used directly for Sets.
        // Thus we use their defNames instead.

        /// <summary>
        /// A set of defNames of all accuracy stats.
        /// </summary>
        public static readonly HashSet<string> AccuracyStats = new HashSet<string>
        {
            StatDefOf.AccuracyTouch.defName,
            StatDefOf.AccuracyShort.defName,
            StatDefOf.AccuracyMedium.defName,
            StatDefOf.AccuracyLong.defName
        };

        /// <summary>
        /// A set of defNames of all direct armor stats.
        /// </summary>
        public static readonly HashSet<string> ArmorStats = new HashSet<string>
        {
            StatDefOf.ArmorRating_Blunt.defName,
            StatDefOf.ArmorRating_Heat.defName,
            StatDefOf.ArmorRating_Sharp.defName
        };

        /// <summary>
        /// A set of defNames of all Pawn stat categories.
        /// </summary>
        public static readonly HashSet<string> PawnStatCategories = new HashSet<string>
        {
            StatCategoryDefOf.BasicsPawn.defName,
            StatCategoryDefOf.BasicsPawnImportant.defName,
            StatCategoryDefOf.PawnCombat.defName,
            StatCategoryDefOf.PawnMisc.defName,
            StatCategoryDefOf.PawnSocial.defName,
            StatCategoryDefOf.PawnWork.defName
        };

        /// <summary>
        /// Resets a Thing's HitPoints to its MaxHitPoints.
        /// </summary>
        /// <typeparam name="T">The type of thing, must inherit from Thing</typeparam>
        /// <param name="thing">The thing to reset HP for</param>
        public static void ResetHP<T>(T thing) where T : Thing
        {
            thing.HitPoints = thing.MaxHitPoints;
        }

        /// <summary>
        /// Upcasts any object to Thing.
        /// </summary>
        /// <param name="obj">The object to upcast</param>
        /// <returns>The object as a Thing</returns>
        public static Thing UpcastToThing(object obj)
        {
            return obj as Thing;
        }

        /// <summary>
        /// Safely casts an object to the specified type.
        /// </summary>
        /// <typeparam name="T">The type to cast to</typeparam>
        /// <param name="obj">The object to cast</param>
        /// <returns>The cast object, or null if the cast fails</returns>
        public static T TryCast<T>(object obj) where T : class
        {
            return obj as T;
        }
    }

    /// <summary>
    /// Utility methods for working with Pawns.
    /// </summary>
    public static class PawnUtils
    {
        /// <summary>
        /// Checks if a Thing is a Pawn that is alive and not destroyed.
        /// </summary>
        /// <param name="thing">The thing to check</param>
        /// <returns>True if the thing is an alive, non-destroyed pawn; false otherwise</returns>
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

    /// <summary>
    /// Utility methods for working with Verbs.
    /// </summary>
    public static class VerbUtils
    {
        /// <summary>
        /// Gets the adjusted melee damage amount for a verb.
        /// </summary>
        /// <param name="verb">The verb to get damage for</param>
        /// <returns>The adjusted melee damage amount</returns>
        public static float GetAdjustedMeleeDamage(Verb verb)
        {
            return verb.verbProps.AdjustedMeleeDamageAmount(verb, verb.CasterPawn);
        }
    }
}