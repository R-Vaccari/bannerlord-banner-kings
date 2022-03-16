using System;
using TaleWorlds.CampaignSystem;

namespace BannerKings.Models.Populations
{
    public class BKCaravanAttractionModel : IBannerKingsModel
    {
        public ExplainedNumber CalculateEffect(Settlement settlement)
        {
            ExplainedNumber result = new ExplainedNumber(1f);

            return result;
        }
    }
}
