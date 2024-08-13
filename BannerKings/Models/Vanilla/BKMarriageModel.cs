using BannerKings.Behaviours.Marriage;
using BannerKings.CampaignContent.Skills;
using BannerKings.Managers.Court;
using BannerKings.Managers.Institutions.Religions;
using BannerKings.Managers.Institutions.Religions.Doctrines;
using BannerKings.Managers.Institutions.Religions.Faiths;
using BannerKings.Managers.Skills;
using BannerKings.Utils;
using BannerKings.Utils.Extensions;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using MarriageModel = BannerKings.Models.Vanilla.Abstract.MarriageModel;

namespace BannerKings.Models.Vanilla
{
    public class BKMarriageModel : MarriageModel
    {
        public override ExplainedNumber IsMarriageAdequate(Hero proposer, 
            Hero secondHero,
            bool isConsort = false, 
            bool explanations = false)
        {
            var result = new ExplainedNumber(0f, explanations);
            if (secondHero.Clan == null || proposer.Clan == null) return new ExplainedNumber(-10000f);

            var proposerScore = GetSpouseScore(proposer).ResultNumber * 1.1f;
            var proposedScore = GetSpouseScore(secondHero).ResultNumber;
            result.Add(proposerScore - proposedScore, new TextObject("{=NeydSXjc}Score differences"));

            ExceptionUtils.TryCatch((System.Action)(() =>
            {
                result.Add(proposer.Culture != secondHero.Culture  ? - 50f : 50f, GameTexts.FindText("str_culture"));

                var proposerReligion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(proposer);
                var proposedReligion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(secondHero);

                if (proposerReligion != proposedReligion)
                {
                    float factor = -50f;
                    if (proposerReligion != null && proposedReligion != null)
                    {
                        FaithStance stance = proposedReligion.GetStance(proposerReligion.Faith);
                        if (stance == FaithStance.Hostile)
                        {
                            factor = -1000f;
                        }
                        else if (stance == FaithStance.Tolerated)
                        {
                            if (!proposerReligion.Faith.MarriageDoctrine.AcceptsUntolerated) factor = -1000f;
                        }
                    }

                    result.Add(factor, new TextObject("{=gyHK87NL}Faith differences"));
                }
                else if (proposerReligion != null && proposedReligion != null)
                {
                    result.Add(50f, proposerReligion.Faith.GetFaithName());

                    if (proposerReligion.HasDoctrine(DefaultDoctrines.Instance.AncestorWorship))
                    {
                        result.Add(50f, DefaultDoctrines.Instance.AncestorWorship.Name);
                    }
                }

                if (proposerReligion != null)
                {
                    CheckReligionSuitability(proposerReligion, proposedReligion, ref result, proposer, secondHero);

                    if (isConsort)
                    {
                        BKMarriageBehavior behavior = TaleWorlds.CampaignSystem.Campaign.Current.GetCampaignBehavior<BKMarriageBehavior>();
                        if (proposerReligion.Faith.MarriageDoctrine.IsConcubinage && 
                        behavior.GetHeroPartners(proposer).Count() >= proposerReligion.Faith.MarriageDoctrine.Consorts)
                        {
                            result.Add(-1000f, new TextObject("{=!}{HERO} is already at the limit of concubines")
                                .SetTextVariable("HERO", proposer.Name));
                        }

                        if (!proposerReligion.Faith.MarriageDoctrine.IsConcubinage &&
                        behavior.GetHeroPartners(proposer).Count() >= proposerReligion.Faith.MarriageDoctrine.Consorts)
                        {
                            result.Add(-1000f, new TextObject("{=!}{HERO} is already at the limit of secondary spouses")
                                .SetTextVariable("HERO", proposer.Name));
                        }
                    }
                }

                if (proposedReligion != null)
                {
                    CheckReligionSuitability(proposedReligion, proposerReligion, ref result, secondHero, proposer);
                }

                if (!isConsort && proposer.Spouse != null && proposer.Spouse.IsAlive)
                {
                    result.Add(-1000f, new TextObject("{=!}{HERO} already has a primary spouse")
                        .SetTextVariable("HERO", proposer.Name));
                }

                if (!base.IsCoupleSuitableForMarriage(proposer, secondHero))
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

                    if (proposer.IsFemale == secondHero.IsFemale)
                    {
                        result.Add(-1000f, new TextObject("{=0w2ADdES}Same sex marriages are not accepted."));
                    }

                    if (!proposer.CanMarry())
                    {
                        result.Add(-1000f, new TextObject("{=!}{HERO} is currently held captive, involved in a battle or army")
                            .SetTextVariable("HERO", proposer.Name));
                    }

                    if (!secondHero.CanMarry())
                    {
                        result.Add(-1000f, new TextObject("{=!}{HERO} is currently held captive, involved in a battle or army")
                            .SetTextVariable("HERO", secondHero.Name));
                    }

                    if (!base.IsClanSuitableForMarriage(proposer.Clan))
                    {
                        result.Add(-1000f, new TextObject("{=vjSgVRAm}{CLAN} is not adequate for marriage.")
                                                    .SetTextVariable("CLAN", proposer.Clan.Name));
                    }

                    if (!base.IsClanSuitableForMarriage(secondHero.Clan))
                    {
                        result.Add(-1000f, new TextObject("{=vjSgVRAm}{CLAN} is not adequate for marriage.")
                                                    .SetTextVariable("CLAN", secondHero.Clan.Name));
                    }
                }
            }),
            GetType().Name);

            return result;
        }

        public override bool IsSuitableForMarriage(Hero maidenOrSuitor)
        {
            if (maidenOrSuitor.IsAlive && maidenOrSuitor.PartyBelongedToAsPrisoner == null && maidenOrSuitor.Spouse == null && maidenOrSuitor.IsLord && !maidenOrSuitor.IsMinorFactionHero && !maidenOrSuitor.IsNotable && maidenOrSuitor.PartyBelongedTo?.MapEvent == null && maidenOrSuitor.PartyBelongedTo?.Army == null)
            {
                if (maidenOrSuitor.IsFemale)
                {
                    return maidenOrSuitor.CharacterObject.Age >= (float)MinimumMarriageAgeFemale;
                }

                return maidenOrSuitor.CharacterObject.Age >= (float)MinimumMarriageAgeMale;
            }

            return false;
        }

        private void CheckReligionSuitability(Religion religion, Religion otherReligion, ref ExplainedNumber result, 
            Hero religionsHero, Hero otherHero)
        {
            int generations = religion.Faith.MarriageDoctrine.Consanguinity;
            if (DiscoverAncestors(religionsHero, generations).Intersect(DiscoverAncestors(otherHero, generations)).Any())
            {
                result.Add(-1000f, new TextObject("{=1d2DhozK}Spouses are too closely related."));
            }

            if (!religion.Faith.MarriageDoctrine.AcceptsUntolerated)
            {
                if (otherHero == null || religion.GetStance(otherReligion.Faith) != FaithStance.Tolerated)
                {
                    result.Add(-1000f, new TextObject("{=!}The {FAITH} faith does not accept spouses from untolerated faiths.")
                        .SetTextVariable("FAITH", religion.Faith.GetFaithName()));
                }
            }
        }

        public override ExplainedNumber GetSpouseScore(Hero hero, bool explanations = false)
        {
            var result = new ExplainedNumber(0f, explanations);
            result.LimitMin(hero.Level * 2f);

            var clan = hero.Clan;            
            result.Add(clan.Tier * 100f, clan.Name);
            result.Add(hero.Level * 10f, GameTexts.FindText("str_level"));

            if (clan.Leader == hero)
            {
                result.Add(150f, GameTexts.FindText("role", "ClanLeader"));
            }
            else
            {
                if (IsClanHeir(hero))
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

            var title = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(clan.Leader);
            if (title != null)
            {
                result.Add(500f / (1.2f * ((float)title.TitleType + 1)), new TextObject("{=KaxKgMg1}{CLAN} holds {TITLE}")
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

            if (hero.IsClanLeader())
            {
                result.AddFactor(BKSkillEffects.Instance.SpouseScore.GetPrimaryValue(
                    hero.GetSkillValue(BKSkills.Instance.Lordship)) * 0.01f,
                    BKSkills.Instance.Lordship.Name);
            }
            else
            {
                result.AddFactor(BKSkillEffects.Instance.SpouseScore.GetSecondaryValue(
                    hero.Clan.Leader.GetSkillValue(BKSkills.Instance.Lordship)) * 0.01f,
                    BKSkills.Instance.Lordship.Name);
            }

            return result;
        }

        public override ExplainedNumber GetDowryValue(Hero hero, bool arrangedMarriage = false, bool explanations = false)
        {
            var result = new ExplainedNumber(0f, explanations);
            result.LimitMin(hero.Level * 250f);

            result.Add(GetClanTierDowry(hero.Clan.Tier), hero.Clan.Name);
            result.Add(hero.Level * 2500f, GameTexts.FindText("str_level"));

            if (IsClanHeir(hero))
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

            if (hero.IsFemale && hero.Spouse != null && hero.Spouse.IsDead)
            {
                result.AddFactor(-0.2f, new TextObject("{=aML45YiV}Widow"));
            }

            return result;
        }

        public override ExplainedNumber GetInfluenceCost(Hero proposed, bool explanations = false)
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

        public bool IsClanHeir(Hero hero)
        {
            var sorted = BannerKingsConfig.Instance.TitleModel.CalculateInheritanceLine(hero.Clan);
            if (sorted.IsEmpty()) return false;
            else return sorted.First().Key == hero;
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

        public override IEnumerable<Hero> DiscoverAncestors(Hero hero, int n)
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
