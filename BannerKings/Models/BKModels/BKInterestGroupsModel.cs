using BannerKings.Behaviours.Diplomacy;
using BannerKings.Behaviours.Diplomacy.Groups;
using BannerKings.Managers.Titles;
using BannerKings.Utils.Models;
using TaleWorlds.CampaignSystem;

namespace BannerKings.Models.BKModels
{
    public class BKInterestGroupsModel
    {
        public BKExplainedNumber CalculateHeroInfluence(InterestGroup group, Hero hero, bool explanations = false)
        {
            var result = new BKExplainedNumber(0f, explanations);

            if (hero.IsLord)
            {
                result.Add(hero.Clan.Tier * hero.Clan.Tier * 10f);
                var title = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(hero);
            }

            if (hero.IsNotable)
            {
                result.Add(hero.Power);
            }

            return result;
        }

        public BKExplainedNumber CalculateHeroJoinChance(Hero hero, InterestGroup group)
        {
            var result = new BKExplainedNumber(0f, false);
            result.LimitMin(-1f);
            result.LimitMax(1f);
            if (hero.IsLord && !group.AllowsNobles)
            {
                return result;
            }

            if (!hero.IsLord && !group.AllowsCommoners)
            {
                return result;
            }

            if (hero.IsLord && hero.MapFaction.Leader == hero)
            {
                return result;
            }

            if (group.Equals(DefaultInterestGroup.Instance.Royalists))
            {
                Hero leader = hero.MapFaction.Leader;
                float relation = hero.GetRelation(leader);
                result.Add(relation * 0.3f);
            }

            if (group.Equals(DefaultInterestGroup.Instance.Oligarchists))
            {
                result.Add(hero.Clan.Tier * 0.5f);
                var title = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(hero);
                if (title != null)
                {
                    result.Add((5f - (int)title.type) * 0.25f);
                }
            }

            if (group.PreferredOccupations.Contains(hero.Occupation))
            {
                result.Add(0.2f);
            }
            
            result.AddFactor(hero.GetTraitLevel(group.MainTrait) * 0.15f);
            return result;
        }

        public bool IsGroupAdequateForKingdom(KingdomDiplomacy diplomacy, InterestGroup group)
        {
            var title = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(diplomacy.Kingdom);
            if (title != null)
            {
                if (group.Equals(DefaultInterestGroup.Instance.Royalists) && (title.contract.Government == GovernmentType.Feudal 
                    || title.contract.Government == GovernmentType.Imperial))
                {
                    return true;
                }

                if (group.Equals(DefaultInterestGroup.Instance.Traditionalists) && title.contract.Government == GovernmentType.Tribal)
                {
                    return true;
                }
            }

            if (group.Equals(DefaultInterestGroup.Instance.Commoners) || group.Equals(DefaultInterestGroup.Instance.Oligarchists))
            {
                return true;
            }

            if (group.Equals(DefaultInterestGroup.Instance.Zealots))
            {
                return diplomacy.Religion != null;
            }

            return false;
        }
    }
}
