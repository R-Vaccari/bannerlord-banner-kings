using BannerKings.Managers.Kingdoms;
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
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, new Action(DailyTick));
        }

        public override void SyncData(IDataStore dataStore)
        {
        }


        private void DailyTick()
        {

            CampaignTime now = CampaignTime.Now;
            int day = now.GetDayOfYear;
            if (day != 1) return;

            foreach (Kingdom kingdom in Kingdom.All)
            {
                FeudalTitle title = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(kingdom);
                if (title == null || title.contract == null) return;
  
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
