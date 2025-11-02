using System;
using System.Text;
using RimWorld;
using Verse;

namespace Infusion
{
    public struct StatMod : IEquatable<StatMod>
    {
        public float offset;
        public float multiplier;

        public float Offset
        {
            get => offset * Settings.statsGlobalMultiplier.Value;
        }

        public float Multiplier
        {
            get => multiplier * Settings.statsGlobalMultiplier.Value;
        }

        public StatMod(float offset = 0.0f, float multiplier = 0.0f)
        {
            this.offset = offset;
            this.multiplier = multiplier;
        }

        public static StatMod Empty => new StatMod();

        private static bool IsNotZero(float value)
        {
            return Math.Abs(value) > float.Epsilon;
        }

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

        public float ApplyTo(float value)
        {
            return value * (1.0f + Multiplier) + Offset;
        }

        public string StringForStat(StatDef stat)
        {
            var sb = new StringBuilder();

            // Add multiplier
            if (IsNotZero(Multiplier))
            {
                sb.Append(Multiplier.ToString("x+0.##%;x-0.##%"));

                // Add separator if we also have an offset
                if (IsNotZero(Offset))
                {
                    sb.Append(", ");
                }
            }

            // Add offset
            if (IsNotZero(Offset))
            {
                string styled = Offset.ToStringByStyle(stat.ToStringStyleUnfinalized);

                // Apply format string if available
                if (!string.IsNullOrEmpty(stat.formatString))
                {
                    styled = string.Format(stat.formatString, styled);
                }

                // Force add plus sign for positive values (StatDef formatString doesn't include it)
                if (Offset > 0.0f)
                {
                    sb.Append("+");
                }

                sb.Append(styled);
            }

            return sb.ToString();
        }
    }

    public static class StatModExtensions
    {
        public static string StringForStat(this StatMod statMod, StatDef stat)
        {
            return statMod.StringForStat(stat);
        }
    }
}