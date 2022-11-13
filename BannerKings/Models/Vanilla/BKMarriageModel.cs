using BannerKings.Managers.Court;
using HarmonyLib;
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

            var proposerScore = GetSpouseScore(proposer).ResultNumber;
            var proposedScore = GetSpouseScore(secondHero).ResultNumber;

            result.Add(proposerScore - proposedScore, new TextObject("{=!}"));


            if (proposer.Culture != secondHero.Culture)
            {
                result.AddFactor(-0.1f, GameTexts.FindText("str_culture"));
            }
            else
            {
                result.AddFactor(0.05f, GameTexts.FindText("str_culture"));
            }

            var proposerReligion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(proposer);
            var proposedReligion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(secondHero);

            if (proposerReligion != proposedReligion)
            {
                float factor = -0.1f;
                if (proposerReligion == null || proposedReligion == null)
                {

                }

                result.AddFactor(factor, new TextObject("{=!}Faith differences"));
            } 
            else
            {
                result.AddFactor(0.05f, proposerReligion.Faith.GetFaithName());
            }

            return result;
        }

        public ExplainedNumber GetSpouseScore(Hero hero, bool explanations = false)
        {
            var result = new ExplainedNumber(0f, explanations);

            var clan = hero.Clan;
            var title = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(clan.Leader);
            result.Add(clan.Tier * 100f, clan.Name);

            if (clan.Leader == hero)
            {
                result.AddFactor(0.15f, GameTexts.FindText("role", "ClanLeader"));
            }
            else if (title != null)
            {
                float heirScore = float.MinValue;
                Hero heir = null;
                foreach (var candidate in BannerKingsConfig.Instance.TitleModel.GetInheritanceCandidates(clan.Leader))
                {
                    var score = BannerKingsConfig.Instance.TitleModel.GetInheritanceHeirScore(clan.Leader, candidate,
                        title.contract).ResultNumber;
                    if (score > heirScore)
                    {
                        heir = candidate;
                    }
                }

                if (heir == hero)
                {
                    result.AddFactor(0.1f, new TextObject("{=!}{HERO} is the expected heir to {CLAN}")
                        .SetTextVariable("HERO", hero.Name)
                        .SetTextVariable("CLAN", clan.Name));
                }
            }

            var proposerCouncil = BannerKingsConfig.Instance.CourtManager.GetCouncil(clan);
            if (proposerCouncil.Peerage != null)
            {
                result.AddFactor(GetPeerageScore(proposerCouncil.Peerage), new TextObject("{=!}{CLAN} Peerage")
                    .SetTextVariable("CLAN", clan.Name));
            }

            if (title != null)
            {
                result.Add(500f / (1.5f * (float)title.type), new TextObject("{=!}{CLAN} holds {TITLE}")
                    .SetTextVariable("CLAN", clan.Name)
                    .SetTextVariable("TITLE", title.FullName));
            }


            return result;
        }

        private float GetPeerageScore(Peerage peerage)
        {
            float score = 0f;
            if (peerage.CanStartElection) score += 0.015f;
            if (peerage.CanVote) score += 0.01f;
            if (peerage.CanGrantKnighthood) score += 0.01f;
            if (peerage.CanHaveFief) score += 0.02f;
            if (peerage.CanHaveCouncil) score += 0.01f;

            return score;
        }

        public ExplainedNumber GetDowryValue(Hero spouse, bool explanations = false)
        {
            var result = new ExplainedNumber(0f, explanations);


            return result;
        }

        public ExplainedNumber GetInfluenceCost(Hero proposer, Hero proposed, Clan finalClan, bool explanations = false)
        {
            var result = new ExplainedNumber(0f, explanations);

            result.Add(proposed.Clan.Tier * 20f, proposed.Clan.Name);


            return result;
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
