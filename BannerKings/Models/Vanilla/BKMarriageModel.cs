using BannerKings.Managers.Court;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Models.Vanilla
{
    public class BKMarriageModel : DefaultMarriageModel
    {

        public ExplainedNumber IsMarriageAdequate(Hero proposer, Hero secondHero, bool explanations = false)
        {
            var result = new ExplainedNumber(0f, explanations);

            var firstLeader = proposer.Clan.Leader;
            var secondLeader = secondHero.Clan.Leader;

            Clan finalClan = GetClanAfterMarriage(proposer, secondHero);
            bool proposerAdvantage = finalClan == proposer.Clan;

            var proposerTitle = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(firstLeader);
            var secondTitle = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(secondLeader);

            float proposerTitleScore = 0f;
            float secondTitleScore = 0f;

            if (proposerTitle != null)
            {
                proposerTitleScore = 100f - (10f * (float)proposerTitle.type);
            }

            if (secondTitle != null)
            {
                secondTitleScore = 100f - (10f * (float)proposerTitle.type);
            }

            result.Add(secondHero.Clan.GetRelationWithClan(proposer.Clan) / 2.5f,
                new TextObject("{=!}Relations between {ITEM1} and {ITEM2}")
                .SetTextVariable("ITEM1", proposer.Name)
                .SetTextVariable("ITEM2", secondHero.Name));

            result.Add(secondHero.Clan.GetRelationWithClan(proposer.Clan) / 5f, 
                new TextObject("{=!}Relations between {ITEM1} and {ITEM2}")
                .SetTextVariable("ITEM1", proposer.Clan.Name)
                .SetTextVariable("ITEM2", secondHero.Clan.Name));
            result.Add(proposerTitleScore - secondTitleScore, new TextObject("{=!}Title differences"));


            if (proposer == firstLeader)
            {
                result.AddFactor(0.15f * (proposerAdvantage ? 1f : -1f), GameTexts.FindText("role", "ClanLeader"));
            }
            else
            {
                float heirScore = float.MinValue;
                Hero proposerHeir = null;
                foreach (var candidate in BannerKingsConfig.Instance.TitleModel.GetInheritanceCandidates(firstLeader))
                {
                    var score = BannerKingsConfig.Instance.TitleModel.GetInheritanceHeirScore(firstLeader, candidate,
                        proposerTitle.contract).ResultNumber;
                    if (score > heirScore)
                    {
                        proposerHeir = candidate;
                    }
                }

                if (proposerHeir == proposer)
                {
                    result.AddFactor(0.1f * (proposerAdvantage ? 1f : -1f), new TextObject("{=!}{HERO} is the expected heir to {CLAN}")
                        .SetTextVariable("HERO", proposer.Name)
                        .SetTextVariable("CLAN", proposer.Clan.Name));
                }
            }

            if (secondHero == secondLeader)
            {
                result.AddFactor(0.15f * (proposerAdvantage ? -1f : 1f), GameTexts.FindText("role", "ClanLeader"));
            }
            else
            {
                float heirScore = float.MinValue;
                Hero secondHeir = null;
                foreach (var candidate in BannerKingsConfig.Instance.TitleModel.GetInheritanceCandidates(secondLeader))
                {
                    var score = BannerKingsConfig.Instance.TitleModel.GetInheritanceHeirScore(secondLeader, candidate,
                        proposerTitle.contract).ResultNumber;
                    if (score > heirScore)
                    {
                        secondHeir = candidate;
                    }
                }

                if (secondHeir == secondHero)
                {
                    result.AddFactor(0.1f * (proposerAdvantage ? -1f : 1f), new TextObject("{=!}{HERO} is the expected heir to {CLAN}")
                        .SetTextVariable("HERO", secondHero.Name)
                        .SetTextVariable("CLAN", secondHero.Clan.Name));
                }
            }



            var proposerCouncil = BannerKingsConfig.Instance.CourtManager.GetCouncil(proposer.Clan);
            var secondCouncil = BannerKingsConfig.Instance.CourtManager.GetCouncil(secondHero.Clan);

            float proposerPeerage = 0f;
            if (proposerCouncil.Peerage != null)
            {
                proposerPeerage = GetPeerageScore(proposerCouncil.Peerage);
            }

            float secondPeerage = 0f;
            if (secondCouncil.Peerage != null)
            {
                secondPeerage = GetPeerageScore(secondCouncil.Peerage);
            }

            result.Add(proposerPeerage - secondPeerage, new TextObject("{=!}Peerage difference"));


            if (proposer.Culture != secondHero.Culture)
            {
                result.AddFactor(-0.2f, GameTexts.FindText("str_culture"));
            }

            return result;
        }

        private float GetPeerageScore(Peerage peerage)
        {
            float score = 0f;
            if (peerage.CanStartElection) score += 15;
            if (peerage.CanVote) score += 10;
            if (peerage.CanGrantKnighthood) score += 10;
            if (peerage.CanHaveFief) score += 20;
            if (peerage.CanHaveCouncil) score += 10;

            return score;
        }

        public override float NpcCoupleMarriageChance(Hero firstHero, Hero secondHero)
        {
            float result = base.NpcCoupleMarriageChance(firstHero, secondHero);
            if (IsMarriageAdequate(firstHero, secondHero).ResultNumber <= 0f)
            {
                result = 0f;
            }

            return result;
        }
    }
}
