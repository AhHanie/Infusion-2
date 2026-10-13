using RimWorld;
using Verse;

namespace Infusion.OnHitWorkers
{
    public class DisarmWeapon : OnHitWorker
    {
        public override void BulletHit(ProjectileRecord record)
        {
            if (record.target != null)
            {
                Pawn pawn = Utils.SelectTarget(record, selfCast) as Pawn;
                if (pawn.equipment?.Primary != null)
                {
                    pawn.equipment.TryDropEquipment(pawn.equipment.Primary, out _, pawn.Position);
                    MoteMaker.ThrowText(pawn.DrawPos, pawn.MapHeld, "Infusion.Disarming.Message".Translate(), 3f);
                }
            }
        }

        public override void MeleeHit(VerbRecordData record)
        {
            if (record.target != null)
            {
                Pawn pawn = Utils.SelectTarget(record, selfCast) as Pawn;
                if (pawn.equipment?.Primary != null)
                {
                    pawn.equipment.TryDropEquipment(pawn.equipment.Primary, out _, pawn.Position);
                    MoteMaker.ThrowText(pawn.DrawPos, pawn.MapHeld, "Infusion.Disarming.Message".Translate(), 3f);
                }
            }
        }
    }
}
