using Verse;

namespace Infusion.Conditions
{
    public class AnomalyStudyEnabled: InfusionConditon
    {
        public override bool Check(InfusionDef def)
        {
            return Find.Anomaly.AnomalyStudyEnabled;
        }
    }
}
