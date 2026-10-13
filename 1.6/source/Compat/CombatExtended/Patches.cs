using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace Infusion.Compat.CombatExtended
{
    public static class CombatExtendedPatches
    {
        private static readonly Type projectileType = AccessTools.TypeByName("CombatExtended.ProjectileCE");
        private static readonly Type bulletType = AccessTools.TypeByName("CombatExtended.BulletCE");
        private static readonly Type projectilePropsType = AccessTools.TypeByName("CombatExtended.ProjectilePropertiesCE");
        private static readonly Type trajectoryWorkerType = AccessTools.TypeByName("CombatExtended.BaseTrajectoryWorker");
        private static readonly MethodInfo bulletImpactMethod = bulletType == null
            ? null
            : AccessTools.Method(bulletType, "Impact", new[] { typeof(Thing) });
        private static readonly MethodInfo launchMethod = projectileType == null
            ? null
            : AccessTools.Method(projectileType, "Launch", new[]
            {
                typeof(Thing),
                typeof(Vector2),
                typeof(float),
                typeof(float),
                typeof(float),
                typeof(float),
                typeof(Thing),
                typeof(float)
            });
        private static readonly FieldInfo launcherField = projectileType == null ? null : AccessTools.Field(projectileType, "launcher");
        private static readonly FieldInfo equipmentField = projectileType == null ? null : AccessTools.Field(projectileType, "equipment");
        private static readonly FieldInfo intendedTargetField = projectileType == null ? null : AccessTools.Field(projectileType, "intendedTarget");
        private static readonly FieldInfo shotSpeedField = projectileType == null ? null : AccessTools.Field(projectileType, "shotSpeed");
        private static readonly FieldInfo canTargetSelfField = projectileType == null ? null : AccessTools.Field(projectileType, "canTargetSelf");
        private static readonly FieldInfo minCollisionDistanceField = projectileType == null ? null : AccessTools.Field(projectileType, "minCollisionDistance");
        private static readonly PropertyInfo damageAmountProperty = projectileType == null ? null : AccessTools.Property(projectileType, "DamageAmount");
        private static readonly PropertyInfo penetrationAmountProperty = projectileType == null ? null : AccessTools.Property(projectileType, "PenetrationAmount");
        private static readonly PropertyInfo exactRotationProperty = projectileType == null ? null : AccessTools.Property(projectileType, "ExactRotation");
        private static readonly PropertyInfo exactPositionProperty = projectileType == null ? null : AccessTools.Property(projectileType, "ExactPosition");
        private static readonly PropertyInfo trajectoryWorkerProperty = projectilePropsType == null ? null : AccessTools.Property(projectilePropsType, "TrajectoryWorker");
        private static readonly MethodInfo shotAngleMethod = trajectoryWorkerType == null || projectilePropsType == null
            ? null
            : AccessTools.Method(trajectoryWorkerType, "ShotAngle", new[] { projectilePropsType, typeof(Vector3), typeof(Vector3), typeof(float?) });
        private static readonly MethodInfo shotRotationMethod = trajectoryWorkerType == null || projectilePropsType == null
            ? null
            : AccessTools.Method(trajectoryWorkerType, "ShotRotation", new[] { projectilePropsType, typeof(Vector3), typeof(Vector3) });

        public static void Apply(Harmony harmony)
        {
            if (bulletImpactMethod == null)
            {
                Log.Warning("[Infusion 2] Combat Extended was detected, but BulletCE.Impact could not be located.");
                return;
            }

            Log.Message("[Infusion 2] Applied CE patch");

            harmony.Patch(
                bulletImpactMethod,
                prefix: new HarmonyMethod(typeof(CombatExtendedPatches), nameof(BulletImpactPrefix)),
                postfix: new HarmonyMethod(typeof(CombatExtendedPatches), nameof(BulletImpactPostfix)));
        }

        public static void BulletImpactPrefix(object __instance, Thing hitThing, out CombatExtendedImpactPatchState __state)
        {
            StaticFlags.DuringBulletImpact = true;
            __state = new CombatExtendedImpactPatchState
            {
                map = (__instance as Thing)?.MapHeld
            };

            ThingWithComps equipmentSource = GetEquipmentSource(__instance);
            if (equipmentSource == null)
            {
                return;
            }

            __state.canRunOnHitWorkers = true;
            __state.equipmentSource = equipmentSource;

            (List<PreHitWorker>, CompInfusion)? tuple = CompInfusionExtensions.ForPreHitWorkers(equipmentSource);
            if (!tuple.HasValue)
            {
                return;
            }
            (List<PreHitWorker>, CompInfusion) value = tuple.Value;
            List<PreHitWorker> workers = value.Item1;
            CompInfusion comp = value.Item2;
            ProjectileRecord record = new ProjectileRecord(GetDamageAmount(__instance), __state.map ?? (__instance as Thing)?.MapHeld, new CombatExtendedProjectileInfo(__instance), comp.parent, hitThing);
            foreach (PreHitWorker worker in workers)
            {
                worker.PreBulletHit(record);
            }
        }

        public static void BulletImpactPostfix(object __instance, Thing hitThing, CombatExtendedImpactPatchState __state)
        {
            StaticFlags.DuringBulletImpact = false;
            if (__state == null || !__state.canRunOnHitWorkers)
            {
                return;
            }

            (List<OnHitWorker>, CompInfusion)? tuple = CompInfusionExtensions.ForOnHitWorkers(__state.equipmentSource);
            if (!tuple.HasValue)
            {
                return;
            }

            (List<OnHitWorker>, CompInfusion) value = tuple.Value;
            List<OnHitWorker> workers = value.Item1;
            CompInfusion comp = value.Item2;
            ProjectileRecord record = new ProjectileRecord(GetDamageAmount(__instance), __state.map ?? (__instance as Thing)?.MapHeld, new CombatExtendedProjectileInfo(__instance), comp.parent, hitThing);
            foreach (OnHitWorker worker in workers)
            {
                worker.BulletHit(record);
            }
        }

        internal static ThingWithComps GetEquipmentSource(object projectile)
        {
            if (equipmentField?.GetValue(projectile) is ThingWithComps equipmentSource)
            {
                return equipmentSource;
            }

            if (GetLauncher(projectile) is Pawn pawn)
            {
                return pawn.equipment?.Primary;
            }

            return null;
        }

        internal static Thing GetLauncher(object projectile)
        {
            return launcherField.GetValue(projectile) as Thing;
        }

        internal static LocalTargetInfo GetIntendedTarget(object projectile)
        {
            if (intendedTargetField.GetValue(projectile) is LocalTargetInfo target)
            {
                return target;
            }

            return LocalTargetInfo.Invalid;
        }

        internal static float GetDamageAmount(object projectile)
        {
            return damageAmountProperty.GetValue(projectile) is float value ? value : 0f;
        }

        internal static float GetPenetrationAmount(object projectile)
        {
            return penetrationAmountProperty.GetValue(projectile) is float value ? value : 0f;
        }

        internal static Quaternion GetExactRotation(object projectile)
        {
            return exactRotationProperty.GetValue(projectile) is Quaternion value ? value : Quaternion.identity;
        }

        internal static Vector3 GetExactPosition(object projectile)
        {
            if (exactPositionProperty.GetValue(projectile) is Vector3 value)
            {
                return value;
            }

            return (projectile as Thing)?.DrawPos ?? Vector3.zero;
        }

        internal static float GetShotSpeed(object projectile, Thing spawnedThing)
        {
            if (shotSpeedField.GetValue(projectile) is float value && value > 0f)
            {
                return value;
            }

            return spawnedThing.def.projectile.speed;
        }

        internal static Vector3 GetTargetPosition(Thing target)
        {
            if (target == null)
            {
                return Vector3.zero;
            }

            IntVec3 pos = target.SpawnedOrAnyParentSpawned ? target.Position : target.PositionHeld;
            // Use actual map cell center to avoid the rendering Z-offset from DrawPos (ExactPosition.z + height * 0.84)
            // Y of 0.7 is approximate pawn center of mass height in CE
            return new Vector3(pos.x + 0.5f, 0.7f, pos.z + 0.5f);
        }

        internal static float GetMinCollisionDistance(float targetDistance)
        {
            const float shortRangeMinCollisionDistance = 1.5f;
            const float longRangeMinCollisionDistMult = 0.2f;

            if (targetDistance <= shortRangeMinCollisionDistance / longRangeMinCollisionDistMult)
            {
                return Mathf.Min(shortRangeMinCollisionDistance, targetDistance * 0.75f);
            }

            return targetDistance * longRangeMinCollisionDistMult;
        }

        internal static bool TryLaunchRicochet(object projectile, Map map, ThingWithComps source, Thing nextTarget, out int projectileThingId)
        {
            projectileThingId = -1;
            if (launchMethod == null || trajectoryWorkerProperty == null || shotAngleMethod == null || shotRotationMethod == null)
            {
                return false;
            }

            Thing spawnedThing = GenSpawn.Spawn(((Thing)projectile).def, ((Thing)projectile).Position, map);
            if (spawnedThing == null || !projectileType.IsInstanceOfType(spawnedThing))
            {
                spawnedThing?.Destroy(DestroyMode.Vanish);
                return false;
            }

            Thing launcher = GetLauncher(projectile);
            Vector3 origin = GetExactPosition(projectile);
            Vector3 targetPos = GetTargetPosition(nextTarget);
            Vector3 delta = targetPos - origin;
            Vector2 horizontal = new Vector2(delta.x, delta.z);
            if (horizontal.sqrMagnitude <= 0.001f)
            {
                spawnedThing.Destroy(DestroyMode.Vanish);
                return false;
            }

            object projectileProps = spawnedThing.def.projectile;
            object trajectoryWorker = trajectoryWorkerProperty.GetValue(projectileProps);
            if (trajectoryWorker == null)
            {
                spawnedThing.Destroy(DestroyMode.Vanish);
                return false;
            }

            float shotHeight = origin.y;
            float shotSpeed = GetShotSpeed(projectile, spawnedThing);
            float shotRotation = (float)shotRotationMethod.Invoke(trajectoryWorker, new object[] { projectileProps, origin, targetPos });
            float shotAngle = (float)shotAngleMethod.Invoke(trajectoryWorker, new object[] { projectileProps, origin, targetPos, (float?)shotSpeed });
            float minCollisionDistance = GetMinCollisionDistance(horizontal.magnitude);

            try
            {
                canTargetSelfField?.SetValue(spawnedThing, false);
                minCollisionDistanceField?.SetValue(spawnedThing, minCollisionDistance);
                intendedTargetField?.SetValue(spawnedThing, new LocalTargetInfo(nextTarget));
                launchMethod.Invoke(spawnedThing, new object[]
                {
                    launcher,
                    new Vector2(origin.x, origin.z),
                    shotAngle,
                    shotRotation,
                    shotHeight,
                    shotSpeed,
                    source,
                    horizontal.magnitude
                });
            }
            catch (Exception ex)
            {
                spawnedThing.Destroy(DestroyMode.Vanish);
                Log.Warning($"[Infusion] Failed to launch Combat Extended ricochet projectile {spawnedThing.def?.defName}: {ex}");
                return false;
            }

            projectileThingId = spawnedThing.thingIDNumber;
            return true;
        }
    }

    public sealed class CombatExtendedImpactPatchState
    {
        public Map map;
        public bool canRunOnHitWorkers;
        public ThingWithComps equipmentSource;
    }

    internal sealed class CombatExtendedProjectileInfo : ProjectileInfo
    {
        private readonly object projectile;
        private readonly Thing thing;

        public CombatExtendedProjectileInfo(object projectile)
        {
            this.projectile = projectile;
            thing = projectile as Thing;
        }

        public override float ArmorPenetration => CombatExtendedPatches.GetPenetrationAmount(projectile);

        public override Quaternion ExactRotation => CombatExtendedPatches.GetExactRotation(projectile);

        public override Thing Launcher => CombatExtendedPatches.GetLauncher(projectile);

        public override LocalTargetInfo intendedTarget => CombatExtendedPatches.GetIntendedTarget(projectile);

        public override IntVec3 Position => thing.Position;

        public override Vector3 ExactPosition => CombatExtendedPatches.GetExactPosition(projectile);

        public override ThingDef def => thing.def;

        public override int thingIDNumber => thing.thingIDNumber;

        public override bool TryLaunchRicochet(Map map, ThingWithComps source, Thing nextTarget, out int projectileThingId)
        {
            return CombatExtendedPatches.TryLaunchRicochet(projectile, map, source, nextTarget, out projectileThingId);
        }
    }
}
