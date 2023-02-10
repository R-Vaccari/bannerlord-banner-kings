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
            return new BKExplainedNumber(hero, explanations);
        }

        public BKExplainedNumber CalculateHeroJoinChance(Hero hero, InterestGroup group)
        {

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

            return false;
        }
    }
}
