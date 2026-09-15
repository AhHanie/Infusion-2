using Verse;

namespace Infusion
{
    public class SmartInfusionHelper
    {
        private static bool ShouldBulletHit(InfusionDef def)
        {
            return Rand.Chance(def.keyedFloats[KeyedDataHelper.ConvertToString(KeyedData.SMART_INFUSION_CHANCE)]);
        }

        public static bool ShouldForceSmartHit(Thing equipmentSource)
        {
            if (equipmentSource == null)
            {
                return false;
            }

            CompInfusion compInfusion = equipmentSource.TryGetComp<CompInfusion>();
            if (compInfusion == null)
            {
                return false;
            }

            InfusionDef infusionDef = compInfusion.TryGetInfusionDefWithTag(InfusionTags.SMART);
            if (infusionDef == null)
            {
                return false;
            }

            return ShouldBulletHit(infusionDef);
        }
    }
}
