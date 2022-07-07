using BannerKings.Managers.Duties;
using BannerKings.Managers.Titles;
using BannerKings.Models;
using HarmonyLib;
using Helpers;
using System;
using TaleWorlds.CampaignSystem;

namespace BannerKings.Behaviours
{
    public class BKArmyBehavior : CampaignBehaviorBase
    {
        public static AuxiliumDuty playerArmyDuty;
        public static CampaignTime lastDutyTime = CampaignTime.Never;

        public override void RegisterEvents()
        {
            CampaignEvents.OnPartyJoinedArmyEvent.AddNonSerializedListener(this, new Action<MobileParty>(this.OnPartyJoinedArmyEvent));
            CampaignEvents.ArmyCreated.AddNonSerializedListener(this, new Action<Army>(this.OnArmyCreated));
            CampaignEvents.ArmyDispersed.AddNonSerializedListener(this, new Action<Army, Army.ArmyDispersionReason, bool>(this.OnArmyDispersed));
            CampaignEvents.AiHourlyTickEvent.AddNonSerializedListener(this, new Action<MobileParty, PartyThinkParams>(this.AiHourlyTick));
        }

        public override void SyncData(IDataStore dataStore)
        {
            if (BannerKingsConfig.Instance.wipeData) playerArmyDuty = null;

            dataStore.SyncData("bannerkings-military-duty", ref playerArmyDuty);
            dataStore.SyncData("bannerkings-military-duty-time", ref lastDutyTime);
        }

        public void OnPartyJoinedArmyEvent(MobileParty party)
        {
            Kingdom playerKingdom = Clan.PlayerClan.Kingdom;
            if (playerKingdom == null || playerKingdom != party.LeaderHero.Clan.Kingdom ||
                BannerKingsConfig.Instance.TitleManager == null || party == MobileParty.MainParty)
                return;

            FeudalTitle playerTitle = BannerKingsConfig.Instance.TitleManager.GetHighestTitleWithinFaction(Hero.MainHero, playerKingdom);
            if (playerTitle != null)
                this.EvaluateSummonPlayer(playerTitle, party.Army, party);
        }

        public void OnArmyDispersed(Army army, Army.ArmyDispersionReason reason, bool isPlayersArmy)
        {

            MobileParty leaderParty = army.LeaderParty;
            Kingdom playerKingdom = Clan.PlayerClan.Kingdom;
            if (playerKingdom == null || playerKingdom != army.Kingdom || playerArmyDuty == null ||
                BannerKingsConfig.Instance.TitleManager == null || leaderParty == MobileParty.MainParty)
                return;

            if (army.LeaderParty == playerArmyDuty.Party || army.Parties.Contains(playerArmyDuty.Party))
            {
                playerArmyDuty.Finish();
                playerArmyDuty = null;
                lastDutyTime = CampaignTime.Now;
            }   
        }

        public void OnArmyCreated(Army army)
        {

            MobileParty leaderParty = army.LeaderParty;
            Kingdom playerKingdom = Clan.PlayerClan.Kingdom;
            if (playerKingdom == null || playerKingdom != leaderParty.LeaderHero.Clan.Kingdom ||
                BannerKingsConfig.Instance.TitleManager == null || leaderParty == MobileParty.MainParty)
                return;

            FeudalTitle playerTitle = BannerKingsConfig.Instance.TitleManager.GetHighestTitleWithinFaction(Hero.MainHero, playerKingdom);
            if (playerTitle != null)
                this.EvaluateSummonPlayer(playerTitle, army);
        }

        private void EvaluateSummonPlayer(FeudalTitle playerTitle, Army army, MobileParty joinningParty = null)
        {
            FeudalContract contract = playerTitle.contract;
            if (contract == null || !contract.Duties.ContainsKey(FeudalDuties.Auxilium)) return;
            float completion = contract.Duties[FeudalDuties.Auxilium];

            FeudalTitle suzerain = BannerKingsConfig.Instance.TitleManager.GetImmediateSuzerain(playerTitle);
            if (suzerain == null) return;
            if (Hero.MainHero.IsPrisoner) return;

            MobileParty suzerainParty = this.EvaluateSuzerainParty(army, suzerain.deJure, joinningParty);
            if (suzerainParty != null && playerArmyDuty == null && lastDutyTime.ElapsedWeeksUntilNow >= 1f)
            {
                float days = 2f;
                Settlement settlement = SettlementHelper.FindNearestSettlement((Settlement x) => x.IsFortification || x.IsVillage, army.AiBehaviorObject);
                BKArmyBehavior.playerArmyDuty = new AuxiliumDuty(CampaignTime.DaysFromNow(days), suzerainParty, completion, settlement, army.Name);
            }
        }

        private MobileParty EvaluateSuzerainParty(Army army, Hero target, MobileParty joinningParty = null)
        {
            MobileParty suzerainParty = null;
            MobileParty leaderParty = army.LeaderParty;
            if (leaderParty.LeaderHero == target)
                suzerainParty = leaderParty;
            else if (joinningParty != null && joinningParty.LeaderHero == target)
                suzerainParty = joinningParty;
            else foreach (MobileParty party in army.Parties)
                    if (party.LeaderHero == target)
                        suzerainParty = party;

            return suzerainParty;
        }

        public void AiHourlyTick(MobileParty mobileParty, PartyThinkParams p)
        {
            if (mobileParty.IsMilitia || mobileParty.IsCaravan || mobileParty.IsVillager || 
                mobileParty.IsBandit || !mobileParty.MapFaction.IsKingdomFaction)
                return;

            if (playerArmyDuty == null || mobileParty != playerArmyDuty.Party) return;

            Army army = mobileParty.Army;
            if (army == null)
            {
                playerArmyDuty.Finish();
                playerArmyDuty = null;
                return;
            }

            playerArmyDuty.Tick();
        }
    }

    namespace Patches
    {
        [HarmonyPatch(typeof(Kingdom), "CreateArmy")]
        class CreateArmyPatch
        {
            static bool Prefix(Hero armyLeader, IMapPoint target, Army.ArmyTypes selectedArmyType)
            {
                return new BKArmyManagementModel().CanCreateArmy(armyLeader);
            }
        }
    }
}
