using BannerKings.Behaviours.Diplomacy.Groups;
using BannerKings.Behaviours.Diplomacy;
using TaleWorlds.CampaignSystem;
using BannerKings.Utils.Models;

namespace BannerKings.Models.BKModels.Abstract
{
    public abstract class GroupsModel
    {
        public abstract bool WillHeroCreateGroup(DiplomacyGroup group, Hero hero, KingdomDiplomacy diplomacy);
        public abstract BKExplainedNumber CalculateHeroJoinChance(Hero hero, DiplomacyGroup group, KingdomDiplomacy diplomacy, bool explanations = false);
        public abstract BKExplainedNumber CalculateHeroJoinRadicalGroup(Hero hero, RadicalGroup group, KingdomDiplomacy diplomacy, ref BKExplainedNumber result);
    }
}
