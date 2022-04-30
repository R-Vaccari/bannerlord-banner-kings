using BannerKings.Managers.Kingdoms;
using BannerKings.Managers.Titles;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Election;

namespace BannerKings.Behaviours
{
    public class BKRepublicBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, DailyTick);
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
  
                GovernmentType government = title.contract.Government;
                if (government == GovernmentType.Republic)
                {
                    bool inElection = false;
                    foreach (KingdomDecision decision in kingdom.UnresolvedDecisions)
                        if (decision is RepublicElectionDecision)
                        {
                            inElection = true;
                            break;
                        }
                                    
                    if (!inElection && kingdom.Clans.Count > 2)
                        kingdom.AddDecision(new RepublicElectionDecision(kingdom.RulingClan, kingdom.RulingClan), true);
                }     
                
            }
                 
        }
    }
}
