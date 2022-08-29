using BannerKings.Behaviours;
using BannerKings.Managers.Innovations;
using BannerKings.Managers.Items;
using BannerKings.Managers.Kingdoms.Policies;
using BannerKings.Managers.Skills;
using BannerKings.Models.Vanilla;
using Bannerlord.UIExtenderEx;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace BannerKings
{
    public class Main : MBSubModuleBase
    {
        private static readonly UIExtender Xtender = new(typeof(Main).Namespace!);

        protected override void OnGameStart(Game game, IGameStarter gameStarter)
        {
            if (gameStarter is not CampaignGameStarter campaignStarter)
            {
                return;
            }
            
            campaignStarter.AddBehavior(new BKSettlementBehavior());
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
            //campaignStarter.AddBehavior(new BKCombatBehavior());

            campaignStarter.AddModel(new BKCompanionPrices());
            campaignStarter.AddModel(new BKProsperityModel());
            campaignStarter.AddModel(new BKTaxModel());
            campaignStarter.AddModel(new BKFoodModel());
            campaignStarter.AddModel(new BKConstructionModel());
            campaignStarter.AddModel(new BKMilitiaModel());
            campaignStarter.AddModel(new BKInfluenceModel());
            campaignStarter.AddModel(new BKLoyaltyModel());
            campaignStarter.AddModel(new BKVillageProductionModel());
            campaignStarter.AddModel(new BKSecurityModel());
            campaignStarter.AddModel(new BKPartyLimitModel());
            campaignStarter.AddModel(new BKEconomyModel());
            campaignStarter.AddModel(new BKPriceFactorModel());
            campaignStarter.AddModel(new BKWorkshopModel());
            campaignStarter.AddModel(new BKClanFinanceModel());
            campaignStarter.AddModel(new BKArmyManagementModel());
            campaignStarter.AddModel(new BKSiegeEventModel());
            campaignStarter.AddModel(new BKTournamentModel());
            campaignStarter.AddModel(new BKRaidModel());
            campaignStarter.AddModel(new BKVolunteerModel());
            campaignStarter.AddModel(new BKNotableSpawnModel());
            campaignStarter.AddModel(new BKGarrisonModel());
            campaignStarter.AddModel(new BKRansomModel());
            campaignStarter.AddModel(new BKClanTierModel());
            campaignStarter.AddModel(new BKPartyWageModel());
            campaignStarter.AddModel(new BKSettlementValueModel());
            campaignStarter.AddModel(new BKNotablePowerModel());
            campaignStarter.AddModel(new BKPartyFoodConsumption());
            campaignStarter.AddModel(new BKSmithingModel());
            campaignStarter.AddModel(new BKMapTrackModel());
            campaignStarter.AddModel(new BKAgentDamageModel());
            campaignStarter.AddModel(new BKAgentStatsModel());
            campaignStarter.AddModel(new BKPartySpeedModel());
            campaignStarter.AddModel(new BKBattleSimulationModel());
            campaignStarter.AddModel(new BKPartyConsumptionModel());
            campaignStarter.AddModel(new BKWallHitpointModel());
            campaignStarter.AddModel(new BKInventoryCapacityModel());
            campaignStarter.AddModel(new BKMapVisibilityModel());
            campaignStarter.AddModel(new BKPartyImpairmentModel());
            campaignStarter.AddModel(new BKCrimeModel());
            campaignStarter.AddModel(new BKTroopUpgradeModel());
            campaignStarter.AddModel(new BKVolunteerAccessModel());
            campaignStarter.AddModel(new BKBattleRewardModel());
            campaignStarter.AddModel(new BKCombatXpModel());
            campaignStarter.AddModel(new BKBattleMoraleModel());
            //campaignStarter.LoadGameTexts(BasePath.Name + "Modules/BannerKings/ModuleData/module_strings.xml");

            BKAttributes.Instance.Initialize();
            BKSkills.Instance.Initialize();
            BKPerks.Instance.Initialize();
            BKItemCategories.Instance.Initialize();
            BKItems.Instance.Initialize();
            BKPolicies.Instance.Initialize();
            DefaultInnovations.Instance.Initialize();
        }

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            new Harmony("BannerKings").PatchAll();
            Xtender.Register(typeof(Main).Assembly);
            Xtender.Enable();
        }
    }
}