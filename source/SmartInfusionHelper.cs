using Infusion;
using UnityEngine;
using Verse;

namespace Infusion
{
    public class SmartInfusionHelper
    {
        private static bool ShouldBulletHit(InfusionDef def)
        {
            return Rand.Chance(def.keyedFloats[KeyedDataHelper.ConvertToString(KeyedData.SMART_INFUSION_CHANCE)]);
        }

        private static void LaunchProjectileSmart(Projectile projectile, Thing manningPawn, Vector3 drawPos, LocalTargetInfo currentTarget, bool preventFriendlyFire, Thing equipmentSource)
        {
            ProjectileHitFlags hitFlags = ProjectileHitFlags.IntendedTarget;
            if (currentTarget.Thing != null)
            {
                projectile.Launch(manningPawn, drawPos, currentTarget, currentTarget, hitFlags, preventFriendlyFire, equipmentSource);
            }
        }

        public static bool TryLaunchProjectileSmart(Projectile projectile, Thing manningPawn, Vector3 drawPos, LocalTargetInfo currentTarget, bool preventFriendlyFire, Thing equipmentSource)
        {
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
            if (ShouldBulletHit(infusionDef))
            {
                LaunchProjectileSmart(projectile, manningPawn, drawPos, currentTarget, preventFriendlyFire, equipmentSource);
                return true;
            }
            return false;
        }
    }
}