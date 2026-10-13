using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Infusion.Helpers
{
    public class ApparelInfusionPolicyStore : IExposable
    {
        private readonly Dictionary<int, HashSet<InfusionDef>> disallowedByPolicyId = new Dictionary<int, HashSet<InfusionDef>>();
        private List<ApparelInfusionPolicyStoreEntry> savedEntries = new List<ApparelInfusionPolicyStoreEntry>();

        public int CountDisallowedInfusions(ApparelPolicy policy)
        {
            return TryGetDisallowedSet(policy, out HashSet<InfusionDef> disallowed)
                ? disallowed.Count
                : 0;
        }

        public bool HasCustomRestrictions(ApparelPolicy policy)
        {
            return disallowedByPolicyId.ContainsKey(policy.id);
        }

        public bool IsDisallowed(ApparelPolicy policy, InfusionDef infusion)
        {
            return TryGetDisallowedSet(policy, out HashSet<InfusionDef> disallowed)
                && disallowed.Contains(infusion);
        }

        public IEnumerable<InfusionDef> GetDisallowedInfusions(ApparelPolicy policy)
        {
            if (!TryGetDisallowedSet(policy, out HashSet<InfusionDef> disallowed))
            {
                return Enumerable.Empty<InfusionDef>();
            }

            return disallowed;
        }

        public void SetAllowed(ApparelPolicy policy, InfusionDef infusion, bool allowed)
        {
            if (!disallowedByPolicyId.TryGetValue(policy.id, out HashSet<InfusionDef> disallowed))
            {
                if (allowed)
                {
                    return;
                }

                disallowed = new HashSet<InfusionDef>();
                disallowedByPolicyId[policy.id] = disallowed;
            }

            if (allowed)
            {
                disallowed.Remove(infusion);
            }
            else
            {
                disallowed.Add(infusion);
            }

            if (disallowed.Count == 0)
            {
                disallowedByPolicyId.Remove(policy.id);
            }
        }

        public void SetDisallowedInfusions(ApparelPolicy policy, IEnumerable<InfusionDef> infusions)
        {
            HashSet<InfusionDef> disallowed = new HashSet<InfusionDef>(
                (infusions ?? Enumerable.Empty<InfusionDef>()).Where(infusion => infusion != null));

            if (disallowed.Count == 0)
            {
                disallowedByPolicyId.Remove(policy.id);
            }
            else
            {
                disallowedByPolicyId[policy.id] = disallowed;
            }
        }

        public void RemovePolicy(ApparelPolicy policy)
        {
            disallowedByPolicyId.Remove(policy.id);
        }

        public void RemoveMissingPolicies(IEnumerable<ApparelPolicy> policies)
        {
            HashSet<int> validIds = new HashSet<int>((policies ?? Enumerable.Empty<ApparelPolicy>())
                .Where(policy => policy != null)
                .Select(policy => policy.id));

            List<int> invalidIds = disallowedByPolicyId.Keys
                .Where(id => !validIds.Contains(id))
                .ToList();

            foreach (int invalidId in invalidIds)
            {
                disallowedByPolicyId.Remove(invalidId);
            }
        }

        public bool IsAnyInfusionDisallowed(ApparelPolicy policy, Apparel apparel)
        {
            if (!TryGetDisallowedSet(policy, out HashSet<InfusionDef> disallowed))
            {
                return false;
            }

            CompInfusion compInfusion = apparel.TryGetComp<CompInfusion>();
            if (compInfusion == null || compInfusion.infusionsCount <= 0)
            {
                return false;
            }

            foreach (InfusionDef infusion in compInfusion.InfusionsRaw)
            {
                if (infusion != null && disallowed.Contains(infusion))
                {
                    return true;
                }
            }

            return false;
        }

        public bool AllowsApparel(ApparelPolicy policy, Apparel apparel)
        {
            return !IsAnyInfusionDisallowed(policy, apparel);
        }

        public void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                savedEntries = disallowedByPolicyId
                    .Where(kvp => kvp.Value != null && kvp.Value.Count > 0)
                    .Select(kvp => new ApparelInfusionPolicyStoreEntry(kvp.Key, kvp.Value))
                    .ToList();
            }

            Scribe_Collections.Look(ref savedEntries, "savedEntries", LookMode.Deep);

            if (Scribe.mode != LoadSaveMode.PostLoadInit)
            {
                return;
            }

            disallowedByPolicyId.Clear();
            if (savedEntries == null)
            {
                savedEntries = new List<ApparelInfusionPolicyStoreEntry>();
                return;
            }

            foreach (ApparelInfusionPolicyStoreEntry entry in savedEntries)
            {
                if (entry == null || entry.PolicyId <= 0 || entry.DisallowedInfusions.NullOrEmpty())
                {
                    continue;
                }

                HashSet<InfusionDef> disallowed = new HashSet<InfusionDef>(
                    entry.DisallowedInfusions.Where(infusion => infusion != null));

                if (disallowed.Count > 0)
                {
                    disallowedByPolicyId[entry.PolicyId] = disallowed;
                }
            }
        }

        private bool TryGetDisallowedSet(ApparelPolicy policy, out HashSet<InfusionDef> disallowed)
        {
            if (disallowedByPolicyId.TryGetValue(policy.id, out disallowed))
            {
                return true;
            }

            disallowed = null;
            return false;
        }
    }

    public class ApparelInfusionPolicyStoreEntry : IExposable
    {
        private int policyId;
        private List<InfusionDef> disallowedInfusions = new List<InfusionDef>();

        public int PolicyId => policyId;
        public List<InfusionDef> DisallowedInfusions => disallowedInfusions;

        public ApparelInfusionPolicyStoreEntry()
        {
        }

        public ApparelInfusionPolicyStoreEntry(int policyId, IEnumerable<InfusionDef> disallowedInfusions)
        {
            this.policyId = policyId;
            this.disallowedInfusions = disallowedInfusions?
                .Where(infusion => infusion != null)
                .Distinct()
                .ToList()
                ?? new List<InfusionDef>();
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref policyId, "policyId");
            Scribe_Collections.Look(ref disallowedInfusions, "disallowedInfusions", LookMode.Def);
        }
    }
}
