using System;
using RimWorld;
using Verse;

namespace Infusion
{
    /// <summary>
    /// Quality -> Value map.
    /// </summary>
    public class QualityMap
    {
        public float awful;
        public float poor;
        public float normal;
        public float good;
        public float excellent;
        public float masterwork;
        public float legendary;

        public QualityMap()
        {
            awful = 0.0f;
            poor = 0.0f;
            normal = 0.0f;
            good = 0.0f;
            excellent = 0.0f;
            masterwork = 0.0f;
            legendary = 0.0f;
        }
    }

    public static class DefFields
    {
        /// <summary>
        /// Retrieves a value for the quality from a QualityMap.
        /// </summary>
        /// <param name="quality">The quality category to retrieve</param>
        /// <param name="qmap">The quality map to retrieve from</param>
        /// <returns>The value associated with the given quality</returns>
        /// <exception cref="ArgumentException">Thrown when an unknown quality is provided</exception>
        public static float ValueFor(QualityCategory quality, QualityMap qmap)
        {
            switch (quality)
            {
                case QualityCategory.Awful:
                    return qmap.awful;
                case QualityCategory.Poor:
                    return qmap.poor;
                case QualityCategory.Normal:
                    return qmap.normal;
                case QualityCategory.Good:
                    return qmap.good;
                case QualityCategory.Excellent:
                    return qmap.excellent;
                case QualityCategory.Masterwork:
                    return qmap.masterwork;
                case QualityCategory.Legendary:
                    return qmap.legendary;
                default:
                    throw new ArgumentException($"Unknown quality received: {quality}");
            }
        }
    }

    public enum Position
    {
        Prefix = 0,
        Suffix = 1
    }

    public class Migration<T> where T : Def
    {
        public bool remove;
        public T replace;

        public Migration()
        {
            remove = false;
            replace = null;
        }

        /// <summary>
        /// Gets the replace value. In F#, this was wrapped in an Option type.
        /// </summary>
        public T Replace => replace;

        /// <summary>
        /// Indicates whether a replacement value is available.
        /// </summary>
        public bool HasReplace => replace != null;
    }
}