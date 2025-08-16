namespace Infusion
{
    public enum InfusionTags
    {
        None,
        SMART,
        SOOTHING,
        AEGIS,
        BARRAGE
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
            }
            return null;
        }
    }
}
