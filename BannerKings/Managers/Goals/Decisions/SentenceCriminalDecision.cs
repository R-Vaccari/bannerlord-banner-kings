using BannerKings.Behaviours.Criminality;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Goals.Decisions
{
    public class SentenceCriminalDecision : Goal
    {
        private CriminalSentence sentence;
        private Crime crime;

        public SentenceCriminalDecision(Hero fulfiller = null) : base("goal_sentence_criminal",
            GoalCategory.Kingdom, GoalUpdateType.Hero, fulfiller)
        {
            Initialize(new TextObject("{=pKoKaKNd}Sentence Criminal"),
                new TextObject("{=R6X7JFKz}As a Peer within a realm, you are able to sentence those found to be criminals that you hold within your dungeons."));
        }

        public override void DoAiDecision()
        {
            throw new NotImplementedException();
        }

        public override void ApplyGoal()
        {
            sentence.ExecuteSentence(crime, GetFulfiller());
        }

        public override bool IsAvailable()
        {
            return Clan.PlayerClan.Kingdom != null;
        }

        public override bool IsFulfilled(out List<TextObject> failedReasons)
        {
            failedReasons = new List<TextObject>();

            BKCriminalityBehavior behavior = TaleWorlds.CampaignSystem.Campaign.Current.GetCampaignBehavior<BKCriminalityBehavior>();
            Dictionary<Hero, List<Crime>> criminals = behavior.GetCriminals(GetFulfiller());
            if (criminals.Count == 0)
            {
                failedReasons.Add(new TextObject("{=CcisWSka}You do not have any criminals within your settlement prisons."));
            }

            return failedReasons.IsEmpty();
        }

        public override void ShowInquiry()
        {
            var crimes = new List<InquiryElement>();
            BKCriminalityBehavior behavior = TaleWorlds.CampaignSystem.Campaign.Current.GetCampaignBehavior<BKCriminalityBehavior>();
            Dictionary<Hero, List<Crime>> criminals = behavior.GetCriminals(GetFulfiller());

            foreach (var pair in criminals)
            {
                foreach (var crime in pair.Value)
                {
                    crimes.Add(new InquiryElement(crime,
                       new TextObject("{=5oS3A1Gy}{CRIME} - {NAME}")
                       .SetTextVariable("CRIME", crime.Name)
                       .SetTextVariable("NAME", crime.Hero.Name)
                       .ToString(),
                       null,
                       true,
                       new TextObject("{=Pb2ZQBVH}{HERO} has been found guilty of the {CRIME} crime on {DATE}")
                       .SetTextVariable("HERO", crime.Hero.Name)
                       .SetTextVariable("CRIME", crime.Name)
                       .SetTextVariable("DATE", crime.Date.ToString())
                       .ToString()));
                }
            }

            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                new TextObject("{=pKoKaKNd}Sentence Criminal (1/2)").ToString(),
                new TextObject("{=cNVFv1sW}Choose a criminal to be sentenced. The possible sentences are contextual. Sentences can be considered tyrannical also depending on context, and will negatively impact your standing with your peers.").ToString(),
                crimes,
                true,
                1,
                1,
                GameTexts.FindText("str_done").ToString(),
                GameTexts.FindText("str_cancel").ToString(),
                delegate (List<InquiryElement> crimeOptions)
                {
                    crime = (Crime)crimeOptions.First().Identifier;
                    var sentences = new List<InquiryElement>();
                    foreach (var sentence in DefaultCriminalSentences.Instance.All)
                    {
                        TextObject name = sentence.Name;
                        if (sentence.IsSentenceTyranical(crime, Hero.MainHero))
                        {
                            name = new TextObject("{=iOOx1Tca}{SENTENCE} (Tyrannical)")
                            .SetTextVariable("SENTENCE", sentence.Name);
                        }
                        sentences.Add(new InquiryElement(sentence,
                            name.ToString(),
                    null,
                            sentence.IsAdequateForHero(crime),
                            sentence.Description.ToString()));
                    }

                    MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                        new TextObject("{=pKoKaKNd}Sentence Criminal (2/2)").ToString(),
                        new TextObject("{=GaQTC0YT}{HERO} will be sentenced for the crime of {CRIME}. Tyrannical sentences will impact your standing with your peers.").ToString(),
                        sentences,
                        true,
                        1,
                        1,
                        GameTexts.FindText("str_done").ToString(),
                        GameTexts.FindText("str_cancel").ToString(),
                        delegate (List<InquiryElement> sentenceOptions)
                        {
                            sentence = (CriminalSentence)sentenceOptions.First().Identifier;
                            ApplyGoal();
                        },
                        null,
                        string.Empty));
                },
                null,
                string.Empty));
        }
    }
}
