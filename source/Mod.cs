using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Infusion;
using RimWorld;
using UnityEngine;
using Verse;

public class ModBase : Mod
{
    public static Harmony instance;

    public ModBase(ModContentPack content)
        : base(content)
    {
        instance = new Harmony("rimworld.sk.infusion");
        LongEventHandler.QueueLongEvent(DefsLoaded, "Sk.Infusion.Init", doAsynchronously: true, null);
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
        Infusion.Harmonize.StatWorker.populateStatsEligibilityMap();
        Constants.Init();
    }

    private void Inject()
    {
        InjectToThings();
        InjectToStats();
    }

    private void InjectToThings()
    {
        Type typeFromHandle = typeof(InfusedTab);
        InspectTabBase sharedInstance = InspectTabManager.GetSharedInstance(typeFromHandle);
        foreach (ThingDef item in DefsForReading.allThingsInfusable)
        {
            item.comps.Insert(0, new CompProperties(typeof(CompInfusion)));
            if (item.inspectorTabs.NullOrEmpty())
            {
                item.inspectorTabs = new List<Type>(1);
                item.inspectorTabsResolved = new List<InspectTabBase>(1);
            }
            item.inspectorTabs.Add(typeFromHandle);
            item.inspectorTabsResolved.Add(sharedInstance);
        }
    }

    private void InjectToStats()
    {
        IEnumerable<StatDef> enumerable = DefDatabase<InfusionDef>.AllDefs.SelectMany((InfusionDef def) => def.stats.Keys).Distinct();
        foreach (StatDef item2 in enumerable)
        {
            InfusionStatPart item = new InfusionStatPart(item2);
            if (item2.parts == null)
            {
                item2.parts = new List<StatPart>(1);
            }
            item2.parts.Add(item);
        }
        StatDef.SetImmutability();
    }
}
