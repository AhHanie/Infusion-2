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
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var codes = new List<CodeInstruction>(instructions);
            var tryLaunchMethod = AccessTools.Method(typeof(SmartInfusionHelper), nameof(SmartInfusionHelper.TryLaunchProjectileSmart));

            // Find the pattern: ldarg.0, ldfld verbProps, callvirt get_ForcedMissRadius, ldc.r4 0.5
            // This is the convergence point after CompUniqueWeapon check (whether it exists or not)
            int insertIndex = -1;
            for (int i = 0; i < codes.Count - 3; i++)
            {
                if (codes[i].opcode == OpCodes.Ldarg_0 &&
                    codes[i + 1].opcode == OpCodes.Ldfld &&
                    codes[i + 1].operand is FieldInfo field && field.Name == "verbProps" &&
                    codes[i + 2].opcode == OpCodes.Callvirt &&
                    codes[i + 2].operand is MethodInfo method && method.Name == "get_ForcedMissRadius" &&
                    codes[i + 3].opcode == OpCodes.Ldc_R4 &&
                    Math.Abs((float)codes[i + 3].operand - 0.5f) < 0.001f)
                {
                    insertIndex = i;
                    break;
                }
            }

            if (insertIndex == -1)
            {
                Log.Error("[Infusion] Could not find ForcedMissRadius check pattern!");
                return codes;
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
            if (continueInstruction.labels == null)
                continueInstruction.labels = new List<Label>();
            continueInstruction.labels.Add(continueLabel);

            // Move any existing labels from the original instruction to our first instruction
            if (continueInstruction.labels.Count > 1)
            {
                var firstInstruction = codes[insertIndex];
                if (firstInstruction.labels == null)
                    firstInstruction.labels = new List<Label>();

                var labelsToMove = continueInstruction.labels.Where(l => !l.Equals(continueLabel)).ToList();
                firstInstruction.labels.AddRange(labelsToMove);
                continueInstruction.labels.RemoveAll(l => labelsToMove.Contains(l));
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
