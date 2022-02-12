using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using TaleWorlds.Localization;
using static BannerKings.Managers.PopulationManager;
using static BannerKings.Managers.PolicyManager;
using BannerKings.Populations;

namespace BannerKings.Models
{
    class BKInfluenceModel : DefaultClanPoliticsModel
    {
        public override ExplainedNumber CalculateInfluenceChange(Clan clan, bool includeDescriptions = false)
        {
            ExplainedNumber baseResult = base.CalculateInfluenceChange(clan, includeDescriptions);

            foreach (Settlement settlement in clan.Settlements)
            {
                if (BannerKingsConfig.Instance.PopulationManager != null && BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(settlement))
                {
                    PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
                    int nobles = data.GetTypeCount(PopType.Nobles);
                    if (BannerKingsConfig.Instance.PopulationManager.PopSurplusExists(settlement, PopType.Nobles, true))
                    {
                        int extra = BannerKingsConfig.Instance.PopulationManager.GetPopCountOverLimit(settlement, PopType.Nobles);
                        baseResult.Add(((float)extra * -2f) * 0.01f, new TextObject(string.Format("Excess noble population at {0}", settlement.Name)));
                    }
                    baseResult.Add((float)nobles * 0.01f, new TextObject(string.Format("Nobles influence from {0}", settlement.Name)));
                        
                }
            }
                
            return baseResult;
        }

        public int GetMilitiaInfluenceCost(MobileParty party, Settlement settlement, Hero lord)
        {
            float cost = party.MemberRoster.TotalManCount;
            float mediumRelation = 1f;
            if (settlement.Notables != null)
                foreach (Hero notable in settlement.Notables)
                    mediumRelation += ((float)notable.GetRelation(lord) / 5f) * -0.01f;

            return (int)(cost * mediumRelation / 2f);
        }
    }
}
