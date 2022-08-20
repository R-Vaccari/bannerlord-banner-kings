using BannerKings.Components;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;

namespace BannerKings.Models.Vanilla
{
    public class BKPartyFoodConsumption : DefaultMobilePartyFoodConsumptionModel
    {
        public override bool DoesPartyConsumeFood(MobileParty mobileParty)
        {
            var baseResult = base.DoesPartyConsumeFood(mobileParty);
            return baseResult && mobileParty.PartyComponent is not PopulationPartyComponent;
        }
    }
}