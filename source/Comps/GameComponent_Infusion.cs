using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Infusion.Comps
{
    public class GameComponent_Infusion : GameComponent
    {
        private List<ThingWithComps> aegisItems = new List<ThingWithComps>();
        private Dictionary<ThingWithComps, AegisShieldCompData> aegisShieldData = new Dictionary<ThingWithComps, AegisShieldCompData>();
        List<ThingWithComps> tempKeys = null;
        List<AegisShieldCompData> tempValues = null;

        static GameComponent_Infusion()
        {
            
        }

        public GameComponent_Infusion(Game game)
        {
            aegisItems = new List<ThingWithComps>();
            aegisShieldData = new Dictionary<ThingWithComps, AegisShieldCompData>();
        }

        public override void ExposeData()
        {
            base.ExposeData();

            if (Scribe.mode == LoadSaveMode.Saving)
            {
                aegisShieldData.Clear();

                foreach (ThingWithComps item in aegisItems)
                {
                    if (item != null && !item.DestroyedOrNull())
                    {
                        CompShield shieldComp = item.TryGetComp<CompShield>();
                        if (shieldComp != null)
                        {
                            float energy = (float)Constants.energyField.GetValue(shieldComp);
                            int ticksToReset = (int)Constants.ticksToResetField.GetValue(shieldComp);
                            int lastKeepDisplayTick = (int)Constants.lastKeepDisplayTickField.GetValue(shieldComp);

                            aegisShieldData[item] = new AegisShieldCompData(energy, ticksToReset, lastKeepDisplayTick);
                        }
                    }
                }
            }

            Scribe_Collections.Look(ref aegisItems, "aegisItems", LookMode.Reference);
            Scribe_Collections.Look(ref aegisShieldData, "aegisShieldData", LookMode.Reference, LookMode.Deep, ref tempKeys, ref tempValues);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (aegisItems == null)
                {
                    aegisItems = new List<ThingWithComps>();
                }
                else
                {
                    aegisItems.RemoveAll(x => x == null);
                }

                if (aegisShieldData == null)
                {
                    aegisShieldData = new Dictionary<ThingWithComps, AegisShieldCompData>();
                }
                else
                {
                    var nullKeys = aegisShieldData.Where(kvp => kvp.Key == null).Select(kvp => kvp.Key).ToList();
                    foreach (var key in nullKeys)
                    {
                        aegisShieldData.Remove(key);
                    }
                }

                if (aegisShieldData != null && aegisShieldData.Count > 0)
                {
                    foreach (var kvp in aegisShieldData)
                    {
                        ThingWithComps item = kvp.Key;
                        AegisShieldCompData data = kvp.Value;

                        if (item != null && !item.DestroyedOrNull() && data != null)
                        {
                            List<ThingComp> compList = (List<ThingComp>)Constants.thingWithCompsCompsField.GetValue(item);
                            CompProperties_Shield compProperties_Shield = new CompProperties_Shield();
                            ThingComp thingComp = (ThingComp)Activator.CreateInstance(compProperties_Shield.compClass);
                            thingComp.parent = item;
                            compList.Add(thingComp);
                            Dictionary<Type, ThingComp[]> dictionary = (Dictionary<Type, ThingComp[]>)Constants.thingWithCompsCompsByTypeField.GetValue(item);
                            List<ThingComp> list2 = new List<ThingComp> { thingComp };
                            dictionary.Add(compProperties_Shield.compClass, list2.ToArray());
                            thingComp.Initialize(compProperties_Shield);
                            CompShield shieldComp = item.TryGetComp<CompShield>();
                            Constants.energyField.SetValue(shieldComp, data.Energy);
                            Constants.ticksToResetField.SetValue(shieldComp, data.TicksToReset);
                            Constants.lastKeepDisplayTickField.SetValue(shieldComp, data.LastKeepDisplayTick);
                        }
                    }

                    aegisShieldData.Clear();
                }
            }
        }

        public void AddAegisItem(ThingWithComps item)
        {
            if (!aegisItems.Contains(item))
            {
                aegisItems.Add(item);
            }
        }

        public void RemoveAegisItem(ThingWithComps item)
        {
            aegisItems.Remove(item);
        }

        public bool ContainsAegisItem(ThingWithComps item)
        {
            return aegisItems.Contains(item);
        }
    }
}