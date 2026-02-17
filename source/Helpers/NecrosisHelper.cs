using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Infusion.Comps
{
    public static class NecrosisHelper
    {
        public static void ExposeData(ref List<PendingNecrosis> pendingNecrosis)
        {
            Scribe_Collections.Look(ref pendingNecrosis, "pendingNecrosis", LookMode.Deep);
        }

        public static void PostLoadInit(ref List<PendingNecrosis> pendingNecrosis)
        {
            if (pendingNecrosis == null)
            {
                pendingNecrosis = new List<PendingNecrosis>();
            }
            else
            {
                pendingNecrosis.RemoveAll(x => x == null || x.corpse == null);
            }
        }

        public static void Tick(List<PendingNecrosis> pendingNecrosis)
        {
            int now = Find.TickManager.TicksGame;
            for (int i = pendingNecrosis.Count - 1; i >= 0; i--)
            {
                PendingNecrosis pending = pendingNecrosis[i];
                if (pending == null || pending.corpse == null || pending.corpse.InnerPawn == null)
                {
                    pendingNecrosis.RemoveAt(i);
                    continue;
                }

                if (!pending.corpse.InnerPawn.Dead)
                {
                    Infusion.Harmonize.PawnKill_Necrosis.CleanupNecrosisBuildup(ref pending.riseEffecter);
                    pendingNecrosis.RemoveAt(i);
                    continue;
                }

                if (now >= pending.nextFxTick)
                {
                    Infusion.Harmonize.PawnKill_Necrosis.PlayNecrosisBuildup(pending.corpse, ref pending.riseEffecter);
                    pending.nextFxTick = now + 8;
                }

                if (now < pending.triggerTick)
                {
                    continue;
                }

                Infusion.Harmonize.PawnKill_Necrosis.ResolveQueuedNecrosis(pending.corpse.InnerPawn, pending.apparel);
                Infusion.Harmonize.PawnKill_Necrosis.CleanupNecrosisBuildup(ref pending.riseEffecter);
                pendingNecrosis.RemoveAt(i);
            }
        }

        public static void Queue(List<PendingNecrosis> pendingNecrosis, Corpse corpse, Apparel apparel, int delayTicks = 600)
        {
            if (!corpse.InnerPawn.health.hediffSet.HasHediff(HediffDefOf.Rising))
            {
                corpse.InnerPawn.health.AddHediff(HediffMaker.MakeHediff(HediffDefOf.Rising, corpse.InnerPawn));
            }

            pendingNecrosis.Add(new PendingNecrosis(corpse, apparel, Find.TickManager.TicksGame + delayTicks));
        }
    }
}
