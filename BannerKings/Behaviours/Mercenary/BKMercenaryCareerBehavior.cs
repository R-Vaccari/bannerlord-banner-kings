using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Behaviours.Mercenary
{
    internal class BKMercenaryCareerBehavior : CampaignBehaviorBase
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
            CampaignEvents.ClanChangedKingdom.AddNonSerializedListener(this, OnClanChangedKingdom);
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
                "{=!}Recruit {CUSTOM_TROOP}",
                (MenuCallbackArgs args) =>
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.Recruit;
                    if (!careers.ContainsKey(Clan.PlayerClan))
                    {
                        return false;
                    }

                    if (Clan.PlayerClan.Kingdom == null)
                    {
                        return false;
                    }

                    var career = careers[Clan.PlayerClan];
                    var troop = career.GetTroop(Clan.PlayerClan.Kingdom, true);
                    if (troop == null)
                    {
                        return false;
                    }

                    if (troop.Character.Culture != Hero.MainHero.CurrentSettlement.Culture)
                    {
                        return false;
                    }

                    MBTextManager.SetTextVariable("CUSTOM_TROOP", troop.Name);

                    return true;
                },
                (MenuCallbackArgs args) =>
                {
                    var career = careers[Clan.PlayerClan];
                    var troop = career.GetTroop(Clan.PlayerClan.Kingdom, true);
                    var cost = Campaign.Current.Models.PartyWageModel.GetTroopRecruitmentCost(troop.Character, Hero.MainHero);
                    Hero.MainHero.ChangeHeroGold(-cost);
                    MobileParty.MainParty.AddElementToMemberRoster(troop.Character, 1);
                });

            starter.AddGameMenuOption("bannerkings_actions", "action_recruit_custom_professional",
                "{=!}Recruit {CUSTOM_TROOP}",
                (MenuCallbackArgs args) =>
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.Recruit;
                    if (!careers.ContainsKey(Clan.PlayerClan))
                    {
                        return false;
                    }

                    if (Clan.PlayerClan.Kingdom == null)
                    {
                        return false;
                    }

                    var career = careers[Clan.PlayerClan];
                    var troop = career.GetTroop(Clan.PlayerClan.Kingdom, false);
                    if (troop == null)
                    {
                        return false;
                    }

                    if (troop.Character.Culture != Hero.MainHero.CurrentSettlement.Culture)
                    {
                        return false;
                    }

                    MBTextManager.SetTextVariable("CUSTOM_TROOP", troop.Name);

                    return true;
                },
                (MenuCallbackArgs args) =>
                {
                    var career = careers[Clan.PlayerClan];
                    var troop = career.GetTroop(Clan.PlayerClan.Kingdom, false);
                    var cost = Campaign.Current.Models.PartyWageModel.GetTroopRecruitmentCost(troop.Character, Hero.MainHero);
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
            if (hero.Clan == null || !careers.ContainsKey(hero.Clan))
            {
                return;
            }

            var career = careers[hero.Clan];
            career.AddReputation(gainedRenown / 50f, new TaleWorlds.Localization.TextObject("{=!}Reputation from gained renown."));
        }

        private void OnClanChangedKingdom(Clan clan, Kingdom oldKingdom, Kingdom newKingdom,
            ChangeKingdomAction.ChangeKingdomActionDetail detail, bool showNotification = true)
        {
            if (detail == ChangeKingdomAction.ChangeKingdomActionDetail.JoinAsMercenary)
            {
                AddCareer(clan, newKingdom);
            }
        }

        private void OnClanDailyTick(Clan clan)
        {
            if (clan.IsUnderMercenaryService)
            {
                if (!careers.ContainsKey(clan))
                {
                    AddCareer(clan, clan.Kingdom);
                }
                else
                {
                    careers[clan].Tick(GetDailyCareerPointsGain(clan).ResultNumber);
                }
            }
        }

        private void AddCareer(Clan clan, Kingdom kingdom)
        {
            if (kingdom == null)
            {
                return;
            }

            if (!careers.ContainsKey(clan))
            {
                careers.Add(clan, new MercenaryCareer(clan, kingdom));
            }

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
            result.Add(careers[clan].Reputation * 2f, new TaleWorlds.Localization.TextObject("{=!}Reputation"));

            foreach (var party in clan.WarPartyComponents)
            {
                if (party.MobileParty.Army != null)
                {
                    result.Add(1f, party.Name);
                    if (party.MobileParty.Army.LeaderParty == party.MobileParty)
                    {
                        result.AddFactor(0.2f, new TaleWorlds.Localization.TextObject("{=!}Leading an Army"));
                    }
                }
            }

            return result;
        }
    }
}