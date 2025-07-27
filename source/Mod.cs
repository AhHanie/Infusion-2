using HarmonyLib;
using Infusion.Utils;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using UnityEngine;
using Verse;

namespace Infusion.Mod
{
    [StaticConstructorOnStartup]
    public class StartupConstructor
    {
        static StartupConstructor()
        {
            // Initialize the def hasher
            InjectedDefHasher.Initialize();

            //var infuserDefs = DefGenerator.makeInfuserDefs();

            //foreach (var def in infuserDefs)
            //{
            //    def.ResolveReferences();
            //    //DefGenerator.AddImpliedDef<ThingDef>(def);

            //    if (def.thingCategories != null)
            //    {
            //        foreach (var cat in def.thingCategories)
            //        {
            //            cat.childThingDefs.Add(def);
            //        }
            //    }

            //    InjectedDefHasher.GiveShortHashToDef(def, typeof(ThingDef));
            //}

            //// Resolve references for filters
            //// ThingCategory itself (for stockpiles)
            //var infusionCategory = ThingCategoryDef.Named("Infusion_Infusers");
            //if (infusionCategory != null)
            //{
            //    infusionCategory.ResolveReferences();
            //}

            //// Storable buildings require separate calls
            //var storageSettings = DefsForReading.allBuildings
            //    .Where(def => def.building != null)
            //    .SelectMany(def =>
            //    {
            //        var building = def.building;
            //        var settings = new List<StorageSettings>();

            //        if (building.defaultStorageSettings != null)
            //            settings.Add(building.defaultStorageSettings);
            //        if (building.fixedStorageSettings != null)
            //            settings.Add(building.fixedStorageSettings);

            //        return settings;
            //    });

            //foreach (var storage in storageSettings)
            //{
            //    storage.filter.ResolveReferences();
            //}
        }
    }

    public class ModBase : Verse.Mod
    {
        public static Harmony instance;
        public ModBase(ModContentPack content) : base(content)
        {
            instance = new Harmony("rimworld.sk.infusion");

            LongEventHandler.QueueLongEvent(DefsLoaded, "Sk.Infusion.Init", true, null);
        }

        public override string SettingsCategory()
        {
            return "Infusion 2";
        }

        public override void DoSettingsWindowContents(Rect rect)
        {
            ModSettingsWindow.Draw(rect);
            base.DoSettingsWindowContents(rect);
        }

        public void DefsLoaded()
        {
            GetSettings<Settings>();
            instance.PatchAll();
            Inject();
            Harmonize.StatWorker.populateStatsEligibilityMap();
        }

        private void Inject()
        {
            InjectToThings();
            InjectToStats();
        }

        private void InjectToThings()
        {
            var iTabType = typeof(InfusedTab);
            var iTab = InspectTabManager.GetSharedInstance(iTabType);

            foreach (var def in DefsForReading.allThingsInfusable)
            {
                // Needs to be the first one, label making is order-dependent
                def.comps.Insert(0, new CompProperties(typeof(CompInfusion)));

                if (def.inspectorTabs.NullOrEmpty())
                {
                    def.inspectorTabs = new List<Type>(1);
                    def.inspectorTabsResolved = new List<InspectTabBase>(1);
                }

                def.inspectorTabs.Add(iTabType);
                def.inspectorTabsResolved.Add(iTab);
            }
        }

        private void InjectToStats()
        {
            // Many stats are now cached at StatWorker level, and some of them with certain conditions met
            // (e.g. No specific StatWorker, no StatParts) are considered immutable with their cache never expiring.
            // In other words, adding a StatPart causes immutability check to break which may cause minor
            // performance degradation. So we only add our StatPart to stats which have related infusions.

            var statsWithInfusions = DefDatabase<InfusionDef>.AllDefs
                .SelectMany(def => def.stats.Keys)
                .Distinct();

            foreach (var statDef in statsWithInfusions)
            {
                var statPart = new InfusionStatPart(statDef);

                if (statDef.parts == null)
                {
                    statDef.parts = new List<StatPart>(1);
                }

                statDef.parts.Add(statPart);
            }

            // And we have to manually update the caches
            StatDef.SetImmutability();
        }
    }
}