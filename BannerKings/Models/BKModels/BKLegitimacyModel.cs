using BannerKings.Managers.Titles;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;

namespace BannerKings.Models.BKModels;

public class BKLegitimacyModel : IBannerKingsModel
{
    public ExplainedNumber CalculateEffect(Settlement settlement)
    {
        var result = new ExplainedNumber();
        var title = BannerKingsConfig.Instance.TitleManager.GetTitle(settlement);

        if (title != null)
        {
            var foreigner = settlement.Culture != settlement.Owner.Culture;
            if (title.deJure == settlement.Owner)
            {
                result.Add((float) (foreigner ? LegitimacyType.Lawful_Foreigner : LegitimacyType.Lawful));
            }
            else
            {
                result.Add((float) (foreigner ? LegitimacyType.Unlawful_Foreigner : LegitimacyType.Unlawful));
            }
        }

        return result;
    }
}