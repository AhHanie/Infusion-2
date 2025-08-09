using Verse;

namespace Infusion
{
    public class Utils
    {
        public static Thing SelectTarget(VerbCastedRecordMelee record, bool selfCast)
        {
            if (selfCast)
            {
                return record.Data.verb.CasterPawn;
            }
            return record.Data.target;
        }

        public static Thing SelectTarget(VerbCastedRecordRanged record, bool selfCast)
        {
            if (selfCast)
            {
                return record.Data.verb.CasterPawn;
            }
            return record.Data.target;
        }

        public static Thing SelectTarget(ProjectileRecord record, bool selfCast)
        {
            if (selfCast)
            {
                return record.projectile.Launcher;
            }
            return record.target;
        }

        public static Thing SelectTarget(VerbRecordData record, bool selfCast)
        {
            if (selfCast)
            {
                return record.verb.CasterPawn;
            }
            return record.target;
        }
    }
}