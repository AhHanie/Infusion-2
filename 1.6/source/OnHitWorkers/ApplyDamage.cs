using RimWorld;
using UnityEngine;
using Verse;

namespace Infusion.OnHitWorkers
{
    public class ApplyDamage : DamageBase
    {
        public bool onMeleeCast = true;
        public bool onMeleeImpact = true;
        public bool sameAsOriginal = false;

        public ApplyDamage()
        {
            onMeleeCast = true;
            onMeleeImpact = true;
            sameAsOriginal = false;
        }

        public override void AfterAttack(VerbCastedRecord record)
        {
            if (record is VerbCastedRecordMelee meleeRecord && onMeleeCast)
            {
                if (PawnUtils.IsAliveAndWell(meleeRecord.Data.target))
                {
                    var damageInfo = CreateMeleeDamageInfo(meleeRecord.Data);
                    meleeRecord.Data.target.TakeDamage(damageInfo);
                }
            }
        }

        public override void BulletHit(ProjectileRecord record)
        {
            if (record.target != null && PawnUtils.IsAliveAndWell(record.target))
            {
                var damageInfo = CreateRangedDamageInfo(record);
                record.target.TakeDamage(damageInfo);
            }
        }

        public override void MeleeHit(VerbRecordData record)
        {
            if (onMeleeImpact && PawnUtils.IsAliveAndWell(record.target))
            {
                var damageInfo = CreateMeleeDamageInfo(record);
                record.target.TakeDamage(damageInfo);
            }
        }

        private DamageInfo CreateMeleeDamageInfo(VerbRecordData record)
        {
            var amount = record.baseDamage * this.Amount;
            var direction = (record.target.Position - record.verb.caster.Position).ToVector3();
            DamageDef damageDef = sameAsOriginal ? Harmonize.ThingPatches.TakeDamage.damageTakenDuringBulletImpact.Value.Def : this.def;

            var damageInfo = new DamageInfo(
                damageDef,
                Rand.Range(amount * 0.8f, amount * 1.2f),
                this.MeleeArmorPen(record.verb),
                -1.0f,
                record.verb.caster,
                null
            );

            // Apply additional damage info settings
            SetDamageInfoAngle(ref damageInfo, direction);
            SetDamageInfoBodyRegion(ref damageInfo, BodyPartHeight.Undefined, BodyPartDepth.Outside);
            SetDamageInfoWeaponBodyPartGroup(ref damageInfo,
                record.verb.verbProps.AdjustedLinkedBodyPartsGroup(record.verb.tool));

            return damageInfo;
        }

        private DamageInfo CreateRangedDamageInfo(ProjectileRecord record)
        {
            var amount = record.baseDamage * this.Amount;
            DamageDef damageDef = sameAsOriginal ? Harmonize.ThingPatches.TakeDamage.damageTakenDuringBulletImpact.Value.Def : this.def;

            return new DamageInfo(
                damageDef,
                amount,
                this.RangedArmorPen(record.projectile),
                record.projectile.ExactRotation.eulerAngles.y,
                record.projectile.Launcher,
                null,
                record.source.def
            );
        }

        private static void SetDamageInfoAngle(ref DamageInfo damageInfo, Vector3 direction)
        {
            damageInfo.SetAngle(direction);
        }

        private static void SetDamageInfoBodyRegion(ref DamageInfo damageInfo,
            BodyPartHeight height, BodyPartDepth depth)
        {
            damageInfo.SetBodyRegion(height, depth);
        }

        private static void SetDamageInfoWeaponBodyPartGroup(ref DamageInfo damageInfo,
            BodyPartGroupDef bodyPartGroup)
        {
            damageInfo.SetWeaponBodyPartGroup(bodyPartGroup);
        }
    }
}