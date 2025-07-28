using HarmonyLib;
using RimWorld;
using Verse;

namespace Infusion.Harmonize
{
    [HarmonyPatch(typeof(Verb_MeleeAttackDamage), "ApplyMeleeDamageToTarget")]
    public static class VerbMeleeAttackDamagePatches
    {
        public static class ApplyMeleeDamageToTarget
        {
            public static void Postfix(LocalTargetInfo target, Verb_MeleeAttackDamage __instance)
            {
                var equipmentSource = __instance.EquipmentSource;
                if (equipmentSource?.def?.IsMeleeWeapon != true)
                    return;

                var onHitData = CompInfusionExtensions.ForOnHitWorkers(equipmentSource);
                if (!onHitData.HasValue)
                    return;

                var (workers, comp) = onHitData.Value;

                foreach (var onHit in workers)
                {
                    var verbData = new VerbRecordData(
                        baseDamage: GetAdjustedMeleeDamage(__instance),
                        source: comp.parent,
                        target: target.Thing,
                        verb: __instance
                    );

                    onHit.MeleeHit(verbData);
                }
            }
        }
        private static float GetAdjustedMeleeDamage(Verb_MeleeAttackDamage verb)
        {
            // This would need to be implemented based on your mod's damage calculation logic
            // For now, returning a basic damage amount from the verb properties
            if (verb.verbProps?.meleeDamageDef != null)
            {
                return verb.verbProps.meleeDamageDef.defaultDamage;
            }
            return 0f;
        }
    }
}