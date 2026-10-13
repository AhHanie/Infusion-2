using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Infusion
{
    public static class UnstableProjectileHelper
    {
        private static List<ThingDef> cachedBulletProjectiles;

        private static List<ThingDef> BulletProjectiles
        {
            get
            {
                if (cachedBulletProjectiles == null)
                {
                    cachedBulletProjectiles = BuildBulletProjectileCache();
                }

                return cachedBulletProjectiles;
            }
        }

        public static ThingDef GetRandomBulletProjectile(ThingDef originalProjectile)
        {
            List<ThingDef> projectiles = BulletProjectiles;
            if (projectiles.Count == 0)
            {
                return originalProjectile;
            }

            if (projectiles.Count == 1)
            {
                return projectiles[0];
            }

            ThingDef result = projectiles.RandomElement();
            int attempts = 8;
            while (attempts-- > 0 && result == originalProjectile)
            {
                result = projectiles.RandomElement();
            }

            return result ?? originalProjectile;
        }

        private static List<ThingDef> BuildBulletProjectileCache()
        {
            List<ThingDef> result = new List<ThingDef>();

            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefsListForReading)
            {
                if (!typeof(Bullet).IsAssignableFrom(def.thingClass))
                {
                    continue;
                }

                result.Add(def);
            }

            return result;
        }
    }
}
