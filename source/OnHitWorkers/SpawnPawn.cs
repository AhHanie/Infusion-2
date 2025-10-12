using RimWorld;
using Verse;
using Verse.AI.Group;

namespace Infusion.OnHitWorkers
{
    public class SpawnPawn : OnHitWorker
    {
        public PawnKindDef pawnKind;

        public override void BulletHit(ProjectileRecord record)
        {
            if (record.target != null)
            {
                Thing thing = Utils.SelectTarget(record, selfCast);
                Faction faction = FactionUtility.DefaultFactionFrom(pawnKind.defaultFactionDef);
                Pawn pawn = PawnGenerator.GeneratePawn(pawnKind, faction, Find.CurrentMap.Tile);
                GenSpawn.Spawn(pawn, thing.Position, Find.CurrentMap);
                PostPawnSpawn(pawn);
                DebugActionsUtility.DustPuffFrom(pawn);
            }
        }

        public override void MeleeHit(VerbRecordData record)
        {
            if (record.target != null)
            {
                Thing thing = Utils.SelectTarget(record, selfCast);
                Faction faction = FactionUtility.DefaultFactionFrom(pawnKind.defaultFactionDef);
                Pawn pawn = PawnGenerator.GeneratePawn(pawnKind, faction, Find.CurrentMap.Tile);
                GenSpawn.Spawn(pawn, thing.Position, Find.CurrentMap);
                PostPawnSpawn(pawn);
                DebugActionsUtility.DustPuffFrom(pawn);
            }
        }

        private void PostPawnSpawn(Pawn pawn)
        {
            if (pawn.Spawned && pawn.Faction != null && pawn.Faction != Faction.OfPlayer)
            {
                Lord lord = null;
                if (pawn.Map.mapPawns.SpawnedPawnsInFaction(pawn.Faction).Any((Pawn p) => p != pawn))
                {
                    lord = ((Pawn)GenClosest.ClosestThing_Global(pawn.Position, pawn.Map.mapPawns.SpawnedPawnsInFaction(pawn.Faction), 99999f, (Thing p) => p != pawn && ((Pawn)p).GetLord() != null))?.GetLord();
                }
                if (lord == null || !lord.CanAddPawn(pawn))
                {
                    lord = LordMaker.MakeNewLord(pawn.Faction, new LordJob_DefendPoint(pawn.Position), Find.CurrentMap);
                }
                if (lord != null && lord.LordJob.CanAutoAddPawns)
                {
                    lord.AddPawn(pawn);
                }
            }
            pawn.Rotation = Rot4.South;
        }
    }
}