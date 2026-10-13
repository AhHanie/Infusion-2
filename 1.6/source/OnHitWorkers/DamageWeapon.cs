// Infusion.OnHitWorkers.DamageWeapon
using Infusion;
using Verse;

namespace Infusion.OnHitWorkers
{
    public class DamageWeapon : OnHitWorker
    {
        public override void BulletHit(ProjectileRecord record)
        {
            DamageWeaponHitpoints(record.source as Pawn, record.source);
        }

        public override void MeleeHit(VerbRecordData record)
        {
            DamageWeaponHitpoints(record.source as Pawn, record.source);
        }

        private void DamageWeaponHitpoints(Pawn caster, Thing weapon)
        {
            weapon.HitPoints -= (int)((float)weapon.MaxHitPoints * amount);
            if (weapon.HitPoints < 0 && !weapon.Destroyed)
            {
                weapon.Destroy();
            }
        }
    }
}
