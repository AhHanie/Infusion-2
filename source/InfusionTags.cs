namespace Infusion
{
    public enum InfusionTags
    {
        None,
        SMART,
        SOOTHING,
        AEGIS,
        BARRAGE,
        ALLURE,
        NECROSIS,
        PROSPECTOR,
        UNSTABLE,
        CONSTRUCTION_REFUND,
        CRAFTING_REFUND
    }

    public class InfusionTagsHelper
    {
        public static InfusionTags ConvertToEnum(string tag)
        {
            switch(tag) 
            {
                case "smart":
                    return InfusionTags.SMART;
                case "soothing":
                    return InfusionTags.SOOTHING;
                case "aegis":
                    return InfusionTags.AEGIS;
                case "barrage":
                    return InfusionTags.BARRAGE;
                case "allure":
                    return InfusionTags.ALLURE;
                case "necrosis":
                    return InfusionTags.NECROSIS;
                case "prospector":
                    return InfusionTags.PROSPECTOR;
                case "unstable":
                    return InfusionTags.UNSTABLE;
                case "construction_refund":
                    return InfusionTags.CONSTRUCTION_REFUND;
                case "crafting_refund":
                    return InfusionTags.CRAFTING_REFUND;
                default:
                    return InfusionTags.None;
            }
        }

        public static string ConvertToString(InfusionTags tag)
        {
            switch(tag)
            {
                case InfusionTags.SMART:
                    return "smart";
                case InfusionTags.SOOTHING:
                    return "soothing";
                case InfusionTags.AEGIS:
                    return "aegis";
                case InfusionTags.BARRAGE:
                    return "barrage";
                case InfusionTags.ALLURE:
                    return "allure";
                case InfusionTags.NECROSIS:
                    return "necrosis";
                case InfusionTags.PROSPECTOR:
                    return "prospector";
                case InfusionTags.UNSTABLE:
                    return "unstable";
                case InfusionTags.CONSTRUCTION_REFUND:
                    return "construction_refund";
                case InfusionTags.CRAFTING_REFUND:
                    return "crafting_refund";
            }
            return null;
        }
    }
}
