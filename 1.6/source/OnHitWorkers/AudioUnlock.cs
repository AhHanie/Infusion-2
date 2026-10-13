using Infusion;

namespace Infusion.OnHitWorkers
{
    public class AudioUnlock : OnHitWorker
    {
        public override void BulletHit(ProjectileRecord record)
        {
            Locks.LifeStageUtiltiyAudioLock = false;
        }

        public override void MeleeHit(VerbRecordData record)
        {
            Locks.LifeStageUtiltiyAudioLock = false;
        }
    }
}

