using BannerKings.Managers.Titles;
using TaleWorlds.CampaignSystem;
using static BannerKings.Managers.TitleManager;

namespace BannerKings.Models
{
    class BKLegitimacyModel : IBannerKingsModel
    {
        public ExplainedNumber CalculateEffect(Settlement settlement)
        {
            ExplainedNumber result = new ExplainedNumber();
            FeudalTitle title = BannerKingsConfig.Instance.TitleManager.GetTitle(settlement);

            if (title != null)
            {
                bool foreigner = settlement.Culture != settlement.Owner.Culture;
                if (title.deJure == settlement.Owner)
                    result.Add((float)(foreigner ? LegitimacyType.Lawful_Foreigner : LegitimacyType.Lawful));
                else result.Add((float)(foreigner ? LegitimacyType.Unlawful_Foreigner : LegitimacyType.Unlawful));
            }

            return result;
        }
    }
}
