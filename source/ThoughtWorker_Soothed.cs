using System.Collections.Generic;
using Infusion;
using RimWorld;
using Verse;

namespace Infusion
{
    public class ThoughtWorker_Soothed : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (p.equipment == null)
            {
                return false;
            }
            List<ThingWithComps> list = new List<ThingWithComps>();
            list.AddRange(p.equipment.AllEquipmentListForReading);
            list.AddRange(p.apparel.WornApparel);
            foreach (ThingWithComps item in list)
            {
                CompInfusion compInfusion = item.TryGetComp<CompInfusion>();
                if (compInfusion == null)
                {
                    continue;
                }
                InfusionDef infusionDef = compInfusion.TryGetInfusionDefWithTag(InfusionTags.SOOTHING);
                if (infusionDef == null)
                {
                    continue;
                }
                if (infusionDef.tier == TierDefOf.Uncommon)
                {
                    return ThoughtState.ActiveAtStage(0);
                }
                if (infusionDef.tier == TierDefOf.Rare)
                {
                    return ThoughtState.ActiveAtStage(1);
                }
                return ThoughtState.ActiveAtStage(2);
            }
            return ThoughtState.Inactive;
        }
    }
}