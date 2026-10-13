using System;
using System.Collections.Generic;
using System.Linq;
using LudeonTK;
using RimWorld;
using Verse;

namespace Infusion
{
    public static class DebugActions
    {
        private static List<QualityCategory> ListAllQualities()
        {
            return Enum.GetValues(typeof(QualityCategory))
                .Cast<QualityCategory>()
                .ToList();
        }

        private static void SpawnThingOfQuality(ThingDef def, QualityCategory qc)
        {
            var stuff = GenStuff.RandomStuffFor(def);
            var thing = ThingMaker.MakeThing(def, stuff);

            var compQuality = thing.TryGetComp<CompQuality>();
            compQuality?.SetQuality(qc, ArtGenerationContext.Colony);

            GenPlace.TryPlaceThing(thing, UI.MouseCell(), Find.CurrentMap, ThingPlaceMode.Near);
        }

        private static void OpenDebugOptionsLister(IEnumerable<DebugMenuOption> options)
        {
            Find.WindowStack.Add(new Dialog_DebugOptionListLister(options.ToList()));
        }

        private static List<DebugMenuOption> EquipmentSelector(Func<ThingDef, bool> predicate)
        {
            return DefsForReading.allThingsInfusable
                .Where(predicate)
                .Select(def =>
                {
                    Action onClick = () =>
                    {
                        var qualityOptions = ListAllQualities()
                            .Select(qc => new DebugMenuOption(
                                Enum.GetName(typeof(QualityCategory), qc),
                                DebugMenuOptionMode.Tool,
                                () => SpawnThingOfQuality(def, qc)
                            ));

                        OpenDebugOptionsLister(qualityOptions);
                    };

                    return new DebugMenuOption(def.defName, DebugMenuOptionMode.Action, onClick);
                })
                .ToList();
        }

        [DebugAction("Infusion", "Spawn weapon with quality...",
            actionType = DebugActionType.Action,
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void SpawnWeaponWithQuality()
        {
            var weaponOptions = EquipmentSelector(def => def.IsWeapon);
            OpenDebugOptionsLister(weaponOptions);
        }

        [DebugAction("Infusion", "Spawn apparel with quality...",
            actionType = DebugActionType.Action,
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void SpawnApparelWithQuality()
        {
            var apparelOptions = EquipmentSelector(def => def.IsApparel);
            OpenDebugOptionsLister(apparelOptions);
        }

        private static IEnumerable<Thing> PointedThings()
        {
            return Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell());
        }

        private static T FirstCompAtPointer<T>(IEnumerable<Thing> things) where T : ThingComp
        {
            return things
                .Select(thing => thing.TryGetComp<T>())
                .FirstOrDefault(comp => comp != null);
        }

        private static void InfusePointed(InfusionDef infDef)
        {
            var comp = FirstCompAtPointer<CompInfusion>(PointedThings());
            comp?.AddInfusion(infDef);
        }

        [DebugAction("Infusion", "Infuse...", actionType = DebugActionType.ToolMap)]
        public static void AddInfusion()
        {
            var infusionOptions = DefDatabase<InfusionDef>.AllDefs
                .Where(InfusionDef.ActiveForUse)
                .OrderBy(infDef => infDef.defName)
                .Select(infDef => new DebugMenuOption(
                    infDef.defName,
                    DebugMenuOptionMode.Tool,
                    () => InfusePointed(infDef)
                ));

            OpenDebugOptionsLister(infusionOptions);
        }

        [DebugAction("Infusion", "Remove an infusion...", actionType = DebugActionType.ToolMap)]
        public static void RemoveInfusion()
        {
            var comp = FirstCompAtPointer<CompInfusion>(PointedThings());
            if (comp != null)
            {
                var removalOptions = comp.Infusions
                    .Select(infDef => new DebugMenuOption(
                        infDef.defName,
                        DebugMenuOptionMode.Action,
                        () => comp.RemoveInfusion(infDef)
                    ));

                OpenDebugOptionsLister(removalOptions);
            }
        }

        [DebugAction("Infusion", "Remove all infusions", actionType = DebugActionType.ToolMap)]
        public static void RemoveAllInfusions()
        {
            var comp = FirstCompAtPointer<CompInfusion>(PointedThings());
            int maxHitPoints = comp.parent.MaxHitPoints;
            comp?.SetInfusions(Enumerable.Empty<InfusionDef>(), false);
            comp?.TryUpdateMaxHitpoints(maxHitPoints);
        }

        [DebugAction("Infusion", "Reroll infusions", actionType = DebugActionType.ToolMap)]
        public static void RerollInfusions()
        {
            var comp = FirstCompAtPointer<CompInfusion>(PointedThings());
            comp?.RerollInfusions();
        }

        [DebugAction("Infusion", "Reroll everything in current map",
            actionType = DebugActionType.Action,
            allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void RerollEverything()
        {
            var thingLister = Find.CurrentMap.listerThings;

            var allEquipment = thingLister.ThingsInGroup(ThingRequestGroup.Weapon)
                .Concat(thingLister.ThingsInGroup(ThingRequestGroup.Apparel));

            foreach (var thing in allEquipment)
            {
                var comp = thing.TryGetComp<CompInfusion>();
                if (comp != null && comp.SlotCount > 0)
                {
                    comp.RerollInfusions();
                }
            }
        }
    }
}