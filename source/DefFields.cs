using System;
using RimWorld;
using Verse;

namespace Infusion
{
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

        public T Replace => replace;

        public bool HasReplace => replace != null;
    }
}