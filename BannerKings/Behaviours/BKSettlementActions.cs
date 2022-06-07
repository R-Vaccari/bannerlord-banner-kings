using BannerKings.Managers.Institutions.Guilds;
using BannerKings.Populations;
using BannerKings.UI;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Overlay;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Behaviours
{
    public class BKSettlementActions : CampaignBehaviorBase
    {
        private static float actionGold = 0f;
        private static int actionHuntGame = 0;
        private static CampaignTime actionStart = CampaignTime.Now;
        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(OnSessionLaunched));
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        private void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
            campaignGameStarter.AddGameMenu("bannerkings", "Banner Kings", new OnInitDelegate(MenuBannerKingsInit));
            campaignGameStarter.AddGameMenu("bannerkings_actions", "Banner Kings", new OnInitDelegate(MenuBannerKingsInit));

            // ------- WAIT MENUS --------

            campaignGameStarter.AddWaitGameMenu("bannerkings_wait_guard", "{=!}You are serving as a guard in {CURRENT_SETTLEMENT}.",
                new OnInitDelegate(MenuWaitInit),
                new OnConditionDelegate(MenuGuardActionPeasantCondition),
                new OnConsequenceDelegate(MenuActionConsequenceWithGold),
                new OnTickDelegate(TickWaitGuard), GameMenu.MenuAndOptionType.WaitMenuShowProgressAndHoursOption, GameOverlays.MenuOverlayType.SettlementWithBoth, 8f, GameMenu.MenuFlags.None, null);

            campaignGameStarter.AddGameMenuOption("bannerkings_wait_guard", "wait_leave", "{=3sRdGQou}Leave",
                delegate (MenuCallbackArgs args) {
                    args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                    return true;
                },
                delegate (MenuCallbackArgs args)
                {
                    PlayerEncounter.Current.IsPlayerWaiting = false;
                    this.SwitchToMenuIfThereIsAnInterrupt(args.MenuContext.GameMenu.StringId);
                }, true, -1, false);

            campaignGameStarter.AddWaitGameMenu("bannerkings_wait_train_guards", "{=!}You are trainning the guards in {CURRENT_SETTLEMENT}.",
                new OnInitDelegate(MenuWaitInit),
                new OnConditionDelegate(MenuTrainGuardActionPeasantCondition),
                new OnConsequenceDelegate(MenuActionConsequenceWithGold),
                new OnTickDelegate(TickWaitTrainGuard), GameMenu.MenuAndOptionType.WaitMenuShowProgressAndHoursOption, GameOverlays.MenuOverlayType.SettlementWithBoth, 8f, GameMenu.MenuFlags.None, null);

            campaignGameStarter.AddGameMenuOption("bannerkings_wait_train_guards", "wait_leave", "{=3sRdGQou}Leave",
                delegate (MenuCallbackArgs args) {
                    args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                    return true;
                },
                delegate (MenuCallbackArgs args)
                {
                    PlayerEncounter.Current.IsPlayerWaiting = false;
                    this.SwitchToMenuIfThereIsAnInterrupt(args.MenuContext.GameMenu.StringId);
                }, true, -1, false);


            campaignGameStarter.AddWaitGameMenu("bannerkings_wait_hunt", "{=!}You are hunting in the region of {CURRENT_SETTLEMENT}. Game quantity in this region is {HUNTING_GAME}.",
                new OnInitDelegate(MenuWaitInit),
                new OnConditionDelegate(MenuHuntingActionCondition),
                new OnConsequenceDelegate(MenuActionHuntingConsequence),
                new OnTickDelegate(TickWaitHunt), GameMenu.MenuAndOptionType.WaitMenuShowProgressAndHoursOption, GameOverlays.MenuOverlayType.SettlementWithBoth, 8f, GameMenu.MenuFlags.None, null);

            campaignGameStarter.AddGameMenuOption("bannerkings_wait_hunt", "wait_leave", "{=3sRdGQou}Leave",
                delegate (MenuCallbackArgs args) {
                    args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                    return true;
                },
                delegate (MenuCallbackArgs args)
                {
                    PlayerEncounter.Current.IsPlayerWaiting = false;
                    this.SwitchToMenuIfThereIsAnInterrupt(args.MenuContext.GameMenu.StringId);
                }, true, -1, false);

            campaignGameStarter.AddWaitGameMenu("bannerkings_wait_meet_nobility", "{=!}You are meeting with the high society of {CURRENT_SETTLEMENT}.",
                new OnInitDelegate(MenuWaitInit),
                new OnConditionDelegate(MenuMeetNobilityActionCondition),
                new OnConsequenceDelegate(MenuActionMeetNobilityConsequence),
                new OnTickDelegate(TickWaitMeetNobility), GameMenu.MenuAndOptionType.WaitMenuShowProgressAndHoursOption, GameOverlays.MenuOverlayType.SettlementWithBoth, 4f, GameMenu.MenuFlags.None, null);

            campaignGameStarter.AddGameMenuOption("bannerkings_wait_meet_nobility", "wait_leave", "{=3sRdGQou}Leave",
                delegate (MenuCallbackArgs args) {
                    args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                    return true;
                },
                delegate (MenuCallbackArgs args)
                {
                    PlayerEncounter.Current.IsPlayerWaiting = false;
                    this.SwitchToMenuIfThereIsAnInterrupt(args.MenuContext.GameMenu.StringId);
                }, true, -1, false);

            // ------- ACTIONS --------

            campaignGameStarter.AddGameMenuOption("bannerkings_actions", "action_slave_transfer", "{=!}Transfer slaves",
                new GameMenuOption.OnConditionDelegate(MenuSlavesActionCondition), delegate (MenuCallbackArgs x)
                {
                    UIHelper.ShowSlaveTransferScreen();

                }, false, -1, false);

            campaignGameStarter.AddGameMenuOption("bannerkings_actions", "action_meet_nobility", "{=!}Meet nobility",
                new GameMenuOption.OnConditionDelegate(MenuMeetNobilityActionCondition), delegate (MenuCallbackArgs x)
                {
                    GameMenu.SwitchToMenu("bannerkings_wait_meet_nobility");
                }, false, -1, false);

            campaignGameStarter.AddGameMenuOption("bannerkings_actions", "action_guard", "{=!}Serve as guard",
                new GameMenuOption.OnConditionDelegate(MenuGuardActionPeasantCondition), delegate (MenuCallbackArgs x)
                {
                    GameMenu.SwitchToMenu("bannerkings_wait_guard");
                }, false, -1, false);

            campaignGameStarter.AddGameMenuOption("bannerkings_actions", "action_train_guards", "{=!}Train guards",
                new GameMenuOption.OnConditionDelegate(MenuTrainGuardActionPeasantCondition), delegate (MenuCallbackArgs x)
                {
                    GameMenu.SwitchToMenu("bannerkings_wait_train_guards");
                }, false, -1, false);

            campaignGameStarter.AddGameMenuOption("bannerkings_actions", "action_hunt", "{=!}Go hunting",
                new GameMenuOption.OnConditionDelegate(MenuHuntingActionCondition), delegate (MenuCallbackArgs x)
                {
                    GameMenu.SwitchToMenu("bannerkings_wait_hunt");
                }, false, -1, false);

            campaignGameStarter.AddGameMenuOption("bannerkings_actions", "bannerkings_leave", "{=3sRdGQou}Leave",
                delegate (MenuCallbackArgs x)
                {
                    x.optionLeaveType = GameMenuOption.LeaveType.Leave;
                    return true;
                }, delegate (MenuCallbackArgs x)
                {
                    GameMenu.SwitchToMenu("bannerkings");
                }, true, -1, false);


            // ------- TOWN --------

            campaignGameStarter.AddGameMenuOption("town", "bannerkings_submenu", "{=!}Banner Kings",
                delegate (MenuCallbackArgs x)
                {
                    x.optionLeaveType = GameMenuOption.LeaveType.Submenu;
                    return BannerKingsConfig.Instance.PopulationManager != null && BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(Settlement.CurrentSettlement);
                },
                delegate
                {
                    GameMenu.SwitchToMenu("bannerkings");
                }, false, 4, false);

            campaignGameStarter.AddGameMenuOption("bannerkings", "manage_demesne", "{=!}Demesne management",
                new GameMenuOption.OnConditionDelegate(MenuSettlementManageCondition),
                new GameMenuOption.OnConsequenceDelegate(MenuSettlementManageConsequence), false, -1, false);

            campaignGameStarter.AddGameMenuOption("bannerkings", "manage_titles", "{=!}Demesne hierarchy",
               new GameMenuOption.OnConditionDelegate(MenuTitlesCondition),
               new GameMenuOption.OnConsequenceDelegate(MenuTitlesConsequence), false, -1, false);

            campaignGameStarter.AddGameMenuOption("bannerkings", "manage_court", "{=!}Noble Court",
               new GameMenuOption.OnConditionDelegate(MenuCourtCondition),
               new GameMenuOption.OnConsequenceDelegate(MenuCourtConsequence), false, -1, false);

            campaignGameStarter.AddGameMenuOption("bannerkings", "manage_guild", "{=!}{GUILD_NAME}",
                new GameMenuOption.OnConditionDelegate(MenuGuildCondition),
                new GameMenuOption.OnConsequenceDelegate(MenuGuildManageConsequence), false, -1, false);


            campaignGameStarter.AddGameMenuOption("bannerkings", "bannerkings_action", "{=!}Take an action",
                delegate (MenuCallbackArgs args) {
                    args.optionLeaveType = GameMenuOption.LeaveType.Wait;
                    return true;
                },
                delegate (MenuCallbackArgs args) {
                    GameMenu.SwitchToMenu("bannerkings_actions");
                }, false, -1, false);

            campaignGameStarter.AddGameMenuOption("bannerkings", "bannerkings_leave", "{=3sRdGQou}Leave",
                delegate (MenuCallbackArgs x)
                {
                    x.optionLeaveType = GameMenuOption.LeaveType.Leave;
                    return true;
                }, delegate (MenuCallbackArgs x)
                {
                    string menu = Settlement.CurrentSettlement.IsVillage ? "village" : (Settlement.CurrentSettlement.IsCastle ? "castle" : "town");
                    GameMenu.SwitchToMenu(menu);
                }, true, -1, false);

            


            // ------- CASTLE --------


            campaignGameStarter.AddGameMenuOption("castle", "bannerkings_castle_submenu", "{=!}Banner Kings",
                delegate (MenuCallbackArgs x)
                {
                    x.optionLeaveType = GameMenuOption.LeaveType.Submenu;
                    return BannerKingsConfig.Instance.PopulationManager != null && BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(Settlement.CurrentSettlement);
                },
                delegate
                {
                    GameMenu.SwitchToMenu("bannerkings");
                }, false, 4, false);

            campaignGameStarter.AddGameMenuOption("bannerkings", "castle_recruit_volunteers", "{=E31IJyqs}Recruit troops",
               new GameMenuOption.OnConditionDelegate(MenuCastleRecruitsCondition),
               delegate (MenuCallbackArgs args) { args.MenuContext.OpenRecruitVolunteers(); },
               false, 3, false);


            // ------- VILLAGE --------


            campaignGameStarter.AddGameMenuOption("village", "bannerkings_village_submenu", "{=!}Banner Kings",
                delegate (MenuCallbackArgs x)
                {
                    x.optionLeaveType = GameMenuOption.LeaveType.Submenu;
                    return BannerKingsConfig.Instance.PopulationManager != null && BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(Settlement.CurrentSettlement);
                },
                delegate
                {
                    GameMenu.SwitchToMenu("bannerkings");
                }, false, 2, false);

            campaignGameStarter.AddGameMenuOption("bannerkings", "manage_projects", "{=!}Village Projects",
               new GameMenuOption.OnConditionDelegate(MenuVillageBuildingCondition),
               new GameMenuOption.OnConsequenceDelegate(MenuVillageProjectsConsequence), false, 2, false);
        }


        // -------- TICKS ----------

        private static void TickWaitGuard(MenuCallbackArgs args, CampaignTime dt)
        {
            TickCheckHealth();
            float progress = args.MenuContext.GameMenu.Progress;
            int diff = (int)actionStart.ElapsedHoursUntilNow;
            if (diff > 0)
            {
                args.MenuContext.GameMenu.SetProgressOfWaitingInMenu(diff * 0.125f);

                if (args.MenuContext.GameMenu.Progress != progress)
                {
                    Settlement settlement = Settlement.CurrentSettlement;
                    float wage = Campaign.Current.Models.PartyWageModel.GetCharacterWage(3);
                    wage *= settlement.Prosperity / 8000f;
                    actionGold += wage;

                    float random = MBRandom.RandomFloat;
                    float injury = 0.1f;

                    if (settlement.Town == null)
                    {
                        InformationManager.DisplayMessage(
                            new InformationMessage(
                                new TextObject("{=!}Not a town!").ToString()));
                        Hero.MainHero.HitPoints -= MBRandom.RandomInt(3, 10);
                        return;
                    }
                    injury -= settlement.Town.Security * 0.001f;
                    if (random <= injury)
                    {
                        InformationManager.DisplayMessage(
                            new InformationMessage(
                                new TextObject("{=!}You have been hurt in your current action.").ToString()));
                        Hero.MainHero.HitPoints -= MBRandom.RandomInt(3, 10);
                    }

                    float skill = 0.1f;
                    skill += settlement.Town.Security * 0.001f;
                    if (random <= skill)
                    {
                        List<(SkillObject, float)> skills = new List<(SkillObject, float)>();
                        skills.Add(new(DefaultSkills.OneHanded, 10f));
                        skills.Add(new(DefaultSkills.TwoHanded, 2f));
                        skills.Add(new(DefaultSkills.Polearm, 8f));
                        skills.Add(new(DefaultSkills.Bow, 4f));
                        skills.Add(new(DefaultSkills.Crossbow, 4f));
                        skills.Add(new(DefaultSkills.Athletics, 2f));

                        SkillObject target = MBRandom.ChooseWeighted(skills);
                        GameTexts.SetVariable("SKILL", target.Name);
                        Hero.MainHero.AddSkillXp(target, MBRandom.RandomFloatRanged(10f, 25f));
                        InformationManager.DisplayMessage(
                            new InformationMessage(
                                new TextObject("{=!}You have improved your {SKILL} skill during your current action.").ToString()));
                    }
                }
            }
        }

        private static void TickWaitTrainGuard(MenuCallbackArgs args, CampaignTime dt)
        {
            float progress = args.MenuContext.GameMenu.Progress;
            int diff = (int)actionStart.ElapsedHoursUntilNow;
            if (diff > 0)
            {
                args.MenuContext.GameMenu.SetProgressOfWaitingInMenu(diff * 0.125f);
                if (args.MenuContext.GameMenu.Progress != progress)
                {
                    Settlement settlement = Settlement.CurrentSettlement;
                    float wage = Campaign.Current.Models.PartyWageModel.GetCharacterWage(5);
                    wage *= settlement.Prosperity / 8000f;
                    actionGold += wage;

                    float random = MBRandom.RandomFloat;
                    float skill = 0.15f;
                    if (random <= skill)
                    {
                        List<(SkillObject, float)> skills = new List<(SkillObject, float)>();
                        skills.Add(new(DefaultSkills.Leadership, 10f));
                        skills.Add(new(DefaultSkills.OneHanded, 2f));
                        skills.Add(new(DefaultSkills.Polearm, 2f));
                        skills.Add(new(DefaultSkills.Bow, 2f));
                        skills.Add(new(DefaultSkills.Crossbow, 2f));

                        SkillObject target = MBRandom.ChooseWeighted(skills);
                        GameTexts.SetVariable("SKILL", target.Name);
                        Hero.MainHero.AddSkillXp(target, MBRandom.RandomFloatRanged(10f, 25f));
                        InformationManager.DisplayMessage(
                            new InformationMessage(
                                new TextObject("{=!}You have improved your {SKILL} skill during your current action.").ToString()));
                    }
                }
            }
        }

        private static void TickWaitHunt(MenuCallbackArgs args, CampaignTime dt)
        {
            float progress = args.MenuContext.GameMenu.Progress;
            int diff = (int)actionStart.ElapsedHoursUntilNow;
            if (diff > 0)
            {
                args.MenuContext.GameMenu.SetProgressOfWaitingInMenu(diff * 0.125f);

                Settlement settlement = Settlement.CurrentSettlement;
                LandData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement).LandData;
                float woodland = data.Woodland;
                string game = "";
                if (woodland >= 50000)
                    game = new TextObject("{=!}Bountiful").ToString();
                else if (woodland >= 25000)
                    game = new TextObject("{=!}Mediocre").ToString();
                else
                    game = new TextObject("{=!}Poor").ToString();

                GameTexts.SetVariable("HUNTING_GAME", game);
                if (args.MenuContext.GameMenu.Progress != progress)
                {
                    float chance = woodland * 0.001f;
                    float random = MBRandom.RandomFloatRanged(1f, 100f);
                    if (random <= chance)
                    {
                        actionHuntGame += 1;
                        List<(SkillObject, float)> skills = new List<(SkillObject, float)>();
                        skills.Add(new(DefaultSkills.Bow, 10f));
                        skills.Add(new(DefaultSkills.Crossbow, 5f));
                        skills.Add(new(DefaultSkills.Athletics, 8f));

                        SkillObject target = MBRandom.ChooseWeighted(skills);
                        GameTexts.SetVariable("SKILL", target.Name);
                        Hero.MainHero.AddSkillXp(target, MBRandom.RandomFloatRanged(10f, 25f));
                        InformationManager.DisplayMessage(
                            new InformationMessage(
                                new TextObject("{=!}You have have caught an animal and improved your {SKILL} skill while hunting.").ToString()));
                    }
                }
            }
        }

        private static void TickWaitMeetNobility(MenuCallbackArgs args, CampaignTime dt)
        {
            float progress = args.MenuContext.GameMenu.Progress;
            int diff = (int)actionStart.ElapsedHoursUntilNow;
            if (diff > 0)
            {
                args.MenuContext.GameMenu.SetProgressOfWaitingInMenu(diff * 0.250f);
                if (args.MenuContext.GameMenu.Progress != progress)
                {
                    float chance = ((float)Hero.MainHero.GetSkillValue(DefaultSkills.Charm) * 0.05f) + 15f;
                    float random = MBRandom.RandomFloatRanged(1f, 100f);
                    if (random <= chance)
                    {
                        float influence = MBRandom.RandomFloatRanged(0.1f, 0.5f);
                        GainKingdomInfluenceAction.ApplyForDefault(Hero.MainHero, influence);
                        GameTexts.SetVariable("INFLUENCE", influence);
                        GameTexts.SetVariable("SKILL", DefaultSkills.Charm.Name);
                        Hero.MainHero.AddSkillXp(DefaultSkills.Charm, MBRandom.RandomFloatRanged(10f, 25f));
                        InformationManager.DisplayMessage(
                            new InformationMessage(
                                new TextObject("{=!}You have have improved your {SKILL} skill and gained {INFLUENCE} influence while meeting with nobles.").ToString()));
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
                        new TextObject("{=!}You have stopped your current action due to health conditions.").ToString()));
                GameMenu.SwitchToMenu("bannerkings_actions");
            }
        }

        // -------- CONDITIONS ----------

        private static bool IsPeasant() => Clan.PlayerClan.Kingdom == null && (Clan.PlayerClan.Fiefs == null || Clan.PlayerClan.Fiefs.Count == 0);

        private static bool IsWounded() => ((float)Hero.MainHero.HitPoints / (float)Hero.MainHero.MaxHitPoints) <= 0.4f;

        private static bool IsCriminal(Clan ownerClan)
        {
            bool criminal = false;
            if (ownerClan != null)
            {
                Kingdom kingdom = ownerClan.Kingdom;
                if (kingdom != null)
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
            MBTextManager.SetTextVariable("CURRENT_SETTLEMENT", Settlement.CurrentSettlement.EncyclopediaLinkWithName, false);
            //Clan.PlayerClan.Kingdom == null && MobileParty.MainParty.MemberRoster.TotalManCount == 1
            return IsPeasant();
        }

        private static bool MenuGuardActionPeasantCondition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Recruit;
            MBTextManager.SetTextVariable("CURRENT_SETTLEMENT", Settlement.CurrentSettlement.EncyclopediaLinkWithName, false);
            bool criminal = false;

            //Clan.PlayerClan.Kingdom == null && MobileParty.MainParty.MemberRoster.TotalManCount == 1
            return IsPeasant() && !IsWounded() && !IsCriminal(Settlement.CurrentSettlement.OwnerClan) && !Settlement.CurrentSettlement.IsVillage;
        }

        private static bool MenuTrainGuardActionPeasantCondition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.OrderTroopsToAttack;
            MBTextManager.SetTextVariable("CURRENT_SETTLEMENT", Settlement.CurrentSettlement.EncyclopediaLinkWithName, false);
            int leadership = Hero.MainHero.GetSkillValue(DefaultSkills.Leadership);
            int combat = Hero.MainHero.GetSkillValue(DefaultSkills.OneHanded) + Hero.MainHero.GetSkillValue(DefaultSkills.Polearm) +
                Hero.MainHero.GetSkillValue(DefaultSkills.Bow) + Hero.MainHero.GetSkillValue(DefaultSkills.Crossbow);
            return IsPeasant() && !IsWounded() && !IsCriminal(Settlement.CurrentSettlement.OwnerClan) && leadership >= 50 && combat >= 160;
        }

        private static bool MenuMeetNobilityActionCondition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Conversation;
            MBTextManager.SetTextVariable("CURRENT_SETTLEMENT", Settlement.CurrentSettlement.EncyclopediaLinkWithName, false);
            bool inFaction = Clan.PlayerClan.Kingdom != null;
            return !IsWounded() && !IsCriminal(Settlement.CurrentSettlement.OwnerClan) && inFaction;
        }

        private static bool MenuHuntingActionCondition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Continue;
            MBTextManager.SetTextVariable("CURRENT_SETTLEMENT", Settlement.CurrentSettlement.EncyclopediaLinkWithName, false);
            bool criminal = false;
            bool huntingRight = false;
            Clan clan = Settlement.CurrentSettlement.OwnerClan;
            if (clan != null)
            {
                Kingdom kingdom = clan.Kingdom;
                if (kingdom != null)
                {
                    criminal = kingdom.MainHeroCrimeRating > 0;
                    huntingRight = kingdom.HasPolicy(DefaultPolicies.HuntingRights);
                }
            }
            return !IsWounded() && !criminal && (huntingRight || Settlement.CurrentSettlement.OwnerClan == Clan.PlayerClan);
        }

        private static bool MenuCastleRecruitsCondition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Recruit;
            Kingdom kingdom = Clan.PlayerClan.Kingdom;
            return Settlement.CurrentSettlement.IsCastle && Settlement.CurrentSettlement.Notables.Count > 0 &&
                (Settlement.CurrentSettlement.OwnerClan == Clan.PlayerClan || kingdom == Settlement.CurrentSettlement.OwnerClan.Kingdom);
        }

        private static bool MenuGuildCondition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Trade;
            Settlement settlement = Settlement.CurrentSettlement;
            bool hasGuild = false;
            if (BannerKingsConfig.Instance.PopulationManager != null && BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(settlement))
            {
                Guild guild = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement).EconomicData.Guild;
                hasGuild = guild != null;
                if (hasGuild) GameTexts.SetVariable("GUILD_NAME", guild.GuildType.Name.ToString());
            }
               
            return hasGuild;
        }

        private static bool MenuCourtCondition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.RansomAndBribe;
            Settlement currentSettlement = Settlement.CurrentSettlement;
            return currentSettlement.OwnerClan == Hero.MainHero.Clan && !currentSettlement.IsVillage;
        }

        private static bool MenuTitlesCondition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Surrender;
            Settlement currentSettlement = Settlement.CurrentSettlement;
            return currentSettlement.OwnerClan == Hero.MainHero.Clan && !currentSettlement.IsVillage;
        }

        private static bool MenuSettlementManageCondition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Manage;
            return Settlement.CurrentSettlement.OwnerClan == Hero.MainHero.Clan;
        }

        private static bool MenuVillageBuildingCondition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Manage;
            return Settlement.CurrentSettlement.OwnerClan == Hero.MainHero.Clan && Settlement.CurrentSettlement.IsVillage;
        }

        private static bool MenuGuildManageCondition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Manage;
            Settlement currentSettlement = Settlement.CurrentSettlement;
            return currentSettlement.OwnerClan == Hero.MainHero.Clan && BannerKingsConfig.Instance.PopulationManager != null && BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(currentSettlement)
                && BannerKingsConfig.Instance.PopulationManager.GetPopData(currentSettlement).EconomicData.Guild != null;
        }

        // -------- CONSEQUENCES ----------

        private static void MenuActionMeetNobilityConsequence(MenuCallbackArgs args)
        {
            PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(Settlement.CurrentSettlement);
            args.MenuContext.GameMenu.EndWait();
            args.MenuContext.GameMenu.SetProgressOfWaitingInMenu(0f);
            GameMenu.SwitchToMenu("bannerkings");
        }

        private static void MenuActionHuntingConsequence(MenuCallbackArgs args)
        {
            int meat = (int)((float)actionHuntGame * MBRandom.RandomFloatRanged(1f, 3f));
            int fur = (int)((float)actionHuntGame * MBRandom.RandomFloatRanged(0.5f, 2f));
            actionHuntGame = 0;

            MobileParty.MainParty.ItemRoster.AddToCounts(Game.Current.ObjectManager.GetObject<ItemObject>("meat"), meat);
            MobileParty.MainParty.ItemRoster.AddToCounts(Game.Current.ObjectManager.GetObject<ItemObject>("fur"), fur);
            args.MenuContext.GameMenu.EndWait();
            args.MenuContext.GameMenu.SetProgressOfWaitingInMenu(0f);
            GameMenu.SwitchToMenu("bannerkings");
        }

        private static void MenuActionConsequenceWithGold(MenuCallbackArgs args)
        {
            GiveGoldAction.ApplyForSettlementToCharacter(Settlement.CurrentSettlement, Hero.MainHero, (int)actionGold);
            actionGold = 0f;
            args.MenuContext.GameMenu.EndWait();
            args.MenuContext.GameMenu.SetProgressOfWaitingInMenu(0f);
            GameMenu.SwitchToMenu("bannerkings");
        }

        private static void MenuCourtConsequence(MenuCallbackArgs args) => UIManager.Instance.ShowWindow("court");

        private static void MenuSettlementManageConsequence(MenuCallbackArgs args) => UIManager.Instance.ShowWindow("population");

        private static void MenuGuildManageConsequence(MenuCallbackArgs args) => UIManager.Instance.ShowWindow("guild");

        private static void MenuVillageProjectsConsequence(MenuCallbackArgs args) => UIManager.Instance.ShowWindow("vilage_project");

        private static void MenuTitlesConsequence(MenuCallbackArgs args) => UIManager.Instance.ShowWindow("titles");

        // -------- MENUS ----------

        private void SwitchToMenuIfThereIsAnInterrupt(string currentMenuId)
        {
            string genericStateMenu = Campaign.Current.Models.EncounterGameMenuModel.GetGenericStateMenu();
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
            args.MenuTitle = new TextObject("{=!}Banner Kings");
        }
    }
}
