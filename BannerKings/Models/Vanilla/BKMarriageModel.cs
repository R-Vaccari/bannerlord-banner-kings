using BannerKings.Managers.Court;
using BannerKings.Managers.Institutions.Religions.Faiths;
using BannerKings.Managers.Titles;
using BannerKings.Utils.Extensions;
using System.Linq;
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

            var proposerScore = GetSpouseScore(proposer).ResultNumber * 1.1f;
            var proposedScore = GetSpouseScore(secondHero).ResultNumber;
            result.Add(proposerScore - proposedScore, new TextObject("{=!}Score differences"));

            if (proposer.Culture != secondHero.Culture)
            {
                result.Add(-50f, GameTexts.FindText("str_culture"));
            }
            else
            {
                result.Add(50f, GameTexts.FindText("str_culture"));
            }

            var proposerReligion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(proposer);
            var proposedReligion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(secondHero);

            if (proposerReligion != proposedReligion)
            {
                float factor = -50f;
                if (proposerReligion != null && proposedReligion != null)
                {
                    FaithStance stance = proposedReligion.Faith.GetStance(proposerReligion.Faith);
                    if (stance == FaithStance.Hostile)
                    {
                        factor = -100f;
                    }
                    else if (stance == FaithStance.Tolerated)
                    {
                        factor = -20f;
                    }
                }

                result.Add(factor, new TextObject("{=!}Faith differences"));
            } 
            else
            {
                result.Add(50f, proposerReligion.Faith.GetFaithName());
            }

            return result;
        }

        public ExplainedNumber GetSpouseScore(Hero hero, bool explanations = false)
        {
            var result = new ExplainedNumber(0f, explanations);

            var clan = hero.Clan;
            var title = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(clan.Leader);
            result.Add(clan.Tier * 100f, clan.Name);
            result.Add(hero.Level * 10f, GameTexts.FindText("str_level"));

            if (clan.Leader == hero)
            {
                result.Add(150f, GameTexts.FindText("role", "ClanLeader"));
            }
            else if (title != null)
            {
                if (IsClanHeir(title, hero))
                {
                    result.Add(100, new TextObject("{=!}{HERO} is the expected heir to {CLAN}")
                        .SetTextVariable("HERO", hero.Name)
                        .SetTextVariable("CLAN", clan.Name));
                }
            }

            var proposerCouncil = BannerKingsConfig.Instance.CourtManager.GetCouncil(clan);
            if (proposerCouncil.Peerage != null)
            {
                result.Add(GetPeerageScore(proposerCouncil.Peerage), new TextObject("{=!}{CLAN} Peerage")
                    .SetTextVariable("CLAN", clan.Name));
            }

            if (title != null)
            {
                result.Add(500f / (1.5f * (float)title.type), new TextObject("{=!}{CLAN} holds {TITLE}")
                    .SetTextVariable("CLAN", clan.Name)
                    .SetTextVariable("TITLE", title.FullName));
            }

            if (hero.IsCommonBorn())
            {
                result.AddFactor(-0.5f, new TextObject("{=!}Common born"));
            }

            if (hero.Spouse != null && hero.Spouse.IsDead)
            {
                result.AddFactor(-0.2f, new TextObject("{=!}Widow"));
            }


            return result;
        }

        public ExplainedNumber GetDowryValue(Hero hero, bool arrangedMarriage = false, bool explanations = false)
        {
            var result = new ExplainedNumber(0f, explanations);

            result.Add(GetClanTierDowry(hero.Clan.Tier), hero.Clan.Name);
            result.Add(hero.Level * 2500f, GameTexts.FindText("str_level"));

            var title = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(hero.Clan.Leader);
            if (title != null && IsClanHeir(title, hero))
            {
                result.AddFactor(0.5f, new TextObject("{=!}{HERO} is the expected heir to {CLAN}")
                    .SetTextVariable("HERO", hero.Name)
                    .SetTextVariable("CLAN", hero.Clan.Name));
            }

            if (arrangedMarriage)
            {
                result.AddFactor(0.4f, new TextObject("{=!}Arranged marriage"));
            }

            if (hero.IsCommonBorn())
            {
                result.AddFactor(-0.5f, new TextObject("{=!}Common born"));
            }

            if (hero.IsFemale && !(hero.Age >= 18f && hero.Age <= 45f))
            {
                result.AddFactor(-0.2f, new TextObject("{=!}Infertile"));
            }

            if (hero.Spouse != null && hero.Spouse.IsDead)
            {
                result.AddFactor(-0.2f, new TextObject("{=!}Widow"));
            }

            return result;
        }

        public ExplainedNumber GetInfluenceCost(Hero proposed, bool explanations = false)
        {
            var result = new ExplainedNumber(0f, explanations);
            result.Add(proposed.Clan.Tier * 20f, proposed.Clan.Name);


            return result;
        }

        private float GetClanTierDowry(int tier)
        {
            if (tier < 1)
            {
                return 10000;
            }
            else if (tier == 1)
            {
                return 20000;
            }
            else if (tier == 2)
            {
                return 35000;
            }
            else if (tier == 3)
            {
                return 60000;
            }
            else if (tier == 4)
            {
                return 100000;
            }
            else if (tier == 5)
            {
                return 180000;
            }
            else
            {
                return tier * 50000f;
            }
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

        public bool IsClanHeir(FeudalTitle title, Hero hero)
        {
            var sorted = BannerKingsConfig.Instance.TitleModel.CalculateInheritanceLine(hero.Clan);
            return sorted.First().Key == hero;
        }

        private float GetPeerageScore(Peerage peerage)
        {
            float score = 0f;
            if (peerage.CanStartElection) score += 25f;
            if (peerage.CanVote) score += 20f;
            if (peerage.CanGrantKnighthood) score += 20f;
            if (peerage.CanHaveFief) score += 50f;
            if (peerage.CanHaveCouncil) score += 20f;

            return score;
        }
    }
}
