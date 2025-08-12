
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
            Log.Message($"THE TRUTH: {record.projectile.Position.ToString()}, {gas}");
            if (record.projectile.MapHeld == null)
            {
                Log.Message("Map Held is null ..");
            }
            
            GasUtility.AddGas(record.projectile.Position, record.projectile.MapHeld, gas, amount);
        }
    }
}
