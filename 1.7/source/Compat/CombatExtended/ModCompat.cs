using Verse;

namespace Infusion.Compat.CombatExtended
{
    public sealed class CombatExtendedModCompat : ModCompat
    {
        public const string PackageId = "ceteam.combatextended";

        public override bool IsEnabled()
        {
            return ModsConfig.IsActive(PackageId);
        }

        public override void Init()
        {
            CombatExtendedPatches.Apply(ModBase.instance);
        }

        public override string GetModPackageIdentifier()
        {
            return PackageId;
        }
    }
}
