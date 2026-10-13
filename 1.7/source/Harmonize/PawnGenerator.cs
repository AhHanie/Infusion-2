using HarmonyLib;
using RimWorld;
using System.Linq;
using System.Reflection;
using Verse;

namespace Infusion.Harmonize
{
    public static class PawnGeneratorPatches
    {
        private static void HandleGear(ThingWithComps thing)
        {
            var compInfusion = thing.TryGetComp<CompInfusion>();
            var compBiocodable = thing.TryGetComp<CompBiocodable>();

            if (compBiocodable?.Biocodable == true && compBiocodable.Biocoded)
            {
                if (compInfusion != null)
                {
                    var qualityInt = (byte)compInfusion.Quality + 2;

                    QualityCategory quality;
                    if (qualityInt > (byte)QualityCategory.Legendary)
                    {
                        quality = QualityCategory.Legendary;
                    }
                    else
                    {
                        quality = (QualityCategory)qualityInt;
                    }

                    compInfusion.Biocoder = compBiocodable;
                    compInfusion.SlotCount = compInfusion.CalculateSlotCountFor(quality);
                    compInfusion.SetInfusions(compInfusion.PickInfusions(quality), false);
                    compInfusion.TryUpdateMaxHitpoints();
                }
            }
        }

        [HarmonyPatch(typeof(PawnGenerator), "GenerateGearFor")]
        public static class GenerateGearFor
        {
            public static bool Prepare(MethodBase original)
            {
                return !Settings.disableItemInfusion.Value;
            }

            public static void Postfix(Pawn pawn, PawnGenerationRequest request)
            {
                if (Settings.biocodeBonus.Value)
                {
                    if (pawn.apparel?.WornApparel != null)
                    {
                        foreach (var apparel in pawn.apparel.WornApparel)
                        {
                            HandleGear(apparel);
                        }
                    }

                    if (pawn.equipment?.Primary != null)
                    {
                        HandleGear(pawn.equipment.Primary);
                    }
                }
            }
        }
    }
}