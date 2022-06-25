using BannerKings.Components;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents.Map;

namespace BannerKings.Models.Vanilla
{
    public class BKPartyFoodConsumption : DefaultMobilePartyFoodConsumptionModel
    {

        public override bool DoesPartyConsumeFood(MobileParty mobileParty)
        {
            bool baseResult = base.DoesPartyConsumeFood(mobileParty);
            return baseResult && mobileParty.PartyComponent is not PopulationPartyComponent;
        }
    }
}
