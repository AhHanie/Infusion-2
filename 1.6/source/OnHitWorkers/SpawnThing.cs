using Verse;

namespace Infusion.OnHitWorkers
{
    public class SpawnThing : OnHitWorker
    {
        public int stackCount = 1;
        public ThingDef thingDef = null;
        public bool randomStackCount = false;
        public IntRange randomStackRange = IntRange.Zero;

        public SpawnThing()
        {
            stackCount = 1;
            thingDef = null;
            randomStackCount = false;
            randomStackRange = IntRange.Zero;
        }

        public override void BulletHit(ProjectileRecord record)
        {
            if (record.target != null && thingDef != null)
            {
                Thing thing = Utils.SelectTarget(record, selfCast);
                DebugThingPlaceHelper.DebugSpawn(thingDef, thing.Position, randomStackCount ? randomStackRange.RandomInRange : stackCount);
            }
        }

        public override void MeleeHit(VerbRecordData record)
        {
            if (record.target != null && thingDef != null)
            {
                Thing thing = Utils.SelectTarget(record, selfCast);
                DebugThingPlaceHelper.DebugSpawn(thingDef, thing.Position, randomStackCount ? randomStackRange.RandomInRange : stackCount);
            }
        }
    }
}
