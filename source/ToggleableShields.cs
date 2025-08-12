using Verse;

namespace Infusion.Compat
{
    public class ToggleableShields
    {
        public static bool Enabled => ModsConfig.IsActive("owlchemist.toggleableshields");
    }
}
