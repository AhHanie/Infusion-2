using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Infusion
{
    public enum BestInfusionLabelLength
    {
        Long,
        Short
    }

    public class CompInfusion : ThingComp, IComparable
    {
        private static HashSet<CompInfusion> wantingCandidates = new HashSet<CompInfusion>();
        private static HashSet<CompInfusion> extractionCandidates = new HashSet<CompInfusion>();
        private static HashSet<CompInfusion> removalCandidates = new HashSet<CompInfusion>();

        private HashSet<InfusionDef> infusions = new HashSet<InfusionDef>();
        private HashSet<InfusionDef> wantingSet = new HashSet<InfusionDef>();
        private HashSet<InfusionDef> extractionSet = new HashSet<InfusionDef>();
        private HashSet<InfusionDef> removalSet = new HashSet<InfusionDef>();

        private CompBiocodable biocoder = null;
        private bool effectsEnabled = true;
        private int slotCount = -1;

        private InfusionDef bestInfusionCache = null;
        private List<OnHitWorker> onHitsCache = null;
        private List<PreHitWorker> preHitsCache = null;
        private Dictionary<StatDef, StatMod?> infusionsStatModCache = new Dictionary<StatDef, StatMod?>();
        public static HashSet<CompInfusion> WantingCandidates
        {
            get => wantingCandidates;
            set => wantingCandidates = value;
        }

        public static HashSet<CompInfusion> ExtractionCandidates
        {
            get => extractionCandidates;
            set => extractionCandidates = value;
        }

        public static HashSet<CompInfusion> RemovalCandidates
        {
            get => removalCandidates;
            set => removalCandidates = value;
        }

        public static void ClearCaches()
        {
            wantingCandidates = new HashSet<CompInfusion>();
            extractionCandidates = new HashSet<CompInfusion>();
            removalCandidates = new HashSet<CompInfusion>();
        }

        public static void RegisterWantingCandidate(CompInfusion comp)
        {
            WantingCandidates.Add(comp);
        }

        public static void RegisterExtractionCandidate(CompInfusion comp)
        {
            ExtractionCandidates.Add(comp);
        }

        public static void RegisterRemovalCandidate(CompInfusion comp)
        {
            RemovalCandidates.Add(comp);
        }

        public static void UnregisterWantingCandidates(CompInfusion comp)
        {
            WantingCandidates.Remove(comp);
        }

        public static void UnregisterExtractionCandidates(CompInfusion comp)
        {
            ExtractionCandidates.Remove(comp);
        }

        public static void UnregisterRemovalCandidate(CompInfusion comp)
        {
            RemovalCandidates.Remove(comp);
        }

        public InfusionDef BestInfusion => bestInfusionCache;

        public CompBiocodable Biocoder
        {
            get => biocoder?.Biocodable == true ? biocoder : null;
            set => biocoder = value;
        }

        public bool EffectsEnabled => effectsEnabled;

        public QualityCategory Quality
        {
            get 
            {
                if (parent.TryGetQuality(out QualityCategory qc))
                {
                    return qc;
                }
                return QualityCategory.Normal;
            }
        }

        public IEnumerable<InfusionDef> Infusions => infusions.OrderByDescending(x => x);

        public HashSet<InfusionDef> InfusionsRaw => infusions;

        public HashSet<InfusionDef> WantingSet
        {
            get => wantingSet;
            set
            {
                wantingSet = value;
                FinalizeSetMutations();
            }
        }

        public InfusionDef FirstWanting => wantingSet.FirstOrDefault();

        public HashSet<InfusionDef> ExtractionSet
        {
            get => extractionSet;
            set
            {
                extractionSet = value;
                removalSet.ExceptWith(extractionSet);
                FinalizeSetMutations();
            }
        }

        public InfusionDef FirstExtraction => extractionSet.FirstOrDefault();

        public (List<InfusionDef> Prefixes, List<InfusionDef> Suffixes) InfusionsByPosition
        {
            get
            {
                var prefixes = new List<InfusionDef>();
                var suffixes = new List<InfusionDef>();

                foreach (var inf in Infusions)
                {
                    if (inf.position == Position.Prefix)
                        prefixes.Add(inf);
                    else
                        suffixes.Add(inf);
                }

                prefixes.Reverse();
                suffixes.Reverse();

                return (prefixes, suffixes);
            }
        }

        public string Descriptions
        {
            get
            {
                return string.Join("\n\n", Infusions.Select(InfusionDef.MakeDescriptionString));
            }
        }

        public string InspectionLabel
        {
            get
            {
                if (infusions.Count == 0)
                {
                    return "Infusion.Label.NotInfused".Translate(parent.def.label).CapitalizeFirst();
                }

                var (prefixes, suffixes) = InfusionsByPosition;

                string suffixedPart;
                if (suffixes.Count == 0)
                {
                    suffixedPart = parent.def.label;
                }
                else
                {
                    var suffixString = suffixes.Select(def => def.label).ToCommaList(true);
                    suffixedPart = "Infusion.Label.Suffixed".Translate(suffixString, parent.def.label);
                }

                string prefixedPart;
                if (prefixes.Count == 0)
                {
                    prefixedPart = suffixedPart;
                }
                else
                {
                    var prefixString = string.Join(" ", prefixes.Select(def => def.label));
                    prefixedPart = "Infusion.Label.Prefixed".Translate(prefixString, suffixedPart);
                }

                return prefixedPart.CapitalizeFirst();
            }
        }

        public Command_Toggle EffectGizmo
        {
            get
            {
                if (onHitsCache?.Count > 0)
                {
                    return new Command_Toggle
                    {
                        defaultLabel = ResourceBank.Strings.Gizmo.label,
                        defaultDesc = ResourceBank.Strings.Gizmo.desc,
                        icon = ResourceBank.Textures.Flame,
                        isActive = () => effectsEnabled,
                        toggleAction = () => effectsEnabled = !effectsEnabled
                    };
                }
                return null;
            }
        }

        public List<OnHitWorker> OnHits => onHitsCache ?? new List<OnHitWorker>();
        public List<PreHitWorker> PreHits => preHitsCache ?? new List<PreHitWorker>();


        public HashSet<InfusionDef> RemovalSet
        {
            get => removalSet;
            set
            {
                removalSet = value;
                extractionSet.ExceptWith(removalSet);
                FinalizeSetMutations();
            }
        }

        public int Size => infusions.Count;

        public int SlotCount
        {
            get => slotCount;
            set => slotCount = value;
        }

        public void PopulateInfusionsStatModCache(StatDef stat)
        {
            if (!infusionsStatModCache.ContainsKey(stat))
            {
                var eligibles = infusions
                    .Select(inf =>
                    {
                        if (inf.stats.TryGetValue(stat, out StatMod statMod))
                            return (StatMod?)statMod;
                        return null;
                    })
                    .Where(statMod => statMod.HasValue)
                    .Select(statMod => statMod.Value);

                StatMod? result = null;
                if (eligibles.Any())
                {
                    result = eligibles.Aggregate((a, b) => a + b);
                }

                infusionsStatModCache.Add(stat, result);
            }
        }

        public int CalculateSlotCountFor(QualityCategory qc)
        {
            var apparelProps = parent.def.apparel;

            int limit;
            if (Settings.bodyPartHandle.Value)
            {
                limit = apparelProps?.bodyPartGroups?.Count ?? int.MaxValue;
            }
            else if (qc < QualityCategory.Normal)
            {
                limit = 0;
            }
            else
            {
                limit = int.MaxValue;
            }

            int layerBonus = 0;
            if (apparelProps != null && Settings.layerHandle.Value)
            {
                layerBonus = apparelProps.layers.Count - 1;
            }

            return Math.Min(limit, Settings.GetBaseSlotsFor(qc)) + layerBonus;
        }

        public StatMod GetModForStat(StatDef stat)
        {
            PopulateInfusionsStatModCache(stat);
            if (infusionsStatModCache.TryGetValue(stat, out var mod) && mod.HasValue)
            {
                return mod.Value;
            }
            return StatMod.Empty;
        }

        public bool HasInfusionForStat(StatDef stat)
        {
            PopulateInfusionsStatModCache(stat);
            return infusionsStatModCache.TryGetValue(stat, out var mod) && mod.HasValue;
        }

        public void InvalidateCache()
        {
            infusionsStatModCache.Clear();
            bestInfusionCache = Infusions.FirstOrDefault();

            onHitsCache = infusions
                .SelectMany(inf => inf.OnHits)
                .ToList();

            preHitsCache = infusions.SelectMany(inf => inf.PreHits).ToList();
        }

        public void MarkForInfuser(InfusionDef infDef)
        {
            var newSet = new HashSet<InfusionDef>(wantingSet) { infDef };
            WantingSet = newSet;
        }

        public void MarkForExtractor(InfusionDef infDef)
        {
            var newSet = new HashSet<InfusionDef>(extractionSet) { infDef };
            ExtractionSet = newSet;
        }

        public void MarkForRemoval(InfusionDef infDef)
        {
            var newSet = new HashSet<InfusionDef>(removalSet) { infDef };
            RemovalSet = newSet;
        }

        public void UnmarkForInfuser(InfusionDef infDef)
        {
            var newSet = new HashSet<InfusionDef>(wantingSet);
            newSet.Remove(infDef);
            WantingSet = newSet;
        }

        public void UnmarkForExtractor(InfusionDef infDef)
        {
            var newSet = new HashSet<InfusionDef>(extractionSet);
            newSet.Remove(infDef);
            ExtractionSet = newSet;
        }

        public void UnmarkForRemoval(InfusionDef infDef)
        {
            var newSet = new HashSet<InfusionDef>(removalSet);
            newSet.Remove(infDef);
            RemovalSet = newSet;
        }

        public void FinalizeSetMutations()
        {
            if (wantingSet.Count == 0)
                UnregisterWantingCandidates(this);
            else
                RegisterWantingCandidate(this);

            if (extractionSet.Count == 0)
                UnregisterExtractionCandidates(this);
            else
                RegisterExtractionCandidate(this);

            if (removalSet.Count == 0)
                UnregisterRemovalCandidate(this);
            else
                RegisterRemovalCandidate(this);
        }

        public string MakeBestInfusionLabel(BestInfusionLabelLength length)
        {
            if (BestInfusion == null)
                return "";

            var label = length == BestInfusionLabelLength.Long
                ? BestInfusion.label
                : BestInfusion.LabelShort;

            if (Size > 1)
            {
                return new StringBuilder(label, label.Length + 5)
                    .Append("(+")
                    .Append(Size - 1)
                    .Append(")")
                    .ToString();
            }

            return label;
        }

        public void SetInfusions(IEnumerable<InfusionDef> value, bool respawningAfterLoad)
        {
            infusions = new HashSet<InfusionDef>(value);
            wantingSet.ExceptWith(infusions);
            extractionSet.IntersectWith(infusions);
            removalSet.IntersectWith(infusions);

            InvalidateCache();
            FinalizeSetMutations();
        }

        public override void DrawGUIOverlay()
        {
            if (Find.CameraDriver.CurrentZoom == CameraZoomRange.Closest && BestInfusion != null)
            {
                var pos = Find.CameraDriver.CurrentZoom > CameraZoomRange.Closest
                    || parent.def.defName.StartsWith("Infusion_")
                    ? -0.4f
                    : -0.65f;

                GenMapUI.DrawThingLabel(
                    GenMapUI.LabelDrawPosFor(parent, pos),
                    MakeBestInfusionLabel(BestInfusionLabelLength.Short),
                    Utils.GetTierColor(BestInfusion.tier)
                );
            }
        }

        public override string TransformLabel(string label)
        {
            if (BestInfusion == null)
                return label;

            var baseLabel = parent.StyleDef?.overrideLabel
                ?? GenLabel.ThingLabel(parent.def, parent.Stuff);

            var translationKey = BestInfusion.position == Position.Prefix
                ? "Infusion.Label.Prefixed"
                : "Infusion.Label.Suffixed";

            var infusionLabel = MakeBestInfusionLabel(BestInfusionLabelLength.Long);
            var translatedLabel = translationKey.Translate(infusionLabel, baseLabel);
            var colorizedInfusionLabel = infusionLabel.Colorize(Utils.GetTierColor(BestInfusion.tier));
            var sb = new StringBuilder(translatedLabel);

            // Check if this is an infuser
            var isInfuser = parent.def.tradeTags?.Contains("Infusion_Infuser") == true;
            if (isInfuser && BestInfusion != null)
            {
                var reqString = InfusionDef.MakeRequirementString(BestInfusion);
                sb.Append(" ").Append(reqString);
            }

            sb.Append(GenLabel.LabelExtras(parent, true, true));
            return sb.ToString().Replace(infusionLabel, colorizedInfusionLabel);
        }

        public void TryUpdateMaxHitpoints()
        {
            int maxHitpoints = parent.MaxHitPoints;
            int maxHitpointsAfterStatWorker = (int)parent.GetStatValue(StatDefOf.MaxHitPoints);
            int diff = maxHitpointsAfterStatWorker - maxHitpoints;

            // Only lose hitpoints if hitpoints is larger than the new max
            if (diff < 0 && parent.HitPoints > maxHitpointsAfterStatWorker)
            {
                parent.HitPoints += diff;
            }
            else if(diff > 0 && parent.HitPoints < maxHitpointsAfterStatWorker)
            {
                parent.HitPoints += diff;
            }
        }

        public void TryUpdateMaxHitpoints(int previousMaxHitPoints)
        {
            int maxHitpointsAfterStatWorker = (int)parent.GetStatValue(StatDefOf.MaxHitPoints);
            int diff = maxHitpointsAfterStatWorker - previousMaxHitPoints;

            // Only lose hitpoints if hitpoints is larger than the new max
            if (diff < 0 && parent.HitPoints > maxHitpointsAfterStatWorker)
            {
                parent.HitPoints += diff;
            }
            else if (diff > 0 && parent.HitPoints < maxHitpointsAfterStatWorker)
            {
                parent.HitPoints += diff;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            if (respawningAfterLoad)
            {
                return;
            }

            if (slotCount == -1 && Quality >= QualityCategory.Normal)
            {
                slotCount = CalculateSlotCountFor(Quality);
            }

            if (removalSet.Count > 0)
            {
                RegisterRemovalCandidate(this);
            }
        }

        public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
        {
            UnregisterRemovalCandidate(this);
        }

        public override string GetDescriptionPart()
        {
            return Descriptions;
        }

        public override void PostExposeData()
        {
            Scribe_Values.Look(ref effectsEnabled, "effectsEnabled", true);
            Scribe_Values.Look(ref slotCount, "slotCount", CalculateSlotCountFor(Quality));
            
            var infusionsList = infusions.ToList();
            Scribe_Collections.Look(ref infusionsList, "infusions", LookMode.Def);
                

            if (infusionsList != null)
            {
                var loadedInfusions = infusionsList
                    .Where(inf => !InfusionDef.ShouldRemoveItself(inf))
                    .Select(inf => inf.Migration?.Replace ?? inf);

                SetInfusions(loadedInfusions, true);
            }

            var wantingList = wantingSet.ToList();
            Scribe_Collections.Look(ref wantingList, "wanting", LookMode.Def);
            if (wantingList != null)
            {
                wantingSet = new HashSet<InfusionDef>(wantingList);
            }

            var removalList = removalSet.ToList();
            Scribe_Collections.Look(ref removalList, "removal", LookMode.Def);
            if (removalList != null)
            {
                removalSet = new HashSet<InfusionDef>(
                    removalList.Where(inf => !InfusionDef.ShouldRemoveItself(inf))
                );
            }
        }

        public override bool AllowStackWith(Thing other)
        {
            return false;
        }

        public override void PostSplitOff(Thing other)
        {
            if (!(other is ThingWithComps otherWithComps))
            {
                return;
            }
            var comp = otherWithComps.GetComp<CompInfusion>();
            comp?.SetInfusions(Infusions, false);
        }

        public override int GetHashCode()
        {
            return parent.thingIDNumber;
        }

        public override bool Equals(object obj)
        {
            return obj is CompInfusion comp && parent.thingIDNumber == comp.parent.thingIDNumber;
        }

        public int CompareTo(object obj)
        {
            if (obj is CompInfusion comp)
            {
                return parent.ThingID.CompareTo(comp.parent.ThingID);
            }
            return 0;
        }

        public InfusionDef TryGetInfusionDefWithTag(InfusionTags tag)
        {
            foreach (InfusionDef infusion in infusions)
            {
                if (infusion.tags.Contains(InfusionTagsHelper.ConvertToString(tag)))
                {
                    return infusion;
                }
            }
            return null;
        }

        public bool ContainsTag(InfusionTags tag)
        {
            foreach (InfusionDef infusion in infusions)
            {
                if (infusion.tags.Contains(InfusionTagsHelper.ConvertToString(tag)))
                {
                    return true;
                }
            }
            return false;
        }
    }

    public static class CompInfusionExtensions
    {
        public static void AddInfusion(this CompInfusion comp, InfusionDef infDef)
        {
            int maxHitPoints = comp.parent.MaxHitPoints;
            var newInfusions = new List<InfusionDef> { infDef };
            newInfusions.AddRange(comp.Infusions);
            comp.SetInfusions(newInfusions, false);
            comp.TryUpdateMaxHitpoints(maxHitPoints);
        }

        public static List<InfusionDef> PickInfusions(this CompInfusion comp, QualityCategory quality)
        {
            bool CheckChance(InfusionDef infDef)
            {
                var chance = infDef.ChanceFor(quality) * Settings.chanceHandle.Value;
                return Rand.Chance(chance);
            }

            return DefDatabase<InfusionDef>.AllDefs
                .Where(infDef => Settings.IsTierEnabled(infDef.tier))
                .Where(infDef => InfusionDef.ActiveForUse(infDef) && InfusionDef.MatchesAll(infDef, comp.parent, quality))
                .Select(infDef => new
                {
                    InfDef = infDef,
                    Weight = Rand.Gaussian(
                        Settings.muHandle.Value * infDef.WeightFor(quality) * infDef.weight,
                        Settings.sigmaHandle.Value)
                })
                .OrderByDescending(x => x.Weight)
                .Take(comp.SlotCount)
                .Where(x => CheckChance(x.InfDef))
                .Select(x => x.InfDef)
                .ToList();
        }

        public static void RerollInfusions(this CompInfusion comp)
        {
            int maxHitPoints = comp.parent.MaxHitPoints;
            var newInfusions = comp.PickInfusions(comp.Quality);
            comp.SetInfusions(newInfusions, false);
            comp.TryUpdateMaxHitpoints(maxHitPoints);
        }

        public static void RemoveMarkedInfusions(this CompInfusion comp)
        {
            int maxHitPoints = comp.parent.MaxHitPoints;
            var newInfusions = comp.InfusionsRaw.Except(comp.RemovalSet);
            comp.SetInfusions(newInfusions, false);
            comp.RemovalSet = new HashSet<InfusionDef>();
            comp.TryUpdateMaxHitpoints(maxHitPoints);
        }

        public static void RemoveInfusion(this CompInfusion comp, InfusionDef def)
        {
            int maxHitPoints = comp.parent.MaxHitPoints;
            var newInfusions = comp.InfusionsRaw.Where(inf => inf != def);
            comp.SetInfusions(newInfusions, false);
            comp.TryUpdateMaxHitpoints(maxHitPoints);
        }

        public static (List<OnHitWorker> OnHits, CompInfusion Comp)? ForOnHitWorkers(ThingWithComps thing)
        {
            var comp = thing.GetComp<CompInfusion>();
            if (comp?.EffectsEnabled == true)
            {
                var onHits = comp.OnHits.Where(OnHitWorker.CheckChance).ToList();
                if (onHits.Count > 0)
                {
                    return (onHits, comp);
                }
            }
            return null;
        }

        public static (List<PreHitWorker>, CompInfusion Comp)? ForPreHitWorkers(ThingWithComps thing)
        {
            CompInfusion comp = thing.GetComp<CompInfusion>();
            if (comp != null && comp.EffectsEnabled)
            {
                List<PreHitWorker> list = comp.PreHits.Where(PreHitWorker.CheckChance).ToList();
                if (list.Count > 0)
                {
                    return (list, comp);
                }
            }
            return null;
        }
    }
}