using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Infusion.OnHitWorkers
{
    public class Ricochet : OnHitWorker
    {
        private class RicochetState
        {
            public float remainingChance;
            public int chainCount;
            public readonly HashSet<int> hitThingIds = new HashSet<int>();
        }

        private static readonly Dictionary<int, RicochetState> stateByProjectileId = new Dictionary<int, RicochetState>();

        public float chanceLossPerHit = 0.05f;
        public float radius = 8f;

        public override void BulletHit(ProjectileRecord record)
        {
            int projectileId = record.projectile.thingIDNumber;
            if (!stateByProjectileId.TryGetValue(projectileId, out RicochetState currentState))
            {
                currentState = new RicochetState
                {
                    remainingChance = Mathf.Clamp01(chance),
                    chainCount = 0
                };
            }
            else
            {
                stateByProjectileId.Remove(projectileId);
            }

            if (currentState.remainingChance <= 0f)
            {
                return;
            }

            currentState.hitThingIds.Add(record.target.thingIDNumber);
            if (!Rand.Chance(currentState.remainingChance))
            {
                return;
            }

            Thing launcher = record.projectile.Launcher;
            Thing nextTarget = FindNextTarget(record, launcher, currentState.hitThingIds);
            if (nextTarget == null)
            {
                return;
            }

            Projectile nextProjectile = SpawnRicochetProjectile(record);

            float nextChance = Mathf.Max(0f, currentState.remainingChance - chanceLossPerHit);
            stateByProjectileId[nextProjectile.thingIDNumber] = new RicochetState
            {
                remainingChance = nextChance,
                chainCount = currentState.chainCount + 1
            };
            stateByProjectileId[nextProjectile.thingIDNumber].hitThingIds.UnionWith(currentState.hitThingIds);

            LaunchRicochet(nextProjectile, record, nextTarget);
        }

        private Thing FindNextTarget(ProjectileRecord record, Thing launcher, HashSet<int> excludedIds)
        {
            IntVec3 center = record.target.PositionHeld;
            int radialCount = GenRadial.NumCellsInRadius(radius);

            for (int i = 0; i < radialCount; i++)
            {
                IntVec3 cell = center + GenRadial.RadialPattern[i];
                if (!cell.InBounds(record.map))
                {
                    continue;
                }

                List<Thing> thingsAtCell = record.map.thingGrid.ThingsListAtFast(cell);
                for (int j = 0; j < thingsAtCell.Count; j++)
                {
                    Thing thing = thingsAtCell[j];
                    if (excludedIds.Contains(thing.thingIDNumber))
                    {
                        continue;
                    }

                    if (!thing.HostileTo(launcher))
                    {
                        continue;
                    }

                    return thing;
                }
            }

            return null;
        }

        private static Projectile SpawnRicochetProjectile(ProjectileRecord record)
        {
            Thing spawnedThing = GenSpawn.Spawn(record.projectile.def, record.projectile.Position, record.map);
            return spawnedThing as Projectile;
        }

        private static void LaunchRicochet(Projectile projectile, ProjectileRecord record, Thing nextTarget)
        {
            Vector3 origin = record.projectile.ExactPosition;
            LocalTargetInfo targetInfo = new LocalTargetInfo(nextTarget);
            projectile.Launch(record.projectile.Launcher, origin, targetInfo, targetInfo, ProjectileHitFlags.IntendedTarget, false, record.source);
        }
    }
}
