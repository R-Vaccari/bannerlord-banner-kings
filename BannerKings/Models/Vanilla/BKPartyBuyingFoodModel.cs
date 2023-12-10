using TaleWorlds.CampaignSystem.GameComponents;

namespace BannerKings.Models.Vanilla
{
    public class BKPartyBuyingFoodModel : DefaultPartyFoodBuyingModel
    {
        public override float MinimumDaysFoodToLastWhileBuyingFoodFromTown => 100f;
        public override float MinimumDaysFoodToLastWhileBuyingFoodFromVillage => 40f;
    }
}
