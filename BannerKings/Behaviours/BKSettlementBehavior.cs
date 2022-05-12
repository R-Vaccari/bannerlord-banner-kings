using BannerKings.Managers;
using BannerKings.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using static BannerKings.Managers.PopulationManager;
using HarmonyLib;
using BannerKings.Populations;
using TaleWorlds.CampaignSystem.Overlay;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;
using BannerKings.Managers.Policies;
using BannerKings.Managers.Populations.Villages;
using static BannerKings.Managers.Policies.BKTaxPolicy;
using BannerKings.Managers.Decisions;
using BannerKings.Components;
using static BannerKings.Managers.Policies.BKWorkforcePolicy;

namespace BannerKings.Behaviors
{
    public class BKSettlementBehavior : CampaignBehaviorBase
    {
        private PopulationManager populationManager = null;
        private PolicyManager policyManager = null;
        private TitleManager titleManager = null;
        private CourtManager courtManager = null;
        private static float actionGold = 0f;
        private static int actionHuntGame = 0;
        private static CampaignTime actionStart = CampaignTime.Now;

        public override void RegisterEvents()
        {
            CampaignEvents.OnSiegeAftermathAppliedEvent.AddNonSerializedListener(this, new Action<MobileParty, Settlement, SiegeAftermathCampaignBehavior.SiegeAftermath, Clan, Dictionary<MobileParty, float>>(OnSiegeAftermath));
            CampaignEvents.SettlementEntered.AddNonSerializedListener(this, new Action<MobileParty, Settlement, Hero>(OnSettlementEntered));
            CampaignEvents.DailyTickSettlementEvent.AddNonSerializedListener(this, new Action<Settlement>(DailySettlementTick));
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(OnSessionLaunched));
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

        private void OnSiegeAftermath(MobileParty attackerParty, Settlement settlement, SiegeAftermathCampaignBehavior.SiegeAftermath aftermathType, Clan previousSettlementOwner, Dictionary<MobileParty, float> partyContributions)
        {
            if (aftermathType == SiegeAftermathCampaignBehavior.SiegeAftermath.ShowMercy || settlement == null || settlement.Town == null ||
                BannerKingsConfig.Instance.PopulationManager == null ||!BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(settlement)) 
                return;

            PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
            float shareToKill;
            if (aftermathType == SiegeAftermathCampaignBehavior.SiegeAftermath.Pillage)
                shareToKill = MBRandom.RandomFloatRanged(0.1f, 0.16f);
            else shareToKill = MBRandom.RandomFloatRanged(0.16f, 0.24f);
            int killTotal = (int)((float)data.TotalPop * shareToKill);
            int lognum = killTotal;
            List<ValueTuple<PopType, float>> weights = new List<(PopType, float)>();
            foreach (KeyValuePair<PopType, float[]> pair in PopulationManager.GetDesiredPopTypes(settlement))
                weights.Add(new (pair.Key, pair.Value[0]));

            if (killTotal <= 0) return;

            while (killTotal > 0)
            {
                int random = MBRandom.RandomInt(10, 20);
                PopType target = MBRandom.ChooseWeighted(weights);
                int finalNum = MBMath.ClampInt(random, 0, data.GetTypeCount(target));
                data.UpdatePopType(target, -finalNum);
                killTotal -= finalNum;
            }

            InformationManager.DisplayMessage(new InformationMessage(new TextObject("{=!}{NUMBER} have been killed in the siege aftermath of {SETTLEMENT}.")
                .SetTextVariable("NUMBER", lognum)
                .SetTextVariable("SETTLEMENT", settlement.Name)
                .ToString()));
        }

        private void OnSettlementEntered(MobileParty party, Settlement target, Hero hero)
        {
            if (party == null || party.LeaderHero == null || BannerKingsConfig.Instance.PopulationManager == null) return;

            if (target == null || target.OwnerClan == null) return;

            if (party.LeaderHero != target.Owner || party.LeaderHero == Hero.MainHero ||
                !BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(target)) return;

            int random = MBRandom.RandomInt(1, 100);
            if (random > 5) return;

            Kingdom kingdom = target.OwnerClan.Kingdom;
            List<BannerKingsDecision> currentDecisions = BannerKingsConfig.Instance.PolicyManager.GetDefaultDecisions(target);
            List<BannerKingsDecision> changedDecisions = new List<BannerKingsDecision>();
            if (target.Town != null)
            {
                Town town = target.Town;
                if (town.FoodStocks < (float)town.FoodStocksUpperLimit() * 0.2f && town.FoodChange < 0f)
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

        private void UpdateTaxPolicy(int value, Settlement settlement)
        {
            BKTaxPolicy tax = ((BKTaxPolicy)BannerKingsConfig.Instance.PolicyManager.GetPolicy(settlement, "tax"));
            TaxType taxType = tax.Policy;
            if ((value == 1 && taxType != TaxType.High) || value == -1 && taxType != TaxType.Low)
            {
                BannerKingsConfig.Instance.PolicyManager.UpdateSettlementPolicy(settlement, new BKTaxPolicy((TaxType)taxType + value, settlement));
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
                        if (Helpers.Helpers._buildingCastleRetinue != null && castleBuilding.BuildingType == Helpers.Helpers._buildingCastleRetinue)
                        {
                            MobileParty garrison = settlement.Town.GarrisonParty;
                            if (garrison.MemberRoster != null && garrison.MemberRoster.Count > 0)
                            {
                                List<TroopRosterElement> elements = garrison.MemberRoster.GetTroopRoster();
                                int currentRetinue = 0;
                                foreach (TroopRosterElement soldierElement in elements)
                                    if (Helpers.Helpers.IsRetinueTroop(soldierElement.Character, settlement.Culture))
                                        currentRetinue += soldierElement.Number;

                                int maxRetinue = castleBuilding.CurrentLevel == 1 ? 20 : (castleBuilding.CurrentLevel == 2 ? 40 : 60);
                                if (currentRetinue < maxRetinue)
                                    if (garrison.MemberRoster.Count < garrison.Party.PartySizeLimit)
                                        garrison.MemberRoster.AddToCounts(settlement.Culture.EliteBasicTroop, 1);
                            }
                        }


                    if (settlement.Town.FoodStocks <= (float)settlement.Town.FoodStocksUpperLimit() * 0.05f && 
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
                

            BuildingType retinueType = MBObjectManager.Instance.GetObjectTypeList<BuildingType>().FirstOrDefault(x => x == Helpers.Helpers._buildingCastleRetinue);
            if (retinueType == null)
            {
                Helpers.Helpers._buildingCastleRetinue.Initialize(new TextObject("{=!}Retinue Barracks", null), new TextObject("{=!}Barracks for the castle retinue, a group of elite soldiers. The retinue is added to the garrison over time, up to a limit of 20, 40 or 60 (building level).", null), new int[]
                {
                     1000,
                     1500,
                     2000
                }, BuildingLocation.Castle, new Tuple<BuildingEffectEnum, float, float, float>[]
                {
                }, 0);
            }
        }

        private void AddMenus(CampaignGameStarter campaignGameStarter)
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

    namespace Patches
    {


        [HarmonyPatch(typeof(UrbanCharactersCampaignBehavior), "SpawnNotablesIfNeeded")]
        class SpawnNotablesIfNeededPatch
        {
            static bool Prefix(Settlement settlement)
            {
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
                float num2 = settlement.Notables.Any<Hero>() ? ((float)(num - settlement.Notables.Count) / (float)num) : 1f;
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
                        EnterSettlementAction.ApplyForCharacterOnly(HeroCreator.CreateHeroAtOccupation(list2.GetRandomElement<Occupation>(), settlement), settlement);
                    
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
                if (currentSettlement != null && (currentSettlement.IsCastle || currentSettlement.IsTown) && BannerKingsConfig.Instance.PopulationManager != null && BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(currentSettlement))
                {
                    if (!currentSettlement.IsVillage && !currentSettlement.IsTown && !currentSettlement.IsCastle)
                        return true;

                    BKCriminalPolicy policy = (BKCriminalPolicy)BannerKingsConfig.Instance.PolicyManager.GetPolicy(currentSettlement, "criminal");
                    if (policy.Policy == BKCriminalPolicy.CriminalPolicy.Enslavement)
                    {
                        PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(currentSettlement);
                        if (data != null)
                            data.UpdatePopType(PopType.Slaves, Helpers.Helpers.GetRosterCount(prisoners));
                    }
                        
                    else if (policy.Policy == BKCriminalPolicy.CriminalPolicy.Forgiveness)
                    {
                        Dictionary<CultureObject, int> dic = new Dictionary<CultureObject, int>();
                        foreach (TroopRosterElement element in prisoners.GetTroopRoster())
                        {
                            if (element.Character.Occupation == Occupation.Bandit) continue;
                            CultureObject culture = element.Character.Culture;
                            if (culture == null || culture.IsBandit) continue;
                            if (dic.ContainsKey(culture))
                                dic[culture] += element.Number;
                            else dic.Add(culture, element.Number);
                        }

                        foreach (KeyValuePair<CultureObject, int> pair in dic)
                        {
                            if (Settlement.All.Any(x => x.Culture == pair.Key))
                            {
                                Settlement random = Settlement.All.FirstOrDefault(x => x.Culture == pair.Key);
                                if (random != null && BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(random))
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
                    if (!currentSettlement.IsVillage && !currentSettlement.IsTown && !currentSettlement.IsCastle)
                        return true;

                    BKCriminalPolicy policy = (BKCriminalPolicy)BannerKingsConfig.Instance.PolicyManager.GetPolicy(currentSettlement, "criminal");
                    if (policy.Policy == BKCriminalPolicy.CriminalPolicy.Enslavement)
                    {
                        PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(currentSettlement);
                        if (data != null)
                            data.UpdatePopType(PopType.Slaves, Helpers.Helpers.GetRosterCount(prisoners));
                    }
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
                            if (Settlement.All.Any(x => x.Culture == pair.Key))
                            {
                                Settlement random = Settlement.All.FirstOrDefault(x => x.Culture == pair.Key);
                                if (random != null && BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(random))
                                {
                                    PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(currentSettlement);
                                    if (data != null)
                                        data.UpdatePopType(PopType.Serfs, pair.Value);
                                }
                            }  
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
                    int result = (int)((float)total / 6.5f);

                    __result = (int)((float)(Campaign.Current.Models.SettlementFoodModel.FoodStocksUpperLimit +
                        (__instance.IsCastle ? Campaign.Current.Models.SettlementFoodModel.CastleFoodStockUpperLimitBonus : 0)) +
                        __instance.GetEffectOfBuildings(BuildingEffectEnum.Foodstock) +
                        result);
                    return false;
                }
                else return true;
            }
        }

        [HarmonyPatch(typeof(DefaultBuildingTypes), "InitializeAll")]
        class InitializeBuildingsPatch
        {
            static void Postfix()
            {
                Helpers.Helpers._buildingCastleRetinue.Initialize(new TextObject("{=!}Retinue Barracks", null), new TextObject("{=!}Barracks for the castle retinue, a group of elite soldiers. The retinue is added to the garrison over time, up to a limit of 20, 40 or 60 (building level).", null), new int[]
                {
                     800,
                     1200,
                     1500
                }, BuildingLocation.Castle, new Tuple<BuildingEffectEnum, float, float, float>[]
                {
                }, 0);
            }
        }
    }
}
