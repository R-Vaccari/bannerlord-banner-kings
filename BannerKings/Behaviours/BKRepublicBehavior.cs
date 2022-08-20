using BannerKings.Managers.Kingdoms;
using BannerKings.Managers.Titles;
using TaleWorlds.CampaignSystem;

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
            var now = CampaignTime.Now;
            var day = now.GetDayOfYear;
            if (day != 1)
            {
                return;
            }

            foreach (var kingdom in Kingdom.All)
            {
                var title = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(kingdom);
                if (title == null || title.contract == null)
                {
                    return;
                }

                var government = title.contract.Government;
                if (government == GovernmentType.Republic)
                {
                    var inElection = false;
                    foreach (var decision in kingdom.UnresolvedDecisions)
                    {
                        if (decision is RepublicElectionDecision)
                        {
                            inElection = true;
                            break;
                        }
                    }

                    if (!inElection && kingdom.Clans.Count > 2)
                    {
                        kingdom.AddDecision(new RepublicElectionDecision(kingdom.RulingClan, kingdom.RulingClan), true);
                    }
                }
            }
        }
    }
}