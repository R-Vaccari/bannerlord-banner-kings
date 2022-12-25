using BannerKings.Managers.Duties;
using BannerKings.Managers.Titles;
using BannerKings.Models.Vanilla;
using HarmonyLib;
using Helpers;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;

namespace BannerKings.Behaviours
{
    public class BKArmyBehavior : CampaignBehaviorBase
    {
        public static AuxiliumDuty playerArmyDuty;
        public static CampaignTime lastDutyTime = CampaignTime.Zero;

        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickPartyEvent.AddNonSerializedListener(this, OnPartyDailyTick);
            CampaignEvents.OnPartyJoinedArmyEvent.AddNonSerializedListener(this, OnPartyJoinedArmyEvent);
            CampaignEvents.ArmyCreated.AddNonSerializedListener(this, OnArmyCreated);
            CampaignEvents.ArmyDispersed.AddNonSerializedListener(this, OnArmyDispersed);
            CampaignEvents.AiHourlyTickEvent.AddNonSerializedListener(this, AiHourlyTick);
        }

        public override void SyncData(IDataStore dataStore)
        {
            if (BannerKingsConfig.Instance.wipeData)
            {
                playerArmyDuty = null;
            }

            dataStore.SyncData("bannerkings-military-duty", ref playerArmyDuty);
            dataStore.SyncData("bannerkings-military-duty-time", ref lastDutyTime);
        }

        private void OnPartyDailyTick(MobileParty party)
        {
            EvaluateCreateArmy(party);
        }

        private void EvaluateCreateArmy(MobileParty party)
        {
            if (!party.IsLordParty || party.LeaderHero == null || party.LeaderHero.Clan == null)
            {
                return;
            }

            var leader = party.LeaderHero;
            var kingdom = leader.Clan.Kingdom;
            if (kingdom == null || leader != leader.Clan.Leader || leader.Clan.Influence < 100f)
            {
                return;
            }

            if (kingdom.Armies.Count == 0 && FactionManager.GetEnemyKingdoms(kingdom).Count() > 0)
            {
                kingdom.CreateArmy(leader, SettlementHelper.FindNearestSettlement(x => x.IsFortification || x.IsVillage,
                        party), Army.ArmyTypes.Besieger);
            }
        }

        public void OnPartyJoinedArmyEvent(MobileParty party)
        {
            var playerKingdom = Clan.PlayerClan.Kingdom;
            if (playerKingdom == null || playerKingdom != party.LeaderHero.Clan.Kingdom ||
                BannerKingsConfig.Instance.TitleManager == null || party == MobileParty.MainParty)
            {
                return;
            }

            var playerTitle =
                BannerKingsConfig.Instance.TitleManager.GetHighestTitleWithinFaction(Hero.MainHero, playerKingdom);
            if (playerTitle != null)
            {
                EvaluateSummonPlayer(playerTitle, party.Army, party);
            }
        }

        public void OnArmyDispersed(Army army, Army.ArmyDispersionReason reason, bool isPlayersArmy)
        {
            var leaderParty = army.LeaderParty;
            var playerKingdom = Clan.PlayerClan.Kingdom;
            if (playerKingdom == null || playerKingdom != army.Kingdom || playerArmyDuty == null ||
                BannerKingsConfig.Instance.TitleManager == null || leaderParty == MobileParty.MainParty)
            {
                return;
            }

            if (army.LeaderParty == playerArmyDuty.Party || army.Parties.Contains(playerArmyDuty.Party))
            {
                playerArmyDuty.Finish();
                playerArmyDuty = null;
                lastDutyTime = CampaignTime.Now;
            }
        }

        public void OnArmyCreated(Army army)
        {
            var leaderParty = army.LeaderParty;
            var playerKingdom = Clan.PlayerClan.Kingdom;
            if (playerKingdom == null || playerKingdom != leaderParty.LeaderHero.Clan.Kingdom ||
                BannerKingsConfig.Instance.TitleManager == null || leaderParty == MobileParty.MainParty)
            {
                return;
            }

            var playerTitle =
                BannerKingsConfig.Instance.TitleManager.GetHighestTitleWithinFaction(Hero.MainHero, playerKingdom);
            if (playerTitle != null)
            {
                EvaluateSummonPlayer(playerTitle, army);
            }
        }

        private void EvaluateSummonPlayer(FeudalTitle playerTitle, Army army, MobileParty joinningParty = null)
        {
            var contract = playerTitle.contract;
            if (contract == null || !contract.Duties.ContainsKey(FeudalDuties.Auxilium))
            {
                return;
            }

            var completion = contract.Duties[FeudalDuties.Auxilium];

            var suzerain = BannerKingsConfig.Instance.TitleManager.GetImmediateSuzerain(playerTitle);
            if (suzerain == null)
            {
                return;
            }

            if (Hero.MainHero.IsPrisoner || MobileParty.MainParty.Army != null)
            {
                return;
            }

            if (lastDutyTime == CampaignTime.Never)
            {
                lastDutyTime = CampaignTime.Zero;
            }

            var suzerainParty = EvaluateSuzerainParty(army, suzerain.deJure, joinningParty);
            if (suzerainParty != null && playerArmyDuty == null && lastDutyTime.ElapsedWeeksUntilNow >= 1f)
            {
                var days = 2f;
                var settlement =
                    SettlementHelper.FindNearestSettlement(x => x.IsFortification || x.IsVillage,
                        army.AiBehaviorObject);
                playerArmyDuty = new AuxiliumDuty(CampaignTime.DaysFromNow(days), suzerainParty, completion, settlement,
                    army.Name);
            }
        }

        private MobileParty EvaluateSuzerainParty(Army army, Hero target, MobileParty joinningParty = null)
        {
            MobileParty suzerainParty = null;
            var leaderParty = army.LeaderParty;
            if (leaderParty.LeaderHero == target)
            {
                suzerainParty = leaderParty;
            }
            else if (joinningParty != null && joinningParty.LeaderHero == target)
            {
                suzerainParty = joinningParty;
            }
            else
            {
                foreach (var party in army.Parties)
                {
                    if (party.LeaderHero == target)
                    {
                        suzerainParty = party;
                    }
                }
            }

            return suzerainParty;
        }

        public void AiHourlyTick(MobileParty mobileParty, PartyThinkParams p)
        {
            if (mobileParty.IsMilitia || mobileParty.IsCaravan || mobileParty.IsVillager ||
                mobileParty.IsBandit || !mobileParty.MapFaction.IsKingdomFaction)
            {
                return;
            }

            if (playerArmyDuty == null || mobileParty != playerArmyDuty.Party)
            {
                return;
            }

            var army = mobileParty.Army;
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
        internal class CreateArmyPatch
        {
            private static bool Prefix(Hero armyLeader, Settlement targetSettlement, Army.ArmyTypes selectedArmyType)
            {
                return new BKArmyManagementModel().CanCreateArmy(armyLeader);
            }
        }
    }
}