using System.Collections.Generic;
using BannerKings.Managers.Education;
using BannerKings.Managers.Skills;
using BannerKings.UI;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Overlay;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Behaviours
{
    public class BKSettlementActions : CampaignBehaviorBase
    {
        private static float actionGold;
        private static int actionHuntGame;
        private static CampaignTime actionStart = CampaignTime.Now;
        private float totalHours;
        private Dictionary<Town, CampaignTime> lastMercenaryRecruitment;

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("bannerkings-towns-mercenary-recruitment", ref lastMercenaryRecruitment);
        }

        public int GetRosterCost(TroopRoster roster, Hero hero)
        {
            float cost = 0;
            foreach (var element in roster.GetTroopRoster())
            {
                cost += Campaign.Current.Models.PartyWageModel.GetTroopRecruitmentCost(element.Character, hero) 
                    * (float)element.Number * 5f;
            }

            return (int)cost;
        }

        public TroopRoster GetMercenaryTemplateRoster(PartyTemplateObject template)
        {
            var roster = new TroopRoster(null);
            foreach (var stack in template.Stacks)
            {
                var element = new TroopRosterElement(stack.Character);
                element.Number = stack.MinValue;
                roster.Add(element);
            }

            return roster;
        }

        public List<(PartyTemplateObject, TextObject)> GetLocalMercenaryTemplates(Town town)
        {
            var templates = new List<(PartyTemplateObject, TextObject)>();
            foreach (var clan in Clan.All)
            {
                if (clan.IsMinorFaction && clan.Culture == town.Culture && clan.DefaultPartyTemplate != null
                    && clan != Clan.PlayerClan)
                {
                    templates.Add(new (clan.DefaultPartyTemplate, clan.Name));
                }
            }

            return templates;
        }

        public bool CanRecruitLocalMercenaries(Hero hero)
        {
            var education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(hero);
            var hasMercenaries = false;
            if (hero.CurrentSettlement != null && hero.CurrentSettlement.IsTown)
            {
                hasMercenaries = GetLocalMercenaryTemplates(hero.CurrentSettlement.Town).Count > 0 &&
                    lastMercenaryRecruitment[hero.CurrentSettlement.Town].ElapsedWeeksUntilNow >= 1f;
            }
            return hasMercenaries && ((hero.Clan.IsMinorFaction && hero != Hero.MainHero) || education.HasPerk(BKPerks.Instance.MercenaryLocalConnections));
        }

        private void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
            if (lastMercenaryRecruitment == null)
            {
                lastMercenaryRecruitment = new Dictionary<Town, CampaignTime>();
                foreach (var town in Town.AllTowns)
                {
                    lastMercenaryRecruitment.Add(town, CampaignTime.Now - CampaignTime.Weeks(1f));
                }
            }


            campaignGameStarter.AddGameMenu("bannerkings", "Banner Kings", MenuBannerKingsInit);
            campaignGameStarter.AddGameMenu("bannerkings_actions", "Banner Kings", MenuBannerKingsInit);

            // ------- WAIT MENUS --------

            campaignGameStarter.AddWaitGameMenu("bannerkings_wait_guard",
                "{=HJL6rSZ9}You are serving as a guard in {CURRENT_SETTLEMENT}.",
                MenuWaitInit,
                MenuGuardActionPeasantCondition,
                MenuActionConsequenceWithGold,
                TickWaitGuard, GameMenu.MenuAndOptionType.WaitMenuShowProgressAndHoursOption,
                GameOverlays.MenuOverlayType.SettlementWithBoth, 8f);

            campaignGameStarter.AddGameMenuOption("bannerkings_wait_guard", "wait_leave", "{=1kJ3hNWg}Leave",
                delegate(MenuCallbackArgs args)
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                    return true;
                },
                delegate(MenuCallbackArgs args)
                {
                    PlayerEncounter.Current.IsPlayerWaiting = false;
                    SwitchToMenuIfThereIsAnInterrupt(args.MenuContext.GameMenu.StringId);
                }, true);

            campaignGameStarter.AddWaitGameMenu("bannerkings_wait_train_guards",
                "{=nATj4Nc1}You are training the guards in {CURRENT_SETTLEMENT}.",
                MenuWaitInit,
                MenuTrainGuardActionPeasantCondition,
                MenuActionConsequenceWithGold,
                TickWaitTrainGuard, GameMenu.MenuAndOptionType.WaitMenuShowProgressAndHoursOption,
                GameOverlays.MenuOverlayType.SettlementWithBoth, 8f);

            campaignGameStarter.AddGameMenuOption("bannerkings_wait_train_guards", "wait_leave", "{=1kJ3hNWg}Leave",
                delegate(MenuCallbackArgs args)
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                    return true;
                },
                delegate(MenuCallbackArgs args)
                {
                    PlayerEncounter.Current.IsPlayerWaiting = false;
                    SwitchToMenuIfThereIsAnInterrupt(args.MenuContext.GameMenu.StringId);
                }, true);


            campaignGameStarter.AddWaitGameMenu("bannerkings_wait_hunt",
                "{=ZicUVOPr}You are hunting in the region of {CURRENT_SETTLEMENT}. Game quantity in this region is {HUNTING_GAME}.",
                MenuWaitInit,
                MenuHuntingActionCondition,
                MenuActionHuntingConsequence,
                TickWaitHunt, GameMenu.MenuAndOptionType.WaitMenuShowProgressAndHoursOption,
                GameOverlays.MenuOverlayType.SettlementWithBoth, 8f);

            campaignGameStarter.AddGameMenuOption("bannerkings_wait_hunt", "wait_leave", "{=1kJ3hNWg}Leave",
                delegate(MenuCallbackArgs args)
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                    return true;
                },
                delegate(MenuCallbackArgs args)
                {
                    PlayerEncounter.Current.IsPlayerWaiting = false;
                    SwitchToMenuIfThereIsAnInterrupt(args.MenuContext.GameMenu.StringId);
                }, true);

            campaignGameStarter.AddWaitGameMenu("bannerkings_wait_meet_nobility",
                "{=N9Jznx5N}You are meeting with the high society of {CURRENT_SETTLEMENT}.",
                MenuWaitInit,
                MenuMeetNobilityActionCondition,
                MenuActionMeetNobilityConsequence,
                TickWaitMeetNobility, GameMenu.MenuAndOptionType.WaitMenuShowProgressAndHoursOption,
                GameOverlays.MenuOverlayType.SettlementWithBoth, 4f);

            campaignGameStarter.AddGameMenuOption("bannerkings_wait_meet_nobility", "wait_leave", "{=1kJ3hNWg}Leave",
                delegate(MenuCallbackArgs args)
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                    return true;
                },
                delegate(MenuCallbackArgs args)
                {
                    PlayerEncounter.Current.IsPlayerWaiting = false;
                    SwitchToMenuIfThereIsAnInterrupt(args.MenuContext.GameMenu.StringId);
                }, true);

            campaignGameStarter.AddWaitGameMenu("bannerkings_wait_study",
                "{=533oQJOp}You are studying scholarship with {SCHOLARSHIP_TUTOR}. The instruction costs {SCHOLARSHIP_GOLD} per hour.",
                MenuWaitInit,
                MenuActionStudyCondition,
                MenuActionConsequenceNeutral,
                TickWaitStudy, GameMenu.MenuAndOptionType.WaitMenuShowProgressAndHoursOption,
                GameOverlays.MenuOverlayType.SettlementWithBoth, 4f);

            campaignGameStarter.AddGameMenuOption("bannerkings_wait_study", "wait_leave", "{=1kJ3hNWg}Leave",
                delegate(MenuCallbackArgs args)
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                    return true;
                },
                delegate(MenuCallbackArgs args)
                {
                    PlayerEncounter.Current.IsPlayerWaiting = false;
                    SwitchToMenuIfThereIsAnInterrupt(args.MenuContext.GameMenu.StringId);
                }, true);


            campaignGameStarter.AddWaitGameMenu("bannerkings_wait_crafting",
                "{=EUWd2dC5}You are working on the smith for {CRAFTING_HOURS} hours. The current hourly rate of this smith is: {CRAFTING_RATE} {GOLD_ICON}.{CRAFTING_EXPLANATION}",
                MenuWaitInit,
                _ => true,
                MenuActionConsequenceNeutral,
                TickWaitCrafting, GameMenu.MenuAndOptionType.WaitMenuShowProgressAndHoursOption,
                GameOverlays.MenuOverlayType.SettlementWithBoth);


            // ------- ACTIONS --------

            campaignGameStarter.AddGameMenuOption("bannerkings_actions", "action_local_connections", "{=B0KoTpr4}Recruit local mercenaries",
                MenuActionLocalConnectionsCondition, MenuActionLocalConnectionsConsequence);

            campaignGameStarter.AddGameMenuOption("bannerkings_actions", "action_study", "{=rThJzFpn}Study scholarship",
                MenuActionStudyCondition, delegate { GameMenu.SwitchToMenu("bannerkings_wait_study"); });

            campaignGameStarter.AddGameMenuOption("bannerkings_actions", "action_slave_transfer", "{=WbqhDh40}Transfer slaves",
                MenuSlavesActionCondition, delegate { UIHelper.ShowSlaveTransferScreen(); });

            campaignGameStarter.AddGameMenuOption("bannerkings_actions", "action_meet_nobility", "{=EFoATzkQ}Meet nobility",
                MenuMeetNobilityActionCondition, delegate { GameMenu.SwitchToMenu("bannerkings_wait_meet_nobility"); });

            campaignGameStarter.AddGameMenuOption("bannerkings_actions", "action_guard", "{=UyQA2wzu}Serve as guard",
                MenuGuardActionPeasantCondition, delegate { GameMenu.SwitchToMenu("bannerkings_wait_guard"); });

            campaignGameStarter.AddGameMenuOption("bannerkings_actions", "action_train_guards", "{=6OJTPxY0}Train guards",
                MenuTrainGuardActionPeasantCondition, delegate { GameMenu.SwitchToMenu("bannerkings_wait_train_guards"); });

            campaignGameStarter.AddGameMenuOption("bannerkings_actions", "action_hunt", "{=PQSzdrkg}Go hunting",
                MenuHuntingActionCondition, delegate { GameMenu.SwitchToMenu("bannerkings_wait_hunt"); });

            campaignGameStarter.AddGameMenuOption("bannerkings_actions", "bannerkings_leave", "{=1kJ3hNWg}Leave",
                delegate(MenuCallbackArgs x)
                {
                    x.optionLeaveType = GameMenuOption.LeaveType.Leave;
                    return true;
                }, delegate { GameMenu.SwitchToMenu("bannerkings"); }, true);


            // ------- TOWN --------

            campaignGameStarter.AddGameMenuOption("town", "bannerkings_submenu", "{=WaBMVVH9}Banner Kings",
                delegate(MenuCallbackArgs x)
                {
                    x.optionLeaveType = GameMenuOption.LeaveType.Submenu;
                    return BannerKingsConfig.Instance.PopulationManager != null &&
                           BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(Settlement.CurrentSettlement);
                },
                delegate { GameMenu.SwitchToMenu("bannerkings"); }, false, 4);

            campaignGameStarter.AddGameMenuOption("bannerkings", "manage_demesne", "{=agYwzCm1}Demesne management",
                MenuSettlementManageCondition,
                MenuSettlementManageConsequence);

            campaignGameStarter.AddGameMenuOption("bannerkings", "manage_titles", "{=sSyB0ovj}Demesne hierarchy",
                MenuTitlesCondition,
                MenuTitlesConsequence);

            //campaignGameStarter.AddGameMenuOption("bannerkings", "manage_faith", "{=m5mzZkkS}{RELIGION_NAME}",
            //    MenuFaithCondition,
            //    MenuFaithConsequence);

            campaignGameStarter.AddGameMenuOption("bannerkings", "manage_guild", "{=sywChwxo}{GUILD_NAME}",
                MenuGuildCondition,
                MenuGuildManageConsequence);


            campaignGameStarter.AddGameMenuOption("bannerkings", "bannerkings_action", "{=NtkWYD54}Take an action",
                delegate(MenuCallbackArgs args)
                {
                    args.optionLeaveType = GameMenuOption.LeaveType.Wait;
                    return true;
                },
                delegate { GameMenu.SwitchToMenu("bannerkings_actions"); });

            campaignGameStarter.AddGameMenuOption("bannerkings", "bannerkings_leave", "{=1kJ3hNWg}Leave",
                delegate(MenuCallbackArgs x)
                {
                    x.optionLeaveType = GameMenuOption.LeaveType.Leave;
                    return true;
                }, delegate
                {
                    var menu = Settlement.CurrentSettlement.IsVillage ? "village" :
                        Settlement.CurrentSettlement.IsCastle ? "castle" : "town";
                    GameMenu.SwitchToMenu(menu);
                }, true);


            // ------- CASTLE --------


            campaignGameStarter.AddGameMenuOption("castle", "bannerkings_castle_submenu", "{=WaBMVVH9}Banner Kings",
                delegate(MenuCallbackArgs x)
                {
                    x.optionLeaveType = GameMenuOption.LeaveType.Submenu;
                    return BannerKingsConfig.Instance.PopulationManager != null &&
                           BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(Settlement.CurrentSettlement);
                },
                delegate { GameMenu.SwitchToMenu("bannerkings"); }, false, 4);

            campaignGameStarter.AddGameMenuOption("bannerkings", "castle_recruit_volunteers", "{=nRm78XAk}Recruit troops",
                MenuCastleRecruitsCondition,
                delegate(MenuCallbackArgs args) { args.MenuContext.OpenRecruitVolunteers(); },
                false, 3);


            // ------- VILLAGE --------


            campaignGameStarter.AddGameMenuOption("village", "bannerkings_village_submenu", "{=WaBMVVH9}Banner Kings",
                delegate(MenuCallbackArgs x)
                {
                    x.optionLeaveType = GameMenuOption.LeaveType.Submenu;
                    return BannerKingsConfig.Instance.PopulationManager != null &&
                           BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(Settlement.CurrentSettlement);
                },
                delegate { GameMenu.SwitchToMenu("bannerkings"); }, false, 2);

            campaignGameStarter.AddGameMenuOption("bannerkings", "manage_projects", "{=fsNhEwHz}Village Projects",
                MenuVillageBuildingCondition,
                MenuVillageProjectsConsequence, false, 2);
        }


        // -------- TICKS ----------

        private static void TickWaitGuard(MenuCallbackArgs args, CampaignTime dt)
        {
            TickCheckHealth();
            var progress = args.MenuContext.GameMenu.Progress;
            var diff = (int) actionStart.ElapsedHoursUntilNow;
            if (diff > 0)
            {
                args.MenuContext.GameMenu.SetProgressOfWaitingInMenu(diff * 0.125f);

                if (args.MenuContext.GameMenu.Progress != progress)
                {
                    var settlement = Settlement.CurrentSettlement;
                    float wage = Campaign.Current.Models.PartyWageModel.GetCharacterWage(3);
                    wage *= settlement.Prosperity / 8000f;
                    actionGold += wage;

                    var random = MBRandom.RandomFloat;
                    var injury = 0.1f;

                    if (settlement.Town == null)
                    {
                        InformationManager.DisplayMessage(
                            new InformationMessage(
                                new TextObject("{=5FDBQiU5}Not a town!").ToString()));
                        Hero.MainHero.HitPoints -= MBRandom.RandomInt(3, 10);
                        return;
                    }

                    injury -= settlement.Town.Security * 0.001f;
                    if (random <= injury)
                    {
                        InformationManager.DisplayMessage(
                            new InformationMessage(
                                new TextObject("{=eLyGCZMi}You have been hurt in your current action.").ToString()));
                        Hero.MainHero.HitPoints -= MBRandom.RandomInt(3, 10);
                    }

                    var skill = 0.1f;
                    skill += settlement.Town.Security * 0.001f;
                    if (random <= skill)
                    {
                        var skills = new List<(SkillObject, float)>
                        {
                            new(DefaultSkills.OneHanded, 10f),
                            new(DefaultSkills.TwoHanded, 2f),
                            new(DefaultSkills.Polearm, 8f),
                            new(DefaultSkills.Bow, 4f),
                            new(DefaultSkills.Crossbow, 4f),
                            new(DefaultSkills.Athletics, 2f)
                        };

                        var target = MBRandom.ChooseWeighted(skills);
                        GameTexts.SetVariable("SKILL", target.Name);
                        Hero.MainHero.AddSkillXp(target, MBRandom.RandomFloatRanged(10f, 25f));
                        InformationManager.DisplayMessage(
                            new InformationMessage(
                                new TextObject("{=ArMJ9nUV}You have improved your {SKILL} skill during your current action.")
                                    .ToString()));
                    }
                }
            }
        }

        public void StartCraftingMenu(float totalHours)
        {
            this.totalHours = totalHours;
            MBTextManager.SetTextVariable("CRAFTING_HOURS", totalHours.ToString("0.0"));
            var cost = BannerKingsConfig.Instance.SmithingModel.GetSmithingHourlyPrice(Settlement.CurrentSettlement,
                Hero.MainHero);
            var costInt = (int) cost.ResultNumber;
            GameTexts.SetVariable("CRAFTING_RATE", costInt);
            GameTexts.SetVariable("CRAFTING_EXPLANATION", cost.GetExplanations());
            GameTexts.SetVariable("GOLD_ICON", "<img src=\"General\\Icons\\Coin@2x\" extend=\"8\">");
            GameMenu.SwitchToMenu("bannerkings_wait_crafting");
        }

        private static void TickWaitStudy(MenuCallbackArgs args, CampaignTime dt)
        {
            TickCheckHealth();
            var progress = args.MenuContext.GameMenu.Progress;
            var diff = (int) actionStart.ElapsedHoursUntilNow;
            if (diff > 0)
            {
                args.MenuContext.GameMenu.SetProgressOfWaitingInMenu(diff * 0.25f);

                if (args.MenuContext.GameMenu.Progress != progress)
                {
                    var main = Hero.MainHero;
                    var seller = Campaign.Current.GetCampaignBehavior<BKEducationBehavior>()
                        .GetBookSeller(Settlement.CurrentSettlement);
                    if (seller != null)
                    {
                        main.AddSkillXp(BKSkills.Instance.Scholarship, 5f);
                        GiveGoldAction.ApplyBetweenCharacters(Hero.MainHero, seller,
                            (int) BannerKingsConfig.Instance.EducationModel.CalculateLessonsCost(Hero.MainHero, seller)
                                .ResultNumber);
                        InformationManager.DisplayMessage(new InformationMessage(
                            new TextObject("{=ArMJ9nUV}You have improved your {SKILL} skill during your current action.")
                                .SetTextVariable("SKILL", BKSkills.Instance.Scholarship.Name)
                                .ToString()));
                    }
                    else
                    {
                        InformationManager.DisplayMessage(new InformationMessage(
                            new TextObject("{=yLhzjPib}You have stopped your current action because the instructor has left the settlement.")
                                .ToString()));
                        GameMenu.SwitchToMenu("bannerkings_actions");
                    }
                }
            }
        }

        private static void TickWaitTrainGuard(MenuCallbackArgs args, CampaignTime dt)
        {
            var progress = args.MenuContext.GameMenu.Progress;
            var diff = (int) actionStart.ElapsedHoursUntilNow;
            if (diff > 0)
            {
                args.MenuContext.GameMenu.SetProgressOfWaitingInMenu(diff * 0.125f);
                if (args.MenuContext.GameMenu.Progress != progress)
                {
                    var settlement = Settlement.CurrentSettlement;
                    float wage = Campaign.Current.Models.PartyWageModel.GetCharacterWage(5);
                    wage *= settlement.Prosperity / 8000f;
                    actionGold += wage;

                    var random = MBRandom.RandomFloat;
                    var skill = 0.15f;
                    if (random <= skill)
                    {
                        var skills = new List<(SkillObject, float)>
                        {
                            new(DefaultSkills.Leadership, 10f),
                            new(DefaultSkills.OneHanded, 2f),
                            new(DefaultSkills.Polearm, 2f),
                            new(DefaultSkills.Bow, 2f),
                            new(DefaultSkills.Crossbow, 2f)
                        };

                        var target = MBRandom.ChooseWeighted(skills);
                        GameTexts.SetVariable("SKILL", target.Name);
                        Hero.MainHero.AddSkillXp(target, MBRandom.RandomFloatRanged(10f, 25f));
                        InformationManager.DisplayMessage(
                            new InformationMessage(
                                new TextObject("{=ArMJ9nUV}You have improved your {SKILL} skill during your current action.")
                                    .ToString()));
                    }
                }
            }
        }

        private static void TickWaitHunt(MenuCallbackArgs args, CampaignTime dt)
        {
            var progress = args.MenuContext.GameMenu.Progress;
            var diff = (int) actionStart.ElapsedHoursUntilNow;
            if (diff > 0)
            {
                args.MenuContext.GameMenu.SetProgressOfWaitingInMenu(diff * 0.125f);

                var settlement = Settlement.CurrentSettlement;
                var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement).LandData;
                var woodland = data.Woodland;
                var game = woodland switch
                {
                    >= 50000 => new TextObject("{=mwaS94oD}Bountiful").ToString(),
                    >= 25000 => new TextObject("{=6qcxi3hF}Mediocre").ToString(),
                    _ => new TextObject("{=fphXj90S}Poor").ToString()
                };

                GameTexts.SetVariable("HUNTING_GAME", game);
                if (args.MenuContext.GameMenu.Progress != progress)
                {
                    var chance = woodland * 0.001f;
                    var random = MBRandom.RandomFloatRanged(1f, 100f);
                    if (random <= chance)
                    {
                        actionHuntGame += 1;
                        var skills = new List<(SkillObject, float)>
                        {
                            new(DefaultSkills.Bow, 10f),
                            new(DefaultSkills.Crossbow, 5f),
                            new(DefaultSkills.Athletics, 8f)
                        };

                        var target = MBRandom.ChooseWeighted(skills);
                        GameTexts.SetVariable("SKILL", target.Name);
                        Hero.MainHero.AddSkillXp(target, MBRandom.RandomFloatRanged(10f, 25f));
                        InformationManager.DisplayMessage(
                            new InformationMessage(
                                new TextObject("{=vsOFAeWQ}You have have caught an animal and improved your {SKILL} skill while hunting.")
                                    .ToString()));
                    }
                }
            }
        }

        private void TickWaitCrafting(MenuCallbackArgs args, CampaignTime dt)
        {
            var progress = args.MenuContext.GameMenu.Progress;
            var diff = (int) actionStart.ElapsedHoursUntilNow;


            if (diff > 0)
            {
                args.MenuContext.GameMenu.SetProgressOfWaitingInMenu(diff / totalHours);
                if (args.MenuContext.GameMenu.Progress != progress)
                {
                    var cost = BannerKingsConfig.Instance.SmithingModel.GetSmithingHourlyPrice(Settlement.CurrentSettlement,
                        Hero.MainHero);
                    var costInt = (int) cost.ResultNumber;
                    GameTexts.SetVariable("CRAFTING_RATE", costInt);
                    GameTexts.SetVariable("CRAFTING_EXPLANATION", cost.GetExplanations());
                    GiveGoldAction.ApplyForCharacterToSettlement(Hero.MainHero, Settlement.CurrentSettlement, costInt);
                }
            }
        }

        private static void TickWaitMeetNobility(MenuCallbackArgs args, CampaignTime dt)
        {
            var progress = args.MenuContext.GameMenu.Progress;
            var diff = (int) actionStart.ElapsedHoursUntilNow;
            if (diff > 0)
            {
                args.MenuContext.GameMenu.SetProgressOfWaitingInMenu(diff * 0.250f);
                if (args.MenuContext.GameMenu.Progress != progress)
                {
                    var chance = Hero.MainHero.GetSkillValue(DefaultSkills.Charm) * 0.05f + 15f;
                    var random = MBRandom.RandomFloatRanged(1f, 100f);
                    if (random <= chance)
                    {
                        var influence = MBRandom.RandomFloatRanged(0.1f, 0.5f);
                        GainKingdomInfluenceAction.ApplyForDefault(Hero.MainHero, influence);
                        GameTexts.SetVariable("INFLUENCE", influence);
                        GameTexts.SetVariable("SKILL", DefaultSkills.Charm.Name);
                        Hero.MainHero.AddSkillXp(DefaultSkills.Charm, MBRandom.RandomFloatRanged(10f, 25f));
                        InformationManager.DisplayMessage(new InformationMessage(
                            new TextObject("{=mXfR84iG}You have improved your {SKILL} skill and gained {INFLUENCE} influence while meeting with nobles.")
                                .ToString()));
                    }
                }
            }
        }

        private static void TickCheckHealth()
        {
            if (IsWounded())
            {
                InformationManager.DisplayMessage(
                    new InformationMessage(
                        new TextObject("{=DHMTLgw4}You have stopped your current action due to health conditions.").ToString()));
                GameMenu.SwitchToMenu("bannerkings_actions");
            }
        }

        // -------- CONDITIONS ----------

        private static bool IsPeasant()
        {
            return Clan.PlayerClan.Kingdom == null &&
                   (Clan.PlayerClan.Fiefs == null || Clan.PlayerClan.Fiefs.Count == 0);
        }

        private static bool IsWounded()
        {
            return Hero.MainHero.HitPoints / (float) Hero.MainHero.MaxHitPoints <= 0.4f;
        }

        private static bool IsCriminal(Clan ownerClan)
        {
            var criminal = false;
            var kingdom = ownerClan?.Kingdom;
            if (kingdom != null)
            {
                criminal = kingdom.MainHeroCrimeRating > 0;
            }

            return criminal;
        }

        private static bool MenuSlavesActionCondition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.TroopSelection;
            return Settlement.CurrentSettlement.OwnerClan == Clan.PlayerClan;
        }

        private static bool MenuWaitActionPeasantCondition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Wait;
            MBTextManager.SetTextVariable("CURRENT_SETTLEMENT", Settlement.CurrentSettlement.EncyclopediaLinkWithName);
            return IsPeasant();
        }

        private static bool MenuGuardActionPeasantCondition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Recruit;
            MBTextManager.SetTextVariable("CURRENT_SETTLEMENT", Settlement.CurrentSettlement.EncyclopediaLinkWithName);

            return IsPeasant() && !IsWounded() && !IsCriminal(Settlement.CurrentSettlement.OwnerClan) &&
                   !Settlement.CurrentSettlement.IsVillage;
        }

        private bool MenuActionLocalConnectionsCondition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Recruit;
            var canRecruit = CanRecruitLocalMercenaries(Hero.MainHero);
            return Settlement.CurrentSettlement.IsTown && canRecruit;
        }

        private bool MenuActionStudyCondition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Wait;
            var seller = Campaign.Current.GetCampaignBehavior<BKEducationBehavior>()
                .GetBookSeller(Settlement.CurrentSettlement);
            var hasSeller = seller != null;
            if (hasSeller)
            {
                MBTextManager.SetTextVariable("SCHOLARSHIP_TUTOR", seller.Name);
                MBTextManager.SetTextVariable("SCHOLARSHIP_GOLD", BannerKingsConfig.Instance.EducationModel
                    .CalculateLessonsCost(Hero.MainHero,
                        seller).ResultNumber.ToString());
            }

            return !IsWounded() && hasSeller && !IsCriminal(Settlement.CurrentSettlement.OwnerClan) &&
                   !Settlement.CurrentSettlement.IsVillage;
        }

        private static bool MenuTrainGuardActionPeasantCondition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.OrderTroopsToAttack;
            MBTextManager.SetTextVariable("CURRENT_SETTLEMENT", Settlement.CurrentSettlement.EncyclopediaLinkWithName);
            var leadership = Hero.MainHero.GetSkillValue(DefaultSkills.Leadership);
            var combat = Hero.MainHero.GetSkillValue(DefaultSkills.OneHanded) +
                         Hero.MainHero.GetSkillValue(DefaultSkills.Polearm) +
                         Hero.MainHero.GetSkillValue(DefaultSkills.Bow) +
                         Hero.MainHero.GetSkillValue(DefaultSkills.Crossbow);
            return IsPeasant() && !IsWounded() && !IsCriminal(Settlement.CurrentSettlement.OwnerClan) && leadership >= 50 &&
                   combat >= 160;
        }

        private static bool MenuMeetNobilityActionCondition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Conversation;
            MBTextManager.SetTextVariable("CURRENT_SETTLEMENT", Settlement.CurrentSettlement.EncyclopediaLinkWithName);
            var inFaction = Clan.PlayerClan.Kingdom != null;
            return !IsWounded() && !IsCriminal(Settlement.CurrentSettlement.OwnerClan) && inFaction;
        }

        private static bool MenuHuntingActionCondition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Continue;
            MBTextManager.SetTextVariable("CURRENT_SETTLEMENT", Settlement.CurrentSettlement.EncyclopediaLinkWithName);
            var criminal = false;
            var huntingRight = false;
            var clan = Settlement.CurrentSettlement.OwnerClan;
            var kingdom = clan?.Kingdom;
            if (kingdom != null)
            {
                criminal = kingdom.MainHeroCrimeRating > 0;
                huntingRight = kingdom.HasPolicy(DefaultPolicies.HuntingRights);
            }

            return !IsWounded() && !criminal && (huntingRight || Settlement.CurrentSettlement.OwnerClan == Clan.PlayerClan);
        }

        private static bool MenuCastleRecruitsCondition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Recruit;
            var kingdom = Clan.PlayerClan.Kingdom;
            return Settlement.CurrentSettlement.IsCastle && Settlement.CurrentSettlement.Notables.Count > 0 &&
                   (Settlement.CurrentSettlement.OwnerClan == Clan.PlayerClan ||
                    kingdom == Settlement.CurrentSettlement.OwnerClan.Kingdom);
        }

        private static bool MenuGuildCondition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Trade;
            var settlement = Settlement.CurrentSettlement;
            var hasGuild = false;
            if (BannerKingsConfig.Instance.PopulationManager != null &&
                BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(settlement))
            {
                var guild = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement).EconomicData.Guild;
                hasGuild = guild != null;
                if (hasGuild)
                {
                    GameTexts.SetVariable("GUILD_NAME", guild.GuildType.Name.ToString());
                }
            }

            return hasGuild;
        }

        private static bool MenuFaithCondition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.RansomAndBribe;

            if (BannerKingsConfig.Instance.PopulationManager == null || !BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(Settlement.CurrentSettlement))
            {
                return false;
            }

            var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(Settlement.CurrentSettlement).ReligionData;
            if (data == null)
            {
                return false;
            }

            var faith = data.DominantReligion?.Faith;
            MBTextManager.SetTextVariable("RELIGION_NAME", faith is null ? "{=mVzVY3P9}No Faith available" : faith.GetFaithName());

            return true;
        }

        private static bool MenuTitlesCondition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Surrender;
            var currentSettlement = Settlement.CurrentSettlement;
            return currentSettlement.MapFaction == Hero.MainHero.MapFaction;
        }

        private static bool MenuSettlementManageCondition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Manage;
            var deJure = false;
            var title = BannerKingsConfig.Instance.TitleManager.GetTitle(Settlement.CurrentSettlement);
            if (title != null && title.deJure == Hero.MainHero &&
                Settlement.CurrentSettlement.OwnerClan.Kingdom == Clan.PlayerClan.Kingdom)
            {
                deJure = true;
            }

            return Settlement.CurrentSettlement.OwnerClan == Hero.MainHero.Clan ||
                   (deJure && Settlement.CurrentSettlement.IsVillage);
        }

        private static bool MenuVillageBuildingCondition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Manage;
            var deJure = false;
            var title = BannerKingsConfig.Instance.TitleManager.GetTitle(Settlement.CurrentSettlement);
            if (title != null && title.deJure == Hero.MainHero &&
                Settlement.CurrentSettlement.OwnerClan.Kingdom == Clan.PlayerClan.Kingdom)
            {
                deJure = true;
            }

            return (deJure || Settlement.CurrentSettlement.OwnerClan == Hero.MainHero.Clan) &&
                   Settlement.CurrentSettlement.IsVillage;
        }

        private static bool MenuGuildManageCondition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Manage;
            var currentSettlement = Settlement.CurrentSettlement;
            return currentSettlement.OwnerClan == Hero.MainHero.Clan &&
                   BannerKingsConfig.Instance.PopulationManager != null &&
                   BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(currentSettlement)
                   && BannerKingsConfig.Instance.PopulationManager.GetPopData(currentSettlement).EconomicData.Guild != null;
        }

        // -------- CONSEQUENCES ----------

        private void MenuActionLocalConnectionsConsequence(MenuCallbackArgs args)
        {
            var templates = GetLocalMercenaryTemplates(Hero.MainHero.CurrentSettlement.Town);
            var elements = new List<InquiryElement>();

            foreach (var tuple in templates)
            {
                var roster = GetMercenaryTemplateRoster(tuple.Item1);
                var characterCode = CampaignUIHelper.GetCharacterCode(tuple.Item1.Stacks[0].Character, false);
                var identifier = new ImageIdentifier(characterCode);
                var cost = GetRosterCost(roster, Hero.MainHero);
                var hint = new TextObject("{=ywMJqWA8}Recruit {COUNT} of the {NAME} mercenaries. This will cost {GOLD}{GOLD_ICON}")
                    .SetTextVariable("COUNT", roster.TotalManCount)
                    .SetTextVariable("NAME", tuple.Item2)
                    .SetTextVariable("GOLD", cost);


                elements.Add(new InquiryElement(roster, tuple.Item2.ToString(), identifier,
                    Hero.MainHero.Gold >= cost, hint.ToString()));
            }

            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                new TextObject("{=B0KoTpr4}Recruit local mercenaries").ToString(),
                new TextObject("{=Ow7k6t9V}Choose a mercenary lance to recruit. Available lances depend on the town's culture. Lances cost different prices and may differ in sizes. Lances become available again after a week after the last is recruited.").ToString(),
                elements,
                true,
                1,
                GameTexts.FindText("str_ok").ToString(),
                string.Empty,
                (List<InquiryElement> List) =>
                {
                    var roster = (TroopRoster)List[0].Identifier;
                    Hero.MainHero.ChangeHeroGold(-GetRosterCost(roster, Hero.MainHero));
                    MobileParty.MainParty.MemberRoster.Add(roster);
                },
                null,
                string.Empty
                ));

            GameMenu.SwitchToMenu("bannerkings");
        }

        private static void MenuActionMeetNobilityConsequence(MenuCallbackArgs args)
        {
            var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(Settlement.CurrentSettlement);
            args.MenuContext.GameMenu.EndWait();
            args.MenuContext.GameMenu.SetProgressOfWaitingInMenu(0f);
            GameMenu.SwitchToMenu("bannerkings");
        }

        private static void MenuActionHuntingConsequence(MenuCallbackArgs args)
        {
            var meat = (int) (actionHuntGame * MBRandom.RandomFloatRanged(1f, 3f));
            var fur = (int) (actionHuntGame * MBRandom.RandomFloatRanged(0.5f, 2f));
            actionHuntGame = 0;

            MobileParty.MainParty.ItemRoster.AddToCounts(Game.Current.ObjectManager.GetObject<ItemObject>("meat"), meat);
            MobileParty.MainParty.ItemRoster.AddToCounts(Game.Current.ObjectManager.GetObject<ItemObject>("fur"), fur);
            args.MenuContext.GameMenu.EndWait();
            args.MenuContext.GameMenu.SetProgressOfWaitingInMenu(0f);
            GameMenu.SwitchToMenu("bannerkings");
        }

        private static void MenuActionConsequenceWithGold(MenuCallbackArgs args)
        {
            GiveGoldAction.ApplyForSettlementToCharacter(Settlement.CurrentSettlement, Hero.MainHero, (int) actionGold);
            actionGold = 0f;
            args.MenuContext.GameMenu.EndWait();
            args.MenuContext.GameMenu.SetProgressOfWaitingInMenu(0f);
            GameMenu.SwitchToMenu("bannerkings");
        }

        private static void MenuActionConsequenceNeutral(MenuCallbackArgs args)
        {
            args.MenuContext.GameMenu.EndWait();
            args.MenuContext.GameMenu.SetProgressOfWaitingInMenu(0f);
            GameMenu.SwitchToMenu("bannerkings");
        }


        private static void MenuSettlementManageConsequence(MenuCallbackArgs args)
        {
            UIManager.Instance.ShowWindow("population");
        }

        private static void MenuFaithConsequence(MenuCallbackArgs args)
        {
            UIManager.Instance.ShowWindow("religions");
        }

        private static void MenuGuildManageConsequence(MenuCallbackArgs args)
        {
            UIManager.Instance.ShowWindow("guild");
        }

        private static void MenuVillageProjectsConsequence(MenuCallbackArgs args)
        {
            UIManager.Instance.ShowWindow("vilage_project");
        }

        private static void MenuTitlesConsequence(MenuCallbackArgs args)
        {
            UIManager.Instance.ShowWindow("titles");
        }

        // -------- MENUS ----------

        private void SwitchToMenuIfThereIsAnInterrupt(string currentMenuId)
        {
            var genericStateMenu = Campaign.Current.Models.EncounterGameMenuModel.GetGenericStateMenu();
            if (genericStateMenu != currentMenuId)
            {
                if (!string.IsNullOrEmpty(genericStateMenu))
                {
                    GameMenu.SwitchToMenu(genericStateMenu);
                    return;
                }

                GameMenu.ExitToLast();
            }
        }

        private static void MenuWaitInit(MenuCallbackArgs args)
        {
            PlayerEncounter.Current.IsPlayerWaiting = true;
            args.MenuContext.GameMenu.StartWait();
            actionStart = CampaignTime.Now;
        }

        public static void MenuBannerKingsInit(MenuCallbackArgs args)
        {
            args.MenuTitle = new TextObject("{=WaBMVVH9}Banner Kings");
        }
    }
}