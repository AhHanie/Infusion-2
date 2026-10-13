using RimWorld;
using System;
using System.Collections.Generic;
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
            InfusionMapComp mapComp = target.MapHeld.GetComponent<InfusionMapComp>();
            if (!mapComp.AlreadyHasAllySpawned(source))
            {
                Faction faction = FactionUtility.DefaultFactionFrom(FactionDefOf.PlayerColony);
                Pawn newThing = PawnGenerator.GeneratePawn(ally, faction, target.MapHeld.Tile);
                IntVec3 loc = CellFinder.RandomSpawnCellForPawnNear(target.Position, target.MapHeld);
                GenSpawn.Spawn(newThing, loc, target.MapHeld);

                List<ThingComp> list = (List<ThingComp>)Constants.thingWithCompsCompsField.GetValue(newThing);
                TemporaryAllyComp thingComp = (TemporaryAllyComp)Activator.CreateInstance(Constants.temporaryAllyCompType);
                thingComp.parent = newThing;
                thingComp.TotalTicksToLive = totalTicksToLive;
                list.Add(thingComp);
                Dictionary<Type, ThingComp[]> dictionary = (Dictionary<Type, ThingComp[]>)Constants.thingWithCompsCompsByTypeField.GetValue(newThing);
                List<ThingComp> list2 = new List<ThingComp> { thingComp };
                dictionary.Add(Constants.temporaryAllyCompType, list2.ToArray());

                mapComp.AddTemporaryAlly(newThing, source);
            }
        }
    }
}