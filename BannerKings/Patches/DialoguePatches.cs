using HarmonyLib;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.Localization;
using Helpers;
using System.Linq;
using TaleWorlds.CampaignSystem.Extensions;
using BannerKings.Models.Vanilla;

namespace BannerKings.Patches
{
    internal class DialoguePatches
    {
        [HarmonyPatch(typeof(VassalAndMercenaryOfferCampaignBehavior))]
        internal class OfferPatches
        {
            [HarmonyPrefix]
            [HarmonyPatch("CreateMercenaryOffer")]
            private static bool CreateMercenaryOfferPrefix(Kingdom kingdom)
            {
                if (BannerKingsConfig.Instance.DiplomacyModel.GetScoreOfKingdomToHireMercenary(kingdom, Clan.PlayerClan) >
                    BannerKingsConfig.Instance.DiplomacyModel.GetScoreOfKingdomToSackMercenary(kingdom, Clan.PlayerClan))
                    return true;

                return false;
            }
        }

        //AI companion dialogue fixes
        [HarmonyPatch(typeof(LordConversationsCampaignBehavior))]
        internal class LordDialoguePatches
        {
            [HarmonyPrefix]
            [HarmonyPatch("conversation_player_is_offering_mercenary_on_clickable_condition")]
            private static bool OfferMercenaryPrefix(ref bool __result, out TextObject hintText)
            {
                List<IFaction> list;
                List<IFaction> first;
                Kingdom kingdom = (Kingdom)Hero.OneToOneConversationHero.MapFaction;
                bool flag = FactionHelper.CanPlayerOfferMercenaryService(kingdom, out list, out first);
                hintText = new TextObject("", null);
                if (!flag)
                {
                    if (Clan.PlayerClan.Tier < Campaign.Current.Models.ClanTierModel.MercenaryEligibleTier)
                    {
                        hintText = new TextObject("{=kXcUbkEW}Your Clan Tier needs to be {TIER}", null);
                    }
                    else if (Hero.OneToOneConversationHero.GetRelationWithPlayer() < (float)Campaign.Current.Models.DiplomacyModel.MinimumRelationWithConversationCharacterToJoinKingdom)
                    {
                        hintText = new TextObject("{=S9yOQgb1}You need {RELATION} relation with {HERO.NAME}.", null);
                    }
                    else if (first.Intersect(list).Count<IFaction>() != list.Count)
                    {
                        hintText = new TextObject("{=5Que0iuJ}Your clan is at war with factions that {KINGDOM} is not hostile with.", null);
                    }
                    else if (Clan.PlayerClan.Fiefs.Count() > 1)
                    {
                        hintText = new TextObject("{=!}Clans that own multiple fiefs are not considered for mercenary service.", null);
                    }
                    else
                        hintText = new TextObject("{=S9yOQgb1}You need relations of {RELATION} with {RULER.NAME}.", null);
                    
                    hintText.SetTextVariable("TIER", Campaign.Current.Models.ClanTierModel.MercenaryEligibleTier);
                    hintText.SetTextVariable("RELATION", Campaign.Current.Models.DiplomacyModel.MinimumRelationWithConversationCharacterToJoinKingdom);
                    hintText.SetTextVariable("KINGDOM", Hero.OneToOneConversationHero.MapFaction.Name);
                    hintText.SetCharacterProperties("HERO", Hero.OneToOneConversationHero.CharacterObject, false);
                    hintText.SetCharacterProperties("RULER", Hero.OneToOneConversationHero.MapFaction.Leader.CharacterObject, false);
                    hintText.SetTextVariable("newline", "\n");
                }

                if (flag)
                {
                    float hire = BannerKingsConfig.Instance.DiplomacyModel.GetScoreOfKingdomToHireMercenary(kingdom, Clan.PlayerClan);
                    if (hire <= 0f)
                    {
                        ExplainedNumber hireExplained = default(ExplainedNumber);
                        if (BannerKingsConfig.Instance.DiplomacyModel is BKDiplomacyModel)
                        {
                            hireExplained = (BannerKingsConfig.Instance.DiplomacyModel as BKDiplomacyModel)
                                .KingdomRecruitMercenary(kingdom, Clan.PlayerClan, true);
                        }
                        hintText = new TextObject("{=!}{NAME} is not willing to work with you at this point.{EXPLANATION}")
                            .SetTextVariable("NAME", Hero.OneToOneConversationHero.CharacterObject.Name)
                            .SetTextVariable("EXPLANATION", hireExplained.Equals(default) ?
                            null
                            :
                            new TextObject("{=!}{newline}{newline}Explanation:{newline}{RESULT}")
                            .SetTextVariable("RESULT", hireExplained.GetExplanations()));
                        flag = false;
                    }
                }
                __result = flag;

                return false;
            }

            [HarmonyPrefix]
            [HarmonyPatch("conversation_player_ask_prisoners_on_consequence")]
            private static bool LordPrisonersPrefix()
            {
                List<Hero> list = new List<Hero>();
                foreach (TroopRosterElement troopRosterElement in Hero.OneToOneConversationHero.PartyBelongedTo.PrisonRoster.GetTroopRoster())
                {
                    if (troopRosterElement.Character.IsHero && troopRosterElement.Character.HeroObject.IsLord)
                    {
                        list.Add(troopRosterElement.Character.HeroObject);
                    }
                }
                ConversationSentence.SetObjectsToRepeatOver(list, 5);

                return false;
            }

            [HarmonyPrefix]
            [HarmonyPatch("conversation_wanderer_meet_player_on_condition")]
            private static bool MeetCompanionPrefix(ref bool __result)
            {
                __result = CharacterObject.OneToOneConversationCharacter != null &&
                    CharacterObject.OneToOneConversationCharacter.IsHero &&
                    CharacterObject.OneToOneConversationCharacter.Occupation == Occupation.Wanderer &&
                    CharacterObject.OneToOneConversationCharacter.HeroObject.HeroState != Hero.CharacterStates.Prisoner &&
                    Hero.OneToOneConversationHero.Clan == null;

                return false;
            }

            [HarmonyPostfix]
            [HarmonyPatch("conversation_wanderer_preintroduction_on_condition")]
            private static void PreIntroductionPostfix(ref bool __result)
            {
                if (Hero.OneToOneConversationHero.Clan != null && Hero.OneToOneConversationHero.Clan != Clan.PlayerClan)
                {
                    __result = false;
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch("conversation_companion_hire_gold_on_condition")]
            private static void HirePostfix(ref bool __result)
            {
                if (Hero.OneToOneConversationHero.Clan != null && Hero.OneToOneConversationHero.Clan != Clan.PlayerClan)
                {
                    __result = false;
                }
            }
        }
    }
}
