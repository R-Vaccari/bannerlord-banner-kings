using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Election;
using static BannerKings.Managers.TitleManager;

namespace BannerKings.Behaviours
{
    public class BKRepublicBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickSettlementEvent.AddNonSerializedListener(this, new Action<Settlement>(DailySettlementTick));
        }

        public override void SyncData(IDataStore dataStore)
        {
        }


        private void DailySettlementTick(Settlement settlement)
        {

            if (settlement.OwnerClan == null || BannerKingsConfig.Instance.TitleManager == null) return;

            CampaignTime now = CampaignTime.Now;
            int day = now.GetDayOfYear;

            if (day == 1)
            {
                Kingdom kingdom = settlement.OwnerClan.Kingdom;
                if (kingdom != null)
                {
                    FeudalTitle title = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(kingdom);
                    if (title != null)
                    {
                        GovernmentType government = title.contract.government;
                        if (government == GovernmentType.Republic)
                        {
                            bool inElection = false;
                            foreach (KingdomDecision decision in kingdom.UnresolvedDecisions)
                                if (decision is RepublicElectionDecision)
                                {
                                    inElection = true;
                                    break;
                                }
                                    
                            if (!inElection)
                                kingdom.AddDecision(new RepublicElectionDecision(kingdom.RulingClan, kingdom.RulingClan), true);
                        }     
                    }
                }
            }      
        }
    }
}
