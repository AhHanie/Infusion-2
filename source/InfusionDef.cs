using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Infusion
{
    public class InfusionDef : HashEqualDef, IComparable
    {
        /// <summary>
        /// Will not infuse new Things.
        /// </summary>
        public bool disabled = false;

        /// <summary>
        /// Descriptions for special effects.
        /// </summary>
        [MustTranslate]
        [TranslationCanChangeCount]
        public List<string> extraDescriptions = new List<string>();

        /// <summary>
        /// Label for map overlay.
        /// </summary>
        [MustTranslate]
        public string labelShort = "";

        /// <summary>
        /// Matcher filters.
        /// </summary>
        public List<Matcher<InfusionDef>> matches = new List<Matcher<InfusionDef>>();

        /// <summary>
        /// Will migrate itself, by removing or replacing itself.
        /// </summary>
        public Migration<InfusionDef> migration = null;

        /// <summary>
        /// On-hit effect workers.
        /// </summary>
        public List<OnHitWorker> onHits = null;

        /// <summary>
        /// Postfix or Suffix.
        /// </summary>
        public Position position = Position.Prefix;

        public Dictionary<StatDef, StatMod> stats = new Dictionary<StatDef, StatMod>();

        /// <summary>
        /// The tier of this infusion.
        /// </summary>
        public TierDef tier = TierDef.Empty;

        public InfusionDef()
        {
            disabled = false;
            extraDescriptions = new List<string>();
            labelShort = "";
            matches = new List<Matcher<InfusionDef>>();
            migration = null;
            onHits = null;
            position = Position.Prefix;
            stats = new Dictionary<StatDef, StatMod>();
            tier = TierDef.Empty;
        }

        /// <summary>
        /// Gets the short label, falling back to the main label if not set.
        /// </summary>
        public string LabelShort
        {
            get
            {
                return string.IsNullOrEmpty(labelShort) ? label : labelShort;
            }
        }

        /// <summary>
        /// Gets the migration object if available.
        /// </summary>
        public Migration<InfusionDef> Migration => migration;

        /// <summary>
        /// Gets the on-hit workers, returning empty list if null.
        /// </summary>
        public List<OnHitWorker> OnHits => onHits ?? new List<OnHitWorker>();

        /// <summary>
        /// Gets the chance for this infusion at the given quality level.
        /// </summary>
        public float ChanceFor(QualityCategory quality)
        {
            return DefFields.ValueFor(quality, tier.chances);
        }

        /// <summary>
        /// Gets the weight for this infusion at the given quality level.
        /// </summary>
        public float WeightFor(QualityCategory quality)
        {
            return DefFields.ValueFor(quality, tier.weights);
        }

        public override IEnumerable<string> ConfigErrors()
        {
            var fromBase = base.ConfigErrors();

            // pawn stats should not use multipliers
            var pawnStats = stats
                .Where(kv => PawnStatCategories.Contains(kv.Key.category.defName) && kv.Value.multiplier != 0.0f);

            var errors = new List<string>();
            errors.AddRange(fromBase);
            errors.AddRange(pawnStats.Select(kv =>
                $"Infusion {defName} has a non-zero multiplier ({kv.Value.multiplier:F3}) for stat {kv.Key.defName} which is a Pawn stat. Multipliers on Pawn stats don't work."));

            return errors;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return $"{base.ToString()} ({label}, {tier.label})";
        }

        public int CompareTo(object obj)
        {
            if (obj is InfusionDef infDef)
            {
                var byTierPriority = tier.priority.CompareTo(infDef.tier.priority);
                return byTierPriority != 0 ? byTierPriority : defName.CompareTo(infDef.defName);
            }
            return 0;
        }

        #region Static Properties

        /// <summary>
        /// Pawn stat categories that should not use multipliers.
        /// </summary>
        private static readonly HashSet<string> PawnStatCategories = new HashSet<string>
        {
            // These would need to be populated with actual pawn stat category names
            // Based on the original F# code reference to `pawnStatCategories`
            "PawnCombat",
            "PawnSocial",
            "PawnMisc",
            "PawnWork"
        };

        #endregion

        /// <summary>
        /// Checks if an infusion is active for use (not disabled and not migrating).
        /// </summary>
        public static bool ActiveForUse(InfusionDef infDef)
        {
            return !infDef.disabled && infDef.migration == null;
        }

        /// <summary>
        /// Creates a requirement string from the infusion's matchers.
        /// </summary>
        public static string MakeRequirementString(InfusionDef infDef)
        {
            var requirements = infDef.matches
                .Select(matcher => matcher.RequirementString)
                .Where(req => !string.IsNullOrEmpty(req));

            return string.Join(", ", requirements);
        }

        /// <summary>
        /// Creates a formatted description string for the infusion.
        /// </summary>
        public static string MakeDescriptionString(InfusionDef infDef)
        {
            var labelSB = new StringBuilder(infDef.LabelCap);
            var reqString = MakeRequirementString(infDef);

            labelSB.Append(" (").Append(infDef.tier.label);

            if (reqString.Length > 0)
            {
                labelSB.Append(" ― ").Append(reqString);
            }

            var label = labelSB.Append(")").ToString();

            var statsDescriptions = new StringBuilder();
            foreach (var kvp in infDef.stats)
            {
                statsDescriptions
                    .Append("\n  ")
                    .Append(kvp.Key.LabelCap)
                    .Append(" ... ")
                    .Append(StatModExtensions.StringForStat(kvp.Value, kvp.Key));
            }

            var extraDescriptions = "";
            if (infDef.extraDescriptions?.Count > 0)
            {
                var extraSB = new StringBuilder();
                foreach (var desc in infDef.extraDescriptions)
                {
                    extraSB.Append("\n  ").Append(desc);
                }
                extraDescriptions = extraSB.ToString();
            }

            return new StringBuilder(label.Colorize(infDef.tier.color))
                .Append(statsDescriptions)
                .Append(extraDescriptions.Colorize(new Color(0.11f, 1.0f, 0.0f)))
                .ToString();
        }

        /// <summary>
        /// Checks if the infusion matches all criteria for the given target and quality.
        /// </summary>
        public static bool MatchesAll(InfusionDef infDef, ThingWithComps target, QualityCategory quality)
        {
            return infDef.ChanceFor(quality) > 0.0f
                && infDef.matches.All(matcher => matcher.Match(target, infDef));
        }

        /// <summary>
        /// Checks if the infusion should remove itself due to migration settings.
        /// </summary>
        public static bool ShouldRemoveItself(InfusionDef infDef)
        {
            return infDef.migration?.remove ?? false;
        }
    }
}