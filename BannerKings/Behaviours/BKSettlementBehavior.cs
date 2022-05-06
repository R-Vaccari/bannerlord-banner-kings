using BannerKings.Components;
using BannerKings.Managers;
using BannerKings.Managers.Decisions;
using BannerKings.Managers.Policies;
using BannerKings.Managers.Populations.Villages;
using BannerKings.Populations;
using BannerKings.UI;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Overlay;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using static BannerKings.Managers.PopulationManager;
using static BannerKings.Managers.Policies.BKTaxPolicy;
using static BannerKings.Managers.Policies.BKWorkforcePolicy;
using BannerKings.Managers.Court;
using BannerKings.Managers.Institutions.Religions;

namespace BannerKings.Behaviors
{
    public class BKSettlementBehavior : CampaignBehaviorBase
    {
        private PopulationManager populationManager;
        private PolicyManager policyManager;
        private TitleManager titleManager;
        private CourtManager courtManager;
        private static float actionGold;
        private static int actionHuntGame;
        private static CampaignTime actionStart = CampaignTime.Now;

        public override void RegisterEvents()
        {
            CampaignEvents.SettlementEntered.AddNonSerializedListener(this, OnSettlementEntered);
            CampaignEvents.DailyTickSettlementEvent.AddNonSerializedListener(this, DailySettlementTick);
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
        }

        public override void SyncData(IDataStore dataStore)
        {
            if (dataStore.IsSaving)
            {
                populationManager = BannerKingsConfig.Instance.PopulationManager;
                policyManager = BannerKingsConfig.Instance.PolicyManager;
                titleManager = BannerKingsConfig.Instance.TitleManager;
                courtManager = BannerKingsConfig.Instance.CourtManager;   
            }

            if (BannerKingsConfig.Instance.wipeData)
            {
                populationManager = null;
                policyManager = null;
                titleManager = null;
                courtManager = null;
            }

            dataStore.SyncData("bannerkings-populations", ref populationManager);
            dataStore.SyncData("bannerkings-titles", ref titleManager);
            dataStore.SyncData("bannerkings-courts", ref courtManager);
            dataStore.SyncData("bannerkings-policies", ref policyManager);

            if (dataStore.IsLoading)
            {
                if (populationManager == null && policyManager == null && titleManager == null && courtManager == null)
                    BannerKingsConfig.Instance.InitManagers();
                
                else BannerKingsConfig.Instance.InitManagers(populationManager, policyManager,
                    titleManager, courtManager);

                BannerKingsConfig.Instance.TitleManager.FixTitles();
            }
        }

        private void OnSettlementEntered(MobileParty party, Settlement target, Hero hero)
        {
            if (party != null && BannerKingsConfig.Instance.PopulationManager != null)
            {
                if (party.LeaderHero != null && party.LeaderHero == target.Owner && party.LeaderHero != Hero.MainHero
                    && BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(target)) 
                {
                    int random = MBRandom.RandomInt(1, 100);
                    if (random > 5) return;

                    Kingdom kingdom = target.OwnerClan.Kingdom;
                    List<BannerKingsDecision> currentDecisions = BannerKingsConfig.Instance.PolicyManager.GetDefaultDecisions(target);
                    List<BannerKingsDecision> changedDecisions = new List<BannerKingsDecision>();
                    if (target.Town != null)
                    {
                        Town town = target.Town;
                        if (town.FoodStocks < town.FoodStocksUpperLimit() * 0.2f && town.FoodChange < 0f)
                        {
                            BKRationDecision rationDecision = (BKRationDecision)currentDecisions.FirstOrDefault(x => x.GetIdentifier() == "decision_ration");
                            rationDecision.Enabled = true;
                            changedDecisions.Add(rationDecision);
                        } else
                        {
                            BKRationDecision rationDecision = (BKRationDecision)currentDecisions.FirstOrDefault(x => x.GetIdentifier() == "decision_ration");
                            rationDecision.Enabled = false;
                            changedDecisions.Add(rationDecision);
                        }

                        MobileParty garrison = town.GarrisonParty;
                        if (garrison != null)
                        {
                            float wage = garrison.TotalWage;
                            float income = Campaign.Current.Models.SettlementTaxModel.CalculateTownTax(town).ResultNumber;
                            if (wage >= income * 0.5f)
                                BannerKingsConfig.Instance.PolicyManager.UpdateSettlementPolicy(target, new BKGarrisonPolicy(BKGarrisonPolicy.GarrisonPolicy.Dischargement, target));
                            else if (wage <= income * 0.2f)
                                BannerKingsConfig.Instance.PolicyManager.UpdateSettlementPolicy(target, new BKGarrisonPolicy(BKGarrisonPolicy.GarrisonPolicy.Enlistment, target));
                            else BannerKingsConfig.Instance.PolicyManager.UpdateSettlementPolicy(target, new BKGarrisonPolicy(BKGarrisonPolicy.GarrisonPolicy.Standard, target));
                        } 

                        if (town.LoyaltyChange < 0)
                            UpdateTaxPolicy(1, target);
                        else UpdateTaxPolicy(-1, target);

                        if (kingdom != null)
                        {
                            IEnumerable<Kingdom> enemies = FactionManager.GetEnemyKingdoms(kingdom);
                            bool atWar = enemies.Count() > 0;

                            if (target.Owner.GetTraitLevel(DefaultTraits.Calculating) > 0)
                            {
                                BKSubsidizeMilitiaDecision subsidizeMilitiaDecision = (BKSubsidizeMilitiaDecision)currentDecisions.FirstOrDefault(x => x.GetIdentifier() == "decision_militia_subsidize");
                                subsidizeMilitiaDecision.Enabled = atWar ? true : false;
                                changedDecisions.Add(subsidizeMilitiaDecision);
                            }
                        }

                        BKCriminalPolicy criminal = (BKCriminalPolicy)BannerKingsConfig.Instance.PolicyManager.GetPolicy(target, "criminal");
                        int mercy = target.Owner.GetTraitLevel(DefaultTraits.Mercy);
                        BKCriminalPolicy targetCriminal = null;

                        if (mercy > 0) targetCriminal = new BKCriminalPolicy(BKCriminalPolicy.CriminalPolicy.Forgiveness, target);
                        else if (mercy < 0) targetCriminal = new BKCriminalPolicy(BKCriminalPolicy.CriminalPolicy.Execution, target);
                        else targetCriminal = new BKCriminalPolicy(BKCriminalPolicy.CriminalPolicy.Enslavement, target);

                        if (targetCriminal.Policy != criminal.Policy) 
                            BannerKingsConfig.Instance.PolicyManager.UpdateSettlementPolicy(target, targetCriminal);

                        BKTaxSlavesDecision taxSlavesDecision = (BKTaxSlavesDecision)currentDecisions.FirstOrDefault(x => x.GetIdentifier() == "decision_slaves_tax");
                        if (target.Owner.GetTraitLevel(DefaultTraits.Authoritarian) > 0)
                            taxSlavesDecision.Enabled = true;
                        else if (target.Owner.GetTraitLevel(DefaultTraits.Egalitarian) > 0)
                            taxSlavesDecision.Enabled = false;
                        changedDecisions.Add(taxSlavesDecision);

                        BKWorkforcePolicy workforce = (BKWorkforcePolicy)BannerKingsConfig.Instance.PolicyManager.GetPolicy(target, "workforce");
                        List<ValueTuple<WorkforcePolicy, float>> workforcePolicies = new List<ValueTuple<WorkforcePolicy, float>>();
                        workforcePolicies.Add((WorkforcePolicy.None, 1f));
                        float saturation = BannerKingsConfig.Instance.PopulationManager.GetPopData(target).LandData.WorkforceSaturation;
                        if (saturation > 1f)
                            workforcePolicies.Add((WorkforcePolicy.Land_Expansion, 2f));
                        if (town.Security < 20f)
                            workforcePolicies.Add((WorkforcePolicy.Martial_Law, 2f));
                        BannerKingsConfig.Instance.PolicyManager.UpdateSettlementPolicy(target, new BKWorkforcePolicy(MBRandom.ChooseWeighted(workforcePolicies), target));

                        foreach (BannerKingsDecision dec in changedDecisions)
                            BannerKingsConfig.Instance.PolicyManager.UpdateSettlementDecision(target, dec);
                    }
                    else if (target.IsVillage)
                    {
                        VillageData villageData = BannerKingsConfig.Instance.PopulationManager.GetPopData(target).VillageData;
                        villageData.StartRandomProject();
                        float hearths = target.Village.Hearth;
                        if (hearths < 300f)
                            UpdateTaxPolicy(-1, target);
                        else if (hearths > 1000f)
                            UpdateTaxPolicy(1, target);
                    }
                }
            }
        }

        private void UpdateTaxPolicy(int value, Settlement settlement)
        {
            BKTaxPolicy tax = ((BKTaxPolicy)BannerKingsConfig.Instance.PolicyManager.GetPolicy(settlement, "tax"));
            TaxType taxType = tax.Policy;
            if ((value == 1 && taxType != TaxType.High) || value == -1 && taxType != TaxType.Low)
            {
                BannerKingsConfig.Instance.PolicyManager.UpdateSettlementPolicy(settlement, new BKTaxPolicy(taxType + value, settlement));
            }
        }

        private void DailySettlementTick(Settlement settlement)
        {
            if (settlement == null || settlement.StringId.Contains("tutorial") || settlement.StringId.Contains("Ruin")) return;
            
            if (BannerKingsConfig.Instance.PopulationManager == null)
                BannerKingsConfig.Instance.InitManagers();

            UpdateSettlementPops(settlement);
            BannerKingsConfig.Instance.PolicyManager.InitializeSettlement(settlement);

            if (settlement.IsCastle)
            {
                //SetNotables(settlement);
                //UpdateVolunteers(settlement);
                if (settlement.Town != null && settlement.Town.GarrisonParty != null)
                {
                    foreach (Building castleBuilding in settlement.Town.Buildings)
                        if (Utils.Helpers._buildingCastleRetinue != null && castleBuilding.BuildingType == Utils.Helpers._buildingCastleRetinue)
                        {
                            MobileParty garrison = settlement.Town.GarrisonParty;
                            if (garrison.MemberRoster != null && garrison.MemberRoster.Count > 0)
                            {
                                List<TroopRosterElement> elements = garrison.MemberRoster.GetTroopRoster();
                                int currentRetinue = 0;
                                foreach (TroopRosterElement soldierElement in elements)
                                    if (Utils.Helpers.IsRetinueTroop(soldierElement.Character, settlement.Culture))
                                        currentRetinue += soldierElement.Number;

                                int maxRetinue = castleBuilding.CurrentLevel == 1 ? 20 : (castleBuilding.CurrentLevel == 2 ? 40 : 60);
                                if (currentRetinue < maxRetinue)
                                    if (garrison.MemberRoster.Count < garrison.Party.PartySizeLimit)
                                        garrison.MemberRoster.AddToCounts(settlement.Culture.EliteBasicTroop, 1);
                            }
                        }


                    if (settlement.Town.FoodStocks <= settlement.Town.FoodStocksUpperLimit() * 0.05f && 
                        settlement.Town.Settlement.Stash != null)
                        ConsumeStash(settlement);
                }
            } else if (settlement.IsVillage)
            {
                VillageData villageData = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement).VillageData;
                if (villageData != null)
                {
                    float manor = villageData.GetBuildingLevel(DefaultVillageBuildings.Instance.Manor);
                    if (manor > 0)
                    {
                        List<MobileParty> retinues = BannerKingsConfig.Instance.PopulationManager.GetParties(Type.GetType("RetinueComponent"));
                        MobileParty retinue = null;
                        if (retinues.Count > 0) retinue = retinues.Find(x => x.HomeSettlement == settlement);
                        if (retinue == null)
                            retinue = RetinueComponent.CreateRetinue(settlement);
                        
                        (retinue.PartyComponent as RetinueComponent).DailyTick(manor);
                    }
                }
            }    
        }

        internal static void ConsumeStash(Settlement settlement)
        {
            List<ItemRosterElement> elements = new List<ItemRosterElement>();
            foreach (ItemRosterElement element in settlement.Stash)
                if (element.EquipmentElement.Item != null && element.EquipmentElement.Item.ItemCategory.Properties == ItemCategory.Property.BonusToFoodStores)
                    elements.Add(element);

            int food = 0;
            foreach (ItemRosterElement element in elements)
            {
                food += element.Amount;
                settlement.Stash.Remove(element);
            }

            if (food > 0) settlement.Town.FoodStocks += food;
        }

        internal static void KillNotables(Settlement settlement, int amount)
        {
            Hero notable = null;
            int i = 0;
            try
            {
                List<Hero> notables = new List<Hero>(settlement.Notables);
                foreach (Hero hero in notables)
                    if (hero.BornSettlement != settlement)
                    {
                        notable = hero;
                        LeaveSettlementAction.ApplyForCharacterOnly(hero);
                        DisableHeroAction.Apply(hero);
                        i++;
                    }
            }
            catch (Exception ex)
            {
                string cause = "Exception in Banner Kings KillNotables method. ";
                string objInfo = null;
                if (notable != null)
                    objInfo = string.Format("Notable: Name [{0}], Id [{1}], Culture [{2}]\nSettleent: Name [{3}], Id [{4}], Culture [{5}]", 
                        notable.Name, notable.StringId, notable.Culture, settlement.Name, settlement.StringId, settlement.Culture);
                else objInfo = "Null notable.";

                throw new BannerKingsException(cause + objInfo, ex);
            }
        }


        private void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
            AddMenus(campaignGameStarter);
            if (BannerKingsConfig.Instance.PopulationManager != null)
            {
                foreach (Settlement settlement in Settlement.All)
                    if (BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(settlement))
                    {
                        PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
                        settlement.Culture = data.CultureData.DominantCulture;
                    }
            }
            
            if (BannerKingsConfig.Instance.PolicyManager == null || BannerKingsConfig.Instance.TitleManager == null)
                BannerKingsConfig.Instance.InitManagers();

            BannerKingsConfig.Instance.ReligionsManager.InitializePresets();
                
            BuildingType retinueType = MBObjectManager.Instance.GetObjectTypeList<BuildingType>().FirstOrDefault(x => x == Utils.Helpers._buildingCastleRetinue);
            if (retinueType == null)
            {
                Utils.Helpers._buildingCastleRetinue.Initialize(new TextObject("{=!}Retinue Barracks"), new TextObject("{=!}Barracks for the castle retinue, a group of elite soldiers. The retinue is added to the garrison over time, up to a limit of 20, 40 or 60 (building level)."), new[]
                {
                     1000,
                     1500,
                     2000
                }, BuildingLocation.Castle, new Tuple<BuildingEffectEnum, float, float, float>[]
                {
                });
            }
        }

        private void AddMenus(CampaignGameStarter campaignGameStarter)
        {

            campaignGameStarter.AddGameMenu("bannerkings", "Banner Kings", MenuBannerKingsInit);
            campaignGameStarter.AddGameMenu("bannerkings_actions", "Banner Kings", MenuBannerKingsInit);

            // ------- WAIT MENUS --------

            campaignGameStarter.AddWaitGameMenu("bannerkings_wait_guard", "{=!}You are serving as a guard in {CURRENT_SETTLEMENT}.", 
                MenuWaitInit, 
                MenuGuardActionPeasantCondition, 
                MenuActionConsequenceWithGold, 
                TickWaitGuard, GameMenu.MenuAndOptionType.WaitMenuShowProgressAndHoursOption, GameOverlays.MenuOverlayType.SettlementWithBoth, 8f);

            campaignGameStarter.AddGameMenuOption("bannerkings_wait_guard", "wait_leave", "{=3sRdGQou}Leave", 
                delegate (MenuCallbackArgs args) {
                    args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                    return true; 
                }, 
                delegate (MenuCallbackArgs args)
                {
                    PlayerEncounter.Current.IsPlayerWaiting = false;
                    SwitchToMenuIfThereIsAnInterrupt(args.MenuContext.GameMenu.StringId);
                }, true);

            campaignGameStarter.AddWaitGameMenu("bannerkings_wait_train_guards", "{=!}You are trainning the guards in {CURRENT_SETTLEMENT}.",
                MenuWaitInit,
                MenuTrainGuardActionPeasantCondition,
                MenuActionConsequenceWithGold,
                TickWaitTrainGuard, GameMenu.MenuAndOptionType.WaitMenuShowProgressAndHoursOption, GameOverlays.MenuOverlayType.SettlementWithBoth, 8f);

            campaignGameStarter.AddGameMenuOption("bannerkings_wait_train_guards", "wait_leave", "{=3sRdGQou}Leave",
                delegate (MenuCallbackArgs args) {
                    args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                    return true;
                },
                delegate (MenuCallbackArgs args)
                {
                    PlayerEncounter.Current.IsPlayerWaiting = false;
                    SwitchToMenuIfThereIsAnInterrupt(args.MenuContext.GameMenu.StringId);
                }, true);


            campaignGameStarter.AddWaitGameMenu("bannerkings_wait_hunt", "{=!}You are hunting in the region of {CURRENT_SETTLEMENT}. Game quantity in this region is {HUNTING_GAME}.",
                MenuWaitInit,
                MenuHuntingActionCondition, 
                MenuActionHuntingConsequence,
                TickWaitHunt, GameMenu.MenuAndOptionType.WaitMenuShowProgressAndHoursOption, GameOverlays.MenuOverlayType.SettlementWithBoth, 8f);

            campaignGameStarter.AddGameMenuOption("bannerkings_wait_hunt", "wait_leave", "{=3sRdGQou}Leave",
                delegate (MenuCallbackArgs args) {
                    args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                    return true;
                },
                delegate (MenuCallbackArgs args)
                {
                    PlayerEncounter.Current.IsPlayerWaiting = false;
                    SwitchToMenuIfThereIsAnInterrupt(args.MenuContext.GameMenu.StringId);
                }, true);

            campaignGameStarter.AddWaitGameMenu("bannerkings_wait_meet_nobility", "{=!}You are meeting with the high society of {CURRENT_SETTLEMENT}.",
                MenuWaitInit,
                MenuMeetNobilityActionCondition,
                MenuActionMeetNobilityConsequence,
                TickWaitMeetNobility, GameMenu.MenuAndOptionType.WaitMenuShowProgressAndHoursOption, GameOverlays.MenuOverlayType.SettlementWithBoth, 4f);

            campaignGameStarter.AddGameMenuOption("bannerkings_wait_meet_nobility", "wait_leave", "{=3sRdGQou}Leave",
                delegate (MenuCallbackArgs args) {
                    args.optionLeaveType = GameMenuOption.LeaveType.Leave;
                    return true;
                },
                delegate (MenuCallbackArgs args)
                {
                    PlayerEncounter.Current.IsPlayerWaiting = false;
                    SwitchToMenuIfThereIsAnInterrupt(args.MenuContext.GameMenu.StringId);
                }, true);

            // ------- ACTIONS --------

            campaignGameStarter.AddGameMenuOption("bannerkings_actions", "action_slave_transfer", "{=!}Transfer slaves",
                MenuSlavesActionCondition, delegate
                {
                    UIHelper.ShowSlaveTransferScreen();

                });

            campaignGameStarter.AddGameMenuOption("bannerkings_actions", "action_meet_nobility", "{=!}Meet nobility",
                MenuMeetNobilityActionCondition, delegate
                {
                    GameMenu.SwitchToMenu("bannerkings_wait_meet_nobility");
                });

            campaignGameStarter.AddGameMenuOption("bannerkings_actions", "action_guard", "{=!}Serve as guard", 
                MenuGuardActionPeasantCondition, delegate
                {
                    GameMenu.SwitchToMenu("bannerkings_wait_guard");
                });

            campaignGameStarter.AddGameMenuOption("bannerkings_actions", "action_train_guards", "{=!}Train guards",
                MenuTrainGuardActionPeasantCondition, delegate
                {
                    GameMenu.SwitchToMenu("bannerkings_wait_train_guards");
                });

            campaignGameStarter.AddGameMenuOption("bannerkings_actions", "action_hunt", "{=!}Go hunting",
                MenuHuntingActionCondition, delegate
                {
                    GameMenu.SwitchToMenu("bannerkings_wait_hunt");
                });

            campaignGameStarter.AddGameMenuOption("bannerkings_actions", "bannerkings_leave", "{=3sRdGQou}Leave",
                delegate (MenuCallbackArgs x)
                {
                    x.optionLeaveType = GameMenuOption.LeaveType.Leave;
                    return true;
                }, delegate
                {
                    GameMenu.SwitchToMenu("bannerkings");
                }, true);


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
                }, false, 4);

            campaignGameStarter.AddGameMenuOption("bannerkings", "manage_demesne", "{=!}Demesne management",
                MenuSettlementManageCondition,
                MenuSettlementManageConsequence);

            campaignGameStarter.AddGameMenuOption("bannerkings", "manage_titles", "{=!}Demesne hierarchy",
               MenuTitlesCondition,
               MenuTitlesConsequence);

            campaignGameStarter.AddGameMenuOption("bannerkings", "manage_court", "{=!}Noble Court",
               MenuCourtCondition,
               MenuCourtConsequence);

            campaignGameStarter.AddGameMenuOption("bannerkings", "manage_court_royal", "{=!}Royal court",
               new GameMenuOption.OnConditionDelegate(MenuCourtRoyalCondition),
               new GameMenuOption.OnConsequenceDelegate(MenuCourtRoyalConsequence), false, -1, false);

            campaignGameStarter.AddGameMenuOption("bannerkings", "manage_court", "{=!}{RELIGION_NAME}",
              new GameMenuOption.OnConditionDelegate(MenuReligionCondition),
              new GameMenuOption.OnConsequenceDelegate(MenuReligionConsequence), false, -1, false);


            campaignGameStarter.AddGameMenuOption("bannerkings", "bannerkings_action", "{=!}Take an action",
                delegate (MenuCallbackArgs args) {
                    args.optionLeaveType = GameMenuOption.LeaveType.Wait;
                    return true;
                },
                delegate
                {
                    GameMenu.SwitchToMenu("bannerkings_actions");
                });

            campaignGameStarter.AddGameMenuOption("bannerkings", "bannerkings_leave", "{=3sRdGQou}Leave",
                delegate (MenuCallbackArgs x)
                {
                    x.optionLeaveType = GameMenuOption.LeaveType.Leave;
                    return true;
                }, delegate
                {
                    string menu = Settlement.CurrentSettlement.IsVillage ? "village" : (Settlement.CurrentSettlement.IsCastle ? "castle" : "town");
                    GameMenu.SwitchToMenu(menu);
                }, true);

            //campaignGameStarter.AddGameMenuOption("bannerkings_keep", "manage_guild", "{=!}Guild management",
            //    new GameMenuOption.OnConditionDelegate(game_menu_town_manage_guild_on_condition),
            //    new GameMenuOption.OnConsequenceDelegate(game_menu_town_manage_guild_on_consequence), false, -1, false);


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
                }, false, 4);

            campaignGameStarter.AddGameMenuOption("bannerkings", "castle_recruit_volunteers", "{=E31IJyqs}Recruit troops",
               MenuCastleRecruitsCondition,
               delegate (MenuCallbackArgs args) { args.MenuContext.OpenRecruitVolunteers(); },
               false, 3);


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
                }, false, 2);

            campaignGameStarter.AddGameMenuOption("bannerkings", "manage_projects", "{=!}Village Projects",
               MenuVillageBuildingCondition,
               MenuVillageProjectsConsequence, false, 2);
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
                    float chance = (Hero.MainHero.GetSkillValue(DefaultSkills.Charm) * 0.05f) + 15f;
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

        private static bool IsWounded() => (Hero.MainHero.HitPoints / (float)Hero.MainHero.MaxHitPoints) <= 0.4f;

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
            MBTextManager.SetTextVariable("CURRENT_SETTLEMENT", Settlement.CurrentSettlement.EncyclopediaLinkWithName);
            //Clan.PlayerClan.Kingdom == null && MobileParty.MainParty.MemberRoster.TotalManCount == 1
            return IsPeasant();
        }

        private static bool MenuGuardActionPeasantCondition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Recruit;
            MBTextManager.SetTextVariable("CURRENT_SETTLEMENT", Settlement.CurrentSettlement.EncyclopediaLinkWithName);
            bool criminal = false;
            
            //Clan.PlayerClan.Kingdom == null && MobileParty.MainParty.MemberRoster.TotalManCount == 1
            return IsPeasant() && !IsWounded() && !IsCriminal(Settlement.CurrentSettlement.OwnerClan) && !Settlement.CurrentSettlement.IsVillage;
        }

        private static bool MenuTrainGuardActionPeasantCondition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.OrderTroopsToAttack;
            MBTextManager.SetTextVariable("CURRENT_SETTLEMENT", Settlement.CurrentSettlement.EncyclopediaLinkWithName);
            int leadership = Hero.MainHero.GetSkillValue(DefaultSkills.Leadership);
            int combat = Hero.MainHero.GetSkillValue(DefaultSkills.OneHanded) + Hero.MainHero.GetSkillValue(DefaultSkills.Polearm) +
                Hero.MainHero.GetSkillValue(DefaultSkills.Bow) + Hero.MainHero.GetSkillValue(DefaultSkills.Crossbow);
            return IsPeasant() && !IsWounded() && !IsCriminal(Settlement.CurrentSettlement.OwnerClan) && leadership >= 50 && combat >= 160;
        }

        private static bool MenuMeetNobilityActionCondition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Conversation;
            MBTextManager.SetTextVariable("CURRENT_SETTLEMENT", Settlement.CurrentSettlement.EncyclopediaLinkWithName);
            bool inFaction = Clan.PlayerClan.Kingdom != null;
            return !IsWounded() && !IsCriminal(Settlement.CurrentSettlement.OwnerClan) && inFaction;
        }

        private static bool MenuHuntingActionCondition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.Continue;
            MBTextManager.SetTextVariable("CURRENT_SETTLEMENT", Settlement.CurrentSettlement.EncyclopediaLinkWithName);
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

        private static bool MenuCourtCondition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.RansomAndBribe;
            Settlement currentSettlement = Settlement.CurrentSettlement;
            return currentSettlement.OwnerClan == Hero.MainHero.Clan && !currentSettlement.IsVillage;
        }

        private static bool MenuCourtRoyalCondition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.RansomAndBribe;
            Settlement currentSettlement = Settlement.CurrentSettlement;
            Kingdom kingdom = Clan.PlayerClan.Kingdom;
            if (kingdom == null) return false;
            CouncilData council = BannerKingsConfig.Instance.CourtManager.GetCouncil(kingdom.Leader);
            return currentSettlement.OwnerClan.Kingdom == kingdom && !currentSettlement.IsVillage &&
                kingdom.Leader != Hero.MainHero && council.IsRoyal;
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

        private static bool MenuReligionCondition(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.ShowMercy;
            Settlement currentSettlement = Settlement.CurrentSettlement;
            ReligionData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(currentSettlement).ReligionData;
            if (data != null) MBTextManager.SetTextVariable("RELIGION_NAME", data.Religion.Faith.GetFaithName());

            return data != null;
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
            int meat = (int)(actionHuntGame * MBRandom.RandomFloatRanged(1f, 3f));
            int fur = (int)(actionHuntGame * MBRandom.RandomFloatRanged(0.5f, 2f));
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

        private static void MenuCourtRoyalConsequence(MenuCallbackArgs args) => UIManager.Instance.ShowWindow("court_royal");

        private static void MenuReligionConsequence(MenuCallbackArgs args) => UIManager.Instance.ShowWindow("religions");

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

    namespace Patches
    {


        [HarmonyPatch(typeof(UrbanCharactersCampaignBehavior), "SpawnNotablesIfNeeded")]
        class SpawnNotablesIfNeededPatch
        {
            static bool Prefix(Settlement settlement)
            {
                if (settlement.IsCastle && settlement.Name.ToString().Contains("Ab Comer"))
                    Console.WriteLine();
                List<Occupation> list = new List<Occupation>();
                if (settlement.IsTown)
                {
                    list = new List<Occupation>
                    {
                        Occupation.GangLeader,
                        Occupation.Artisan,
                        Occupation.Merchant
                    };
                }
                else if (settlement.IsVillage)
                {
                    list = new List<Occupation>
                    {
                        Occupation.RuralNotable,
                        Occupation.Headman
                    };
                } else if (settlement.IsCastle)
                {
                    list = new List<Occupation>
                    {
                        Occupation.Headman
                    };
                }
                float randomFloat = MBRandom.RandomFloat;
                int num = 0;
                foreach (Occupation occupation in list)
                    num += Campaign.Current.Models.NotableSpawnModel.GetTargetNotableCountForSettlement(settlement, occupation);
                
                int count = settlement.Notables.Count;
                float num2 = settlement.Notables.Any<Hero>() ? ((num - settlement.Notables.Count) / (float)num) : 1f;
                num2 *= MathF.Pow(num2, 0.36f);
                if (randomFloat <= num2 && count < num)
                {
                    List<Occupation> list2 = new List<Occupation>();
                    foreach (Occupation occupation2 in list)
                    {
                        int num3 = 0;
                        using (List<Hero>.Enumerator enumerator2 = settlement.Notables.GetEnumerator())
                        {
                            while (enumerator2.MoveNext())
                                if (enumerator2.Current.CharacterObject.Occupation == occupation2)
                                    num3++;
                        }
                        int targetNotableCountForSettlement = Campaign.Current.Models.NotableSpawnModel.GetTargetNotableCountForSettlement(settlement, occupation2);
                        if (num3 < targetNotableCountForSettlement)
                            list2.Add(occupation2);
                        
                    }
                    if (list2.Count > 0)
                        EnterSettlementAction.ApplyForCharacterOnly(HeroCreator.CreateHeroAtOccupation(list2.GetRandomElement(), settlement), settlement);
                    
                }

                return false;
            }
        }

        [HarmonyPatch(typeof(CaravansCampaignBehavior), "SpawnCaravan")]
        class SpawnCaravanPatch
        {
            static bool Prefix(Hero hero, bool initialSpawn = false)
            {
                if (hero.CurrentSettlement != null && hero.CurrentSettlement.IsTown)
                    return true;
                
                return false;
            }
        }

        [HarmonyPatch(typeof(SellPrisonersAction), "ApplyForAllPrisoners")]
        class ApplyAllPrisionersPatch
        {
            static bool Prefix(MobileParty sellerParty, TroopRoster prisoners, Settlement currentSettlement, bool applyGoldChange = true)
            {
                if (currentSettlement != null && sellerParty != null && prisoners != null && currentSettlement != null && BannerKingsConfig.Instance != null && (currentSettlement.IsCastle || currentSettlement.IsTown) &&
                    BannerKingsConfig.Instance.PopulationManager != null &&
                    BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(currentSettlement))
                {
                    BKCriminalPolicy policy = (BKCriminalPolicy)BannerKingsConfig.Instance.PolicyManager.GetPolicy(currentSettlement, "criminal");
                    if (policy != null)
                    {
                        if (policy.Policy == BKCriminalPolicy.CriminalPolicy.Enslavement)
                            BannerKingsConfig.Instance.PopulationManager.GetPopData(currentSettlement)
                                .UpdatePopType(PopType.Slaves, Utils.Helpers.GetRosterCount(prisoners));
                        else if (policy.Policy == BKCriminalPolicy.CriminalPolicy.Forgiveness)
                        {
                            Dictionary<CultureObject, int> dic = new Dictionary<CultureObject, int>();
                            foreach (TroopRosterElement element in prisoners.GetTroopRoster())
                            {
                                if (element.Character.Occupation == Occupation.Bandit) continue;
                                CultureObject culture = element.Character.Culture;
                                if (culture == null) continue;
                                if (dic.ContainsKey(culture))
                                    dic[culture] += element.Number;
                                else dic.Add(culture, element.Number);
                            }

                            foreach (KeyValuePair<CultureObject, int> pair in dic)
                            {
                                Settlement random =
                                    Settlement.All.GetRandomElementWithPredicate(x => x.Culture == pair.Key);
                                if (random != null &&
                                    BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(
                                        currentSettlement))
                                    BannerKingsConfig.Instance.PopulationManager.GetPopData(random)
                                        .UpdatePopType(PopType.Serfs, pair.Value);
                            }
                        }
                    }
                    else return false;
                }
                    
                return true;
            }
        }

        [HarmonyPatch(typeof(SellPrisonersAction), "ApplyForSelectedPrisoners")]
        class ApplySelectedPrisionersPatch
        {
            static bool Prefix(MobileParty sellerParty, TroopRoster prisoners, Settlement currentSettlement)
            {
                if (currentSettlement != null && (currentSettlement.IsCastle || currentSettlement.IsTown) & BannerKingsConfig.Instance.PopulationManager != null && BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(currentSettlement))
                {
                    BKCriminalPolicy policy = (BKCriminalPolicy)BannerKingsConfig.Instance.PolicyManager.GetPolicy(currentSettlement, "criminal");
                    if (policy.Policy == BKCriminalPolicy.CriminalPolicy.Enslavement)
                        BannerKingsConfig.Instance.PopulationManager.GetPopData(currentSettlement)
                            .UpdatePopType(PopType.Slaves, Utils.Helpers.GetRosterCount(prisoners));
                    else if (policy.Policy == BKCriminalPolicy.CriminalPolicy.Forgiveness)
                    {
                        Dictionary<CultureObject, int> dic = new Dictionary<CultureObject, int>();
                        foreach (TroopRosterElement element in prisoners.GetTroopRoster())
                        {
                            if (element.Character.Occupation == Occupation.Bandit) continue;
                            CultureObject culture = element.Character.Culture;
                            if (culture == null) continue;
                            if (dic.ContainsKey(culture))
                                dic[culture] += element.Number;
                            else dic.Add(culture, element.Number);
                        }

                        foreach (KeyValuePair<CultureObject, int> pair in dic)
                        {
                            Settlement random = Settlement.All.GetRandomElementWithPredicate(x => x.Culture == pair.Key);
                            if (random != null && BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(currentSettlement))
                                BannerKingsConfig.Instance.PopulationManager.GetPopData(random)
                                    .UpdatePopType(PopType.Serfs, pair.Value);
                        }
                    }
                    else return false;
                }

                return true;
            }
        }


        [HarmonyPatch(typeof(Town), "FoodStocksUpperLimit")]
        class FoodStockPatch
        {
            static bool Prefix(ref Town __instance, ref int __result)
            {
                if (BannerKingsConfig.Instance.PopulationManager != null && BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(__instance.Settlement))
                {
                    PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(__instance.Settlement);
                    int total = data.TotalPop;
                    int result = (int)(total / 6.5f);

                    __result = (int)(Campaign.Current.Models.SettlementFoodModel.FoodStocksUpperLimit +
                                     (__instance.IsCastle ? Campaign.Current.Models.SettlementFoodModel.CastleFoodStockUpperLimitBonus : 0) +
                        __instance.GetEffectOfBuildings(BuildingEffectEnum.Foodstock) +
                        result);
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(DefaultBuildingTypes), "InitializeAll")]
        class InitializeBuildingsPatch
        {
            static void Postfix()
            {
                Utils.Helpers._buildingCastleRetinue.Initialize(new TextObject("{=!}Retinue Barracks"), new TextObject("{=!}Barracks for the castle retinue, a group of elite soldiers. The retinue is added to the garrison over time, up to a limit of 20, 40 or 60 (building level)."), new[]
                {
                     800,
                     1200,
                     1500
                }, BuildingLocation.Castle, new Tuple<BuildingEffectEnum, float, float, float>[]
                {
                });
            }
        }
    }
}
