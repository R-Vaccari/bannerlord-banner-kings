using BannerKings.Managers.Court;
using BannerKings.Managers.Institutions.Religions.Faiths;
using BannerKings.Managers.Titles;
using BannerKings.Utils;
using BannerKings.Utils.Extensions;
using System.Collections.Generic;
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
            result.Add(proposerScore - proposedScore, new TextObject("{=NeydSXjc}Score differences"));

            ExceptionUtils.TryCatch(() =>
            {
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

                    result.Add(factor, new TextObject("{=gyHK87NL}Faith differences"));
                }
                else if (proposerReligion != null && proposedReligion != null)
                {
                    result.Add(50f, proposerReligion.Faith.GetFaithName());
                }

                if (!IsCoupleSuitableForMarriage(proposer, secondHero))
                {
                    Hero playerCourting = Romance.GetCourtedHeroInOtherClan(proposer, secondHero);
                    if (playerCourting != null && playerCourting != secondHero)
                    {
                        result.Add(-1000f, new TextObject("{=jb7sCNT2}{HERO} is currently courting {COURTING}")
                            .SetTextVariable("HERO", proposer.Name)
                            .SetTextVariable("COURTING", playerCourting.Name));
                    }

                    Hero aiCourting = Romance.GetCourtedHeroInOtherClan(secondHero, proposer);
                    if (aiCourting != null && aiCourting != proposer)
                    {
                        result.Add(-1000f, new TextObject("{=jb7sCNT2}{HERO} is currently courting {COURTING}")
                            .SetTextVariable("HERO", secondHero.Name)
                            .SetTextVariable("COURTING", aiCourting.Name));
                    }

                    if (DiscoverAncestors(proposer, 3).Intersect(DiscoverAncestors(secondHero, 3)).Any())
                    {
                        result.Add(-1000f, new TextObject("{=1d2DhozK}Spouses are too closely related."));
                    }

                    if (proposer.IsFemale == secondHero.IsFemale)
                    {
                        result.Add(-1000f, new TextObject("{=0w2ADdES}Same sex marriages are not accepted."));
                    }

                    if (!proposer.CanMarry())
                    {
                        result.Add(-1000f, new TextObject("{=Ug3zXQdc}{HERO} is not available for marriage.")
                            .SetTextVariable("HERO", proposer.Name));
                    }

                    if (!secondHero.CanMarry())
                    {
                        result.Add(-1000f, new TextObject("{=Ug3zXQdc}{HERO} is not available for marriage.")
                            .SetTextVariable("HERO", secondHero.Name));
                    }

                    if (!IsClanSuitableForMarriage(proposer.Clan))
                    {
                        result.Add(-1000f, new TextObject("{=vjSgVRAm}{CLAN} is not adequate for marriage.")
                                                    .SetTextVariable("CLAN", proposer.Clan.Name));
                    }

                    if (!IsClanSuitableForMarriage(secondHero.Clan))
                    {
                        result.Add(-1000f, new TextObject("{=vjSgVRAm}{CLAN} is not adequate for marriage.")
                                                    .SetTextVariable("CLAN", secondHero.Clan.Name));
                    }
                }
            },
            GetType().Name);

            return result;
        }

        public ExplainedNumber GetSpouseScore(Hero hero, bool explanations = false)
        {
            var result = new ExplainedNumber(0f, explanations);
            result.LimitMin(hero.Level * 2f);

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
                    result.Add(100, new TextObject("{=aoD1zKmp}{HERO} is the expected heir to {CLAN}")
                        .SetTextVariable("HERO", hero.Name)
                        .SetTextVariable("CLAN", clan.Name));
                }
            }

            var proposerCouncil = BannerKingsConfig.Instance.CourtManager.GetCouncil(clan);
            if (proposerCouncil.Peerage != null)
            {
                result.Add(GetPeerageScore(proposerCouncil.Peerage), new TextObject("{=V8eQC16w}{CLAN} Peerage")
                    .SetTextVariable("CLAN", clan.Name));
            }

            if (title != null)
            {
                result.Add(500f / (1.5f * (float)title.TitleType), new TextObject("{=KaxKgMg1}{CLAN} holds {TITLE}")
                    .SetTextVariable("CLAN", clan.Name)
                    .SetTextVariable("TITLE", title.FullName));
            }

            if (hero.IsCommonBorn())
            {
                result.AddFactor(-0.5f, new TextObject("{=9RG3GwJD}Common born"));
            }

            if (hero.CompanionOf != null)
            {
                result.AddFactor(-0.25f, GameTexts.FindText("str_companion"));
            }

            if (hero.Spouse != null && hero.Spouse.IsDead)
            {
                result.AddFactor(-0.2f, new TextObject("{=aML45YiV}Widow"));
            }

            return result;
        }

        public ExplainedNumber GetDowryValue(Hero hero, bool arrangedMarriage = false, bool explanations = false)
        {
            var result = new ExplainedNumber(0f, explanations);
            result.LimitMin(hero.Level * 250f);

            result.Add(GetClanTierDowry(hero.Clan.Tier), hero.Clan.Name);
            result.Add(hero.Level * 2500f, GameTexts.FindText("str_level"));

            var title = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(hero.Clan.Leader);
            if (title != null && IsClanHeir(title, hero))
            {
                result.AddFactor(0.5f, new TextObject("{=aoD1zKmp}{HERO} is the expected heir to {CLAN}")
                    .SetTextVariable("HERO", hero.Name)
                    .SetTextVariable("CLAN", hero.Clan.Name));
            }

            if (arrangedMarriage)
            {
                result.AddFactor(0.4f, new TextObject("{=wxmqHVe1}Arranged marriage"));
            }

            if (hero.IsCommonBorn())
            {
                result.AddFactor(-0.5f, new TextObject("{=9RG3GwJD}Common born"));
            }

            if (hero.CompanionOf != null)
            {
                result.AddFactor(-0.25f, GameTexts.FindText("str_companion"));
            }

            if (hero.IsFemale && !(hero.Age >= 18f && hero.Age <= 45f))
            {
                result.AddFactor(-0.2f, new TextObject("{=2kF9z7Tq}Infertile"));
            }

            if (hero.Spouse != null && hero.Spouse.IsDead)
            {
                result.AddFactor(-0.2f, new TextObject("{=aML45YiV}Widow"));
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

            return result * 1.25f;
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

        private IEnumerable<Hero> DiscoverAncestors(Hero hero, int n)
        {
            if (hero == null)
            {
                yield break;
            }

            yield return hero;
            if (n <= 0)
            {
                yield break;
            }

            foreach (Hero item in DiscoverAncestors(hero.Mother, n - 1))
            {
                yield return item;
            }

            foreach (Hero item2 in DiscoverAncestors(hero.Father, n - 1))
            {
                yield return item2;
            }
        }
    }
}
