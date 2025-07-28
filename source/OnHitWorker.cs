using RimWorld;
using Verse;

namespace Infusion
{
    public struct VerbRecordData
    {
        public float baseDamage;
        public ThingWithComps source;
        public Thing target;
        public Verb verb;

        public VerbRecordData(float baseDamage, ThingWithComps source, Thing target, Verb verb)
        {
            this.baseDamage = baseDamage;
            this.source = source;
            this.target = target;
            this.verb = verb;
        }
    }

    public abstract class VerbCastedRecord
    {
    }

    public class VerbCastedRecordMelee : VerbCastedRecord
    {
        public VerbRecordData Data { get; }

        public VerbCastedRecordMelee(VerbRecordData data)
        {
            Data = data;
        }
    }

    public class VerbCastedRecordRanged : VerbCastedRecord
    {
        public VerbRecordData Data { get; }

        public VerbCastedRecordRanged(VerbRecordData data)
        {
            Data = data;
        }
    }

    public class ProjectileRecord
    {
        public float baseDamage;
        public Map map;
        public Bullet projectile;
        public ThingWithComps source;
        public Thing target;

        public ProjectileRecord(float baseDamage, Map map, Bullet projectile, ThingWithComps source, Thing target = null)
        {
            this.baseDamage = baseDamage;
            this.map = map;
            this.projectile = projectile;
            this.source = source;
            this.target = target;
        }
    }

    public abstract class OnHitRecord
    {
    }

    public class OnHitRecordMeleeCast : OnHitRecord
    {
        public VerbRecordData Data { get; }

        public OnHitRecordMeleeCast(VerbRecordData data)
        {
            Data = data;
        }
    }

    public class OnHitRecordMeleeHit : OnHitRecord
    {
        public VerbRecordData Data { get; }

        public OnHitRecordMeleeHit(VerbRecordData data)
        {
            Data = data;
        }
    }

    public class OnHitRecordRangedCast : OnHitRecord
    {
        public VerbRecordData Data { get; }

        public OnHitRecordRangedCast(VerbRecordData data)
        {
            Data = data;
        }
    }

    public class OnHitRecordRangedImpact : OnHitRecord
    {
        public ProjectileRecord Data { get; }

        public OnHitRecordRangedImpact(ProjectileRecord data)
        {
            Data = data;
        }
    }

    public abstract class OnHitWorker : Editable
    {
        public float amount = 0.0f;
        public float chance = 1.0f;
        public bool selfCast = false;

        public OnHitWorker()
        {
            amount = 0.0f;
            chance = 1.0f;
            selfCast = false;
        }

        public virtual float Chance => chance;

        public virtual void AfterAttack(VerbCastedRecord record) { }

        public virtual void BulletHit(ProjectileRecord record) { }

        public virtual void MeleeHit(VerbRecordData record) { }

        public virtual bool WearerDowned(Pawn pawn, Apparel apparel) => true;

        public Map MapOf(OnHitRecord record)
        {
            if (selfCast)
            {
                switch (record)
                {
                    case OnHitRecordMeleeCast meleeCast:
                        return meleeCast.Data.source.MapHeld;
                    case OnHitRecordMeleeHit meleeHit:
                        return meleeHit.Data.source.MapHeld;
                    case OnHitRecordRangedCast rangedCast:
                        return rangedCast.Data.source.MapHeld;
                    case OnHitRecordRangedImpact rangedImpact:
                        return rangedImpact.Data.source.MapHeld;
                    default:
                        return null;
                }
            }
            else
            {
                switch (record)
                {
                    case OnHitRecordMeleeCast meleeCast:
                        return meleeCast.Data.target.MapHeld;
                    case OnHitRecordMeleeHit meleeHit:
                        return meleeHit.Data.target.MapHeld;
                    case OnHitRecordRangedCast rangedCast:
                        return rangedCast.Data.target.MapHeld;
                    case OnHitRecordRangedImpact rangedImpact:
                        return rangedImpact.Data.map;
                    default:
                        return null;
                }
            }
        }

        public IntVec3 PosOf(OnHitRecord record)
        {
            if (selfCast)
            {
                switch (record)
                {
                    case OnHitRecordMeleeCast meleeCast:
                        return meleeCast.Data.source.PositionHeld;
                    case OnHitRecordMeleeHit meleeHit:
                        return meleeHit.Data.source.PositionHeld;
                    case OnHitRecordRangedCast rangedCast:
                        return rangedCast.Data.source.PositionHeld;
                    case OnHitRecordRangedImpact rangedImpact:
                        return rangedImpact.Data.source.PositionHeld;
                    default:
                        return IntVec3.Invalid;
                }
            }
            else
            {
                switch (record)
                {
                    case OnHitRecordMeleeCast meleeCast:
                        return meleeCast.Data.target.PositionHeld;
                    case OnHitRecordMeleeHit meleeHit:
                        return meleeHit.Data.target.PositionHeld;
                    case OnHitRecordRangedCast rangedCast:
                        return rangedCast.Data.target.PositionHeld;
                    case OnHitRecordRangedImpact rangedImpact:
                        return rangedImpact.Data.projectile.Position;
                    default:
                        return IntVec3.Invalid;
                }
            }
        }

        public (Map, IntVec3) MapPosOf(OnHitRecord record)
        {
            return (MapOf(record), PosOf(record));
        }

        public static bool CheckChance(OnHitWorker worker)
        {
            return Rand.Chance(worker.Chance);
        }
    }
}