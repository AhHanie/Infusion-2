using Infusion;
using RimWorld;
using Verse;

namespace Infusion.OnHitWorkers
{
    public class SpawnAlly : OnHitWorker
    {
        private PawnKindDef ally;

        private int totalTicksToLive;

        public SpawnAlly()
        {
            ally = null;
            totalTicksToLive = 0;
        }

        public override void BulletHit(ProjectileRecord record)
        {
            if (record.target != null)
            {
                SpawnAllyAt(record.target, record.source);
            }
        }

        public override void MeleeHit(VerbRecordData record)
        {
            if (record.target != null)
            {
                SpawnAllyAt(record.target, record.source);
            }
        }

        private void SpawnAllyAt(Thing target, ThingWithComps source)
        {
            InfusionMapComp component = target.MapHeld.GetComponent<InfusionMapComp>();
            if (!component.AlreadyHasAllySpawned(source))
            {
                Faction faction = FactionUtility.DefaultFactionFrom(FactionDefOf.PlayerColony);
                Pawn newThing = PawnGenerator.GeneratePawn(ally, faction, target.MapHeld.Tile);
                IntVec3 loc = CellFinder.RandomSpawnCellForPawnNear(target.Position, target.MapHeld);
                GenSpawn.Spawn(newThing, loc, target.MapHeld);
                component.AddTemporaryAlly(newThing, totalTicksToLive, source);
            }
        }
    }
}