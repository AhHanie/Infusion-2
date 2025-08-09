using Verse;

namespace Infusion
{
    public class TemporaryAllyComp: ThingComp
    {
        private int totalTicksToLive = 0;
        private int currentTicksAlive = 0;

        public int TotalTicksToLive { get => totalTicksToLive; set => totalTicksToLive = value; }
        public ThingWithComps Owner { get => parent; }

        public override void CompTick()
        {
            if (Find.TickManager.TicksGame % 60 == 0)
            {
                if (parent.Destroyed)
                {
                    return;
                }
                currentTicksAlive += 60;
                if (currentTicksAlive >= totalTicksToLive - 120)
                {
                    DebugActionsUtility.DustPuffFrom(parent);
                }
                if (currentTicksAlive >= totalTicksToLive)
                {
                    InfusionMapComp comp = parent.MapHeld.GetComponent<InfusionMapComp>();
                    comp.RemoveTemporaryAlly(parent as Pawn);
                    parent.Destroy();
                }
            }
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref totalTicksToLive, "totalTicksToLive");
            Scribe_Values.Look(ref currentTicksAlive, "currentTicksAlive");
        }

        public override void Notify_Killed(Map prevMap, DamageInfo? dinfo = null)
        {
            InfusionMapComp comp = prevMap.GetComponent<InfusionMapComp>();
            Pawn pawn = parent as Pawn;
            if (pawn.Corpse != null)
            {
                pawn.Corpse.Destroy();
            }
            comp.RemoveTemporaryAlly(pawn);
        }
    }
}
