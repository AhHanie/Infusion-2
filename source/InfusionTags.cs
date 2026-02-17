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
        NECROSIS
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
            }
            return null;
        }
    }
}
