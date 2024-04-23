using BannerKings.Behaviours;
using BannerKings.Behaviours.Criminality;
using BannerKings.Behaviours.Diplomacy;
using BannerKings.Behaviours.Feasts;
using BannerKings.Behaviours.Marriage;
using BannerKings.Behaviours.PartyNeeds;
using BannerKings.Behaviours.Retainer;
using BannerKings.Behaviours.Mercenary;
using BannerKings.Behaviours.Workshops;
using BannerKings.Managers.Buildings;
using BannerKings.Managers.Innovations;
using BannerKings.Managers.Kingdoms.Policies;
using BannerKings.Managers.Skills;
using BannerKings.Models.Vanilla;
using BannerKings.Settings;
using BannerKings.UI;
using Bannerlord.UIExtenderEx;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using BannerKings.Managers.Innovations.Eras;
using BannerKings.Behaviours.Innovations;
using BannerKings.Behaviours.Shipping;
using BannerKings.Campaign.Skills;

namespace BannerKings
{
    public class Main : MBSubModuleBase
    {
        private static readonly UIExtender Xtender = new(typeof(Main).Namespace!);

        protected override void OnGameStart(Game game, IGameStarter gameStarter)
        {
            base.OnGameStart(game, gameStarter);
            if (gameStarter is not CampaignGameStarter campaignStarter)
            {
                return;
            }
            
            campaignStarter.AddBehavior(new BKManagerBehavior());
            campaignStarter.AddBehavior(new BKEducationBehavior());
            campaignStarter.AddBehavior(new BKSettlementActions());
            campaignStarter.AddBehavior(new BKKnighthoodBehavior());
            campaignStarter.AddBehavior(new BKTournamentBehavior());
            campaignStarter.AddBehavior(new BKRepublicBehavior());
            campaignStarter.AddBehavior(new BKPartyBehavior());
            campaignStarter.AddBehavior(new BKClanBehavior());
            campaignStarter.AddBehavior(new BKArmyBehavior());
            campaignStarter.AddBehavior(new BKRansomBehavior());
            campaignStarter.AddBehavior(new BKTitleBehavior());
            campaignStarter.AddBehavior(new BKNotableBehavior());
            campaignStarter.AddBehavior(new BKReligionsBehavior());
            campaignStarter.AddBehavior(new BKSkillBehavior());
            campaignStarter.AddBehavior(new BKLordPropertyBehavior());
            campaignStarter.AddBehavior(new BKInnovationsBehavior());
            campaignStarter.AddBehavior(new BKLifestyleBehavior());
            campaignStarter.AddBehavior(new BKCampaignStartBehavior());
            campaignStarter.AddBehavior(new BKGoalBehavior());
            campaignStarter.AddBehavior(new BKBuildingsBehavior());
            campaignStarter.AddBehavior(new BKGovernorBehavior());
            campaignStarter.AddBehavior(new BKTradeGoodsFixesBehavior());
            campaignStarter.AddBehavior(new BKCapitalBehavior());
            campaignStarter.AddBehavior(new BKMarriageBehavior());
            campaignStarter.AddBehavior(new BKRetainerBehavior());
            if (BannerKingsSettings.Instance.Feasts)
            {
                campaignStarter.AddBehavior(new BKFeastBehavior());
            }
            
            campaignStarter.AddBehavior(new BKWorkshopBehavior());
            campaignStarter.AddBehavior(new BKGentryBehavior());
            campaignStarter.AddBehavior(new BKBanditBehavior());
            campaignStarter.AddBehavior(new BKDiplomacyBehavior());
            campaignStarter.AddBehavior(new BKCriminalityBehavior());
            campaignStarter.AddBehavior(new BKTraitBehavior());
            campaignStarter.AddBehavior(new BKPartyNeedsBehavior());
            campaignStarter.AddBehavior(new BKShippingBehavior());
            campaignStarter.AddBehavior(new BKMercenaryCareerBehavior());
            campaignStarter.AddBehavior(new BKRelationsBehavior());
            campaignStarter.AddBehavior(new BKSettlementBehavior());

            campaignStarter.AddModel(new BKPrisonerModel());
            campaignStarter.AddModel(BannerKingsConfig.Instance.CompanionModel);
            campaignStarter.AddModel(BannerKingsConfig.Instance.ProsperityModel);
            campaignStarter.AddModel(BannerKingsConfig.Instance.TaxModel);
            campaignStarter.AddModel(new BKFoodModel());
            campaignStarter.AddModel(BannerKingsConfig.Instance.ConstructionModel);
            campaignStarter.AddModel(new BKMilitiaModel());
            campaignStarter.AddModel(BannerKingsConfig.Instance.InfluenceModel);
            campaignStarter.AddModel(new BKLoyaltyModel());
            campaignStarter.AddModel(BannerKingsConfig.Instance.VillageProductionModel);
            campaignStarter.AddModel(new BKSecurityModel());
            campaignStarter.AddModel(new BKPartyLimitModel());
            campaignStarter.AddModel(BannerKingsConfig.Instance.EconomyModel);
            campaignStarter.AddModel(new BKPriceFactorModel());
            campaignStarter.AddModel(BannerKingsConfig.Instance.WorkshopModel);
            campaignStarter.AddModel(BannerKingsConfig.Instance.ClanFinanceModel);
            campaignStarter.AddModel(BannerKingsConfig.Instance.ArmyManagementModel);
            campaignStarter.AddModel(new BKSiegeEventModel());
            campaignStarter.AddModel(new BKTournamentModel());
            campaignStarter.AddModel(new BKRaidModel());
            campaignStarter.AddModel(BannerKingsConfig.Instance.VolunteerModel);
            campaignStarter.AddModel(new BKNotableSpawnModel());
            campaignStarter.AddModel(new BKGarrisonModel());
            campaignStarter.AddModel(new BKRansomModel());
            campaignStarter.AddModel(new BKClanTierModel());
            campaignStarter.AddModel(new BKPartyWageModel());
            campaignStarter.AddModel(new BKSettlementValueModel());
            campaignStarter.AddModel(new BKNotablePowerModel());
            campaignStarter.AddModel(BannerKingsConfig.Instance.SmithingModel);
            campaignStarter.AddModel(new BKMapTrackModel());
            campaignStarter.AddModel(new BKAgentDamageModel());
            campaignStarter.AddModel(new BKAgentStatsModel());
            campaignStarter.AddModel(new BKBuildingEffectModel());
            campaignStarter.AddModel(new BKPartySpeedModel());
            campaignStarter.AddModel(new BKBattleSimulationModel());
            campaignStarter.AddModel(new BKPartyConsumptionModel());
            campaignStarter.AddModel(new BKWallHitpointModel());
            campaignStarter.AddModel(new BKInventoryCapacityModel());
            campaignStarter.AddModel(new BKItemDiscardModel());
            campaignStarter.AddModel(new BKMapVisibilityModel());
            campaignStarter.AddModel(new BKPartyImpairmentModel());
            campaignStarter.AddModel(BannerKingsConfig.Instance.CrimeModel);
            campaignStarter.AddModel(new BKTroopUpgradeModel());
            campaignStarter.AddModel(new BKBattleRewardModel());
            campaignStarter.AddModel(new BKCombatXpModel());
            campaignStarter.AddModel(new BKBattleMoraleModel());
            campaignStarter.AddModel(BannerKingsConfig.Instance.LearningModel);
            campaignStarter.AddModel(BannerKingsConfig.Instance.KingdomDecisionModel);  
            campaignStarter.AddModel(new BKPregnancyModel());
            campaignStarter.AddModel(new BKPartyHealingModel());
            campaignStarter.AddModel(new BKBanditModel());
            campaignStarter.AddModel(new BKPartyTrainningModel());
            campaignStarter.AddModel(new BKDiplomacyModel());
            campaignStarter.AddModel(new BKCharacterStatsModel());
            campaignStarter.AddModel(new BKPartyBuyingFoodModel());
            campaignStarter.AddModel(new BKCategorySelector());
            campaignStarter.AddModel(new BKSettlementAccessModel());

            BKAttributes.Instance.Initialize();
            BKSkills.Instance.Initialize();
            BKSkillEffects.Instance.Initialize();
            BKPerks.Instance.Initialize();   
            BKPolicies.Instance.Initialize();
            DefaultEras.Instance.Initialize();
            DefaultInnovations.Instance.Initialize();
            BKBuildings.Instance.Initialize();

            DefaultMercenaryPrivileges.Instance.Initialize();
            DefaultCustomTroopPresets.Instance.Initialize();

            UIManager.Instance.SetScreen(new BannerKingsScreen());
            //TaleWorlds.CampaignSystem.Campaign.Current.TournamentManager = new BKTournamentManager();
        }

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            new Harmony("BannerKings").PatchAll();
            Xtender.Register(typeof(Main).Assembly);
            Xtender.Enable();
        }

        public override void OnGameEnd(Game game)
        {
            base.OnGameEnd(game);
            if (UIManager.Instance.BKScreen != null)
            {
                UIManager.Instance.BKScreen.OnFinalize();
            }
        }
    }
}