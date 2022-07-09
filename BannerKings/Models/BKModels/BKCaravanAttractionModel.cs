using BannerKings.Managers.Court;
using BannerKings.Populations;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace BannerKings.Models.Populations
{
    public class BKCaravanAttractionModel : IBannerKingsModel
    {
        public ExplainedNumber CalculateEffect(Settlement settlement)
        {
            ExplainedNumber result = new ExplainedNumber(1f);

            PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
            result.Add(data.EconomicData.Mercantilism.ResultNumber / 2f, new TextObject("{=!}Mercantilism"));
            result.AddFactor(data.MilitaryData.Militarism.ResultNumber * -1f, new TextObject("{=!}Militarism"));

            BannerKingsConfig.Instance.CourtManager.ApplyCouncilEffect(ref result, settlement.OwnerClan.Leader, CouncilPosition.Steward, 0.15f, true);
            return result;
        }
    }
}
