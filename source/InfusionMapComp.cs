using System.Collections.Generic;
using System.Linq;
using Infusion;
using RimWorld;
using Verse;

namespace Infusion
{
    public class InfusionMapComp : MapComponent
    {
        private List<ThingWithComps> compsToTick = new List<ThingWithComps>();

        private List<TemporaryAlly> tempAllies = new List<TemporaryAlly>();

        public InfusionMapComp(Map map)
            : base(map)
        {
        }

        public override void MapComponentTick()
        {
            foreach (ThingWithComps item in compsToTick)
            {
                foreach (ThingComp allComp in item.AllComps)
                {
                    allComp.CompTick();
                }
            }
            if (Find.TickManager.TicksGame % 60 == 0)
            {
                foreach (TemporaryAlly tempAlly in tempAllies)
                {
                    if (!tempAlly.Destroyed && tempAlly.CurrentTicksAlive >= tempAlly.TotalTicksToLive - 120)
                    {
                        DebugActionsUtility.DustPuffFrom(tempAlly.Pawn);
                    }
                    if (tempAlly.Dead && !tempAlly.Destroyed)
                    {
                        tempAlly.Pawn.Destroy();
                    }
                    else if (tempAlly.ShouldBeDestroyed && !tempAlly.Destroyed)
                    {
                        tempAlly.Pawn.Destroy();
                    }
                    else
                    {
                        tempAlly.AddTicks(60);
                    }
                }
            }
            if (Find.TickManager.TicksGame % 18000 == 0)
            {
                Cleanup();
            }
        }

        public override void FinalizeInit()
        {
            List<Pawn> list = map.mapPawns.AllPawnsSpawned.ToList();
            foreach (Pawn item in list)
            {
                if (item?.apparel?.WornApparel == null)
                {
                    continue;
                }
                foreach (Apparel item2 in item?.apparel?.WornApparel)
                {
                    CompInfusion compInfusion = item2.TryGetComp<CompInfusion>();
                    if (compInfusion != null && compInfusion.ContainsTag(InfusionTags.AEGIS))
                    {
                        compsToTick.Add(item2);
                    }
                }
            }
        }

        public void AddThingToTick(ThingWithComps thing)
        {
            compsToTick.Add(thing);
        }

        public void RemoveThingToTick(ThingWithComps thing)
        {
            compsToTick.Remove(thing);
        }

        private void Cleanup()
        {
            List<ThingWithComps> list = new List<ThingWithComps>();
            foreach (ThingWithComps item in compsToTick)
            {
                if (item == null || item.DestroyedOrNull())
                {
                    list.Add(item);
                }
            }
            foreach (ThingWithComps item2 in list)
            {
                compsToTick.Remove(item2);
            }
            List<TemporaryAlly> list2 = new List<TemporaryAlly>();
            foreach (TemporaryAlly tempAlly in tempAllies)
            {
                if (tempAlly.Destroyed || tempAlly.CurrentTicksAlive >= tempAlly.TotalTicksToLive)
                {
                    list2.Add(tempAlly);
                }
            }
            foreach (TemporaryAlly item3 in list2)
            {
                tempAllies.Remove(item3);
            }
        }

        public void AddTemporaryAlly(Pawn ally, int totalTicksToLive, ThingWithComps source)
        {
            tempAllies.Add(new TemporaryAlly(ally, totalTicksToLive, source));
        }

        public bool AlreadyHasAllySpawned(ThingWithComps source)
        {
            foreach (TemporaryAlly tempAlly in tempAllies)
            {
                if (tempAlly.Owner == source && !tempAlly.Destroyed && !tempAlly.Dead)
                {
                    return true;
                }
            }
            return false;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref tempAllies, "tempAllies", LookMode.Deep);
        }
    }
}

