using HarmonyLib;
using Helpers;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Behaviours.Marriage
{
    public class BKMarriageBehavior : CampaignBehaviorBase
    {
        private Dictionary<Hero, List<MarriageOffer>> marriageOffers = new Dictionary<Hero, List<MarriageOffer>>();

        public MarriageOffer GetHeroMarriageOffers(Hero proposer, Hero proposed)
        {
            if (marriageOffers.ContainsKey(proposer))
            {
                return marriageOffers[proposer].FirstOrDefault(x => x.Proposed == proposed);
            }

            return null;
        }

        public bool IsHeroOfferAccepted(Hero proposer, Hero proposed)
        {
            if (marriageOffers.ContainsKey(proposer))
            {
                var offer = marriageOffers[proposer].FirstOrDefault(x => x.Proposed == proposed);
                if (offer != null)
                {
                    return offer.Accepted;
                }
            }

            return false;
        }

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
        }

        public override void SyncData(IDataStore dataStore)
        {
            
        }

        private void OnSessionLaunched(CampaignGameStarter starter)
        {
            starter.AddPlayerLine("bk_marriage_offered_clan_member", 
                "lord_start_courtship_response_player_offer", 
                "bk_marriage_offered_not_accepted",
                "{=cKtJBdPD}I wish to offer my hand in marriage.", 
                () =>
                {
                    if (Hero.OneToOneConversationHero == null || Hero.OneToOneConversationHero.Clan == null)
                    {
                        return false;
                    }

                    return Hero.MainHero.Spouse == null &&
                    Campaign.Current.Models.MarriageModel.IsCoupleSuitableForMarriage(Hero.MainHero, Hero.OneToOneConversationHero) &&
                    !FactionManager.IsAtWarAgainstFaction(Hero.MainHero.MapFaction, Hero.OneToOneConversationHero.MapFaction) &&
                    !IsHeroOfferAccepted(Hero.MainHero, Hero.OneToOneConversationHero) &&
                    Hero.OneToOneConversationHero.Clan.Leader != Hero.OneToOneConversationHero;
                }, 
                null, 
                120, 
                null, 
                null);


            starter.AddDialogLine("lord_start_courtship_response_3",
                "bk_marriage_offered_not_accepted", 
                "close_window", 
                "{=!}{OFFER_NOT_ACCEPTED}", 
                () =>
                {
                    TextObject text;

                    bool isPlayerHigherRanking = false;
                    var playerTitle = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(Hero.MainHero);
                    var proposedTitle = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(Hero.OneToOneConversationHero.Clan.Leader);
                    int playerRank = int.MaxValue;
                    if (playerTitle != null)
                    {
                        playerRank = (int)playerTitle.type;
                    }

                    int proposedRank = int.MaxValue;
                    if (proposedTitle != null)
                    {
                        proposedRank = (int)proposedTitle.type;
                    }

                    isPlayerHigherRanking = playerRank < proposedRank;
                    int relations = Hero.OneToOneConversationHero.Clan.GetRelationWithClan(Clan.PlayerClan);

                    if (relations >= 50)
                    {
                        if (isPlayerHigherRanking) text = new TextObject("{=!}{TITLE}, I am honored by your most generous request. However, I am not in the position to accept an offer. Please speak to {LEADER}, the head of our family.")
                            .SetTextVariable("TITLE", GameTexts.FindText(Hero.MainHero.IsFemale ? "str_my_lady" :  "str_my_lord"))
                            .SetTextVariable("LEADER", Hero.OneToOneConversationHero.Clan.Leader.Name);
                        else text = new TextObject("{=!}{TITLE}, I am honored by your request. However, I am not in the position to accept an offer. You may speak to {LEADER}, the head of our family.")
                            .SetTextVariable("TITLE", GameTexts.FindText(Hero.MainHero.IsFemale ? "str_my_lady" : "str_my_lord"))
                            .SetTextVariable("LEADER", Hero.OneToOneConversationHero.Clan.Leader.Name);
                    }
                    else if (relations >= 0)
                    {
                        if (isPlayerHigherRanking) text = new TextObject("{=!}{TITLE}, I am not in the position to accept an offer. Please speak to {LEADER}, the head of our family.")
                            .SetTextVariable("TITLE", GameTexts.FindText(Hero.MainHero.IsFemale ? "str_my_lady" : "str_my_lord"))
                            .SetTextVariable("LEADER", Hero.OneToOneConversationHero.Clan.Leader.Name);
                        else text = new TextObject("{=!}It is not in my position to accept an offer. You may speak to {LEADER}, the head of our family.")
                            .SetTextVariable("LEADER", Hero.OneToOneConversationHero.Clan.Leader.Name);
                    } 
                    else if (relations >= -49)
                    {
                        if (isPlayerHigherRanking) text = new TextObject("{=!}\"{TITLE}\", I am not in the position to accept an offer. Yet, dare I say, we are not looking for any.")
                            .SetTextVariable("TITLE", GameTexts.FindText(Hero.MainHero.IsFemale ? "str_my_lady" : "str_my_lord"));
                        else text = new TextObject("{=!}A marriage? Do jesters such as yourself get married?")
                            .SetTextVariable("LEADER", Hero.OneToOneConversationHero.Clan.Leader.Name);
                    }
                    else
                    {
                        if (isPlayerHigherRanking) text = new TextObject("{=!}\"{TITLE}\", I am not in the position to accept an offer. But, if I were, I would not entertain the {CLAN} more than I do stray mongrels.")
                            .SetTextVariable("TITLE", GameTexts.FindText(Hero.MainHero.IsFemale ? "str_my_lady" : "str_my_lord"))
                            .SetTextVariable("CLAN", Hero.MainHero.Clan.Name);
                        else text = new TextObject("{=!}No, you idiot. Get out of my sight.")
                            .SetTextVariable("LEADER", Hero.OneToOneConversationHero.Clan.Leader.Name);
                    }

                    MBTextManager.SetTextVariable("OFFER_NOT_ACCEPTED", text);
                    return true;
                },
                null);


            starter.AddPlayerLine("bk_marriage_offered_clan_leader",
               "lord_start_courtship_response_player_offer",
               "bk_marriage_offered_not_accepted",
               "{=!}I wish to marry our Houses.",
               () =>
               {
                   if (Hero.OneToOneConversationHero == null || Hero.OneToOneConversationHero.Clan == null)
                   {
                       return false;
                   }

                   return Hero.MainHero.Spouse == null &&
                   Campaign.Current.Models.MarriageModel.IsCoupleSuitableForMarriage(Hero.MainHero, Hero.OneToOneConversationHero) &&
                   !FactionManager.IsAtWarAgainstFaction(Hero.MainHero.MapFaction, Hero.OneToOneConversationHero.MapFaction) &&
                   !IsHeroOfferAccepted(Hero.MainHero, Hero.OneToOneConversationHero) &&
                   Hero.OneToOneConversationHero.Clan.Leader == Hero.OneToOneConversationHero;
               },
               null,
               120,
               null,
               null);
        }
    }

    namespace Patches
    {


        [HarmonyPatch(typeof(RomanceCampaignBehavior))]
        internal class MarriageDialoguePatches
        {
            [HarmonyPostfix]
            [HarmonyPatch("conversation_player_eligible_for_marriage_with_conversation_hero_on_condition")]
            private static void PlayerEligiblePostfix(ref bool __result)
            {
                if (__result == true)
                {
                    __result = Campaign.Current.GetCampaignBehavior<BKMarriageBehavior>()
                        .IsHeroOfferAccepted(Hero.MainHero, Hero.OneToOneConversationHero);
                }
            }


            [HarmonyPrefix]
            [HarmonyPatch("conversation_propose_spouse_for_player_nomination_on_condition")]
            private static bool PlayerProposePrefix(RomanceCampaignBehavior __instance, ref bool __result)
            {

                Hero proposer = (Hero)AccessTools.Field("_playerProposalHero").GetValue(__instance);
                foreach (Hero hero in from x in Hero.OneToOneConversationHero.Clan.Lords
                                      orderby x.Age descending
                                      select x)
                {
                    var result = BannerKingsConfig.Instance.MarriageModel.IsMarriageAdequate(proposer, hero);
                    if (Campaign.Current.Models.MarriageModel.IsCoupleSuitableForMarriage(proposer, hero) && 
                        !FactionManager.IsAtWarAgainstFaction(proposer.MapFaction, hero.MapFaction) && 
                        hero != Hero.OneToOneConversationHero && result.ResultNumber > 0f)
                    {
                        AccessTools.Field("_proposedSpouseForPlayerRelative").SetValue(__instance, hero);
                        TextObject textObject = new TextObject("{=TjAQbTab}Well, yes, we are looking for a suitable marriage for {OTHER_CLAN_NOMINEE.LINK}.", null);
                        hero.SetPropertiesToTextObject(textObject, "OTHER_CLAN_NOMINEE");
                        MBTextManager.SetTextVariable("ARRANGE_MARRIAGE_LINE", textObject, false);
                        __result = true;
                    }
                }

                __result = false;
                return false;
            }
        }
    }
}
