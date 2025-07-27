using RimWorld;
using Verse;

namespace Infusion.OnHitWorkers
{
    /// <summary>
    /// On-hit worker that throws fleck effects at impact locations.
    /// </summary>
    public class ThrowFleck : OnHitWorker
    {
        public FleckDef def = null;
        public bool onMeleeCast = true;
        public bool onMeleeImpact = true;
        public bool onRangedCast = true;
        public bool onRangedImpact = true;

        public ThrowFleck()
        {
            def = null;
            onMeleeCast = true;
            onMeleeImpact = true;
            onRangedCast = true;
            onRangedImpact = true;
        }

        public override void AfterAttack(VerbCastedRecord record)
        {
            switch (record)
            {
                case VerbCastedRecordMelee meleeRecord when onMeleeCast:
                    {
                        var onHitRecord = new OnHitRecordMeleeCast(meleeRecord.Data);
                        var location = MapPosOf(onHitRecord);
                        ThrowFleckAtLocation(location);
                        break;
                    }
                case VerbCastedRecordRanged rangedRecord when onRangedCast:
                    {
                        var onHitRecord = new OnHitRecordRangedCast(rangedRecord.Data);
                        var location = MapPosOf(onHitRecord);
                        ThrowFleckAtLocation(location);
                        break;
                    }
            }
        }

        public override void BulletHit(ProjectileRecord record)
        {
            if (onRangedImpact)
            {
                var onHitRecord = new OnHitRecordRangedImpact(record);
                var location = MapPosOf(onHitRecord);
                ThrowFleckAtLocation(location);
            }
        }

        public override void MeleeHit(VerbRecordData record)
        {
            if (onMeleeImpact)
            {
                var onHitRecord = new OnHitRecordMeleeHit(record);
                var location = MapPosOf(onHitRecord);
                ThrowFleckAtLocation(location);
            }
        }

        public override bool WearerDowned(Pawn pawn, Apparel apparel)
        {
            ThrowFleckAtLocation((pawn.Map, pawn.Position));
            return true;
        }

        private void ThrowFleckAtLocation((Map map, IntVec3 pos) location)
        {
            if (def != null && location.map != null && !location.pos.Fogged(location.map))
            {
                FleckMaker.ThrowMetaIcon(location.pos, location.map, def);
            }
        }
    }
}