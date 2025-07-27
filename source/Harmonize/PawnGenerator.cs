using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Infusion.Harmonize
{
    public static class PawnGeneratorPatches
    {
        private static void HandleGear(ThingWithComps thing)
        {
            var compInfusion = thing.TryGetComp<CompInfusion>();
            var compBiocodable = thing.TryGetComp<CompBiocodable>();

            // Filter biocodable component - only proceed if it's biocodable and biocoded
            if (compBiocodable?.Biocodable == true && compBiocodable.Biocoded)
            {
                if (compInfusion != null)
                {
                    var qualityInt = (byte)compInfusion.Quality + 2;

                    // Clamp the quality to legendary maximum
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
                }
            }
        }

        [HarmonyPatch(typeof(PawnGenerator), "GenerateGearFor")]
        public static class GenerateGearFor
        {
            /// <summary>
            /// Postprocess biocoded equipments
            /// </summary>
            public static void Postfix(Pawn pawn, PawnGenerationRequest request)
            {
                if (Settings.biocodeBonus.Value)
                {
                    // Handle worn apparel
                    if (pawn.apparel?.WornApparel != null)
                    {
                        foreach (var apparel in pawn.apparel.WornApparel)
                        {
                            HandleGear(apparel);
                        }
                    }

                    // Handle primary equipment
                    if (pawn.equipment?.Primary != null)
                    {
                        HandleGear(pawn.equipment.Primary);
                    }
                }
            }
        }
    }
}