using BannerKings.Managers.Court;
using BannerKings.Managers.Court.Members.Tasks;
using BannerKings.Managers.Duties;
using BannerKings.Managers.Goals.Decisions;
using BannerKings.Managers.Titles;
using BannerKings.Models.Vanilla;
using HarmonyLib;
using Helpers;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;

namespace BannerKings.Behaviours
{
    public class BKArmyBehavior : CampaignBehaviorBase
    {
        private AuxiliumDuty playerArmyDuty;
        private CampaignTime lastDutyTime = CampaignTime.Zero;
        private Dictionary<Hero, CampaignTime> heroRecords = new Dictionary<Hero, CampaignTime>(); 
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
            dataStore.SyncData("bannerkings-army-records", ref heroRecords);

            if (heroRecords == null)
            {
                heroRecords = new Dictionary<Hero, CampaignTime>();
            }
        }

        public void AddRecord(Hero hero)
        {
            if (heroRecords.ContainsKey(hero))
            {
                heroRecords[hero] = CampaignTime.Now;
            }
            else
            {
                heroRecords.Add(hero, CampaignTime.Now);
            }
        }

        public CampaignTime LastHeroArmy(Hero hero)
        {
            if (heroRecords.ContainsKey(hero))
            {
                return heroRecords[hero];
            }

            return CampaignTime.Zero;
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
            if (kingdom == null || leader != leader.Clan.Leader || party.ActualClan == Clan.PlayerClan || leader.Clan.Influence < 100f)
            {
                return;
            }

            bool war = FactionManager.GetEnemyKingdoms(kingdom).Count() > 0;
            if (war)
            {
                if (!BannerKingsConfig.Instance.ArmyManagementModel.CanCreateArmy(leader)) return;

                CouncilData council = BannerKingsConfig.Instance.CourtManager.GetCouncil(kingdom.RulingClan);
                if (council.GetHeroPositions(leader).Any(x => x.CurrentTask.Equals(DefaultCouncilTasks.Instance.GatherLegion)))
                {
                    if (BannerKingsConfig.Instance.ArmyManagementModel.GetMobilePartiesToCallToArmy(party).Count > 3)
                    {
                        var decision = new CallBannersGoal(leader);
                        decision.DoAiDecision();
                    }
                }

                Clan clan = leader.Clan;
                if (clan.Influence >= BannerKingsConfig.Instance.InfluenceModel.CalculateInfluenceCap(clan).ResultNumber * 0.5f &&
                    party.TotalFoodAtInventory > party.MemberRoster.TotalManCount * 0.5f &&
                    BannerKingsConfig.Instance.ArmyManagementModel.GetMobilePartiesToCallToArmy(party).Count > 3)
                {
                    var decision = new CallBannersGoal(leader);
                    decision.DoAiDecision();
                }
            }
        }

        public void OnPartyJoinedArmyEvent(MobileParty party)
        {
            var playerKingdom = Clan.PlayerClan.Kingdom;
            if (playerKingdom == null || playerKingdom != party.MapFaction || party == MobileParty.MainParty)
            {
                return;
            }

            var playerTitle =
                BannerKingsConfig.Instance.TitleManager.GetHighestTitleWithinFaction(Hero.MainHero, playerKingdom);
            if (playerTitle != null)
            {
               // EvaluateSummonPlayer(playerTitle, party.Army, party);
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
                BannerKingsConfig.Instance.TitleManager == null || leaderParty == MobileParty.MainParty
                || leaderParty.MapFaction != Hero.MainHero.MapFaction)
            {
                return;
            }

            var playerTitle =
                BannerKingsConfig.Instance.TitleManager.GetHighestTitleWithinFaction(Hero.MainHero, playerKingdom);
            if (playerTitle != null)
            {
                //EvaluateSummonPlayer(playerTitle, army);
            }
        }

        /*private void EvaluateSummonPlayer(FeudalTitle playerTitle, Army army, MobileParty joinningParty = null)
        {
            
             return;
            

            //var completion = contract.Duties[FeudalDuties.Auxilium];

            var suzerain = BannerKingsConfig.Instance.TitleManager.GetImmediateSuzerain(playerTitle);
            if (suzerain == null || suzerain.deJure == null)
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
        }*/

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
            TickDuty(mobileParty);
            if (mobileParty != MobileParty.MainParty && mobileParty.Army != null && mobileParty.Army.LeaderParty == mobileParty)
            {
                List<AiBehavior> behaviors = new List<AiBehavior>()
                {
                    AiBehavior.BesiegeSettlement,
                    AiBehavior.RaidSettlement,
                    AiBehavior.DefendSettlement
                };

                if (mobileParty.Ai.HourCounter == 1 && !mobileParty.Ai.IsDisabled && behaviors.Contains(mobileParty.DefaultBehavior))
                {
                    mobileParty.Ai.DisableForHours(6);
                    mobileParty.Ai.HourCounter = 0;
                }
            }
        }

        private void TickDuty(MobileParty mobileParty)
        {
            if (playerArmyDuty == null || mobileParty != playerArmyDuty.Party) return;

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