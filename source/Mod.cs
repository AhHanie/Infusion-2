using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using UnityEngine;
using Verse;

namespace Infusion.Mod
{

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