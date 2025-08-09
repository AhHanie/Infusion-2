
namespace Infusion.PreHitWorkers
{
    public class AudioLock : PreHitWorker
    {
        public override void PreBulletHit(ProjectileRecord record)
        {
            Locks.LifeStageUtiltiyAudioLock = true;
        }

        public override void PreMeleeHit(VerbRecordData record)
        {
            Locks.LifeStageUtiltiyAudioLock = true;
        }
    }
}

