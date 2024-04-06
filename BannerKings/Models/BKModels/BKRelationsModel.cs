using BannerKings.Behaviours.Relations;
using BannerKings.Campaign.Culture;
using BannerKings.Managers.Institutions.Religions.Faiths;
using BannerKings.Managers.Titles;
using BannerKings.Managers.Traits;
using BannerKings.Models.BKModels.Abstract;
using System.Collections.Generic;
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

        public override int PersonalityTraitModifier => 10;

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

            if (target.IsLord)
            {
                if (hero.IsLord)
                {
                    foreach (FeudalTitle title in BannerKingsConfig.Instance.TitleManager.GetAllDeJure(target))
                    {
                        if (title.HeroHasValidClaim(hero))
                        {
                            results.Add(new RelationsModifier(GetClaimRelationImpact(title), 
                                new TextObject("{=!}{HERO} has a claim on title {TITLE}")
                                .SetTextVariable("HERO", hero.Name)
                                .SetTextVariable("TITLE", title.FullName)));
                        }
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
            }
            else if (target.IsNotable)
            {
                if (hero.IsLord)
                {
                    Settlement settlement = target.CurrentSettlement;
                    FeudalTitle title = BannerKingsConfig.Instance.TitleManager.GetTitle(settlement);
                    if (title.DeFacto == hero && title.deJure != hero) 
                        results.Add(new RelationsModifier(IlegitimateFiefModifier,
                        new TextObject("{=!}Ilegitimate rule of {FIEF}")
                        .SetTextVariable("FIEF", settlement.Name)));
                }
            }

            TextObject explanation;
            FaithStance faithStance = Utils.ReligionUtils.CalculateStanceBetweenHeroes(hero, target, out explanation);
            if (faithStance == FaithStance.Hostile) results.Add(new RelationsModifier(HostileFaithModifier, explanation));  
            else if (faithStance == FaithStance.Untolerated) results.Add(new RelationsModifier(UntoleratedFaithModifier, explanation));
            else results.Add(new RelationsModifier(ToleratedFaithModifier, explanation));

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
