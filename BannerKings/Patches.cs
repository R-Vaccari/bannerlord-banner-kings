using System.Collections.Generic;
using System.Linq;
using BannerKings.Managers.Helpers;
using BannerKings.Managers.Institutions.Religions;
using BannerKings.Managers.Skills;
using BannerKings.Managers.Titles;
using BannerKings.Managers.Titles.Laws;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Issues;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.Policies;
using TaleWorlds.Core;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.GauntletUI.GamepadNavigation;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using static TaleWorlds.CampaignSystem.Election.KingSelectionKingdomDecision;
using static TaleWorlds.CampaignSystem.Issues.CaravanAmbushIssueBehavior;
using static TaleWorlds.CampaignSystem.Issues.EscortMerchantCaravanIssueBehavior;
using static TaleWorlds.CampaignSystem.Issues.LandLordNeedsManualLaborersIssueBehavior;
using static TaleWorlds.CampaignSystem.Issues.VillageNeedsToolsIssueBehavior;

namespace BannerKings.Patches
{
    namespace Recruitment
    {
        [HarmonyPatch(typeof(RecruitmentCampaignBehavior))]
        internal class RecruitmentApplyInternalPatch
        {
            [HarmonyPostfix]
            [HarmonyPatch("ApplyInternal", MethodType.Normal)]
            private static void ApplyInternalPostfix(MobileParty side1Party, Settlement settlement, Hero individual,
                CharacterObject troop, int number, int bitCode, RecruitmentCampaignBehavior.RecruitingDetail detail)
            {
                if (settlement == null)
                {
                    return;
                }

                var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
                if (data != null)
                {
                    data.MilitaryData.DeduceManpower(data, number, troop, individual);
                }
            }

            [HarmonyPrefix]
            [HarmonyPatch("UpdateVolunteersOfNotablesInSettlement", MethodType.Normal)]
            private static bool UpdateVolunteersPrefix(Settlement settlement)
            {
                if ((settlement.Town != null && !settlement.Town.InRebelliousState && settlement.Notables != null) || 
                    (settlement.IsVillage && !settlement.Village.Bound.Town.InRebelliousState))
                {
                    var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
                    if (data == null)
                    {
                        return true;
                    }

                    foreach (Hero hero in settlement.Notables)
                    {
                        if (hero.CanHaveRecruits)
                        {
                            bool flag = false;
                            CharacterObject basicVolunteer = Campaign.Current.Models.VolunteerModel.GetBasicVolunteer(hero);
                            if (data.MilitaryData.GetNotableManpower(data.MilitaryData.GetCharacterManpowerType(basicVolunteer),
                                hero, data.EstateData) < 1f)
                            {
                                continue;
                            }

                            for (int i = 0; i < hero.VolunteerTypes.Length; i++)
                            {
                                if (MBRandom.RandomFloat < Campaign.Current.Models.VolunteerModel.GetDailyVolunteerProductionProbability(hero, i, settlement))
                                {
                                    CharacterObject characterObject = hero.VolunteerTypes[i];
                                    if (characterObject == null)
                                    {
                                        hero.VolunteerTypes[i] = basicVolunteer;
                                        flag = true;
                                    }
                                    else if (characterObject.UpgradeTargets != null && characterObject.UpgradeTargets.Length != 0 && characterObject.Tier <= 3)
                                    {
                                        float num = MathF.Log(hero.Power / (float)characterObject.Tier, 2f) * 0.01f;
                                        if (MBRandom.RandomFloat < num)
                                        {
                                            hero.VolunteerTypes[i] = characterObject.UpgradeTargets[MBRandom.RandomInt(characterObject.UpgradeTargets.Length)];
                                            flag = true;
                                        }
                                    }
                                }
                            }
                            if (flag)
                            {
                                CharacterObject[] volunteerTypes = hero.VolunteerTypes;
                                for (int j = 1; j < volunteerTypes.Length; j++)
                                {
                                    CharacterObject characterObject2 = volunteerTypes[j];
                                    if (characterObject2 != null)
                                    {
                                        int num2 = 0;
                                        int num3 = j - 1;
                                        CharacterObject characterObject3 = volunteerTypes[num3];
                                        while (num3 >= 0 && (characterObject3 == null || (float)characterObject2.Level + (characterObject2.IsMounted ? 0.5f : 0f) < (float)characterObject3.Level + (characterObject3.IsMounted ? 0.5f : 0f)))
                                        {
                                            if (characterObject3 == null)
                                            {
                                                num3--;
                                                num2++;
                                                if (num3 >= 0)
                                                {
                                                    characterObject3 = volunteerTypes[num3];
                                                }
                                            }
                                            else
                                            {
                                                volunteerTypes[num3 + 1 + num2] = characterObject3;
                                                num3--;
                                                num2 = 0;
                                                if (num3 >= 0)
                                                {
                                                    characterObject3 = volunteerTypes[num3];
                                                }
                                            }
                                        }
                                        volunteerTypes[num3 + 1 + num2] = characterObject2;
                                    }
                                }
                            }
                        }
                    }
                }

                return false;
            }
        }

        [HarmonyPatch(typeof(DefaultVolunteerModel), "GetDailyVolunteerProductionProbability")]
        internal class GetDailyVolunteerProductionProbabilityPatch
        {
            private static bool Prefix(Hero hero, int index, Settlement settlement, ref float __result)
            {
                __result = BannerKingsConfig.Instance.VolunteerModel.GetDraftEfficiency(hero, index, settlement).ResultNumber;
                return false;
            }
        }
    }

    namespace Peerage
    {
        [HarmonyPatch(typeof(KingdomDecision))]
        internal class DetermineSupportersPatch
        {
            [HarmonyPrefix]
            [HarmonyPatch("DetermineSupporters")]
            private static bool DetermineSupportersPrefix(KingdomDecision __instance, ref IEnumerable<Supporter> __result)
            {
                var list = new List<Supporter>();
                foreach (Clan clan in __instance.Kingdom.Clans)
                {
                    var council = BannerKingsConfig.Instance.CourtManager.GetCouncil(clan);
                    if (council != null && council.Peerage != null)
                    {
                        if (council.Peerage.CanVote)
                        {
                            list.Add(new Supporter(clan));
                        }
                    }
                }

                __result = list;
                return false;
            }

            [HarmonyPrefix]
            [HarmonyPatch("IsPlayerParticipant", MethodType.Getter)]
            private static bool IsPlayerParticipantPrefix(KingdomDecision __instance, ref bool __result)
            {
                var council = BannerKingsConfig.Instance.CourtManager.GetCouncil(Clan.PlayerClan);
                __result = __instance.Kingdom == Clan.PlayerClan.Kingdom && !Clan.PlayerClan.IsUnderMercenaryService &&
                    council.Peerage != null && council.Peerage.CanVote;
                return false;
            }
        }

        [HarmonyPatch(typeof(KingdomPoliciesVM), "GetCanProposeOrDisavowPolicyWithReason")]
        internal class GetCanProposeOrDisavowPolicyWithReasonPatch
        {
            private static bool Prefix(KingdomPoliciesVM __instance, bool hasUnresolvedDecision, ref bool __result, out TextObject disabledReason)
            {
                TextObject textObject;
                if (!CampaignUIHelper.GetMapScreenActionIsEnabledWithReason(out textObject))
                {
                    disabledReason = textObject;
                    __result = false;
                    return false;
                }
                if (Clan.PlayerClan.IsUnderMercenaryService)
                {
                    disabledReason = GameTexts.FindText("str_mercenaries_cannot_propose_policies", null);
                    __result = false;
                    return false;
                }
                if (!hasUnresolvedDecision && Clan.PlayerClan.Influence < (float)__instance.ProposalAndDisavowalCost)
                {
                    disabledReason = GameTexts.FindText("str_warning_you_dont_have_enough_influence", null);
                    __result = false;
                    return false;
                }

                var council = BannerKingsConfig.Instance.CourtManager.GetCouncil(Clan.PlayerClan);
                if (council != null)
                {
                    if (council.Peerage == null || (council.Peerage != null && !council.Peerage.CanStartElection))
                    {
                        disabledReason = new TextObject("{=RDDOdoeR}The Peerage of {CLAN} does not allow starting elections.")
                            .SetTextVariable("CLAN", Clan.PlayerClan.Name);
                        __result = false;
                        return false;
                    }
                }

                 disabledReason = TextObject.Empty;
                __result = true;
                return false;
            }
        }
    }

    namespace Armies
    {
        [HarmonyPatch(typeof(Army), "UpdateName")]
        internal class ArmyUpdateNamePatch
        {
            private static bool Prefix(Army __instance)
            {
                FeudalTitle title = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(__instance.Kingdom);
                TextObject leaderName = __instance.ArmyOwner != null ? 
                    __instance.ArmyOwner.Name : ((__instance.LeaderParty.PartyComponent.PartyOwner != null) ?
                    __instance.LeaderParty.PartyComponent.PartyOwner.Name : TextObject.Empty);
                TextObject result = new TextObject("{=nbmctMLk}{LEADER_NAME}{.o} Army");
                if (title != null)
                {
                    if (title.Contract.IsLawEnacted(DefaultDemesneLaws.Instance.ArmyHorde))
                    {
                        result = new TextObject("{=!}{LEADER_NAME}{.o} Horde");
                    }
                    else if (title.Contract.IsLawEnacted(DefaultDemesneLaws.Instance.ArmyLegion))
                    {
                        result = new TextObject("{=!}{LEADER_NAME}{.o} Legion");
                    }
                }

                AccessTools.Property(__instance.GetType(), "Name").SetValue(__instance,
                    result.SetTextVariable("LEADER_NAME", leaderName));
                return false;
            }
        }
    }

    namespace Perks
    {

        [HarmonyPatch(typeof(MapEventParty), "ContributionToBattle", MethodType.Getter)]
        internal class ContributionToBattlePatch
        {
            private static void Postfix(MapEventParty __instance, ref int __result)
            {
                var leader = __instance.Party.LeaderHero;
                if (leader == null)
                {
                    return;
                }

                var education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(leader);
                if (education.HasPerk(BKPerks.Instance.MercenaryRansacker))
                {
                    __result = (int)(__result * 1.1f);
                }
            }
        }
    }

    namespace Fixes
    {
        // Fix crash on wanderer same gender child born
        [HarmonyPatch(typeof(NameGenerator), "GenerateHeroFullName")]
        internal class NameGeneratorPatch
        {
            private static bool Prefix(ref TextObject __result, Hero hero, TextObject heroFirstName,
                bool useDeterministicValues = true)
            {
                var parent = hero.IsFemale ? hero.Mother : hero.Father;
                if (parent == null)
                {
                    return true;
                }

                if (BannerKingsConfig.Instance.TitleManager.IsHeroKnighted(parent) && hero.IsWanderer)
                {
                    var textObject = heroFirstName;
                    textObject.SetTextVariable("FEMALE", hero.IsFemale ? 1 : 0);
                    textObject.SetTextVariable("IMPERIAL", hero.Culture.StringId == "empire" ? 1 : 0);
                    textObject.SetTextVariable("COASTAL",
                        hero.Culture.StringId is "empire" or "vlandia" ? 1 : 0);
                    textObject.SetTextVariable("NORTHERN",
                        hero.Culture.StringId is "battania" or "sturgia" ? 1 : 0);
                    textObject.SetCharacterProperties("HERO", hero.CharacterObject);
                    textObject.SetTextVariable("FIRSTNAME", heroFirstName);
                    __result = textObject;
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(GauntletGamepadNavigationManager), "OnWidgetNavigationStatusChanged")]
        internal class NavigationPatch
        {
            private static bool Prefix(GauntletGamepadNavigationManager __instance, EventManager source, Widget widget)
            {

                return false;
            }
        }

        [HarmonyPatch(typeof(Hero), nameof(Hero.CanHaveQuestsOrIssues))]
        internal class CanHaveQuestsOrIssuesPatch
        {
            private static bool Prefix(Hero __instance, ref bool __result)
            {
                if (__instance.Issue != null)
                {
                    return false;
                }

                __result = __instance.IsActive && __instance.IsAlive;
                CampaignEventDispatcher.Instance.CanHaveQuestsOrIssues(__instance, ref __result);

                return false;
            }
        }

        [HarmonyPatch(typeof(VillageNeedsToolsIssue), "IssueStayAliveConditions")]
        internal class VillageIssueStayAliveConditionsPatch
        {
            private static bool Prefix(CaravanAmbushIssue __instance, ref bool __result)
            {
                if (__instance.IssueOwner != null)
                {
                    if (__instance.IssueOwner.CurrentSettlement == null ||
                        !__instance.IssueOwner.CurrentSettlement.IsVillage)
                    {
                        __result = false;
                        return false;
                    }
                }
                else
                {
                    __result = false;
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(CaravanAmbushIssue), "IssueStayAliveConditions")]
        internal class CaravanIssueStayAliveConditionsPatch
        {
            private static bool Prefix(CaravanAmbushIssue __instance, ref bool __result)
            {
                if (__instance.IssueOwner != null)
                {
                    if (__instance.IssueOwner.OwnedCaravans == null || __instance.IssueOwner.MapFaction == null)
                    {
                        __result = false;
                        return false;
                    }
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(LandLordNeedsManualLaborersIssue), "IssueStayAliveConditions")]
        internal class LaborersIssueStayAliveConditionsPatch
        {
            private static bool Prefix(LandLordNeedsManualLaborersIssue __instance, ref bool __result)
            {
                if (__instance.IssueOwner != null)
                {
                    if (__instance.IssueOwner.CurrentSettlement == null ||
                        !__instance.IssueOwner.CurrentSettlement.IsVillage)
                    {
                        __result = false;
                        return false;
                    }
                }
                else
                {
                    __result = false;
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(EscortMerchantCaravanIssueBehavior), "ConditionsHold")]
        internal class EscortCaravanConditionsHoldPatch
        {
            private static bool Prefix(Hero issueGiver, ref bool __result)
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
        internal class EscortCaravanIssueStayAliveConditionsPatch
        {
            private static bool Prefix(EscortMerchantCaravanIssue __instance, ref bool __result)
            {
                if (__instance.IssueOwner.CurrentSettlement == null ||
                    __instance.IssueOwner.CurrentSettlement.IsVillage)
                {
                    __result = false;
                    return false;
                }

                return true;
            }
        }


        [HarmonyPatch(typeof(DefaultPartyMoraleModel), "CalculateFoodVarietyMoraleBonus")]
        internal class CalculateFoodVarietyMoraleBonusPatch
        {
            private static bool Prefix(MobileParty party, ref ExplainedNumber result)
            {
                var num = MBMath.ClampFloat(party.ItemRoster.FoodVariety - 5f, -5f, 5f);
                if (num != 0f && (num >= 0f || party.LeaderHero == null ||
                                    !party.LeaderHero.GetPerkValue(DefaultPerks.Steward.WarriorsDiet)))
                {
                    if (num > 0f && party.HasPerk(DefaultPerks.Steward.Gourmet))
                    {
                        result.Add(num * DefaultPerks.Steward.Gourmet.PrimaryBonus,
                            DefaultPerks.Steward.Gourmet.Name);
                        return false;
                    }

                    result.Add(num, GameTexts.FindText("str_food_bonus_morale"));
                }

                var totalModifiers = 0f;
                var modifierRate = 0f;
                foreach (var element in party.ItemRoster.ToList().FindAll(x => x.EquipmentElement.Item.IsFood))
                {
                    var modifier = element.EquipmentElement.ItemModifier;
                    if (modifier != null)
                    {
                        totalModifiers++;
                        modifierRate += modifier.PriceMultiplier;
                    }
                }

                if (modifierRate != 0f)
                {
                    result.Add(MBMath.ClampFloat(modifierRate / totalModifiers, -5f, 5f),
                        new TextObject("{=oy3mdLFG}Food quality"));
                }

                return false;
            }
        }
    }


    namespace Government
    {
        [HarmonyPatch(typeof(KingdomPolicyDecision), "IsAllowed")]
        internal class PolicyIsAllowedPatch
        {
            private static bool Prefix(ref bool __result, KingdomPolicyDecision __instance)
            {
                if (BannerKingsConfig.Instance.TitleManager != null)
                {
                    var sovereign = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(__instance.Kingdom);
                    if (sovereign != null)
                    {
                        __result = !PolicyHelper.GetForbiddenGovernmentPolicies(sovereign.Contract.Government)
                            .Contains(__instance.Policy);
                        return false;
                    }
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(KingSelectionKingdomDecision))]
        internal class KingSelectionKingdomDecisionPatches
        {
            [HarmonyPostfix]
            [HarmonyPatch("ApplyChosenOutcome", MethodType.Normal)]
            private static void ApplyChosenOutcomePostfix(KingSelectionKingdomDecision __instance, DecisionOutcome chosenOutcome)
            {
                var title = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(__instance.Kingdom);
                if (title != null)
                {
                    var deJure = title.deJure;
                    var king = ((KingSelectionDecisionOutcome) chosenOutcome).King;
                    if (deJure != king)
                    {
                        BannerKingsConfig.Instance.TitleManager.InheritTitle(deJure, king, title);
                    }
                }
            }

            [HarmonyPrefix]
            [HarmonyPatch("CalculateMeritOfOutcomeForClan", MethodType.Normal)]
            private static bool CalculateMeritOfOutcomeForClanPrefix(KingSelectionKingdomDecision __instance, Clan clan, 
                DecisionOutcome candidateOutcome, ref float __result)
            {
                var title = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(__instance.Kingdom);
                if (title != null)
                {
                    Hero king = ((KingSelectionDecisionOutcome)candidateOutcome).King;
                    __result = BannerKingsConfig.Instance.TitleModel.GetSuccessionHeirScore(king, clan.Leader, title).ResultNumber;
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(SettlementClaimantDecision))]
        internal class FiefOwnerPatches
        {
            [HarmonyPrefix]
            [HarmonyPatch("DetermineInitialCandidates")]
            private static bool DetermineInitialCandidatesPrefix(SettlementClaimantDecision __instance,
                ref IEnumerable<DecisionOutcome> __result)
            {
                Kingdom kingdom = (Kingdom)__instance.Settlement.MapFaction;
                List<SettlementClaimantDecision.ClanAsDecisionOutcome> list = new List<SettlementClaimantDecision.ClanAsDecisionOutcome>();
                foreach (Clan clan in kingdom.Clans)
                {
                    if (clan != __instance.ClanToExclude && !clan.IsUnderMercenaryService && !clan.IsEliminated && !clan.Leader.IsDead)
                    {
                        var peerage = BannerKingsConfig.Instance.CourtManager.GetCouncil(clan).Peerage;
                        if (peerage == null || !peerage.CanHaveFief) continue;

                        list.Add(new SettlementClaimantDecision.ClanAsDecisionOutcome(clan));
                    }
                }
                __result = list;
                return false;
            }

            [HarmonyPostfix]
            [HarmonyPatch("CalculateMeritOfOutcome")]
            private static void CalculateMeritOfOutcomePostfix(SettlementClaimantDecision __instance,
               DecisionOutcome candidateOutcome, ref float __result)
            {
                if (BannerKingsConfig.Instance.TitleManager != null)
                {
                    __result *= 100f;
                    SettlementClaimantDecision.ClanAsDecisionOutcome clanAsDecisionOutcome = (SettlementClaimantDecision.ClanAsDecisionOutcome)candidateOutcome;
                    Clan clan = clanAsDecisionOutcome.Clan;

                    if (BannerKingsConfig.Instance.ReligionsManager.HasBlessing(clan.Leader, DefaultDivinities.Instance.AseraMain))
                    {
                        __result *= 0.2f;
                    }

                    var limit = BannerKingsConfig.Instance.StabilityModel.CalculateDemesneLimit(clan.Leader).ResultNumber;
                    var current = BannerKingsConfig.Instance.StabilityModel.CalculateCurrentDemesne(clan).ResultNumber;
                    float factor = current / limit;
                    __result *= 1f - factor;
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch("ShouldBeCancelledInternal")]
            private static void ShouldBeCancelledInternalPostfix(SettlementClaimantDecision __instance, ref bool __result)
            {
                if (!__instance.Settlement.Town.IsOwnerUnassigned)
                {
                    __result = true;
                }
            }
        }
    }
}