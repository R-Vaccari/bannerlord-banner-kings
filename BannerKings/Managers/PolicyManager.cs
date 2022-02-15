using BannerKings.Managers.Policies;
using System.Linq;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.SaveSystem;
using static BannerKings.Managers.Policies.BKGarrisonPolicy;
using static BannerKings.Managers.Policies.BKMilitiaPolicy;
using static BannerKings.Managers.Policies.BKCriminalPolicy;
using BannerKings.Managers.Decisions;

namespace BannerKings.Managers
{
    public class PolicyManager
    {
  
        private Dictionary<Settlement, HashSet<BannerKingsDecision>> SettlementDecisions { get; set; }

        private Dictionary<Settlement, HashSet<BannerKingsPolicy>> SettlementPolicies { get; set; }

        private IEnumerable<string> TownDecisions
        {
            get
            {
                yield return "decision_drafting_encourage";
                yield return "decision_militia_encourage";
                yield return "decision_slaves_export";
                yield return "decision_patrol_send";
                yield return "decision_scout_send";
                yield return "decision_militia_subsidize";
                yield break;
            }
        }

        private IEnumerable<string> CastleDecisions
        {
            get
            {
                yield return "";
                yield break;
            }
        }

        private IEnumerable<string> VillageDecisions
        {
            get
            {
                yield return "";
                yield break;
            }
        }

        public IEnumerable<string> Policies
        {
            get
            {
                yield return "criminal";
                yield return "garrison";
                yield return "draft";
                yield return "militia";
                yield return "tax";
                yield return "tariff";
                yield return "workforce";
                yield break;
            }
        }

        public PolicyManager(Dictionary<Settlement, HashSet<BannerKingsDecision>> DECISIONS, Dictionary<Settlement, HashSet<BannerKingsPolicy>> POLICIES)
        {
            this.SettlementDecisions = DECISIONS;
            this.SettlementPolicies = POLICIES;
        }

        public void InitializeSettlement(Settlement settlement)
        {
            if (!this.SettlementDecisions.ContainsKey(settlement))
                InitializeDecisions(settlement);
            if (!SettlementPolicies.ContainsKey(settlement))
                InitializePolicies(settlement);
        }

        public bool IsSettlementPoliciesSet(Settlement settlement) => SettlementDecisions.ContainsKey(settlement);
        public HashSet<BannerKingsDecision> GetDefaultDecisions(Settlement settlement)
        {
            if (!SettlementDecisions.ContainsKey(settlement))
                InitializeDecisions(settlement);

            return SettlementDecisions[settlement];
        }

        private void InitializeDecisions(Settlement settlement)
        {
            HashSet<BannerKingsDecision> decisions = new HashSet<BannerKingsDecision>();
            if (settlement.IsVillage)
            {
                foreach (string id in VillageDecisions)
                    decisions.Add(this.GenerateDecision(settlement, id));
            } else if (settlement.IsCastle)
            {
                foreach (string id in CastleDecisions)
                    decisions.Add(this.GenerateDecision(settlement, id));
            } else if (settlement.IsTown)
            {
                foreach (string id in TownDecisions)
                    decisions.Add(this.GenerateDecision(settlement, id));
            }
            this.SettlementDecisions.Add(settlement, decisions);
        }

        private void InitializePolicies(Settlement settlement)
        {
            HashSet<BannerKingsPolicy> policies = new HashSet<BannerKingsPolicy>();

            foreach (string id in Policies)
                policies.Add(this.GeneratePolicy(settlement, id));
     
            this.SettlementPolicies.Add(settlement, policies);
        }

        public int GetActiveDecisionsNumber(Settlement settlement)
        {
            if (this.SettlementDecisions.ContainsKey(settlement))
            {
                HashSet<BannerKingsDecision> decisions = this.SettlementDecisions[settlement];
                return decisions.Count(x => x.Enabled);
            }
            return 0;
        }
        public bool IsPolicyEnacted(Settlement settlement, string policyType, int value)
        {
            BannerKingsPolicy policy = this.GetPolicy(settlement, policyType);
            return policy.Selected == value;
        }

        public BannerKingsPolicy GetPolicy(Settlement settlement, string policyType)
        {
            BannerKingsPolicy result = null;
            if (SettlementPolicies.ContainsKey(settlement))
            {
                HashSet<BannerKingsPolicy> policies = SettlementPolicies[settlement];
                BannerKingsPolicy policy = policies.FirstOrDefault(x => x.GetIdentifier() == policyType);
                if (policy != null) result = policy;
            } else
            {
                result = GeneratePolicy(settlement, policyType);
                HashSet<BannerKingsPolicy> set = new HashSet<BannerKingsPolicy>();
                set.Add(result);
                SettlementPolicies.Add(settlement, set);
            }

            if (result == null) result = GeneratePolicy(settlement, policyType);

            return result;
        }

        public BannerKingsDecision GenerateDecision(Settlement settlement, string policyType)
        {
            if (policyType == "decision_militia_subsidize")
                return new BKSubsidizeMilitiaDecision(settlement, false);
            else if (policyType == "decision_militia_encourage")
                return new BKEncourageMilitiaDecision(settlement, false);
            else if(policyType == "decision_drafting_encourage")
                return new BKEncourageDraftingDecision(settlement, false);
            else
                return new BKExportSlavesDecision(settlement, true);

        }

        public BannerKingsPolicy GeneratePolicy(Settlement settlement, string policyType)
        {
            if (policyType == "garrison")
                return new BKGarrisonPolicy(GarrisonPolicy.Standard, settlement);
            else if (policyType == "militia")
                return new BKMilitiaPolicy(MilitiaPolicy.Balanced, settlement);
            if (policyType == "tax")
                return new BKTaxPolicy(BKTaxPolicy.TaxType.Standard, settlement);
            else if (policyType == "tariff")
                return new BKTariffPolicy(BKTariffPolicy.TariffType.Standard, settlement);
            if (policyType == "workforce")
                return new BKWorkforcePolicy(BKWorkforcePolicy.WorkforcePolicy.None, settlement);
            else if (policyType == "draft")
                return new BKDraftPolicy(BKDraftPolicy.DraftPolicy.Standard, settlement);
            else return new BKCriminalPolicy(CriminalPolicy.Enslavement, settlement);
        }

        private void AddSettlementPolicy(Settlement settlement)
        {
            SettlementPolicies.Add(settlement, new HashSet<BannerKingsPolicy>());
        }

        private void AddSettlementDecision(Settlement settlement)
        {
            SettlementDecisions.Add(settlement, new HashSet<BannerKingsDecision>());
        }

        public void UpdateSettlementPolicy(Settlement settlement, BannerKingsPolicy policy)
        {
            if (SettlementPolicies.ContainsKey(settlement))
            {
                HashSet<BannerKingsPolicy> policies = SettlementPolicies[settlement];
                BannerKingsPolicy target = policies.FirstOrDefault(x => x.GetIdentifier() == policy.GetIdentifier());
                if (target != null) policies.Remove(target);
                policies.Add(policy);
            }
            else
            {
                AddSettlementPolicy(settlement);
            }
        }

        public bool IsDecisionEnacted(Settlement settlement, string type)
        {
            BannerKingsDecision decision = null;
            if (SettlementDecisions.ContainsKey(settlement))
                decision = SettlementDecisions[settlement].FirstOrDefault(x => x.GetIdentifier() == type);
            return decision != null ? decision.Enabled : false;
        }

        public void UpdateSettlementDecision(Settlement settlement, BannerKingsDecision decision)
        {
            if (SettlementDecisions.ContainsKey(settlement))
            {
                HashSet<BannerKingsDecision> policies = SettlementDecisions[settlement];
                BannerKingsDecision target = policies.FirstOrDefault(x => x.GetIdentifier() == decision.GetIdentifier());
                if (target != null) policies.Remove(target);
                policies.Add(decision);
            }
            else
            {
                AddSettlementPolicy(settlement);
            }
        }
    }
}
