using Infusion;
using RimWorld;
using Verse;

namespace Infusion.OnHitWorkers
{
    public class MakeThingOnTarget : OnHitWorker
    {
        private ThingDef thingToMake;

        private ThingDef thingStuff;

        public MakeThingOnTarget()
        {
            thingToMake = null;
            thingStuff = null;
        }

        public override void BulletHit(ProjectileRecord record)
        {
            if (record.target != null)
            {
                GenPlace.TryPlaceThing(GenerateThingOnTarget(record.target as Pawn, record.projectile.Launcher as Pawn), record.target.Position, Find.CurrentMap, ThingPlaceMode.Near);
            }
        }

        public override void MeleeHit(VerbRecordData record)
        {
            if (record.target != null)
            {
                GenPlace.TryPlaceThing(GenerateThingOnTarget(record.target as Pawn, record.verb.CasterPawn), record.target.Position, Find.CurrentMap, ThingPlaceMode.Near);
            }
        }

        private Thing GenerateThingOnTarget(Pawn target, Pawn caster)
        {
            Thing thing = ThingMaker.MakeThing(thingToMake, thingStuff);
            CompQuality compQuality = thing.TryGetComp<CompQuality>();
            compQuality.SetQuality(QualityCategory.Normal, null);
            DebugActionsUtility.DustPuffFrom(thing);
            return thing;
        }
    }

}
