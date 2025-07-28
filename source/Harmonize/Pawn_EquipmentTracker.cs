using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Infusion.Harmonize
{
    [HarmonyPatch(typeof(Pawn_EquipmentTracker), "GetGizmos")]
    public static class PawnEquipmentTrackerPatches
    {
        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, Pawn_EquipmentTracker __instance)
        {
            var pawn = __instance.pawn;

            foreach (var gizmo in __result)
            {
                yield return gizmo;
            }

            if (pawn.IsColonistPlayerControlled ||
                pawn.IsColonyMech ||
                pawn.IsColonySubhumanPlayerControlled)
            {
                var firstEquipment = __instance.AllEquipmentListForReading?.FirstOrDefault();
                var compInfusion = firstEquipment?.TryGetComp<CompInfusion>();
                var effectGizmo = compInfusion?.EffectGizmo;

                if (effectGizmo != null)
                {
                    yield return effectGizmo;
                }
            }
        }
    }
}