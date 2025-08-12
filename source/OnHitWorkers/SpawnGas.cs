
using Verse;

namespace Infusion.OnHitWorkers
{
    public class SpawnGas : OnHitWorker
    {
        GasType gas;
        public SpawnGas() 
        {
            gas = GasType.BlindSmoke;
        }

        public override void BulletHit(ProjectileRecord record)
        {
            GasUtility.AddGas(record.projectile.Position, record.projectile.Launcher.MapHeld, gas, (int)amount);
        }
    }
}
