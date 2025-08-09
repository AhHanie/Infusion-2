using Infusion;
using RimWorld;
using Verse;

namespace Infusion.OnHitWorkers
{
    public class SpawnLightning : OnHitWorker
    {
        public override void BulletHit(ProjectileRecord record)
        {
            if (record.target != null)
            {
                Find.CurrentMap.weatherManager.eventHandler.AddEvent(new WeatherEvent_LightningStrike(record.target.MapHeld, record.target.Position));
            }
        }
    }
}