using BannerKings.Behaviours.Relations;
using BannerKings.CampaignContent.Culture;
using BannerKings.CampaignContent.Traits;
using BannerKings.Managers.Institutions.Religions;
using BannerKings.Managers.Institutions.Religions.Faiths;
using BannerKings.Managers.Skills;
using BannerKings.Managers.Titles;
using BannerKings.Models.BKModels.Abstract;
using System.Collections.Generic;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Models.BKModels
{
    public class BKRelationsModel : RelationsModel
    {
        public override int HostileFaithModifier => -25;

        public override int UntoleratedFaithModifier => -5;

        public override int ToleratedFaithModifier => 5;

        public override int IlegitimateFiefModifier => -15;

        public override int CloseFamilyModifier => 10;

        public override int PersonalityTraitModifier => 8;
        public override int VassalModifier => 5;

        public override List<RelationsModifier> CalculateModifiers(HeroRelations heroRelations, Hero target)
        {
            Hero hero = heroRelations.Hero;
            List<RelationsModifier> results = new List<RelationsModifier>(5)
            {
                new RelationsModifier(DefaultCulturalStandings.Instance.GetRelation(hero.Culture, target.Culture),
                GameTexts.FindText("str_culture"))
            };

            results.AddRange(heroRelations.GetRelations(target));

            if (Utils.Helpers.IsCloseFamily(target, hero)) 
                results.Add(new RelationsModifier(CloseFamilyModifier, new TextObject("{=!}Close family")));

            if (hero.IsLord)
            {
                if (target.IsLord)
                {
                    foreach (FeudalTitle title in BannerKingsConfig.Instance.TitleManager.GetAllDeJure(hero))
                    {
                        if (title.HeroHasValidClaim(target))
                        {
                            results.Add(new RelationsModifier(GetClaimRelationImpact(title), 
                                new TextObject("{=!}{HERO} has a claim on title {TITLE}")
                                .SetTextVariable("HERO", target.Name)
                                .SetTextVariable("TITLE", title.FullName)));
                        }
                    }

                    if (!target.IsClanLeader)
                    {
                        Hero leader = target.Clan.Leader;
                        if (leader != null) results.Add(new RelationsModifier(
                            (int)(hero.GetRelation(leader) / 3f),
                            new TextObject("{=!}Relation with clan leader {LEADER}")
                            .SetTextVariable("LEADER", leader.Name)));
                    }
                    else
                    {
                        Kingdom kingdom = hero.Clan.Kingdom;
                        if (kingdom != null)
                        {
                            if (target.Clan.Kingdom == kingdom)
                            {
                                if (hero.Clan == kingdom.RulingClan)
                                {
                                    results.Add(new RelationsModifier(VassalModifier, new TextObject("{=!}Vassal")));
                                }
                                else if (target.Clan == kingdom.RulingClan)
                                {
                                    ExplainedNumber justRuler = new ExplainedNumber(0f);
                                    Utils.Helpers.ApplyTraitEffect(target, DefaultTraitEffects.Instance.JustRuler, ref justRuler);
                                    if (justRuler.ResultNumber != 0f)
                                    {
                                        results.Add(new RelationsModifier((int)justRuler.ResultNumber, 
                                            DefaultTraitEffects.Instance.JustRuler.Trait.Name));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else if (hero.IsNotable)
            {
                if (target.IsLord)
                {
                    Settlement settlement = hero.CurrentSettlement;
                    FeudalTitle title = BannerKingsConfig.Instance.TitleManager.GetTitle(settlement);
                    if (title.DeFacto == hero && title.deJure != hero) 
                        results.Add(new RelationsModifier(IlegitimateFiefModifier,
                        new TextObject("{=!}Illegitimate rule of {FIEF}")
                        .SetTextVariable("FIEF", settlement.Name)));

                    ExplainedNumber justLord = new ExplainedNumber(0f);
                    Utils.Helpers.ApplyTraitEffect(target, DefaultTraitEffects.Instance.JustLord, ref justLord);
                    if (justLord.ResultNumber != 0f)
                    {
                        results.Add(new RelationsModifier((int)justLord.ResultNumber,
                            DefaultTraitEffects.Instance.JustLord.Trait.Name));
                    }

                    if (hero.IsPreacher)
                    {
                        Religion heroReligion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(hero);
                        Religion targetReligion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(target);
                        if (heroReligion != null && heroReligion.Equals(targetReligion))
                        {
                            if (target.GetPerkValue(BKPerks.Instance.TheologyLithurgy))
                            {
                                results.Add(new RelationsModifier((int)BKPerks.Instance.TheologyLithurgy.SecondaryBonus,
                                   BKPerks.Instance.TheologyLithurgy.Name));
                            }

                            if (hero == heroReligion.FaithLeader)
                            {
                                if (target.GetPerkValue(BKPerks.Instance.TheologyArchPriest))
                                {
                                    results.Add(new RelationsModifier((int)BKPerks.Instance.TheologyArchPriest.PrimaryBonus,
                                       BKPerks.Instance.TheologyArchPriest.Name));
                                }
                            }
                        }
                    }
                }
            }

            if (!DefaultReligions.Instance.All.IsEmpty())
            {
                TextObject explanation;
                FaithStance faithStance = Utils.ReligionUtils.CalculateStanceBetweenHeroes(hero, target, out explanation);
                if (faithStance == FaithStance.Hostile) results.Add(new RelationsModifier(HostileFaithModifier, explanation));
                else if (faithStance == FaithStance.Untolerated) results.Add(new RelationsModifier(UntoleratedFaithModifier, explanation));
                else results.Add(new RelationsModifier(ToleratedFaithModifier, explanation));
            }

            foreach (TraitObject trait in BKTraits.Instance.PersonalityTraits)
            {
                float value = PersonalityTraitModifier * hero.GetTraitLevel(trait);
                value *= target.GetTraitLevel(trait);
                if (value != 0f) results.Add(new RelationsModifier((int)value, 
                    new TextObject("{=!}Personality trait ({TRAIT})")
                    .SetTextVariable("TRAIT", trait.Name)));
            }

            foreach (TraitObject trait in BKTraits.Instance.PoliticalTraits)
            {
                float diff = MathF.Abs(hero.GetTraitLevel(trait) - target.GetTraitLevel(trait));
                float value = MBMath.Map(diff, 0, 20, 10, -20);
                if (value != 0f) results.Add(new RelationsModifier((int)value,
                    new TextObject("{=!}Political trait ({TRAIT})")
                    .SetTextVariable("TRAIT", trait.Name)));
            }

            ExplainedNumber honor = new ExplainedNumber(0f);
            Utils.Helpers.ApplyTraitEffect(target, DefaultTraitEffects.Instance.HonorRelation, ref honor);
            if (honor.ResultNumber != 0f)
            {
                results.Add(new RelationsModifier((int)honor.ResultNumber,
                    DefaultTraitEffects.Instance.HonorRelation.Trait.Name));
            }

            return results;
        }

        public override ExplainedNumber CalculateModifiersExplained(HeroRelations heroRelations, Hero target, bool explained = false)
        {
            ExplainedNumber result = new ExplainedNumber(0f, explained);
            foreach (var modifier in CalculateModifiers(heroRelations, target))
            {
                result.Add(modifier.Relation, modifier.Explanation);
            }

            return result;
        }

        public override ExplainedNumber CalculateModifiersExplained(List<RelationsModifier> modifiers, bool explained = false)
        {
            ExplainedNumber result = new ExplainedNumber(0f, explained);
            foreach (var modifier in modifiers)
            {
                result.Add(modifier.Relation, modifier.Explanation);
            }

            return result;
        }

        public override int GetClaimRelationImpact(FeudalTitle title)
        {
            if (title.IsSovereignLevel) return -90;
            else if (title.TitleType == TitleType.Dukedom) return -70;
            else if (title.TitleType == TitleType.County) return -50;
            else if (title.TitleType == TitleType.Dukedom) return -35;
            else return -20;
        }
    }
}
