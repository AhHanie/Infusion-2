using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse;
using RimWorld;
using Infusion.Comps;

namespace Infusion.Harmonize
{
    [HarmonyPatch(typeof(Verb_LaunchProjectile), "TryCastShot")]
    public static class TryCastShot_SmartInfusion
    {
        //      Projectile projectile2 = (Projectile)GenSpawn.Spawn(projectile, resultingLine.Source, caster.Map);
		//if (equipmentSource.TryGetComp(out CompUniqueWeapon comp))
		//{
		//	foreach (WeaponTraitDef item in comp.TraitsListForReading)
		//	{
		//		if (item.damageDefOverride != null)
		//		{
		//			projectile2.damageDefOverride = item.damageDefOverride;
		//		}
		//		if (!item.extraDamages.NullOrEmpty())
		//		{
		//			Projectile projectile3 = projectile2;
		//			if (projectile3.extraDamages == null)
		//			{
		//				projectile3.extraDamages = new List<ExtraDamage>();
		//			}
        //      projectile2.extraDamages.AddRange(item.extraDamages);
		//		}
		//	}
		//}
        // <--------------------- Transpiler inserts code here
		//if (verbProps.ForcedMissRadius > 0.5f) {
        // Vanilla 1.6 declares these local slots in TryCastShot: 3 = manningPawn, 4 = equipmentSource,
        // 6 = drawPos, 7 = projectile2. Vanilla Gravship Expanded's transpilers overwrite the *values*
        // stored in slots 3/4/6 (adjusted manning pawn, equipment source, and cast-source draw pos) but
        // do not renumber the locals, so these indices remain valid whether or not VGE has run first.
        private static bool IsForcedMissBranch(OpCode opcode) =>
            opcode == OpCodes.Ble || opcode == OpCodes.Ble_S ||
            opcode == OpCodes.Ble_Un || opcode == OpCodes.Ble_Un_S;

        private static bool IsHalfConstant(CodeInstruction instruction) =>
            instruction.opcode == OpCodes.Ldc_R4 && instruction.operand is float value && Math.Abs(value - 0.5f) < 0.0001f;

        // Structural match for VGE's Verb_LaunchProjectile_ForcedMissRadius_Patch.GetAdjustedForcedMissRadius,
        // recognized by shape (name/signature) rather than a compile-time reference to the VGE assembly.
        private static bool IsAdjustedForcedMissRadiusCall(CodeInstruction instruction)
        {
            if (instruction.opcode != OpCodes.Call && instruction.opcode != OpCodes.Callvirt)
            {
                return false;
            }

            if (!(instruction.operand is MethodInfo method) || method.Name != "GetAdjustedForcedMissRadius" || method.ReturnType != typeof(float))
            {
                return false;
            }

            ParameterInfo[] parameters = method.GetParameters();
            return parameters.Length == 2 &&
                parameters[0].ParameterType == typeof(VerbProperties) &&
                parameters[1].ParameterType == typeof(Verb);
        }

        // Finds the first ldarg.0 of either recognized forced-miss-radius anchor shape, so the
        // evaluation stack is empty when the Smart launch check is inserted before it.
        private static (int index, string shape) FindForcedMissAnchor(List<CodeInstruction> codes, FieldInfo verbPropsField, MethodInfo forcedMissRadiusGetter)
        {
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode != OpCodes.Ldarg_0 || i + 1 >= codes.Count || !codes[i + 1].LoadsField(verbPropsField))
                {
                    continue;
                }

                // Vanilla shape: ldarg.0, ldfld verbProps, call/callvirt get_ForcedMissRadius, ldc.r4 0.5, ble(.un)(.s)
                if (i + 4 < codes.Count &&
                    codes[i + 2].Calls(forcedMissRadiusGetter) &&
                    IsHalfConstant(codes[i + 3]) &&
                    IsForcedMissBranch(codes[i + 4].opcode))
                {
                    return (i, "vanilla");
                }

                // VGE-rewritten shape: ldarg.0, ldfld verbProps, ldarg.0, call GetAdjustedForcedMissRadius(VerbProperties, Verb), ldc.r4 0.5, ble(.un)(.s)
                if (i + 5 < codes.Count &&
                    codes[i + 2].opcode == OpCodes.Ldarg_0 &&
                    IsAdjustedForcedMissRadiusCall(codes[i + 3]) &&
                    IsHalfConstant(codes[i + 4]) &&
                    IsForcedMissBranch(codes[i + 5].opcode))
                {
                    return (i, "VGE adjusted forced-miss radius");
                }
            }

            return (-1, null);
        }

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var codes = new List<CodeInstruction>(instructions);
            var tryLaunchMethod = AccessTools.Method(typeof(SmartInfusionHelper), nameof(SmartInfusionHelper.TryLaunchProjectileSmart));
            var verbPropsField = AccessTools.Field(typeof(Verb), "verbProps");
            var forcedMissRadiusGetter = AccessTools.PropertyGetter(typeof(VerbProperties), nameof(VerbProperties.ForcedMissRadius));

            // Find the convergence point after CompUniqueWeapon processing (whether it exists or not),
            // recognizing both vanilla's IL and VGE's transpiled rewrite of the same check.
            (int insertIndex, string matchedShape) = FindForcedMissAnchor(codes, verbPropsField, forcedMissRadiusGetter);

            if (insertIndex == -1)
            {
                Log.Error($"[Infusion] Could not find the vanilla or VGE-adjusted ForcedMissRadius anchor in {typeof(Verb_LaunchProjectile).FullName}.TryCastShot; Smart infusion launch injection was skipped.");
                return codes;
            }

            if (Prefs.DevMode)
            {
                Log.Message($"[Infusion] Smart infusion launch injected using the {matchedShape} ForcedMissRadius anchor.");
            }

            // Insert our smart launch check at this convergence point
            var continueLabel = generator.DefineLabel();

            var smartLaunchInstructions = new List<CodeInstruction>
            {
                // Load all arguments for TryLaunchProjectileSmart
                new CodeInstruction(OpCodes.Ldloc_S, 7),  // projectile2
                new CodeInstruction(OpCodes.Ldloc_3),     // manningPawn
                new CodeInstruction(OpCodes.Ldloc_S, 6),  // drawPos
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Verb), "currentTarget")),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Verb), "preventFriendlyFire")),
                new CodeInstruction(OpCodes.Ldloc_S, 4),  // equipmentSource
                
                // Call TryLaunchProjectileSmart
                new CodeInstruction(OpCodes.Call, tryLaunchMethod),
                
                // If true, return true
                new CodeInstruction(OpCodes.Brfalse, continueLabel),
                new CodeInstruction(OpCodes.Ldc_I4_1),
                new CodeInstruction(OpCodes.Ret)
            };

            // Insert our code before the ForcedMissRadius check
            codes.InsertRange(insertIndex, smartLaunchInstructions);

            // Add continue label
            var continueInstruction = codes[insertIndex + smartLaunchInstructions.Count];
            continueInstruction.labels.Add(continueLabel);

            // Move any pre-existing labels (branch/leave targets) from the original instruction to our
            // first instruction, leaving only the new continuation label on the original instruction.
            if (continueInstruction.labels.Count > 1)
            {
                var firstInstruction = codes[insertIndex];

                var labelsToMove = continueInstruction.labels.Where(l => !l.Equals(continueLabel)).ToList();
                firstInstruction.labels.AddRange(labelsToMove);
                continueInstruction.labels.RemoveAll(l => labelsToMove.Contains(l));
            }

            // Move any exception-block boundary metadata (e.g. an EndExceptionBlock marking the end of
            // the preceding try/finally) the same way, so it still marks the boundary at the start of the
            // injected block rather than pointing into its middle.
            if (continueInstruction.blocks.Count > 0)
            {
                var firstInstruction = codes[insertIndex];
                firstInstruction.blocks.AddRange(continueInstruction.blocks);
                continueInstruction.blocks.Clear();
            }

            return codes;
        }
    }

    [HarmonyPatch(typeof(Verb_LaunchProjectile), "WarmupComplete")]
    public static class WarmupComplete_UnstableInfusion
    {
        public static void Prefix(Verb_LaunchProjectile __instance)
        {
            ThingWithComps equipmentSource = __instance.EquipmentSource;

            if (equipmentSource == null)
            {
                return;
            }

            GameComponent_Infusion gameComp = Current.Game.GetComponent<GameComponent_Infusion>();

            // Ensure no stale projectile remains from a previous burst.
            gameComp.ClearUnstableBurstProjectile(equipmentSource);

            CompInfusion compInfusion = equipmentSource.TryGetComp<CompInfusion>();
            if (compInfusion == null)
            {
                return;
            }

            InfusionDef unstableInfusion = compInfusion.TryGetInfusionDefWithTag(InfusionTags.UNSTABLE);
            if (unstableInfusion == null)
            {
                return;
            }

            ThingDef originalProjectile = __instance.Projectile;
            if (!typeof(Bullet).IsAssignableFrom(originalProjectile.thingClass))
            {
                return;
            }

            ThingDef randomizedProjectile = Infusion.UnstableProjectileHelper.GetRandomBulletProjectile(originalProjectile);
            gameComp.SetUnstableBurstProjectile(equipmentSource, randomizedProjectile);
        }
    }

    [HarmonyPatch(typeof(Verb_LaunchProjectile), "Projectile", MethodType.Getter)]
    public static class Projectile_UnstableInfusion
    {
        public static void Postfix(Verb_LaunchProjectile __instance, ref ThingDef __result)
        {
            ThingWithComps equipmentSource = __instance.EquipmentSource;

            if (equipmentSource == null)
            {
                return;
            }

            CompInfusion compInfusion = equipmentSource.TryGetComp<CompInfusion>();
            if (compInfusion?.TryGetInfusionDefWithTag(InfusionTags.UNSTABLE) == null)
            {
                return;
            }

            GameComponent_Infusion gameComp = Current.Game.GetComponent<GameComponent_Infusion>();

            if (gameComp.TryGetUnstableBurstProjectile(equipmentSource, out ThingDef cachedProjectile))
            {
                __result = cachedProjectile;
            }
        }
    }
}
