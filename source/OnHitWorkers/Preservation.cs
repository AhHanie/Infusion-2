using RimWorld;
using UnityEngine;
using Verse;

namespace Infusion.OnHitWorkers
{
    /// <summary>
    /// On-hit worker that preserves the wearer from death by sacrificing the apparel item.
    /// When the wearer would be downed, this effect destroys the apparel and applies a preservation hediff instead.
    /// </summary>
    public class Preservation : OnHitWorker
    {
        private static HediffDef hediff = null;
        private static RulePackDef rulePack = null;

        public override bool WearerDowned(Pawn pawn, Apparel apparel)
        {
            // Lazy initialization of static defs
            if (hediff == null)
            {
                hediff = DefDatabase<HediffDef>.GetNamed("Infusion_Preservation");
            }

            if (rulePack == null)
            {
                rulePack = DefDatabase<RulePackDef>.GetNamed("Infusion_Preservation");
            }

            if (PawnUtils.IsAliveAndWell(pawn))
            {
                var prevHediff = pawn.health.hediffSet.GetFirstHediffOfDef(hediff);
                Hediff currentHediff;

                if (prevHediff == null)
                {
                    // Create new preservation hediff
                    var newHediff = HediffMaker.MakeHediff(hediff, pawn);
                    pawn.health.AddHediff(newHediff);
                    currentHediff = newHediff;
                }
                else
                {
                    // Extend existing hediff duration
                    var disappearsComp = prevHediff.TryGetComp<HediffComp_Disappears>();
                    if (disappearsComp != null)
                    {
                        var compProps = disappearsComp.props as HediffCompProperties_Disappears;
                        if (compProps != null)
                        {
                            disappearsComp.ticksToDisappear += compProps.disappearsAfterTicks.min;
                        }
                    }
                    currentHediff = prevHediff;
                }

                // Create battle log entry
                var logEntry = new BattleLogEntry_ItemUsed(pawn, pawn, apparel.def, rulePack);
                currentHediff.combatLogEntry = new WeakReference<LogEntry>(logEntry);
                currentHediff.combatLogText = logEntry.ToGameStringFromPOV(null);
                Find.BattleLog.Add(logEntry);

                // Show text mote indicating the sacrifice
                MoteMaker.ThrowText(
                    new Vector3((float)pawn.Position.x + 1f, (float)pawn.Position.y, (float)pawn.Position.z + 1f),
                    pawn.Map,
                    ResourceBank.Strings.OnHitWorkers.Sacrificed(apparel.Label),
                    new Color(1.0f, 0.5f, 0.0f)
                );

                // Destroy the apparel to complete the sacrifice
                apparel.Destroy();

                // Return false to prevent the pawn from being downed
                return false;
            }
            else
            {
                // Pawn is not alive and well, allow normal downing
                return true;
            }
        }
    }
}