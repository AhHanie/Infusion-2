using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace Infusion.OnHitWorkers
{
    public class ApplyHediff : OnHitWorker
    {
        public bool bodySizeMatters = true;
        public HediffDef def = null;
        public bool inverseStatScaling = false;
        public StatDef severityScaleBy = null;
        public bool onMeleeCast = true;
        public bool onMeleeImpact = true;

        public ApplyHediff()
        {
            bodySizeMatters = true;
            def = null;
            inverseStatScaling = false;
            severityScaleBy = null;
            onMeleeCast = true;
            onMeleeImpact = true;
        }

        public override void AfterAttack(VerbCastedRecord record)
        {
            switch (record)
            {
                case VerbCastedRecordMelee meleeRecord when onMeleeCast:
                    if (selfCast)
                    {
                        AddHediff(meleeRecord.Data.baseDamage, meleeRecord.Data.verb.CasterPawn);
                    }
                    else
                    {
                        var targetPawn = meleeRecord.Data.target as Pawn;
                        if (targetPawn != null)
                        {
                            AddHediff(meleeRecord.Data.baseDamage, targetPawn);
                        }
                    }
                    break;

                case VerbCastedRecordRanged rangedRecord:
                    if (selfCast)
                    {
                        AddHediff(rangedRecord.Data.baseDamage, rangedRecord.Data.verb.CasterPawn);
                    }
                    else
                    {
                        var targetPawn = rangedRecord.Data.target as Pawn;
                        if (targetPawn != null)
                        {
                            AddHediff(rangedRecord.Data.baseDamage, targetPawn);
                        }
                    }
                    break;
            }
        }

        public override void BulletHit(ProjectileRecord record)
        {
            if (selfCast)
            {
                var launcherPawn = record.projectile.Launcher as Pawn;
                if (launcherPawn != null)
                {
                    AddHediff(record.baseDamage, launcherPawn);
                }
            }
            else
            {
                var targetPawn = record.target as Pawn;
                if (targetPawn != null)
                {
                    AddHediff(record.baseDamage, targetPawn);
                }
            }
        }

        public override void MeleeHit(VerbRecordData record)
        {
            if (onMeleeImpact)
            {
                if (selfCast)
                {
                    AddHediff(record.baseDamage, record.verb.CasterPawn);
                }
                else
                {
                    var targetPawn = record.target as Pawn;
                    if (targetPawn != null)
                    {
                        AddHediff(record.baseDamage, targetPawn);
                    }
                }
            }
        }

        private void AddHediff(float baseDamage, Pawn pawn)
        {
            if (PawnUtils.IsAliveAndWell(pawn))
            {
                var amount = baseDamage * this.amount;
                var hediff = HediffMaker.MakeHediff(this.def, pawn);

                var disappearsComp = hediff.TryGetComp<HediffComp_Disappears>();
                if (disappearsComp != null)
                {
                    disappearsComp.ticksToDisappear = GenTicks.SecondsToTicks(amount);
                }
                else
                {
                    hediff.Severity = CalculateSeverity(amount, pawn);
                }
                pawn.health.AddHediff(hediff);
            }
        }

        private float CalculateSeverity(float amount, Pawn pawn)
        {
            float statScale = 1.0f;

            if (severityScaleBy != null)
            {
                var stat = pawn.GetStatValue(severityScaleBy);
                if (inverseStatScaling)
                {
                    statScale = Math.Max(0.0f, 1.0f - stat);
                }
                else
                {
                    statScale = stat;
                }
            }

            float bodySizeScale = bodySizeMatters ? pawn.BodySize : 1.0f;

            return amount * statScale / bodySizeScale / 100.0f;
        }
    }
}