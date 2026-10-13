using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace Infusion
{
    public class InfusionStatPart : StatPart
    {
        public bool IsPawnStat { get; }

        // Pawn stat categories that should be handled differently
        private static readonly HashSet<string> pawnStatCategories = new HashSet<string>
        {
            "PawnCombat",
            "PawnSocial",
            "PawnMisc",
            "PawnWork"
        };

        // Accuracy stats that need special handling for overcapping
        private static readonly HashSet<string> accuracyStats = new HashSet<string>
        {
            "ShootingAccuracyPawn",
            "MeleeHitChance"
        };

        public InfusionStatPart(StatDef stat) : base()
        {
            parentStat = stat;
            IsPawnStat = stat.category != null && pawnStatCategories.Contains(stat.category.defName);
        }

        public override void TransformValue(StatRequest req, ref float value)
        {
            if (!IsPawnStat)
            {
                value = req.Thing == null ? value : TransformThingStat(value, req.Thing);

                if (accuracyStats.Contains(parentStat.defName) && value >= 1.0f)
                {
                    if (Settings.accuracyOvercap.Value)
                    {
                        float overcaps = value <= 1.1f
                            ? value - 1.0f
                            : Mathf.Log((value - 1.0f) * 10.0f + 1.0f, 2.0f) / 10.0f;
                        value = 1.0f + overcaps;
                    }
                    else
                    {
                        value = 1.0f;
                    }
                }
            }
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (!IsPawnStat)
            {
                return req.Thing == null ? null : ExplainForThing(req.Thing);
            }
            return null;
        }

        /// <summary>
        /// Transforms stats for things (not pawns).
        /// Harmonize.StatWorker handles pawn stats.
        /// </summary>
        private float TransformThingStat(float value, Thing thing)
        {
            // Do not apply MaxHitPoints bonus to infusers
            if (parentStat != StatDefOf.MaxHitPoints ||
                (thing.def.thingCategories != null &&
                 !thing.def.thingCategories.Contains(ThingCategoryDef.Named("Infusion_Infusers"))))
            {
                var modifiers = GetAllModifiersFromThing(thing);
                return modifiers.ApplyTo(value);
            }
            return value;
        }

        private string ExplainForThing(Thing thing)
        {
            var sb = new StringBuilder();
            var statModSum = GetAllModifiersFromThing(thing);
            if (statModSum != StatMod.Empty)
            {
                sb.Append("Infusion.StatPart.Start".Translate())
                  .Append(statModSum.StringForStat(parentStat));
            }
            return sb.ToString();
        }

        private StatMod GetAllModifiersFromThing(Thing thing)
        {
            var comp = thing.TryGetComp<CompInfusion>();
            return comp?.GetModForStat(parentStat) ?? StatMod.Empty;
        }
    }
}