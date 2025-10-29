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
        public bool disabled = false;

        [MustTranslate]
        [TranslationCanChangeCount]
        public List<string> extraDescriptions = new List<string>();

        [MustTranslate]
        public string labelShort = "";

        public List<Matcher<InfusionDef>> matches = new List<Matcher<InfusionDef>>();

        public Migration<InfusionDef> migration = null;

        public List<OnHitWorker> onHits = null;
        public List<PreHitWorker> preHits = null;

        public Position position = Position.Prefix;

        public Dictionary<StatDef, StatMod> stats = new Dictionary<StatDef, StatMod>();

        public List<string> tags = null;

        public Dictionary<string, float> keyedFloats = new Dictionary<string, float>();

        public TierDef tier = TierDef.Empty;

        public List<InfusionConditon> conditions = null;

        public float weight = 1.0f;
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
            tags = new List<string>();
            keyedFloats = new Dictionary<string, float>();
            conditions = new List<InfusionConditon>();
            weight = 1.0f;
        }

        public string LabelShort
        {
            get
            {
                return string.IsNullOrEmpty(labelShort) ? label : labelShort;
            }
        }

        public Migration<InfusionDef> Migration => migration;

        public List<OnHitWorker> OnHits => onHits ?? new List<OnHitWorker>();

        public List<PreHitWorker> PreHits => preHits ?? new List<PreHitWorker>();

        public float ChanceFor(QualityCategory quality)
        {
            return DefFields.ValueFor(quality, tier.chances);
        }

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

        private static readonly HashSet<string> PawnStatCategories = new HashSet<string>
        {
            // These would need to be populated with actual pawn stat category names
            // Based on the original F# code reference to `pawnStatCategories`
            "PawnCombat",
            "PawnSocial",
            "PawnMisc",
            "PawnWork"
        };

        public static bool ActiveForUse(InfusionDef infDef)
        {
            return !infDef.disabled && infDef.migration == null && !Settings.infusionDefsDisabledMap.ContainsKey(infDef) && infDef.conditions.All(condition => condition.Check(infDef));
        }

        public static string MakeRequirementString(InfusionDef infDef)
        {
            var requirements = infDef.matches
                .Select(matcher => matcher.RequirementString)
                .Where(req => !string.IsNullOrEmpty(req));

            return string.Join(", ", requirements);
        }
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

            return new StringBuilder(label.Colorize(Utils.GetTierColor(infDef.tier)))
                .Append(statsDescriptions)
                .Append(extraDescriptions.Colorize(new Color(0.11f, 1.0f, 0.0f)))
                .ToString();
        }

        public static bool MatchesAll(InfusionDef infDef, ThingWithComps target, QualityCategory quality)
        {
            return infDef.ChanceFor(quality) > 0.0f
                && infDef.matches.All(matcher => matcher.Match(target, infDef));
        }

        public static bool ShouldRemoveItself(InfusionDef infDef)
        {
            return infDef.migration?.remove ?? false;
        }
    }
}