using BannerKings.Managers.Duties;
using BannerKings.Managers.Titles;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party;

namespace BannerKings.Behaviours;

public class BKRansomBehavior : CampaignBehaviorBase
{
    public static RansomDuty playerRansomDuty;

    public override void RegisterEvents()
    {
        CampaignEvents.HeroPrisonerReleased.AddNonSerializedListener(this,
            OnHeroPrisonerReleased);
    }

    public override void SyncData(IDataStore dataStore)
    {
        if (BannerKingsConfig.Instance.wipeData)
        {
            playerRansomDuty = null;
        }

        dataStore.SyncData("bannerkings-ransom-duty", ref playerRansomDuty);
    }

    private void OnHeroPrisonerReleased(Hero released, PartyBase releasedFrom, IFaction capturer,
        EndCaptivityDetail detail)
    {
        var playerKingdom = Clan.PlayerClan.Kingdom;
        var releasedKingdom = released.Clan != null ? released.Clan.Kingdom : null;
        if (detail != EndCaptivityDetail.Ransom || playerKingdom == null || releasedKingdom == null ||
            playerKingdom != releasedKingdom || BannerKingsConfig.Instance.TitleManager == null)
        {
            return;
        }

        var playerTitle = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(Hero.MainHero);
        if (playerTitle == null)
        {
            return;
        }

        var suzerain = BannerKingsConfig.Instance.TitleManager.GetImmediateSuzerain(playerTitle);
        if (suzerain == null || released != suzerain.deJure)
        {
            return;
        }

        var contract = playerTitle.contract;
        if (contract == null || !contract.Duties.ContainsKey(FeudalDuties.Ransom))
        {
            return;
        }

        var completion = contract.Duties[FeudalDuties.Ransom];
        float ransom =
            Campaign.Current.Models.RansomValueCalculationModel.PrisonerRansomValue(released.CharacterObject);
        playerRansomDuty = new RansomDuty(CampaignTime.DaysFromNow(2), released, ransom * completion);
    }
}