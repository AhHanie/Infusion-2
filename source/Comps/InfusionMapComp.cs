using System.Collections.Generic;
using System.Linq;
using Infusion.Comps;
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
            if (Find.TickManager.TicksGame % Constants.ONE_MINUTE_IN_TICKS == 0)
            {
                Cleanup();
            }
        }

        public override void FinalizeInit()
        {
            List<Pawn> list = map.mapPawns.AllPawnsSpawned.ToList();
            foreach (Pawn item in list)
            {
                ThingComp temporaryAllyComp = item.AllComps.Find(comp => comp.GetType() == Constants.temporaryAllyCompType);
                if (temporaryAllyComp != null)
                {
                    TemporaryAllyComp comp = temporaryAllyComp as TemporaryAllyComp;
                    tempAllies.Add(new TemporaryAlly(item, comp.Owner));
                }
                if (item?.apparel?.WornApparel == null)
                {
                    continue;
                }
                foreach (Apparel item2 in item?.apparel?.WornApparel)
                {
                    CompInfusion compInfusion = item2.TryGetComp<CompInfusion>();
                    if (compInfusion != null && compInfusion.ContainsTag(InfusionTags.AEGIS) && !compsToTick.Contains(item2))
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
            List<ThingWithComps> tickCompsToRemove = new List<ThingWithComps>();
            foreach (ThingWithComps item in compsToTick)
            {
                if (item == null || item.DestroyedOrNull())
                {
                    tickCompsToRemove.Add(item);
                }
            }
            GameComponent_Infusion infusionGameComp = Current.Game.GetComponent<GameComponent_Infusion>();
            foreach (ThingWithComps item2 in tickCompsToRemove)
            {
                compsToTick.Remove(item2);
                if (item2.TryGetComp<CompInfusion>(out CompInfusion infusionComp) && infusionComp.ContainsTag(InfusionTags.AEGIS))
                {
                    infusionGameComp.RemoveAegisItem(item2);
                }
            }
            List<TemporaryAlly> temporaryAlliesToRemove = new List<TemporaryAlly>();
            foreach (TemporaryAlly item in temporaryAlliesToRemove)
            {
                if (item.pawn.DestroyedOrNull() || item.source.DestroyedOrNull())
                {
                    temporaryAlliesToRemove.Add(item);
                }
            }
            foreach (TemporaryAlly item in temporaryAlliesToRemove)
            {
                tempAllies.Remove(item);
            }
        }

        public bool AlreadyHasAllySpawned(ThingWithComps source)
        {
            return tempAllies.Any(ally => ally.source == source);
        }

        public void AddTemporaryAlly(Pawn ally, ThingWithComps source)
        {
            tempAllies.Add(new TemporaryAlly(ally, source));
        }

        public void RemoveTemporaryAlly(Pawn ally)
        {
            tempAllies.RemoveWhere(tempAlly => tempAlly.pawn == ally);
        }
    }
}

