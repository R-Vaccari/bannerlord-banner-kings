using BannerKings.Components;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;

namespace BannerKings.Models.Vanilla
{
    public class BKPartyFoodConsumption : DefaultMobilePartyFoodConsumptionModel
    {
        public override bool DoesPartyConsumeFood(MobileParty mobileParty)
        {
            if (mobileParty.PartyComponent is PopulationPartyComponent || mobileParty.PartyComponent is RetinueComponent)
            {
                return false;
            }

            return base.DoesPartyConsumeFood(mobileParty);
        }
    }
}