using RimWorld;
using Verse;

namespace Infusion.OnHitWorkers
{
    /// <summary>
    /// On-hit worker that spawns filth at impact locations.
    /// </summary>
    public class SpawnFilth : OnHitWorker
    {
        public ThingDef def = null;

        public SpawnFilth()
        {
            def = null;
        }

        public override void BulletHit(ProjectileRecord record)
        {
            if (record.projectile.Position.Walkable(record.map))
            {
                FilthMaker.TryMakeFilth(record.projectile.Position, record.map, this.def);
            }
        }

        public override void MeleeHit(VerbRecordData record)
        {
            if (record.target.Position.Walkable(record.target.Map))
            {
                FilthMaker.TryMakeFilth(record.target.PositionHeld, record.source.MapHeld, this.def);
            }
        }
    }
}