using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Settlements;

namespace BannerKings.Models.Vanilla
{
    public class BKSettlementAccessModel : DefaultSettlementAccessModel
    {
        public override void CanMainHeroEnterSettlement(Settlement settlement, out AccessDetails accessDetails)
        {
            if (settlement.IsCastle)
            {
                Hero mainHero = Hero.MainHero;
                if (FactionManager.IsNeutralWithFaction(mainHero.MapFaction, settlement.MapFaction) &&
                    mainHero.MapFaction.IsClan &&
                    !TaleWorlds.CampaignSystem.Campaign.Current.Models.CrimeModel.DoesPlayerHaveAnyCrimeRating(settlement.MapFaction))
                {
                    accessDetails = new SettlementAccessModel.AccessDetails
                    {
                        AccessLevel = SettlementAccessModel.AccessLevel.FullAccess,
                        AccessMethod = SettlementAccessModel.AccessMethod.ByRequest
                    };
                    return;
                }
            }

            base.CanMainHeroEnterSettlement(settlement, out accessDetails);
        }
    }
}
