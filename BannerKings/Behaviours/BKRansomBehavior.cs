using BannerKings.Managers.Duties;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using static BannerKings.Managers.TitleManager;

namespace BannerKings.Behaviours
{
    public class BKRansomBehavior : CampaignBehaviorBase
    {

        public static RansomDuty playerRansomDuty;
        public override void RegisterEvents()
        {
            CampaignEvents.HeroPrisonerReleased.AddNonSerializedListener(this, 
                new Action<Hero, PartyBase, IFaction, EndCaptivityDetail>(this.OnHeroPrisonerReleased));
        }

        public override void SyncData(IDataStore dataStore)
        {
            if (BannerKingsConfig.Instance.wipeData)
                playerRansomDuty = null;
            dataStore.SyncData("bannerkings-ransom-duty", ref playerRansomDuty);
        }

        private void OnHeroPrisonerReleased(Hero released, PartyBase releasedFrom, IFaction capturer, EndCaptivityDetail detail)
        {
            Kingdom playerKingdom = Clan.PlayerClan.Kingdom;
            Kingdom releasedKingdom = released.Clan.Kingdom;
            if (detail != EndCaptivityDetail.Ransom || playerKingdom == null || releasedKingdom == null || 
                playerKingdom != releasedKingdom || BannerKingsConfig.Instance.TitleManager == null) return;

            FeudalTitle playerTitle = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(Hero.MainHero);
            if (playerTitle == null) return;

            FeudalTitle suzerain = BannerKingsConfig.Instance.TitleManager.GetImmediateSuzerain(playerTitle);
            if (suzerain == null || released != suzerain.deJure) return;

            FeudalContract contract = playerTitle.contract;
            if (contract == null || !contract.duties.ContainsKey(FeudalDuties.Ransom)) return;

            float completion = contract.duties[FeudalDuties.Ransom];
            float ransom = Campaign.Current.Models.RansomValueCalculationModel.PrisonerRansomValue(released.CharacterObject, null);
            playerRansomDuty = new RansomDuty(CampaignTime.DaysFromNow(2), released, ransom * completion);
        }
    }
}
