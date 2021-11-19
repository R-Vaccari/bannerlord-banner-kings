using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using TaleWorlds.Localization;
using static Populations.PopulationManager;
using static Populations.PolicyManager;
using TaleWorlds.Core;

namespace Populations.Models
{
    class InfluenceModel : DefaultClanPoliticsModel
    {
        public override ExplainedNumber CalculateInfluenceChange(Clan clan, bool includeDescriptions = false)
        {
            //InformationManager.DisplayMessage(new InformationMessage("Influence model running..."));
            ExplainedNumber baseResult = base.CalculateInfluenceChange(clan, includeDescriptions);

            foreach (Settlement settlement in clan.Settlements)
            {
                if (IsSettlementPopulated(settlement))
                {
                    PopulationData data = GetPopData(settlement);
                    int nobles = data.GetTypeCount(PopType.Nobles);
                    if (PopSurplusExists(settlement, PopType.Nobles, true))
                    {
                        int extra = GetPopCountOverLimit(settlement, PopType.Nobles);
                        baseResult.Add(((float)extra * -2f) * 0.01f, new TextObject(string.Format("Excess noble population at {0}", settlement.Name)));
                    }
                    baseResult.Add((float)nobles * 0.01f, new TextObject(string.Format("Nobles influence from {0}", settlement.Name)));

                    if (IsPolicyEnacted(settlement, PolicyType.POP_GROWTH))
                        baseResult.AddFactor(-0.5f, new TextObject(string.Format("Population growth policy at {0}", settlement.Name)));
                }
            }
                
            return baseResult;
        }
    }
}
