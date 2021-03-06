using HarmonyLib;
using BannerKings.Behaviors;
using BannerKings.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors.VillageBehaviors;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using static BannerKings.Managers.PopulationManager;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;
using System.Reflection;
using TaleWorlds.ObjectSystem;
using BannerKings.Managers.Helpers;
using BannerKings.Populations;
using BannerKings.Models.Vanilla;
using BannerKings.Behaviours;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using BannerKings.Managers.Titles;
using TaleWorlds.CampaignSystem.SandBox.GameComponents.Map;
using static TaleWorlds.CampaignSystem.SandBox.Issues.CaravanAmbushIssueBehavior;
using static TaleWorlds.CampaignSystem.SandBox.Issues.LandLordNeedsManualLaborersIssueBehavior;
using static TaleWorlds.CampaignSystem.Election.KingSelectionKingdomDecision;
using static TaleWorlds.CampaignSystem.SandBox.Issues.VillageNeedsToolsIssueBehavior;
using static TaleWorlds.CampaignSystem.SandBox.Issues.EscortMerchantCaravanIssueBehavior;
using TaleWorlds.CampaignSystem.SandBox.Issues;
using Helpers;
using BannerKings.Managers.Items;
using BannerKings.Managers.Kingdoms.Policies;
using TaleWorlds.CampaignSystem.SandBox.GameComponents.Party;

namespace BannerKings
{
    public class Main : MBSubModuleBase
    {
        public static Harmony patcher = new Harmony("Patcher");
        //private readonly UIExtender xtender = new UIExtender("BannerKings");

        protected override void OnGameStart(Game game, IGameStarter gameStarter)
        {
            if (game.GameType is Campaign)
            {
                CampaignGameStarter campaignStarter = (CampaignGameStarter)gameStarter;
                campaignStarter.AddBehavior(new BKSettlementBehavior());
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
                campaignStarter.AddBehavior(new BKLordCaravansBehavior());

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
                campaignStarter.LoadGameTexts(BasePath.Name + "Modules/BannerKings/ModuleData/module_strings.xml");
                BKItemCategories.Instance.Initialize();
                BKItems.Instance.Initialize();
                BKPolicies.Instance.Initialize();
            }

            //xtender.Register(typeof(Main).Assembly);
            //xtender.Enable();
        }

        protected override void OnSubModuleLoad()
        {
            new Harmony("BannerKings").PatchAll();
            base.OnSubModuleLoad();
        }
    }

    namespace Patches
    {
        namespace Recruitment
        {
            [HarmonyPatch(typeof(RecruitmentCampaignBehavior), "ApplyInternal")]
            class RecruitmentApplyInternalPatch
            {
                static void Postfix(MobileParty side1Party, Settlement settlement, Hero individual, CharacterObject troop, int number, int bitCode, RecruitmentCampaignBehavior.RecruitingDetail detail)
                {

                    if (settlement == null) return;
                    if (BannerKingsConfig.Instance.PopulationManager != null && BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(settlement))
                    {
                        PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
                        data.MilitaryData.DeduceManpower(data, number, troop);
                    }
                }
            }

            [HarmonyPatch(typeof(DefaultVolunteerProductionModel), "GetDailyVolunteerProductionProbability")]
            class GetDailyVolunteerProductionProbabilityPatch
            {
                static bool Prefix(Hero hero, int index, Settlement settlement, ref float __result)
                {

                    if (BannerKingsConfig.Instance.PopulationManager != null && BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(settlement))
                    {
                        __result = new BKVolunteerModel().GetDraftEfficiency(hero, index, settlement).ResultNumber;
                        return false;
                    }

                    return true;
                }
            }
        }

        namespace Fixes
        {

            // Fix crash on wanderer same gender child born
            [HarmonyPatch(typeof(NameGenerator), "GenerateHeroFullName")]
            class NameGeneratorPatch
            {
                static bool Prefix(ref TextObject __result, Hero hero, TextObject heroFirstName, bool useDeterministicValues = true)
                {

                    Hero parent = hero.IsFemale ? hero.Mother : hero.Father;
                    if (parent == null) return true;
                    if (BannerKingsConfig.Instance.TitleManager.IsHeroKnighted(parent) && hero.IsWanderer)
                    {
                        TextObject textObject = heroFirstName;
                        textObject.SetTextVariable("FEMALE", hero.IsFemale ? 1 : 0);
                        textObject.SetTextVariable("IMPERIAL", (hero.Culture.StringId == "empire") ? 1 : 0);
                        textObject.SetTextVariable("COASTAL", (hero.Culture.StringId == "empire" || hero.Culture.StringId == "vlandia") ? 1 : 0);
                        textObject.SetTextVariable("NORTHERN", (hero.Culture.StringId == "battania" || hero.Culture.StringId == "sturgia") ? 1 : 0);
                        textObject.SetCharacterProperties("HERO", hero.CharacterObject, false);
                        textObject.SetTextVariable("FIRSTNAME", heroFirstName);
                        __result = textObject;
                        return false;
                    }
                    return true;
                }
            }

            [HarmonyPatch(typeof(TownMarketData))]
            class TownItemPricePatch
            {

                [HarmonyPrefix]
                [HarmonyPatch("GetPrice", new Type[] { typeof(EquipmentElement), typeof(MobileParty), typeof(bool), typeof(PartyBase) })]
                static bool Prefix2(TownMarketData __instance, ref int __result, EquipmentElement itemRosterElement, MobileParty tradingParty = null, bool isSelling = false, PartyBase merchantParty = null)
                {
                    if (itemRosterElement.Item == null) __result = 0;
                    else
                    {
                        ItemData categoryData = __instance.GetCategoryData(itemRosterElement.Item.GetItemCategory());
                        __result = Campaign.Current.Models.TradeItemPriceFactorModel.GetPrice(itemRosterElement, tradingParty, merchantParty, isSelling, (float)categoryData.InStoreValue, categoryData.Supply, categoryData.Demand);
                    }
                    return false;
                }
            }


            [HarmonyPatch(typeof(Hero), "CanHaveQuestsOrIssues")]
            class CanHaveQuestsOrIssuesPatch
            {
                static bool Prefix(Hero __instance, ref bool __result)
                {
                    if (__instance.Issue != null)
                        return false;

                    __result = __instance.IsActive && __instance.IsAlive;
                    CampaignEventDispatcher.Instance.CanHaveQuestsOrIssues(__instance, ref __result);

                    return false;
                }
            }

            [HarmonyPatch(typeof(VillageNeedsToolsIssue), "IssueStayAliveConditions")]
            class VillageIssueStayAliveConditionsPatch
            {
                static bool Prefix(CaravanAmbushIssue __instance, ref bool __result)
                {
                    if (__instance.IssueOwner != null)
                    {
                        if (__instance.IssueOwner.CurrentSettlement == null || !__instance.IssueOwner.CurrentSettlement.IsVillage)
                        {
                            __result = false;
                            return false;
                        }
                    } else
                    {
                        __result = false;
                        return false;
                    } 

                    return true;
                }
            }

            [HarmonyPatch(typeof(CaravanAmbushIssue), "IssueStayAliveConditions")]
            class CaravanIssueStayAliveConditionsPatch
            {
                static bool Prefix(CaravanAmbushIssue __instance, ref bool __result)
                {
                    if (__instance.IssueOwner != null)
                        if (__instance.IssueOwner.OwnedCaravans == null || __instance.IssueOwner.MapFaction == null)
                        {
                            __result = false;
                            return false;
                        }

                    return true;
                }
            }

            [HarmonyPatch(typeof(LandLordNeedsManualLaborersIssue), "IssueStayAliveConditions")]
            class LaborersIssueStayAliveConditionsPatch
            {
                static bool Prefix(LandLordNeedsManualLaborersIssue __instance, ref bool __result)
                {
                    if (__instance.IssueOwner != null)
                    {
                        if (__instance.IssueOwner.CurrentSettlement == null || !__instance.IssueOwner.CurrentSettlement.IsVillage)
                        {
                            __result = false;
                            return false;
                        }
                    } else
                    {
                        __result = false;
                        return false;
                    }

                    return true;
                }
            }

            [HarmonyPatch(typeof(EscortMerchantCaravanIssueBehavior), "ConditionsHold")]
            class EscortCaravanConditionsHoldPatch
            {
                static bool Prefix(Hero issueGiver, ref bool __result)
                {
                    if (issueGiver.CurrentSettlement == null || issueGiver.CurrentSettlement.IsVillage)
                    {
                        __result = false;
                        return false;
                    }

                    return true;
                }
            }

            [HarmonyPatch(typeof(EscortMerchantCaravanIssue), "IssueStayAliveConditions")]
            class EscortCaravanIssueStayAliveConditionsPatch
            {
                static bool Prefix(EscortMerchantCaravanIssue __instance, ref bool __result)
                {
                    if (__instance.IssueOwner.CurrentSettlement == null || __instance.IssueOwner.CurrentSettlement.IsVillage)
                    {
                        __result = false;
                        return false;
                    }

                    return true;
                }
            }


            [HarmonyPatch(typeof(DefaultPartyMoraleModel), "CalculateFoodVarietyMoraleBonus")]
            class CalculateFoodVarietyMoraleBonusPatch
            {
                static bool Prefix(MobileParty party, ref ExplainedNumber result)
                {
                    float num = MBMath.ClampFloat(party.ItemRoster.FoodVariety - 5f, -5f, 5f);
                    if (num != 0f && (num >= 0f || party.LeaderHero == null || !party.LeaderHero.GetPerkValue(DefaultPerks.Steward.Spartan)))
                    {
                        if (num > 0f && party.HasPerk(DefaultPerks.Steward.Gourmet, false))
                        {
                            result.Add(num * DefaultPerks.Steward.Gourmet.PrimaryBonus, DefaultPerks.Steward.Gourmet.Name, null);
                            return false;
                        }
                        result.Add(num, GameTexts.FindText("str_food_bonus_morale", null), null);
                    }

                    float totalModifiers = 0f;
                    float modifierRate = 0f;
                    foreach (ItemRosterElement element in party.ItemRoster.ToList().FindAll(x => x.EquipmentElement.Item.IsFood))
                    {
                        ItemModifier modifier = element.EquipmentElement.ItemModifier;
                        if (modifier != null)
                        {
                            totalModifiers++;
                            modifierRate += modifier.PriceMultiplier;
                        }
                    }

                    if (modifierRate != 0f) result.Add(MBMath.ClampFloat(modifierRate / totalModifiers, -5f, 5f), new TextObject("{=!}Food quality"));

                    return false;
                }
            }
        }


        namespace Government
        {

            [HarmonyPatch(typeof(KingdomPolicyDecision), "IsAllowed")]
            class PolicyIsAllowedPatch
            {
                static bool Prefix(ref bool __result, KingdomPolicyDecision __instance)
                {
                    if (BannerKingsConfig.Instance.TitleManager != null)
                    {
                        FeudalTitle sovereign = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(__instance.Kingdom);
                        if (sovereign != null)
                        {
                            __result = !PolicyHelper.GetForbiddenGovernmentPolicies(sovereign.contract.Government).Contains(__instance.Policy);
                            return false;
                        }
                    }
                    return true;
                }
            }

            [HarmonyPatch(typeof(KingSelectionKingdomDecision), "ApplyChosenOutcome")]
            class ApplyChosenOutcomePatch
            {
                static void Postfix(KingSelectionKingdomDecision __instance, DecisionOutcome chosenOutcome)
                {
                    FeudalTitle title = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(__instance.Kingdom);
                    if (title != null)
                    {
                        Hero deJure = title.deJure;
                        Hero king = ((KingSelectionDecisionOutcome)chosenOutcome).King;
                        if (deJure != king) BannerKingsConfig.Instance.TitleManager.InheritTitle(deJure, king, title);
                    }
                }
            }
        }


        namespace Economy
        {

            [HarmonyPatch(typeof(UrbanCharactersCampaignBehavior), "BalanceGoldAndPowerOfNotable")]
            class BalanceGoldAndPowerOfNotablePatch
            {
                static bool Prefix(Hero notable)
                {
                    if (notable.Gold > 10500)
                    {
                        int num = (notable.Gold - 10000) / 500;
                        GiveGoldAction.ApplyBetweenCharacters(notable, null, num * 500, true);
                        notable.AddPower((float)num);
                        return false;
                    }
                    return true;
                }
            }

            [HarmonyPatch(typeof(Workshop), "Expense", MethodType.Getter)]
            class WorkshopExpensePatch
            {
                static bool Prefix(Workshop __instance, ref int __result)
                {
                    __result = (int)(__instance.Settlement.Prosperity * 0.01f +
                    Campaign.Current.Models.WorkshopModel.GetDailyExpense(__instance.Level));
                    return false;
                }
            }

            [HarmonyPatch(typeof(HeroHelper), "StartRecruitingMoneyLimit")]
            class StartRecruitingMoneyLimitPatch
            {
                static bool Prefix(Hero hero, ref float __result)
                {
                    __result = 50f;
                    if (hero.PartyBelongedTo != null)
                        __result += 1000f;
                    return false;
                }
            }

            [HarmonyPatch(typeof(ClanVariablesCampaignBehavior), "UpdateClanSettlementAutoRecruitment")]
            class AutoRecruitmentPatch
            {
                static bool Prefix(Clan clan)
                {
                    if (clan.MapFaction != null && clan.MapFaction.IsKingdomFaction)
                    {
                        IEnumerable<Kingdom> enemies = FactionManager.GetEnemyKingdoms(clan.Kingdom);
                        foreach (Settlement settlement in clan.Settlements)
                        {
                            if (settlement.IsFortification && settlement.Town.GarrisonParty != null)
                            {
                                if (enemies.Count() >= 0 && settlement.Town.GarrisonParty.MemberRoster.TotalManCount < 500)
                                    settlement.Town.GarrisonAutoRecruitmentIsEnabled = true;
                                settlement.Town.GarrisonAutoRecruitmentIsEnabled = false;
                            }
                        }
                    }
                    return false;
                }
            }

            [HarmonyPatch(typeof(ClanVariablesCampaignBehavior), "MakeClanFinancialEvaluation")]
            class MakeClanFinancialEvaluationPatch
            {
                static bool Prefix(Clan clan)
                {
                    if (clan.IsMinorFaction) return true;

                    if (BannerKingsConfig.Instance.TitleManager != null)
                    {
                        bool war = false;
                        if (clan.Kingdom != null)
                            war = FactionManager.GetEnemyKingdoms(clan.Kingdom).Count() > 0;
                        float income = Campaign.Current.Models.ClanFinanceModel.CalculateClanIncome(clan).ResultNumber * (war ? 0.5f : 0.2f);
                        if (war)
                            income += clan.Gold * 0.05f;


                        if (income > 0f)
                        {
                            float knights = 0f;
                            foreach (WarPartyComponent partyComponent in clan.WarPartyComponents)
                                if (partyComponent.Leader != null && partyComponent.Leader != clan.Leader)
                                {
                                    FeudalTitle title = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(partyComponent.Leader);
                                    if (title != null && title.fief != null)
                                    {
                                        knights++;
                                        float limit = 0f;
                                        if (title.fief.IsVillage)
                                            limit = title.fief.Village.TradeTaxAccumulated;
                                        else if (title.fief.Town != null)
                                            limit = Campaign.Current.Models.SettlementTaxModel.CalculateTownTax(title.fief.Town).ResultNumber;

                                        partyComponent.MobileParty.PaymentLimit = (int)(50f + limit);
                                    }
                                }

                            foreach (WarPartyComponent partyComponent in clan.WarPartyComponents)
                            {
                                float share = income / clan.WarPartyComponents.Count - knights;
                                partyComponent.MobileParty.PaymentLimit = (int)(300f + share);
                            }
                            return false;
                        }
                    }

                    return true;
                }
            }

            [HarmonyPatch(typeof(DefaultClanFinanceModel))]
            class ClanFinancesPatches
            {
                [HarmonyPrefix]
                [HarmonyPatch("AddIncomeFromKingdomBudget", MethodType.Normal)]
                static bool KingdomBudgetPrefix(Clan clan, ref ExplainedNumber goldChange, bool applyWithdrawals)
                {
                    if (BannerKingsConfig.Instance.TitleManager != null)
                    {
                        FeudalTitle title = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(clan.Leader);
                        return title != null && title.contract != null && title.contract.Rights.Contains(FeudalRights.Assistance_Rights);
                    }
                    return true;
                }

                [HarmonyPrefix]
                [HarmonyPatch("AddIncomeFromParty", MethodType.Normal)]
                static bool AddIncomeFromPartyPrefix(MobileParty party, Clan clan, ref ExplainedNumber goldChange, bool applyWithdrawals)
                {
                    if (BannerKingsConfig.Instance.TitleManager != null && party.LeaderHero != null && party.LeaderHero != clan.Leader)
                        return BannerKingsConfig.Instance.TitleManager.GetHighestTitle(party.LeaderHero) == null;
                    
                    return true;
                }


                [HarmonyPrefix]
                [HarmonyPatch("AddExpensesFromGarrisons", MethodType.Normal)]
                static bool GarrisonsPrefix(Clan clan, ref ExplainedNumber goldChange, bool applyWithdrawals = false)
                {
                    if (BannerKingsConfig.Instance.TitleManager != null)
                    {
                        DefaultClanFinanceModel model = new DefaultClanFinanceModel();
                        MethodInfo calculateWage = model.GetType().GetMethod("CalculatePartyWage", BindingFlags.Instance | BindingFlags.NonPublic);
                        if (clan == Clan.PlayerClan)
                            Console.WriteLine();

                        foreach (Town town in clan.Fiefs)
                        {
                            MobileParty garrisonParty = town.GarrisonParty;
                            
                            if (garrisonParty != null && garrisonParty.IsActive)
                            {
                                int wage = (int)calculateWage.Invoke(model, new object[] { garrisonParty, clan.Gold, applyWithdrawals });
                                if (wage > 0) goldChange.Add(-wage, new TextObject("{=iPDOLbi3}Party wages {A0}"), garrisonParty.Name);
                            }
                        }
                        return false;
                    }
                    return true;
                }


                [HarmonyPrefix]
                [HarmonyPatch("AddExpensesFromParties", MethodType.Normal)]
                static bool PartyExpensesPrefix(Clan clan, ref ExplainedNumber goldChange, bool applyWithdrawals = false)
                {
                    if (BannerKingsConfig.Instance.TitleManager != null)
                    {
                        List<MobileParty> list = new List<MobileParty>();
                        foreach (Hero hero in clan.Lords)
                            foreach (CaravanPartyComponent caravanPartyComponent in hero.OwnedCaravans)
                                list.Add(caravanPartyComponent.MobileParty);

                        foreach (Hero hero2 in clan.Companions)
                            foreach (CaravanPartyComponent caravanPartyComponent2 in hero2.OwnedCaravans)
                                list.Add(caravanPartyComponent2.MobileParty);

                        foreach (WarPartyComponent warPartyComponent in clan.WarPartyComponents)
                            list.Add(warPartyComponent.MobileParty);

                        DefaultClanFinanceModel model = new DefaultClanFinanceModel();
                        MethodInfo addExpense = model.GetType().GetMethod("AddPartyExpense", BindingFlags.Instance | BindingFlags.NonPublic);
                        foreach (MobileParty mobileParty in list)
                        {
                            if (mobileParty.LeaderHero != null && mobileParty.LeaderHero != clan.Leader)
                            {
                                object[] array = new object[] { mobileParty, clan, new ExplainedNumber(), applyWithdrawals };
                                addExpense.Invoke(model, array);
                                if (BannerKingsConfig.Instance.TitleManager.GetHighestTitle(mobileParty.LeaderHero) == null) 
                                    goldChange.Add(((ExplainedNumber)array[2]).ResultNumber, new TextObject("{=iPDOLbi3}Party wages {A0}"), mobileParty.Name);
                                else
                                {
                                    MethodInfo calculateWage = model.GetType().GetMethod("CalculatePartyWage", BindingFlags.Instance | BindingFlags.NonPublic);
                                    int wage = (int)calculateWage.Invoke(model, new object[] { mobileParty, mobileParty.LeaderHero.Gold, applyWithdrawals });
                                    if (applyWithdrawals)
                                        mobileParty.LeaderHero.Gold -= MathF.Min(mobileParty.LeaderHero.Gold, wage);
                                }
                            }
                        }

                        return false;
                    }
                    return true;
                }


                [HarmonyPrefix]
                [HarmonyPatch("AddVillagesIncome", MethodType.Normal)]
                static bool VillageIncomePrefix(Clan clan, ref ExplainedNumber goldChange, bool applyWithdrawals)
                {
                    if (BannerKingsConfig.Instance.TitleManager != null)
                    {
                        List<FeudalTitle> lordships = BannerKingsConfig.Instance.TitleManager
                            .GetAllDeJure(clan)
                            .FindAll(x => x.type == TitleType.Lordship);
                        foreach (Village village in clan.Villages)
                        {
                            FeudalTitle title = lordships.FirstOrDefault(x => x.fief.Village == village);
                            if (title == null) title = BannerKingsConfig.Instance.TitleManager.GetTitle(village.Settlement);
                            else lordships.Remove(title);
                            int result = CalculateVillageIncome(ref goldChange, village, clan, applyWithdrawals);

                            if (title != null)
                            {
                                Hero deJure = title.deJure;
                                bool knightOwned = title.deJure != clan.Leader && title.deJure.Clan == clan;
                                if (knightOwned)
                                {
                                    deJure.Gold += result;
                                    continue;
                                }
                                else if (deJure.Clan.Kingdom == clan.Kingdom)
                                    continue;
                            }

                            goldChange.Add((float)result, new TextObject("{=!}{A0}", null), village.Name);
                        }

                        foreach (FeudalTitle lordship in lordships)
                        {
                            Village village = lordship.fief.Village;
                            Clan ownerClan = village.Settlement.OwnerClan;
                            if (ownerClan.Kingdom == clan.Kingdom)
                            {
                                int result = CalculateVillageIncome(ref goldChange, village, clan, applyWithdrawals);
                                bool leaderOwned = lordship.deJure == clan.Leader;
                                if (!leaderOwned)
                                {
                                    Hero deJure = lordship.deJure;
                                    deJure.Gold += result;
                                }
                                else goldChange.Add((float)result, new TextObject("{=!}{A0}", null), village.Name);
                            }
                        }
                        return false;
                    }
                    return true;
                }

                private static int CalculateVillageIncome(ref ExplainedNumber goldChange, Village village, Clan clan, bool applyWithdrawals)
                {
                    int total = (village.VillageState == Village.VillageStates.Looted || village.VillageState == Village.VillageStates.BeingRaided) ? 0 : ((int)((float)village.TradeTaxAccumulated / 5f));
                    int num2 = total;
                    if (clan.Kingdom != null && clan.Kingdom.RulingClan != clan && clan.Kingdom.ActivePolicies.Contains(DefaultPolicies.LandTax))
                    {
                        total += (int)((-(float)total) * 0.05f);
                    }

                    if (village.Bound.Town != null && village.Bound.Town.Governor != null && village.Bound.Town.Governor.GetPerkValue(DefaultPerks.Scouting.ForestKin))
                        total += MathF.Round((float)total * DefaultPerks.Scouting.ForestKin.SecondaryBonus * 0.01f);

                    Settlement bound = village.Bound;
                    bool flag;
                    if (bound == null)
                        flag = (null != null);
                    else
                    {
                        Town town = bound.Town;
                        flag = (((town != null) ? town.Governor : null) != null);
                    }
                    if (flag && village.Bound.Town.Governor.GetPerkValue(DefaultPerks.Steward.Logistician))
                        total += MathF.Round((float)total * DefaultPerks.Steward.Logistician.SecondaryBonus * 0.01f);

                    if (applyWithdrawals)
                        village.TradeTaxAccumulated -= num2;

                    if (clan.Kingdom != null && clan.Kingdom.RulingClan == clan && clan.Kingdom.ActivePolicies.Contains(DefaultPolicies.LandTax))
                    {
                        if (!village.IsOwnerUnassigned && village.Settlement.OwnerClan != clan)
                        {
                            int policyTotal = (village.VillageState == Village.VillageStates.Looted || village.VillageState == Village.VillageStates.BeingRaided) ? 0 : ((int)((float)village.TradeTaxAccumulated / 5f));
                            total += (int)((float)policyTotal * 0.05f);
                        }
                    }

                    return total;
                }
            }


            [HarmonyPatch(typeof(CaravansCampaignBehavior), "GetTradeScoreForTown")]
            class GetTradeScoreForTownPatch
            {
                static void Postfix(ref float __result, MobileParty caravanParty, Town town, CampaignTime lastHomeVisitTimeOfCaravan, 
                    float caravanFullness, bool distanceCut)
                {
                    if (BannerKingsConfig.Instance.PopulationManager != null && BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(town.Settlement))
                    {
                        PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(town.Settlement);
                        __result *= data.EconomicData.CaravanAttraction.ResultNumber;
                    }
                }
            }


            [HarmonyPatch(typeof(SellGoodsForTradeAction), "ApplyInternal")]
            class SellGoodsPatch
            {
                static bool Prefix(object[] __args)
                {
                    Settlement settlement = (Settlement)__args[0];
                    MobileParty mobileParty = (MobileParty)__args[1];
                    if (settlement != null && mobileParty != null)
                        if (settlement.IsTown && mobileParty.IsVillager)
                        {
                            Town component = settlement.GetComponent<Town>();
                            List<ValueTuple<ItemObject, int>> list = new List<ValueTuple<ItemObject, int>>();
                            int num = 10000;
                            ItemObject itemObject = null;
                            for (int i = 0; i < mobileParty.ItemRoster.Count; i++)
                            {
                                ItemRosterElement elementCopyAtIndex = mobileParty.ItemRoster.GetElementCopyAtIndex(i);
                                if (elementCopyAtIndex.EquipmentElement.Item.ItemCategory == DefaultItemCategories.PackAnimal && elementCopyAtIndex.EquipmentElement.Item.Value < num)
                                {
                                    num = elementCopyAtIndex.EquipmentElement.Item.Value;
                                    itemObject = elementCopyAtIndex.EquipmentElement.Item;
                                }
                            }
                            for (int j = mobileParty.ItemRoster.Count - 1; j >= 0; j--)
                            {
                                ItemRosterElement elementCopyAtIndex2 = mobileParty.ItemRoster.GetElementCopyAtIndex(j);
                                int itemPrice = component.GetItemPrice(elementCopyAtIndex2.EquipmentElement, mobileParty, true);
                                int num2 = mobileParty.ItemRoster.GetElementNumber(j);
                                if (elementCopyAtIndex2.EquipmentElement.Item == itemObject)
                                {
                                    int num3 = (int)(0.5f * (float)mobileParty.MemberRoster.TotalManCount) + 2;
                                    num2 -= num3;

                                    if (itemObject.StringId == "mule")
                                        num2 -= (int)(mobileParty.MemberRoster.TotalManCount * 0.1f);
                                }
                                if (num2 > 0)
                                {
                                    int num4 = MathF.Min(num2, component.Gold / itemPrice);
                                    if (num4 > 0)
                                    {
                                        mobileParty.PartyTradeGold += num4 * itemPrice;
                                        EquipmentElement equipmentElement = elementCopyAtIndex2.EquipmentElement;
                                        component.ChangeGold(-num4 * itemPrice);
                                        settlement.ItemRoster.AddToCounts(equipmentElement, num4);
                                        mobileParty.ItemRoster.AddToCounts(equipmentElement, -num4);
                                        list.Add(new ValueTuple<ItemObject, int>(equipmentElement.Item, num4));
                                    }
                                }
                            }

                            if (!list.IsEmpty<ValueTuple<ItemObject, int>>())
                                CampaignEventDispatcher.Instance.OnCaravanTransactionCompleted(mobileParty, component, list);

                            return false;
                        }

                    return true;
                }
            }


            //Mules
            [HarmonyPatch(typeof(VillagerCampaignBehavior), "MoveItemsToVillagerParty")]
            class VillagerMoveItemsPatch
            {
                static bool Prefix(Village village, MobileParty villagerParty)
                {
                    ItemObject mule = MBObjectManager.Instance.GetObject<ItemObject>(x => x.StringId == "mule");
                    int muleCount = (int)((float)villagerParty.MemberRoster.TotalManCount * 0.1f);
                    villagerParty.ItemRoster.AddToCounts(mule, muleCount);
                    ItemRoster itemRoster = village.Settlement.ItemRoster;
                    float num = (float)villagerParty.InventoryCapacity - villagerParty.ItemRoster.TotalWeight;
                    for (int i = 0; i < 4; i++)
                    {
                        foreach (ItemRosterElement itemRosterElement in itemRoster)
                        {
                            ItemObject item = itemRosterElement.EquipmentElement.Item;
                            int num2 = MBRandom.RoundRandomized((float)itemRosterElement.Amount * 0.2f);
                            if (num2 > 0)
                            {
                                if (!item.HasHorseComponent && item.Weight * (float)num2 > num)
                                {
                                    num2 = MathF.Ceiling(num / item.Weight);
                                    if (num2 <= 0)
                                    {
                                        continue;
                                    }
                                }
                                if (!item.HasHorseComponent)
                                {
                                    num -= (float)num2 * item.Weight;
                                }
                                villagerParty.Party.ItemRoster.AddToCounts(itemRosterElement.EquipmentElement, num2);
                                itemRoster.AddToCounts(itemRosterElement.EquipmentElement, -num2);
                            }
                        }
                    }
                    return false;
                }
             }


            //Add gold to village and consume some of it, do not reset gold
            [HarmonyPatch(typeof(VillagerCampaignBehavior), "OnSettlementEntered")]
            class VillagerSettlementEnterPatch
            {
                static bool Prefix(ref Dictionary<MobileParty, List<Settlement>> ____previouslyChangedVillagerTargetsDueToEnemyOnWay, MobileParty mobileParty, Settlement settlement, Hero hero)
                {
                    if (BannerKingsConfig.Instance.PopulationManager != null && BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(settlement))
                    {

                        if (mobileParty != null && mobileParty.IsActive && mobileParty.IsVillager)
                        {
                            ____previouslyChangedVillagerTargetsDueToEnemyOnWay[mobileParty].Clear();
                            if (settlement.IsTown)
                                SellGoodsForTradeAction.ApplyByVillagerTrade(settlement, mobileParty);

                            if (settlement.IsVillage)
                            {
                                int tax = Campaign.Current.Models.SettlementTaxModel.CalculateVillageTaxFromIncome(mobileParty.HomeSettlement.Village, mobileParty.PartyTradeGold);
                                float remainder = mobileParty.PartyTradeGold - tax;
                                mobileParty.HomeSettlement.Village.ChangeGold((int)(remainder * 0.5f));
                                mobileParty.PartyTradeGold = 0;
                                mobileParty.HomeSettlement.Village.TradeTaxAccumulated += tax;
                            }
                            if (settlement.IsTown && settlement.Town.Governor != null && settlement.Town.Governor.GetPerkValue(DefaultPerks.Trade.DistributedGoods))
                                settlement.Town.TradeTaxAccumulated += MathF.Round(DefaultPerks.Trade.DistributedGoods.SecondaryBonus);
                        }
                        return false;
                    }
                    else return true;
                }
            }


            // Impact prosperity
            [HarmonyPatch(typeof(ChangeOwnerOfWorkshopAction), "ApplyInternal")]
            class BankruptcyPatch
            {
                static void Postfix(Workshop workshop, Hero newOwner, WorkshopType workshopType, int capital, bool upgradable, int cost, TextObject customName, ChangeOwnerOfWorkshopAction.ChangeOwnerOfWorkshopDetail detail)
                {
                    Settlement settlement = workshop.Settlement;
                    settlement.Prosperity -= 50f;
                }
            }

            // Added productions
            [HarmonyPatch(typeof(VillageGoodProductionCampaignBehavior), "TickGoodProduction")]
            class TickGoodProductionPatch
            {
                static bool Prefix(Village village, bool initialProductionForTowns)
                {

                    if (BannerKingsConfig.Instance.PopulationManager != null && BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(village.Settlement))
                    {

                        List<(ItemObject, float)> productions = BannerKingsConfig.Instance.PopulationManager.GetProductions(BannerKingsConfig.Instance.PopulationManager.GetPopData(village.Settlement).VillageData);
                        foreach (ValueTuple<ItemObject, float> valueTuple in productions)
                        {
                            ItemObject item = valueTuple.Item1;
                            int num = MBRandom.RoundRandomized(Campaign.Current.Models.VillageProductionCalculatorModel.CalculateDailyProductionAmount(village, valueTuple.Item1));
                            if (num > 0)
                            {
                                if (!initialProductionForTowns)
                                {
                                    village.Owner.ItemRoster.AddToCounts(item, num);
                                    CampaignEventDispatcher.Instance.OnItemProduced(item, village.Owner.Settlement, num);
                                }
                                else
                                    village.TradeBound.ItemRoster.AddToCounts(item, num);

                            }
                        }
                        return false;
                    }
                    else return true; 
                }
            }


            // Retain behavior of original while updating satisfaction parameters
            [HarmonyPatch(typeof(ItemConsumptionBehavior), "MakeConsumption")]
            class ItemConsumptionPatch
            {
                static bool Prefix(Town town, Dictionary<ItemCategory, float> categoryDemand, Dictionary<ItemCategory, int> saleLog)
                {
                    if (BannerKingsConfig.Instance.PopulationManager != null && BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(town.Settlement))
                    {
                        saleLog.Clear();
                        TownMarketData marketData = town.MarketData;
                        ItemRoster itemRoster = town.Owner.ItemRoster;
                        PopulationData popData = BannerKingsConfig.Instance.PopulationManager.GetPopData(town.Settlement);
                        for (int i = itemRoster.Count - 1; i >= 0; i--)
                        {
                            ItemRosterElement elementCopyAtIndex = itemRoster.GetElementCopyAtIndex(i);
                            ItemObject item = elementCopyAtIndex.EquipmentElement.Item;
                            int amount = elementCopyAtIndex.Amount;
                            ItemCategory itemCategory = item.GetItemCategory();
                            float demand = categoryDemand[itemCategory];

                            IEnumerable<ItemConsumptionBehavior> behaviors = Campaign.Current.GetCampaignBehaviors<ItemConsumptionBehavior>();
                            MethodInfo dynMethod = behaviors.First().GetType().GetMethod("CalculateBudget", BindingFlags.NonPublic | BindingFlags.Static);
                            float num = (float)dynMethod.Invoke(null, new object[] { town, demand, itemCategory });
                            if (num > 0.01f)
                            {
                                int price = marketData.GetPrice(item, null, false, null);
                                float desiredAmount = num / (float)price;
                                if (desiredAmount > (float)amount)
                                    desiredAmount = (float)amount;


                                if (item.IsFood && town.FoodStocks <= (float)town.FoodStocksUpperLimit() * 0.1f)
                                {
                                    float requiredFood = town.FoodChange * -1f;
                                    if (amount > requiredFood)
                                        desiredAmount += requiredFood + 1f;
                                    else desiredAmount += amount;
                                }

                                int finalAmount = MBRandom.RoundRandomized(desiredAmount);
                                ConsumptionType type = Utils.Helpers.GetTradeGoodConsumptionType(item);
                                if (finalAmount > amount)
                                {
                                    finalAmount = amount;
                                    if (type != ConsumptionType.None) popData.EconomicData.UpdateSatisfaction(type, -0.0015f);
                                }
                                else if (type != ConsumptionType.None) popData.EconomicData.UpdateSatisfaction(type, 0.001f);
                                
                                itemRoster.AddToCounts(elementCopyAtIndex.EquipmentElement, -finalAmount);
                                categoryDemand[itemCategory] = num - desiredAmount * (float)price;
                                town.ChangeGold(finalAmount * price);
                                int num4 = 0;
                                saleLog.TryGetValue(itemCategory, out num4);
                                saleLog[itemCategory] = num4 + finalAmount;
                            }
                        }

                        if (town.FoodStocks <= (float)town.FoodStocksUpperLimit() * 0.05f && town.Settlement.Stash != null)
                        {
                            List<ItemRosterElement> elements = new List<ItemRosterElement>();
                            foreach (ItemRosterElement element in town.Settlement.Stash)
                                if (element.EquipmentElement.Item != null && element.EquipmentElement.Item.ItemCategory.Properties == ItemCategory.Property.BonusToFoodStores)
                                    elements.Add(element);

                            foreach (ItemRosterElement element in elements)
                            {
                                ItemCategory category = element.EquipmentElement.Item.ItemCategory;
                                if (saleLog.ContainsKey(category))
                                    saleLog[category] += element.Amount;
                                else saleLog.Add(category, element.Amount);
                                town.Settlement.Stash.Remove(element);
                            }
                        }


                        List<Town.SellLog> list = new List<Town.SellLog>();
                        foreach (KeyValuePair<ItemCategory, int> keyValuePair in saleLog)
                            if (keyValuePair.Value > 0)
                                list.Add(new Town.SellLog(keyValuePair.Key, keyValuePair.Value));
 
                        town.SetSoldItems(list);
                        return false;
                    }
                    else return true;
                }
            }
        }     
    }
}
