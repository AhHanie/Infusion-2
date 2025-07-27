using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Infusion
{
    public class CompInfuserProperties : CompProperties
    {
        public TierDef forcedTier;

        public CompInfuserProperties() : base(typeof(CompInfusion))
        {
            forcedTier = TierDef.Empty;
        }

        public CompInfuserProperties(TierDef tier) : base(typeof(CompInfusion))
        {
            forcedTier = tier;
        }
    }

    public class Infuser : ThingWithComps, IComparable
    {
        private static HashSet<Infuser> allInfusers = new HashSet<Infuser>();

        public static HashSet<Infuser> AllInfusers => allInfusers;

        public static Dictionary<InfusionDef, Infuser> AllInfusersByDef
        {
            get
            {
                return allInfusers
                    .Where(infuser => infuser.Content != null)
                    .ToDictionary(infuser => infuser.Content, infuser => infuser);
            }
        }

        public InfusionDef Content
        {
            get
            {
                var comp = this.GetComp<CompInfusion>();
                if (comp?.Infusions?.Count() == 1)
                {
                    return comp.Infusions.Single();
                }
                return null;
            }
        }

        public void SetContent(InfusionDef inf)
        {
            var comp = this.GetComp<CompInfusion>();
            if (comp != null)
            {
                comp.SetInfusions(new[] { inf }, false);
                ResetHP();
            }
        }

        private void ResetHP()
        {
            // Reset hit points - this likely adjusts HP based on new max HP from infusions
            if (this.HitPoints > 0)
            {
                var ratio = (float)this.HitPoints / this.MaxHitPoints;
                this.HitPoints = Math.Max(1, (int)(this.MaxHitPoints * ratio));
            }
        }

        public override void PostMake()
        {
            base.PostMake();

            var comp = this.GetComp<CompInfusion>();
            if (comp?.Size == 0 && comp.props is CompInfuserProperties props)
            {
                if (!TierDef.IsEmpty(props.forcedTier))
                {
                    var availableInfusions = DefDatabase<InfusionDef>.AllDefs
                        .Where(inf => inf.tier == props.forcedTier)
                        .Where(InfusionDef.ActiveForUse)
                        .ToList();

                    if (availableInfusions.Count > 0)
                    {
                        var randomInfusion = availableInfusions.RandomElement();
                        comp.SetInfusions(new[] { randomInfusion }, false);
                    }
                }
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            allInfusers.Add(this);
        }

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            base.DeSpawn(mode);
            allInfusers.Remove(this);
        }

        public override Thing SplitOff(int count)
        {
            var otherOne = base.SplitOff(count);
            var content = this.Content;

            if (content != null)
            {
                var comp = otherOne.TryGetComp<CompInfusion>();
                comp?.SetInfusions(new[] { content }, false);
            }

            return otherOne;
        }

        public override bool Equals(object obj)
        {
            return obj is Thing thing && this.ThingID == thing.ThingID;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public int CompareTo(object obj)
        {
            if (obj is Thing thing)
            {
                return this.ThingID.CompareTo(thing.ThingID);
            }
            return 0;
        }

        //public override void DrawGUIOverlay()
        //{
        //    if (Find.CameraDriver.CurrentZoom <= CameraZoomRange.Close && Content != null)
        //    {
        //        var pos = Find.CameraDriver.CurrentZoom > CameraZoomRange.Closest
        //            || Content.defName.StartsWith("Infusion_")
        //            ? -0.4f
        //            : -0.65f;

        //        GenMapUI.DrawThingLabel(
        //            GenMapUI.LabelDrawPosFor(this, pos),
        //            MakeBestInfusionLabel(BestInfusionLabelLength.Short),
        //            Content.tier.color
        //        );
        //    }
        //}

        //public string MakeBestInfusionLabel(BestInfusionLabelLength length)
        //{
        //    if (Content == null)
        //        return "";

        //    var label = length == BestInfusionLabelLength.Long
        //        ? Content.label
        //        : Content.LabelShort;

        //    return new StringBuilder(label, label.Length + 5).ToString();
        //}

        //public override string TransformLabel(string label)
        //{
        //    if (BestInfusion == null)
        //        return label;

        //    var baseLabel = parent.StyleDef?.overrideLabel
        //        ?? GenLabel.ThingLabel(parent.def, parent.Stuff);

        //    var translationKey = BestInfusion.position == Position.Prefix
        //        ? "Infusion.Label.Prefixed"
        //        : "Infusion.Label.Suffixed";

        //    var infusionLabel = MakeBestInfusionLabel(BestInfusionLabelLength.Long);
        //    var sb = new StringBuilder(translationKey.Translate(infusionLabel, baseLabel));

        //    // Check if this is an infuser
        //    var isInfuser = parent.def.tradeTags?.Contains("Infusion_Infuser") == true;
        //    if (isInfuser && BestInfusion != null)
        //    {
        //        var reqString = InfusionDef.MakeRequirementString(BestInfusion);
        //        sb.Append(" ").Append(reqString);
        //    }

        //    sb.Append(GenLabel.LabelExtras(parent, true, true));
        //    return sb.ToString();
        //}
    }
}