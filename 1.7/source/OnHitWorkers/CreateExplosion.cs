using System;
using RimWorld;
using Verse;

namespace Infusion.OnHitWorkers
{
    public class CreateExplosion : DamageBase
    {
        public GasType? postExplosionGasType;
        public float radius = 0.5f;
        public bool onMeleeCast = true;
        public bool onMeleeImpact = true;

        public CreateExplosion()
        {
            postExplosionGasType = null;
            radius = 0.5f;
            onMeleeCast = true;
            onMeleeImpact = true;
        }

        public override void AfterAttack(VerbCastedRecord record)
        {
            if (record is VerbCastedRecordMelee meleeRecord && onMeleeCast)
            {
                var onHitRecord = new OnHitRecordMeleeCast(meleeRecord.Data);
                var map = MapOf(onHitRecord);
                var pos = PosOf(onHitRecord);

                GenExplosion.DoExplosion(
                    center: pos,
                    map: map,
                    radius: radius,
                    damType: def,
                    instigator: meleeRecord.Data.source,
                    damAmount: (int)(meleeRecord.Data.baseDamage * this.Amount),
                    armorPenetration: MeleeArmorPen(meleeRecord.Data.verb),
                    explosionSound: null,
                    weapon: meleeRecord.Data.source.def,
                    projectile: null,
                    intendedTarget: meleeRecord.Data.target,
                    postExplosionSpawnThingDef: null,
                    postExplosionSpawnChance: 0f,
                    postExplosionSpawnThingCount: 1,
                    postExplosionGasType: postExplosionGasType,
                    applyDamageToExplosionCellsNeighbors: false,
                    preExplosionSpawnThingDef: null,
                    preExplosionSpawnChance: 0f,
                    preExplosionSpawnThingCount: 1,
                    chanceToStartFire: 0,
                    damageFalloff: false,
                    direction: null
                );
            }
        }

        public override void BulletHit(ProjectileRecord record)
        {
            var onHitRecord = new OnHitRecordRangedImpact(record);
            var map = MapOf(onHitRecord);
            var pos = PosOf(onHitRecord);

            GenExplosion.DoExplosion(
                center: pos,
                map: map,
                radius: radius,
                damType: def,
                instigator: record.projectile.Launcher,
                damAmount: (int)(record.baseDamage * this.Amount),
                armorPenetration: RangedArmorPen(record.projectile),
                explosionSound: null,
                weapon: record.source.def,
                projectile: record.projectile.def,
                intendedTarget: record.projectile.intendedTarget.Thing,
                postExplosionSpawnThingDef: null,
                postExplosionSpawnChance: 0f,
                postExplosionSpawnThingCount: 1,
                postExplosionGasType: postExplosionGasType,
                applyDamageToExplosionCellsNeighbors: false,
                preExplosionSpawnThingDef: null,
                preExplosionSpawnChance: 0f,
                preExplosionSpawnThingCount: 1,
                chanceToStartFire: 0,
                damageFalloff: false,
                direction: null
            );
        }

        public override void MeleeHit(VerbRecordData record)
        {
            if (onMeleeImpact)
            {
                var onHitRecord = new OnHitRecordMeleeHit(record);
                var map = MapOf(onHitRecord);
                var pos = PosOf(onHitRecord);

                GenExplosion.DoExplosion(
                    center: pos,
                    map: map,
                    radius: radius,
                    damType: def,
                    instigator: record.source,
                    damAmount: (int)(record.baseDamage * this.Amount),
                    armorPenetration: MeleeArmorPen(record.verb),
                    explosionSound: null,
                    weapon: record.source.def,
                    projectile: null,
                    intendedTarget: record.target,
                    postExplosionSpawnThingDef: null,
                    postExplosionSpawnChance: 0f,
                    postExplosionSpawnThingCount: 1,
                    postExplosionGasType: postExplosionGasType,
                    applyDamageToExplosionCellsNeighbors: false,
                    preExplosionSpawnThingDef: null,
                    preExplosionSpawnChance: 0f,
                    preExplosionSpawnThingCount: 1,
                    chanceToStartFire: 0,
                    damageFalloff: false,
                    direction: null
                );
            }
        }
    }
}