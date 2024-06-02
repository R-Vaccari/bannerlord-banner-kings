using BannerKings.Utils;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Behaviours.Mercenary
{
    public class BKMercenaryCareerBehavior : BannerKingsBehavior
    {
        private Dictionary<Clan, MercenaryCareer> careers = new Dictionary<Clan, MercenaryCareer>();

        public MercenaryCareer GetCareer(Clan clan)
        {
            if (careers.ContainsKey(clan))
            {
                return careers[clan];
            }

            return null;
        }

        public override void RegisterEvents()
        {
            CampaignEvents.RenownGained.AddNonSerializedListener(this, OnRenownGained);
            CampaignEvents.DailyTickClanEvent.AddNonSerializedListener(this, OnClanDailyTick);
            CampaignEvents.OnClanChangedKingdomEvent.AddNonSerializedListener(this, OnClanChangedKingdom);
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, OnGameLoaded);
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("bannerkings-mercenary-careers", ref careers);

            if (careers == null)
            {
                careers = new Dictionary<Clan, MercenaryCareer>();
            }
        }

        private void OnSessionLaunched(CampaignGameStarter starter)
        {
            InitCareers();

            starter.AddGameMenuOption("bannerkings_actions", "action_recruit_custom_levy",
                "{=32rn9nkn}Recruit {CUSTOM_TROOP}",
                (MenuCallbackArgs args) =>
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.Recruit;
                    if (!careers.ContainsKey(Clan.PlayerClan))
                    {
                        return false;
                    }

                    if (Settlement.CurrentSettlement.MapFaction.IsAtWarWith(Clan.PlayerClan.MapFaction))
                    {
                        return false;
                    }

                    var career = careers[Clan.PlayerClan];
                    var troop = career.GetTroop(Settlement.CurrentSettlement.Culture, true);
                    if (troop == null)
                    {
                        return false;
                    }

                    MBTextManager.SetTextVariable("CUSTOM_TROOP", troop.Name);
                    return true;
                },
                (MenuCallbackArgs args) =>
                {
                    var career = careers[Clan.PlayerClan];
                    var troop = career.GetTroop(Settlement.CurrentSettlement.Culture, true);
                    var cost = TaleWorlds.CampaignSystem.Campaign.Current.Models.PartyWageModel.GetTroopRecruitmentCost(troop.Character, Hero.MainHero);
                    Hero.MainHero.ChangeHeroGold(-cost);
                    MobileParty.MainParty.AddElementToMemberRoster(troop.Character, 1);
                });

            starter.AddGameMenuOption("bannerkings_actions", "action_recruit_custom_professional",
                "{=32rn9nkn}Recruit {CUSTOM_TROOP}",
                (MenuCallbackArgs args) =>
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.Recruit;
                    if (!careers.ContainsKey(Clan.PlayerClan))
                    {
                        return false;
                    }

                    if (Settlement.CurrentSettlement.MapFaction.IsAtWarWith(Clan.PlayerClan.MapFaction))
                    {
                        return false;
                    }

                    var career = careers[Clan.PlayerClan];
                    var troop = career.GetTroop(Settlement.CurrentSettlement.Culture, false);
                    if (troop == null)
                    {
                        return false;
                    }

                    MBTextManager.SetTextVariable("CUSTOM_TROOP", troop.Name);
                    return true;
                },
                (MenuCallbackArgs args) =>
                {
                    var career = careers[Clan.PlayerClan];
                    var troop = career.GetTroop(Settlement.CurrentSettlement.Culture, false);
                    var cost = TaleWorlds.CampaignSystem.Campaign.Current.Models.PartyWageModel.GetTroopRecruitmentCost(troop.Character, Hero.MainHero);
                    Hero.MainHero.ChangeHeroGold(-cost);
                    MobileParty.MainParty.AddElementToMemberRoster(troop.Character, 1);
                });
        }

        private void OnGameLoaded(CampaignGameStarter starer)
        {
            InitCareers();
            foreach (var career in careers.Values)
            {
                career.PostInitialize();
            }
        }

        private void OnRenownGained(Hero hero, int gainedRenown, bool doNotNotifyPlayer)
        {
            if (hero.Clan == null || !careers.ContainsKey(hero.Clan) || !hero.Clan.IsUnderMercenaryService)
            {
                return;
            }

            var career = careers[hero.Clan];
            career.AddReputation(gainedRenown / 100f, new TextObject("{=1KYFwcr7}Reputation from gained renown."));
        }

        private void OnClanChangedKingdom(Clan clan, Kingdom oldKingdom, Kingdom newKingdom,
            ChangeKingdomAction.ChangeKingdomActionDetail detail, bool showNotification = true)
        {
            if (detail == ChangeKingdomAction.ChangeKingdomActionDetail.JoinAsMercenary)
            {
                AddDownPayment(clan, newKingdom);
                AddCareer(clan, newKingdom);
            }

            if (detail == ChangeKingdomAction.ChangeKingdomActionDetail.LeaveAsMercenary)
            {
                MercenaryCareer career = GetCareer(clan);
                if (career != null) career.RemoveKingdom(oldKingdom);
            }
        }

        private void AddDownPayment(Clan mercenaryClan, Kingdom kingdom)
        {
            int gold = (int)BannerKingsConfig.Instance.DiplomacyModel.GetMercenaryDownPayment(mercenaryClan, kingdom).ResultNumber;
            int result = MathF.Min(gold, kingdom.KingdomBudgetWallet);
            kingdom.KingdomBudgetWallet -= result;
            mercenaryClan.Leader.ChangeHeroGold(result);
            if (mercenaryClan == Clan.PlayerClan)
                InformationManager.DisplayMessage(new InformationMessage(new TextObject("{=!}The {CLAN} has received {GOLD}{GOLD_ICON} as earnest-money for their service.")
                    .SetTextVariable("CLAN", mercenaryClan.Name)
                    .SetTextVariable("GOLD", result)
                    .ToString(),
                    Color.FromUint(TextHelper.COLOR_LIGHT_BLUE)));
        }

        private void OnClanDailyTick(Clan clan)
        {
            if (clan.IsUnderMercenaryService)
            {
                if (!careers.ContainsKey(clan)) AddCareer(clan, clan.Kingdom);
                else
                {
                    var career = careers[clan];
                    career.Tick(GetDailyCareerPointsGain(clan).ResultNumber);
                }
            }
            else if (clan.Kingdom != null && clan.Kingdom.RulingClan == clan)
            {
                List<Clan> toFire = new List<Clan>();
                foreach (Clan merc in clan.Kingdom.Clans)
                {
                    if (!merc.IsUnderMercenaryService) continue;

                    var career = GetCareer(merc);
                    if (career == null) continue;

                    RunWeekly(() =>
                    {
                        if (BannerKingsConfig.Instance.DiplomacyModel.GetScoreOfKingdomToSackMercenary(clan.Kingdom, merc) >
                        BannerKingsConfig.Instance.DiplomacyModel.GetScoreOfKingdomToHireMercenary(clan.Kingdom, merc))
                        {
                            toFire.Add(merc);
                        }
                    },
                    GetType().Name,
                    false);
                }

                foreach (Clan merc in toFire)
                {
                    var career = GetCareer(merc);
                    career.RemoveKingdom(merc.Kingdom, true);
                    ChangeKingdomAction.ApplyByLeaveKingdomAsMercenary(merc);
                }
            }
        }

        private void AddCareer(Clan clan, Kingdom kingdom)
        {
            if (kingdom == null) return;
            
            if (!careers.ContainsKey(clan)) careers.Add(clan, new MercenaryCareer(clan, kingdom));

            careers[clan].AddKingdom(kingdom);
        }

        private void InitCareers()
        {
            foreach (var clan in Clan.All)
            {
                if (clan.IsUnderMercenaryService)
                {
                    AddCareer(clan, clan.Kingdom);
                }
            }
        }

        internal ExplainedNumber GetDailyCareerPointsGain(Clan clan, bool explanations = false)
        {
            var result = new ExplainedNumber(1f, explanations);
            result.Add(clan.Tier / 2f, GameTexts.FindText("str_clan_tier_bonus"));
            result.Add(careers[clan].Reputation * 2f, new TaleWorlds.Localization.TextObject("{=bLLovmn9}Reputation"));

            foreach (var party in clan.WarPartyComponents)
            {
                if (party.MobileParty.Army != null)
                {
                    result.Add(1f, party.Name);
                    if (party.MobileParty.Army.LeaderParty == party.MobileParty)
                    {
                        result.AddFactor(0.2f, new TaleWorlds.Localization.TextObject("{=oV2MhyoO}Leading an Army"));
                    }
                }
            }

            return result;
        }
    }
}