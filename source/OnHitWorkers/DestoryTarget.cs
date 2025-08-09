using Infusion;
using Verse;

namespace Infusion.OnHitWorkers
{
    public class DestroyTarget : OnHitWorker
    {
        public override void BulletHit(ProjectileRecord record)
        {
            if (record.target != null)
            {
                record.target.Destroy(DestroyMode.KillFinalize);
            }
        }

        public override void MeleeHit(VerbRecordData record)
        {
            if (record.target != null)
            {
                record.target.Destroy(DestroyMode.KillFinalize);
            }
        }
    }
}
