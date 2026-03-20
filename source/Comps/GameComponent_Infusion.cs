using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Infusion.Helpers;

namespace Infusion.Comps
{
    public class GameComponent_Infusion : GameComponent
    {
        private List<ThingWithComps> aegisItems = new List<ThingWithComps>();
        private Dictionary<ThingWithComps, AegisShieldCompData> aegisShieldData = new Dictionary<ThingWithComps, AegisShieldCompData>();
        private Dictionary<ThingWithComps, ThingDef> unstableBurstProjectiles = new Dictionary<ThingWithComps, ThingDef>();
        private List<PendingNecrosis> pendingNecrosis = new List<PendingNecrosis>();
        private List<PendingHitPointsReset> pendingHitPointsResets = new List<PendingHitPointsReset>();
        private List<ThingWithComps> tempKeys = null;
        private List<AegisShieldCompData> tempValues = null;
        private List<ThingWithComps> unstableTempKeys = null;
        private List<ThingDef> unstableTempValues = null;
        private ApparelInfusionPolicyStore apparelInfusionPolicies = new ApparelInfusionPolicyStore();
        public static int currentGameTick = 0;

        static GameComponent_Infusion()
        {
            
        }

        public GameComponent_Infusion(Game game)
        {
            aegisItems = new List<ThingWithComps>();
            aegisShieldData = new Dictionary<ThingWithComps, AegisShieldCompData>();
            unstableBurstProjectiles = new Dictionary<ThingWithComps, ThingDef>();
            pendingNecrosis = new List<PendingNecrosis>();
            pendingHitPointsResets = new List<PendingHitPointsReset>();
            apparelInfusionPolicies = new ApparelInfusionPolicyStore();
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

                unstableBurstProjectiles = unstableBurstProjectiles
                    .Where(kvp => kvp.Key != null && !kvp.Key.DestroyedOrNull() && kvp.Value != null)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }

            Scribe_Collections.Look(ref aegisItems, "aegisItems", LookMode.Reference);
            Scribe_Collections.Look(ref aegisShieldData, "aegisShieldData", LookMode.Reference, LookMode.Deep, ref tempKeys, ref tempValues);
            Scribe_Collections.Look(ref unstableBurstProjectiles, "unstableBurstProjectiles", LookMode.Reference, LookMode.Def, ref unstableTempKeys, ref unstableTempValues);
            NecrosisHelper.ExposeData(ref pendingNecrosis);
            Scribe_Collections.Look(ref pendingHitPointsResets, "pendingHitPointsResets", LookMode.Deep);
            Scribe_Deep.Look(ref apparelInfusionPolicies, "apparelInfusionPolicies");

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

                if (unstableBurstProjectiles == null)
                {
                    unstableBurstProjectiles = new Dictionary<ThingWithComps, ThingDef>();
                }
                else
                {
                    unstableBurstProjectiles = unstableBurstProjectiles
                        .Where(kvp => kvp.Key != null && !kvp.Key.DestroyedOrNull() && kvp.Value != null)
                        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                }

                NecrosisHelper.PostLoadInit(ref pendingNecrosis);
                if (pendingHitPointsResets == null)
                {
                    pendingHitPointsResets = new List<PendingHitPointsReset>();
                }
                else
                {
                    pendingHitPointsResets.RemoveAll(x => x == null || x.thing == null);
                }

                if (apparelInfusionPolicies == null)
                {
                    apparelInfusionPolicies = new ApparelInfusionPolicyStore();
                }
                else
                {
                    apparelInfusionPolicies.RemoveMissingPolicies(Current.Game.outfitDatabase.AllOutfits);
                }
            }
        }

        public override void GameComponentTick()
        {
            currentGameTick = Find.TickManager.TicksGame;
            NecrosisHelper.Tick(pendingNecrosis);
            TickHitPointResets();

            if (currentGameTick % 36000 == 0)
            {
                Harmonize.InfusionStatWorkerCache.ClearGearCache();
            } 
        }

        public void QueueNecrosis(Corpse corpse, Apparel apparel, int delayTicks = 600)
        {
            NecrosisHelper.Queue(pendingNecrosis, corpse, apparel, delayTicks);
        }

        public void QueueHitPointReset(ThingWithComps thing, int delayTicks = 10)
        {
            int now = Find.TickManager.TicksGame;
            int triggerTick = now + delayTicks;
            pendingHitPointsResets.Add(new PendingHitPointsReset(thing, now, triggerTick));
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

        public void SetUnstableBurstProjectile(ThingWithComps source, ThingDef projectileDef)
        {
            unstableBurstProjectiles[source] = projectileDef;
        }

        public bool TryGetUnstableBurstProjectile(ThingWithComps source, out ThingDef projectileDef)
        {
            return unstableBurstProjectiles.TryGetValue(source, out projectileDef) && projectileDef != null;
        }

        public void ClearUnstableBurstProjectile(ThingWithComps source)
        {
            unstableBurstProjectiles.Remove(source);
        }

        public bool AllowsApparelForPolicy(ApparelPolicy policy, Apparel apparel)
        {
            return apparelInfusionPolicies.AllowsApparel(policy, apparel);
        }

        public bool IsApparelInfusionDisallowed(ApparelPolicy policy, InfusionDef infusion)
        {
            return apparelInfusionPolicies.IsDisallowed(policy, infusion);
        }

        public int CountDisallowedApparelInfusions(ApparelPolicy policy)
        {
            return apparelInfusionPolicies.CountDisallowedInfusions(policy);
        }

        public IEnumerable<InfusionDef> GetDisallowedApparelInfusions(ApparelPolicy policy)
        {
            return apparelInfusionPolicies.GetDisallowedInfusions(policy);
        }

        public bool HasCustomApparelInfusionRestrictions(ApparelPolicy policy)
        {
            return apparelInfusionPolicies.HasCustomRestrictions(policy);
        }

        public void SetApparelInfusionAllowed(ApparelPolicy policy, InfusionDef infusion, bool allowed)
        {
            apparelInfusionPolicies.SetAllowed(policy, infusion, allowed);
        }

        public void SetDisallowedApparelInfusions(ApparelPolicy policy, IEnumerable<InfusionDef> infusions)
        {
            apparelInfusionPolicies.SetDisallowedInfusions(policy, infusions);
        }

        public void RemoveApparelInfusionPolicy(ApparelPolicy policy)
        {
            apparelInfusionPolicies.RemovePolicy(policy);
        }

        public void PruneMissingApparelInfusionPolicies()
        {
            apparelInfusionPolicies.RemoveMissingPolicies(Current.Game?.outfitDatabase?.AllOutfits);
        }

        private void TickHitPointResets()
        {
            if (pendingHitPointsResets.Count == 0)
            {
                return;
            }

            for (int i = pendingHitPointsResets.Count - 1; i >= 0; i--)
            {
                PendingHitPointsReset pending = pendingHitPointsResets[i];
                if (pending == null || pending.thing == null || pending.thing.DestroyedOrNull())
                {
                    pendingHitPointsResets.RemoveAt(i);
                    continue;
                }

                if (currentGameTick < pending.triggerTick)
                {
                    continue;
                }

                pending.thing.HitPoints = pending.thing.MaxHitPoints;
                pendingHitPointsResets.RemoveAt(i);
            }
        }
    }
}
