using BannerKings.Actions;
using BannerKings.Behaviours.Feasts;
using BannerKings.Dialogue;
using BannerKings.UI;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.BarterSystem.Barterables;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Behaviours.Marriage
{
    public class BKMarriageBehavior : CampaignBehaviorBase
    {
        /*private Dictionary<Hero, List<MarriageOffer>> marriageOffers = new Dictionary<Hero, List<MarriageOffer>>();

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
        } */

        private MarriageContract proposedMarriage;
        private List<Hero> flirtedWith = new List<Hero>();

        public void SetProposedMarriage(MarriageContract contract)
        {
            proposedMarriage = contract;
        }

        public MarriageContract GetProposedMarriage() => proposedMarriage;

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("bannerkings-heroes-flirted", ref flirtedWith);
            dataStore.SyncData("bannerkings-player-betrothal", ref proposedMarriage);

            if (flirtedWith == null)
            {
                flirtedWith = new List<Hero>();
            }
        }

        public bool IsCoupleMatchedByFamily(Hero proposer, Hero proposed) => proposedMarriage != null && proposedMarriage.Confirmed
            && proposedMarriage.Proposer == proposer && proposedMarriage.Proposed == proposed;

        private void OnSessionLaunched(CampaignGameStarter starter)
        {
            starter.AddPlayerLine("lord_special_request_flirt", 
                "lord_talk_speak_diplomacy_2", 
                "lord_start_courtship_response", 
                "{=ntXogRSG}{FLIRTATION_LINE}", 
                () =>
                {
                    if (Hero.MainHero.IsFemale)
                    {
                        MBTextManager.SetTextVariable("FLIRTATION_LINE", "{=bjJs0eeB}My lord, I note that you have not yet taken a wife.", false);
                    }
                    else
                    {
                        MBTextManager.SetTextVariable("FLIRTATION_LINE", "{=v1hC6Aem}My lady, I wish to profess myself your most ardent admirer.", false);
                    }

                    return Campaign.Current.Models.MarriageModel.IsCoupleSuitableForMarriage(Hero.MainHero, Hero.OneToOneConversationHero) && 
                    !FactionManager.IsAtWarAgainstFaction(Hero.MainHero.MapFaction, Hero.OneToOneConversationHero.MapFaction) &&
                    Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero) == Romance.RomanceLevelEnum.Untested &&
                    !flirtedWith.Contains(Hero.OneToOneConversationHero);
                },
                () => flirtedWith.Add(Hero.OneToOneConversationHero));

            starter.AddDialogLine("lord_start_courtship_response", 
                "lord_start_courtship_response", 
                "lord_start_courtship_response_player_offer", 
                "{=jbVg1aYL}{INITIAL_COURTSHIP_REACTION}", 
                () =>
                {
                    if (Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero) == Romance.RomanceLevelEnum.FailedInPracticalities 
                    || Romance.GetRomanticLevel(Hero.MainHero, Hero.OneToOneConversationHero) == Romance.RomanceLevelEnum.FailedInCompatibility)
                    {
                        return false;
                    }

                    var hero = Hero.OneToOneConversationHero;
                    int attraction = Campaign.Current.Models.RomanceModel.GetAttractionValuePercentage(hero, Hero.MainHero);

                    TextObject text = null;
                    if (attraction >= 0.7)
                    {
                        text = new TextObject("{=zikiRHun}I am delighted to hear. We are currently taking in proposal, as I am yet to be wed...");
                        ChangeRelationAction.ApplyPlayerRelation(hero, 3, false);
                    }
                    else if (attraction <= 0.3)
                    {
                        text = new TextObject("{=Ww4NGDkb}Is that so? I'm afraid I cannot say the same.");
                    }
                    else
                    {
                        text = new TextObject("{=rPP2kBi2}Well, we are currently taking in proposals.");
                        ChangeRelationAction.ApplyPlayerRelation(hero, -3, false);
                    }

                    if (hero.GetHeroTraits().Mercy < 0)
                    {
                        if (attraction >= 0.7)
                        {
                            text = new TextObject("{=6mz2KkwR}I see... you look like you have potential.");
                        }
                        else if (attraction <= 0.3)
                        {
                            text = new TextObject("{=emfu56Oe}That sounded as dumb as you look.");
                        }
                        else
                        {
                            text = new TextObject("{=8vRvXFAg}Thanks... I suppose. Though you'll need more than that to impress me.");
                        }
                    }
                    else if (hero.GetHeroTraits().Honor > 0)
                    {
                        if (attraction >= 0.7)
                        {
                            text = new TextObject("{=040mMgky}I am delighted to hear it. You know, I am still to be wed...");
                        }
                        else if (attraction <= 0.3)
                        {
                            text = new TextObject("{=AsnqT2cw}I am grateful for the compliment. I am afraid I have more important matters to attend to.");
                        }
                        else
                        {
                            text = new TextObject("{=9nseh0fu}Thank you. I am looking for spouse candidates. You strike me well yourself.");
                        }
                    }
                    else if (hero.GetHeroTraits().Calculating > 0)
                    {
                        if (attraction >= 0.7)
                        {
                            text = new TextObject("{=Au3zuxf1}Indeed. I must say, you strike me well. Perhaps uniting would benefit us mutually.");
                        }
                        else if (attraction <= 0.3)
                        {
                            text = new TextObject("{=kTBPuKE8}I am afraid your play had no effect... I do not believe you and me together would be fruitful.");
                        }
                        else
                        {
                            text = new TextObject("{=BsuNUiLG}Thank you. I am looking for spouse candidates, and will remember your kindness.");
                        }
                    }
                    else if (hero.GetHeroTraits().Generosity > 1)
                    {
                        if (attraction >= 0.7)
                        {
                            text = new TextObject("{=SfvuXf8A}You are most generous, {TITLE}. You look quite well yourself, if I may say.")
                                .SetTextVariable("TITLE", GameTexts.FindText(Hero.MainHero.IsFemale ? "str_player_salutation_my_lady" : "str_player_salutation_my_lord"));
                        }
                        else if (attraction <= 0.3)
                        {
                            text = new TextObject("{=ZGkcnvDN}I am grateful, {HERO}. I can tell you my family is currently seeking out proposals.")
                                .SetTextVariable("HERO", Hero.MainHero.Name);
                        }
                        else
                        {
                            text = new TextObject("{=MwEA1OB0}You are a kind spirit. We are looking for spouse candidates. Though it is not my place to decide, you strike me as a decent person.");
                        }
                    }

                    MBTextManager.SetTextVariable("INITIAL_COURTSHIP_REACTION", text);
                    return IsPotentialSpouseBK();
                },
                null, 100, null);


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

                    return IsPotentialSpouseBK();
                }, 
                null);

            starter.AddPlayerLine("bk_marriage_offered_clan_member_already_flirted",
               "lord_talk_speak_diplomacy_2",
               "bk_marriage_offered_not_accepted",
               "{=cKtJBdPD}I wish to offer my hand in marriage.",
               () =>
               {
                   if (Hero.OneToOneConversationHero == null || Hero.OneToOneConversationHero.Clan == null)
                   {
                       return false;
                   }

                   return IsPotentialSpouseBK() && flirtedWith.Contains(Hero.OneToOneConversationHero);
               },
               null);


            starter.AddDialogLine("lord_start_courtship_response_3",
                "bk_marriage_offered_not_accepted",
                "lord_pretalk", 
                "{=htd0GSac}{OFFER_NOT_ACCEPTED}", 
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
                        if (isPlayerHigherRanking) text = new TextObject("{=ixhPCwdF}{TITLE}, I am honored by your most generous request. However, I am not in the position to accept an offer. Please speak to {LEADER}, the head of our family.")
                            .SetTextVariable("TITLE", GameTexts.FindText(Hero.MainHero.IsFemale ? "str_my_lady" :  "str_my_lord"))
                            .SetTextVariable("LEADER", Hero.OneToOneConversationHero.Clan.Leader.Name);
                        else text = new TextObject("{=UB0oinVR}{TITLE}, I am honored by your request. However, I am not in the position to accept an offer. You may speak to {LEADER}, the head of our family.")
                            .SetTextVariable("TITLE", GameTexts.FindText(Hero.MainHero.IsFemale ? "str_my_lady" : "str_my_lord"))
                            .SetTextVariable("LEADER", Hero.OneToOneConversationHero.Clan.Leader.Name);
                    }
                    else if (relations >= 0)
                    {
                        if (isPlayerHigherRanking) text = new TextObject("{=ogAieLQE}{TITLE}, I am not in the position to accept an offer. Please speak to {LEADER}, the head of our family.")
                            .SetTextVariable("TITLE", GameTexts.FindText(Hero.MainHero.IsFemale ? "str_my_lady" : "str_my_lord"))
                            .SetTextVariable("LEADER", Hero.OneToOneConversationHero.Clan.Leader.Name);
                        else text = new TextObject("{=e5MpCH3G}It is not in my position to accept an offer. You may speak to {LEADER}, the head of our family.")
                            .SetTextVariable("LEADER", Hero.OneToOneConversationHero.Clan.Leader.Name);
                    } 
                    else if (relations >= -49)
                    {
                        if (isPlayerHigherRanking) text = new TextObject("{=qJ30OzLa}\"{TITLE}\", I am not in the position to accept an offer. Yet, dare I say, we are not looking for any.")
                            .SetTextVariable("TITLE", GameTexts.FindText(Hero.MainHero.IsFemale ? "str_my_lady" : "str_my_lord"));
                        else text = new TextObject("{=T8qzYXR5}A marriage? Do jesters such as yourself get married?")
                            .SetTextVariable("LEADER", Hero.OneToOneConversationHero.Clan.Leader.Name);
                    }
                    else
                    {
                        if (isPlayerHigherRanking) text = new TextObject("{=qJ30OzLa}\"{TITLE}\", I am not in the position to accept an offer. But, if I were, I would not entertain the {CLAN} more than I do stray mongrels.")
                            .SetTextVariable("TITLE", GameTexts.FindText(Hero.MainHero.IsFemale ? "str_my_lady" : "str_my_lord"))
                            .SetTextVariable("CLAN", Hero.MainHero.Clan.Name);
                        else text = new TextObject("{=XMMjS27Y}No, you idiot. Get out of my sight.")
                            .SetTextVariable("LEADER", Hero.OneToOneConversationHero.Clan.Leader.Name);
                    }

                    MBTextManager.SetTextVariable("OFFER_NOT_ACCEPTED", text);
                    return true;
                },
                null);

            starter.AddPlayerLine("lord_propose_marriage_contract", 
                "lord_talk_speak_diplomacy_2", 
                "propose_marriage_contract", 
                "{=v9tQv4eN}I would like to propose an alliance between our families through marriage.",
                () =>
                {
                    if (Hero.OneToOneConversationHero == null || Hero.OneToOneConversationHero.Clan == null)
                    {
                        return false;
                    }

                    var clan = Hero.OneToOneConversationHero.Clan;

                    return clan != Clan.PlayerClan && clan.Leader == Hero.OneToOneConversationHero && 
                    !FactionManager.IsAtWarAgainstFaction(Hero.MainHero.MapFaction, Hero.OneToOneConversationHero.MapFaction);
                },
                () =>
                {
                    UIManager.Instance.ShowWindow("marriage");
                }, 
                120, 
                delegate (out TextObject reason)
                {
                    reason = new TextObject("{=hcvHjyDM}Marriage candidates are available.");

                    if (!Clan.PlayerClan.Heroes.Any(x => !x.IsChild && x.Spouse == null))
                    {
                        reason = new TextObject("{=Ht8aufCz}{CLAN} has no available candidates")
                            .SetTextVariable("CLAN", Clan.PlayerClan.Name);
                        return false;
                    }

                    if (!Hero.OneToOneConversationHero.Clan.Heroes.Any(x => !x.IsChild && x.Spouse == null))
                    {
                        reason = new TextObject("{=Ht8aufCz}{CLAN} has no available candidates")
                            .SetTextVariable("CLAN", Clan.PlayerClan.Name);
                        return false;
                    }

                    return true;
                },
                null);

            starter.AddDialogLine("propose_marriage_contract",
               "propose_marriage_contract",
               "propose_marriage_contract_response",
               "{=WjPfeaqG}Tell me the specifics of your proposal.",
               null,
               null);

            starter.AddPlayerLine("propose_marriage_contract_response",
               "propose_marriage_contract_response",
               "marriage_contract_proposed",
               "{=oQR06Uzy}This is my proposal.",
               null,
               null);

            starter.AddPlayerLine("propose_marriage_contract_response",
               "propose_marriage_contract_response",
               "propose_marriage_contract",
               "{=LfTvDD1e}Let me review my proposal.",
               null,
               () => UIManager.Instance.ShowWindow("marriage"));

            starter.AddPlayerLine("propose_marriage_contract_response",
                "propose_marriage_contract_response",
                "lord_pretalk",
                "{=D33fIGQe}Never mind.",
                null,
                null);

            starter.AddDialogLine("marriage_contract_proposed",
              "marriage_contract_proposed",
              "propose_marriage_contract",
              "{=5dspZ1M2}I'm afraid you didn't make an adequate proposal.",
              () => 
              {
                  return proposedMarriage == null;
              },
              null);

            starter.AddDialogLine("marriage_contract_proposed",
              "marriage_contract_proposed",
              "propose_marriage_contract",
              "{=KXOsnc3G}This proposal is not acceptable. {REJECTION_REASON}",
              () =>
              {
                  bool rejected = true;
                  bool notNull = proposedMarriage != null;
                  if (notNull)
                  {
                      (TextObject, bool) result = proposedMarriage.IsContractAdequate();
                      MBTextManager.SetTextVariable("REJECTION_REASON", result.Item1);
                      rejected = !result.Item2;
                  }
                 
                  return notNull && rejected;
              },
              null);

            starter.AddDialogLine("marriage_contract_proposed",
             "marriage_contract_proposed",
             "marriage_contract_confirmation",
             "{=xodThNQy}{PROPOSAL_ACCEPTED}",
             () =>
             {
                 bool rejected = true;
                 bool notNull = proposedMarriage != null;
                 if (notNull)
                 {
                     (TextObject, bool) result = proposedMarriage.IsContractAdequate();
                     rejected = !result.Item2;

                     if (!rejected)
                     {
                         MBTextManager.SetTextVariable("PROPOSAL_ACCEPTED", 
                             new TextObject("{=4KPONSvT}{PLAYER}, I am happy to accept this proposal. {CONFIRMATION}")
                             .SetTextVariable("PLAYER", Hero.MainHero.Name)
                             .SetTextVariable("CONFIRMATION", proposedMarriage.ArrangedMarriage ? new TextObject("{=tvjAsBsw}Will you confirm this union?")
                             : new TextObject("{=EZrjFHMt}Will you confirm this betrothal? Know that we will not take lightly if you do, and yet go back on your word.")));
                     }
                 }

                 return notNull && !rejected;
             },
             () =>
             {
                 if (!proposedMarriage.ArrangedMarriage)
                 {
                     ChangeRomanticStateAction.Apply(Hero.MainHero, proposedMarriage.Proposed, Romance.RomanceLevelEnum.MatchMadeByFamily);
                 }
             });

            starter.AddPlayerLine("marriage_contract_confirmation",
               "marriage_contract_confirmation",
               "marriage_contract_confirmed_by_player",
               "{=mM8ajh6s}I confirm it.",
               null,
               () => proposedMarriage.Confirmed = true);

            starter.AddPlayerLine("marriage_contract_confirmation",
               "marriage_contract_confirmation",
               "propose_marriage_contract",
               "{=LfTvDD1e}Let me review my proposal.",
               null,
               () => UIManager.Instance.ShowWindow("marriage"));


            starter.AddDialogLine("marriage_contract_confirmed_by_player",
               "marriage_contract_confirmed_by_player",
               "close_window",
                   "{=O4vPn7p1}{PROPOSAL_CONFIRMED}",
               () =>
               {
                    MBTextManager.SetTextVariable("PROPOSAL_CONFIRMED",
                        new TextObject("{=m48122Zw}It is decided then. {CONFIRMATION}")
                        .SetTextVariable("CONFIRMATION", DialogueHelper.GetRandomText(Hero.OneToOneConversationHero, DialogueHelper.GetMarriageConfirmationTexts(proposedMarriage))));

                   return proposedMarriage != null;
               },
               () =>
               {
                   if (proposedMarriage.Influence > 0)
                   {
                       GainKingdomInfluenceAction.ApplyForDefault(Hero.MainHero, -proposedMarriage.Influence);
                   }

                   var finalClan = proposedMarriage.FinalClan;

                   if (!proposedMarriage.ArrangedMarriage)
                   {
                       AnnounceBetrothal();
                       ChangeRomanticStateAction.Apply(Hero.MainHero, proposedMarriage.Proposed, Romance.RomanceLevelEnum.MatchMadeByFamily);
                   }
                   else
                   {
                       if (proposedMarriage.Alliance)
                       {
                           Utils.Helpers.SetAlliance(Clan.PlayerClan, Hero.OneToOneConversationHero.Clan);
                       }

                       if (proposedMarriage.Feast && proposedMarriage.FinalClan.Kingdom != null)
                       {
                           AnnounceBetrothal();
                           var town = proposedMarriage.FinalClan.Fiefs.GetRandomElement();
                           var clanCount = MathF.Min(proposedMarriage.FinalClan.Kingdom.Clans.Count, MBRandom.RandomInt(3, 8));
                           Campaign.Current.GetCampaignBehavior<BKFeastBehavior>().LaunchFeast(town, 
                               proposedMarriage.FinalClan.Kingdom.Clans.Take(clanCount).ToList(), 
                               proposedMarriage);
                       }
                       else
                       {
                           ApplyMarriageContract();
                       }
                   }

                   if (PlayerEncounter.Current != null)
                   {
                       PlayerEncounter.LeaveEncounter = true;
                   }
               });
        }

        private void AnnounceBetrothal()
        {
            MBInformationManager.AddQuickInformation(new TextObject("{=5sPJgrsx}{HERO1} and {HERO2} are now betrothed!")
                .SetTextVariable("HERO", proposedMarriage.Proposer.Name)
                .SetTextVariable("HERO2", proposedMarriage.Proposed.Name),
                100,
                null,
                Utils.Helpers.GetKingdomDecisionSound());
        }

        public void ApplyMarriageContract()
        {
            if (proposedMarriage != null)
            {
                MarriageAction.Apply(proposedMarriage.Proposer, proposedMarriage.Proposed);
                var finalClan = proposedMarriage.FinalClan;
                Hero proposerLeader = proposedMarriage.Proposer.Clan.Leader;
                Hero proposedLeader = proposedMarriage.Proposed.Clan.Leader;
                if (proposedMarriage.Proposer.Clan != finalClan)
                {
                    GiveGoldAction.ApplyBetweenCharacters(proposedLeader, proposerLeader, proposedMarriage.Dowry);
                    ClanActions.JoinClan(proposedMarriage.Proposer, proposedMarriage.FinalClan);
                }

                if (proposedMarriage.Proposed.Clan != finalClan)
                {
                    GiveGoldAction.ApplyBetweenCharacters(proposerLeader, proposedLeader, proposedMarriage.Dowry);
                    ClanActions.JoinClan(proposedMarriage.Proposed, proposedMarriage.FinalClan);
                }

                proposedMarriage = null;
            }
        }

        private bool IsPotentialSpouseBK() => Hero.MainHero.Spouse == null &&
                        Campaign.Current.Models.MarriageModel.IsCoupleSuitableForMarriage(Hero.MainHero, Hero.OneToOneConversationHero) &&
                        !FactionManager.IsAtWarAgainstFaction(Hero.MainHero.MapFaction, Hero.OneToOneConversationHero.MapFaction) &&
                        !IsCoupleMatchedByFamily(Hero.MainHero, Hero.OneToOneConversationHero) &&
                        Hero.OneToOneConversationHero.Clan.Leader != Hero.OneToOneConversationHero;
    }

    namespace Patches
    {

        [HarmonyPatch(typeof(MarriageBarterable))]
        internal class MarriageBarterablePatches
        {
            [HarmonyPrefix]
            [HarmonyPatch("GetUnitValueForFaction")]
            private static bool DowryPrefix(MarriageBarterable __instance, ref int __result, IFaction faction)
            {
                var proposer = __instance.ProposingHero;
                var proposed = __instance.HeroBeingProposedTo;
                if (Campaign.Current.GetCampaignBehavior<BKMarriageBehavior>().IsCoupleMatchedByFamily(proposer, proposed))
                {
                    var contract = Campaign.Current.GetCampaignBehavior<BKMarriageBehavior>().GetProposedMarriage();
                    __result = contract.Dowry;

                    return false;
                }

                return true;
            }
        }

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
                        .IsCoupleMatchedByFamily(Hero.MainHero, Hero.OneToOneConversationHero);
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch("conversation_romance_at_stage_1_discussions_on_condition")]
            private static void RomanceStage1Postfix(ref bool __result)
            {
                if (__result == true)
                {
                    __result = Campaign.Current.GetCampaignBehavior<BKMarriageBehavior>()
                        .IsCoupleMatchedByFamily(Hero.MainHero, Hero.OneToOneConversationHero);
                }
            }

            [HarmonyPrefix]
            [HarmonyPatch("conversation_player_can_open_courtship_on_condition")]
            private static bool DoNotStartCourtshipPrefix(ref bool __result)
            {
                __result = false;
                return false;
            }



            /*[HarmonyPrefix]
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
            }*/

            [HarmonyPrefix]
            [HarmonyPatch("conversation_discuss_marriage_alliance_on_condition")]
            private static bool PlayerProposeAlliancePrefix(RomanceCampaignBehavior __instance, ref bool __result)
            {
                __result = false;
                return false;
            }
        }
    }
}
