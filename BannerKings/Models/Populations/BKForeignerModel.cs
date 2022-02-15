using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace BannerKings.Models.Populations
{
    public class BKForeignerModel : IBannerKingsModel
    {
        public ExplainedNumber CalculateEffect(Settlement settlement)
        {
            ExplainedNumber result = new ExplainedNumber();
            float baseValue = settlement.IsTown ? 6f : (settlement.IsCastle ? 3f : 1f);
            result.Add(baseValue, new TextObject("Base"));

            if (settlement.IsTown)
            {
                float prosp = settlement.Town.Prosperity;
                result.Add((prosp - 10000f) / 7500f, new TextObject("Prosperity"));
            }

            return result;
        }
    }
}
