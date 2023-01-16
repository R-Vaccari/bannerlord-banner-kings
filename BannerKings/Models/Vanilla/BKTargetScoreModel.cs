using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;

namespace BannerKings.Models.Vanilla
{
    public class BKTargetScoreModel : DefaultTargetScoreCalculatingModel
    {
        public override float CurrentObjectiveValue(MobileParty mobileParty)
        {
            float result = base.CurrentObjectiveValue(mobileParty);

            return result;
        }
    }
}
