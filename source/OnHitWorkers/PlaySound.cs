using RimWorld;
using Verse;
using Verse.Sound;

namespace Infusion.OnHitWorkers
{
    /// <summary>
    /// On-hit worker that plays sound effects at impact locations.
    /// </summary>
    public class PlaySound : OnHitWorker
    {
        public SoundDef def = null;
        public bool onMeleeCast = true;
        public bool onMeleeImpact = true;
        public bool onRangedCast = true;
        public bool onRangedImpact = true;

        public PlaySound()
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
                        PlaySoundAtLocation(location);
                        break;
                    }
                case VerbCastedRecordRanged rangedRecord when onRangedCast:
                    {
                        var onHitRecord = new OnHitRecordRangedCast(rangedRecord.Data);
                        var location = MapPosOf(onHitRecord);
                        PlaySoundAtLocation(location);
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
                PlaySoundAtLocation(location);
            }
        }

        public override void MeleeHit(VerbRecordData record)
        {
            if (onMeleeImpact)
            {
                var onHitRecord = new OnHitRecordMeleeHit(record);
                var location = MapPosOf(onHitRecord);
                PlaySoundAtLocation(location);
            }
        }

        public override bool WearerDowned(Pawn pawn, Apparel apparel)
        {
            PlaySoundAtLocation((pawn.Map, pawn.Position));
            return true;
        }

        private void PlaySoundAtLocation((Map map, IntVec3 pos) location)
        {
            if (def != null && location.map != null)
            {
                var targetInfo = new TargetInfo(location.pos, location.map);
                var soundInfo = SoundInfo.InMap(targetInfo);
                def.PlayOneShot(soundInfo);
            }
        }
    }
}