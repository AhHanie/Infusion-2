using System;
using System.Text;
using RimWorld;
using Verse;

namespace Infusion
{
    /// <summary>
    /// Represents a stat modification with offset and multiplier values.
    /// </summary>
    public struct StatMod : IEquatable<StatMod>
    {
        public float offset;
        public float multiplier;

        public StatMod(float offset = 0.0f, float multiplier = 0.0f)
        {
            this.offset = offset;
            this.multiplier = multiplier;
        }

        /// <summary>
        /// Gets an empty StatMod with zero offset and multiplier.
        /// </summary>
        public static StatMod Empty => new StatMod();

        /// <summary>
        /// Checks if a float is approximately equal to zero.
        /// </summary>
        private static bool IsNotZero(float value)
        {
            return Math.Abs(value) > float.Epsilon;
        }

        /// <summary>
        /// Checks if two floats are approximately equal.
        /// </summary>
        private static bool FloatEquals(float a, float b)
        {
            return Math.Abs(a - b) <= float.Epsilon;
        }

        public override bool Equals(object obj)
        {
            return obj is StatMod other && Equals(other);
        }

        public bool Equals(StatMod other)
        {
            return FloatEquals(offset, other.offset) &&
                   FloatEquals(multiplier, other.multiplier);
        }

        public override int GetHashCode()
        {
            return offset.GetHashCode() ^ multiplier.GetHashCode();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            if (IsNotZero(multiplier))
            {
                sb.Append(multiplier.ToString("+0.##%;-0.##%"));

                if (IsNotZero(offset))
                {
                    sb.Append(", ");
                }
            }

            if (IsNotZero(offset))
            {
                sb.Append(offset.ToString("+0.##;-0.##"));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Adds two StatMod instances together.
        /// </summary>
        public static StatMod operator +(StatMod a, StatMod b)
        {
            return new StatMod(a.offset + b.offset, a.multiplier + b.multiplier);
        }

        public static bool operator ==(StatMod left, StatMod right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(StatMod left, StatMod right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Applies a StatMod to a float value.
        /// Formula: (value * (1 + multiplier)) + offset
        /// </summary>
        /// <param name="value">The base value to modify</param>
        /// <returns>The modified value</returns>
        public float ApplyTo(float value)
        {
            return value * (1.0f + multiplier) + offset;
        }

        /// <summary>
        /// Formats StatMod for display in the info window.
        /// </summary>
        /// <param name="stat">The stat definition for formatting context</param>
        /// <returns>A formatted string representation</returns>
        public string StringForStat(StatDef stat)
        {
            var sb = new StringBuilder();

            // Add multiplier
            if (IsNotZero(multiplier))
            {
                sb.Append(multiplier.ToString("x+0.##%;x-0.##%"));

                // Add separator if we also have an offset
                if (IsNotZero(offset))
                {
                    sb.Append(", ");
                }
            }

            // Add offset
            if (IsNotZero(offset))
            {
                string styled = offset.ToStringByStyle(stat.ToStringStyleUnfinalized);

                // Apply format string if available
                if (!string.IsNullOrEmpty(stat.formatString))
                {
                    styled = string.Format(stat.formatString, styled);
                }

                // Force add plus sign for positive values (StatDef formatString doesn't include it)
                if (offset > 0.0f)
                {
                    sb.Append("+");
                }

                sb.Append(styled);
            }

            return sb.ToString();
        }
    }

    /// <summary>
    /// Extension methods for StatMod and related types.
    /// </summary>
    public static class StatModExtensions
    {
        /// <summary>
        /// Applies a StatMod to a float value.
        /// </summary>
        /// <param name="value">The base value to modify</param>
        /// <param name="statMod">The StatMod to apply</param>
        /// <returns>The modified value</returns>
        public static float ApplyStatMod(this float value, StatMod statMod)
        {
            return statMod.ApplyTo(value);
        }

        /// <summary>
        /// Formats StatMod for display in the info window.
        /// </summary>
        /// <param name="statMod">The StatMod to format</param>
        /// <param name="stat">The stat definition for formatting context</param>
        /// <returns>A formatted string representation</returns>
        public static string StringForStat(this StatMod statMod, StatDef stat)
        {
            return statMod.StringForStat(stat);
        }
    }
}