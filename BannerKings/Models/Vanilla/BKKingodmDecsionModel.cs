using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;

namespace BannerKings.Models.Vanilla
{
    public class BKKingodmDecsionModel : DefaultKingdomDecisionPermissionModel
    {
        public override bool IsKingSelectionDecisionAllowed(Kingdom kingdom)
        {
            if (BannerKingsConfig.Instance.TitleManager != null)
            {
                var title = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(kingdom);
                if (title != null)
                {
                    var succession = title.Contract.Succession;
                    return succession == Managers.Titles.SuccessionType.Elective_Monarchy || succession == Managers.Titles.SuccessionType.Republic;
                }
            }

            return base.IsKingSelectionDecisionAllowed(kingdom);
        }
    }
}
