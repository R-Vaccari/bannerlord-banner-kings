using System.Collections.Generic;
using System.Linq;
using BannerKings.Behaviours.Diplomacy;
using BannerKings.Behaviours.Diplomacy.Groups;
using BannerKings.Managers.Skills;
using BannerKings.Managers.Titles;
using BannerKings.Managers.Titles.Laws;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.KingdomManagement.Policies;
using TaleWorlds.Core;
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

            /*[HarmonyPrefix]
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
                            CharacterObject basicVolunteer = TaleWorlds.CampaignSystem.Campaign.Current.Models.VolunteerModel.GetBasicVolunteer(hero);

                            for (int i = 0; i < hero.VolunteerTypes.Length; i++)
                            {
                                if (MBRandom.RandomFloat < TaleWorlds.CampaignSystem.Campaign.Current.Models.VolunteerModel.GetDailyVolunteerProductionProbability(hero, i, settlement))
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

            [HarmonyPrefix]
            [HarmonyPatch("RecruitVolunteersFromNotable", MethodType.Normal)]
            private static bool RecruitVolunteersFromNotablePrefix(RecruitmentCampaignBehavior __instance, MobileParty mobileParty, Settlement settlement)
            {
                if (mobileParty.ActualClan != null && mobileParty.ActualClan.IsClanTypeMercenary)
                {
                    Console.Write("");
                }

                if (((float)mobileParty.Party.NumberOfAllMembers + 0.5f) / (float)mobileParty.LimitedPartySize <= 1f)
                {
                    foreach (Hero hero in settlement.Notables)
                    {
                        if (hero.IsAlive)
                        {
                            if (mobileParty.IsWageLimitExceeded())
                            {
                                break;
                            }
                            int num = MBRandom.RandomInt(6);
                            int num2 = Campaign.Current.Models.VolunteerModel.MaximumIndexHeroCanRecruitFromHero(mobileParty.IsGarrison ? mobileParty.Party.Owner : mobileParty.LeaderHero, hero, -101);
                            for (int i = num; i < num + 6; i++)
                            {
                                int num3 = i % 6;
                                if (num3 >= num2)
                                {
                                    break;
                                }
                                int num4 = (mobileParty.LeaderHero != null) ? ((int)MathF.Sqrt((float)mobileParty.LeaderHero.Gold / 10000f)) : 0;
                                float num5 = MBRandom.RandomFloat;
                                for (int j = 0; j < num4; j++)
                                {
                                    float randomFloat = MBRandom.RandomFloat;
                                    if (randomFloat > num5)
                                    {
                                        num5 = randomFloat;
                                    }
                                }
                                if (mobileParty.Army != null)
                                {
                                    float y = (mobileParty.Army.LeaderParty == mobileParty) ? 0.5f : 0.67f;
                                    num5 = MathF.Pow(num5, y);
                                }
                                float num6 = (float)mobileParty.Party.NumberOfAllMembers / (float)mobileParty.LimitedPartySize;
                                if (num5 > num6 - 0.1f)
                                {
                                    CharacterObject characterObject = hero.VolunteerTypes[num3];
                                    if (characterObject != null && mobileParty.LeaderHero.Gold > Campaign.Current.Models.PartyWageModel.GetTroopRecruitmentCost(characterObject, mobileParty.LeaderHero, false) && mobileParty.PaymentLimit >= mobileParty.TotalWage + Campaign.Current.Models.PartyWageModel.GetCharacterWage(characterObject))
                                    {
                                        MethodInfo recruit = __instance.GetType().GetMethod("GetRecruitVolunteerFromIndividual", BindingFlags.NonPublic | BindingFlags.Instance);
                                        recruit.Invoke(__instance, new object[] { mobileParty, characterObject, hero, num3 });
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }

                return false;
            }*/
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
                    if (council != null && council.Peerage != null && !clan.IsUnderMercenaryService)
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
                        result = new TextObject("{=HCWYbPOa}{LEADER_NAME}{.o} Horde");
                    }
                    else if (title.Contract.IsLawEnacted(DefaultDemesneLaws.Instance.ArmyLegion))
                    {
                        result = new TextObject("{=4ubaOxe2}{LEADER_NAME}{.o} Legion");
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
       

        [HarmonyPatch(typeof(GauntletGamepadNavigationManager), "OnWidgetNavigationStatusChanged")]
        internal class NavigationPatch
        {
            private static bool Prefix(Widget widget)
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
        [HarmonyPatch(typeof(KingSelectionKingdomDecision))]
        internal class KingdomPolicyDecisionPatches
        {
            [HarmonyPostfix]
            [HarmonyPatch("IsAllowed", MethodType.Normal)]
            private static bool Prefix(ref bool __result, KingdomPolicyDecision __instance)
            {
                if (BannerKingsConfig.Instance.TitleManager != null)
                {
                    var sovereign = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(__instance.Kingdom);
                    if (sovereign != null)
                    {
                        __result = !sovereign.Contract.Government.ProhibitedPolicies.Contains(__instance.Policy);
                        return false;
                    }
                }

                return true;
            }

            [HarmonyPostfix]
            [HarmonyPatch("DetermineSupport", MethodType.Normal)]
            private static void OutcomeMeritPostfix(ref float __result, KingdomPolicyDecision __instance,
                Clan clan, DecisionOutcome possibleOutcome)
            {
                KingdomPolicyDecision.PolicyDecisionOutcome policyDecisionOutcome = 
                    possibleOutcome as KingdomPolicyDecision.PolicyDecisionOutcome;
                BKDiplomacyBehavior behavior = TaleWorlds.CampaignSystem.Campaign.Current.GetCampaignBehavior<BKDiplomacyBehavior>();
                KingdomDiplomacy diplomacy = behavior.GetKingdomDiplomacy(clan.Kingdom);
     
                if (diplomacy != null)
                {
                    InterestGroup group = diplomacy.GetHeroGroup(clan.Leader);
                    if (group != null)
                    {
                        bool neutral = true;
                        bool supports = false;
                        if (group.SupportedPolicies.Contains(__instance.Policy))
                        {
                            neutral = false;
                            supports = policyDecisionOutcome.ShouldDecisionBeEnforced;
                        }
                        else if (group.ShunnedPolicies.Contains(__instance.Policy))
                        {
                            neutral = false;
                            supports = !policyDecisionOutcome.ShouldDecisionBeEnforced;
                        }

                        if (!neutral) __result += supports ? 80f : -80;
                    }
                }
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

            [HarmonyPrefix]
            [HarmonyPatch("CalculateMeritOfOutcome")]
            private static bool CalculateMeritOfOutcomePrefix(SettlementClaimantDecision __instance,
               DecisionOutcome candidateOutcome, ref float __result)
            {
                SettlementClaimantDecision.ClanAsDecisionOutcome clanAsDecisionOutcome = (SettlementClaimantDecision.ClanAsDecisionOutcome)candidateOutcome;  
                Settlement s = __instance.Settlement;
                ExplainedNumber result = BannerKingsConfig.Instance.DiplomacyModel.CalculateHeroFiefScore(s,
                    clanAsDecisionOutcome.Clan.Leader);

                __result = result.ResultNumber;
                return false;
            }

            [HarmonyPrefix]
            [HarmonyPatch("IsAllowed")]
            private static bool IsAllowedPrefix(SettlementClaimantDecision __instance, ref bool __result)
            {
                __result = __instance.DetermineInitialCandidates().Count() > 2;
                return false;
            }

            /* [HarmonyPostfix]
             [HarmonyPatch("ShouldBeCancelledInternal")]
             private static void ShouldBeCancelledInternalPostfix(SettlementClaimantDecision __instance, ref bool __result)
             {
                 if (!__instance.Settlement.Town.IsOwnerUnassigned)
                 {
                     __result = true;
                 }
             }*/
        }
    }
}