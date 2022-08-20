using BannerKings.Managers.Populations;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using static BannerKings.Managers.Policies.BKDraftPolicy;
using static BannerKings.Managers.PopulationManager;

namespace BannerKings.Models.BKModels;

public class BKGrowthModel : IGrowthModel
{
    private static readonly float POP_GROWTH_FACTOR = 0.005f;
    private static readonly float SLAVE_GROWTH_FACTOR = 0.0015f;

    public ExplainedNumber CalculateEffect(Settlement settlement, PopulationData data)
    {
        var result = new ExplainedNumber();
        if (settlement.IsVillage || !settlement.IsStarving)
        {
            result.Add(5f, new TextObject("Base"));
            var filledCapacity = data.TotalPop / (float) CalculateSettlementCap(settlement);
            data.Classes.ForEach(popClass =>
            {
                if (popClass.type != PopType.Slaves)
                {
                    result.Add(popClass.count * POP_GROWTH_FACTOR * (1f - filledCapacity),
                        new TextObject("{0} growth"));
                }
            });
        }
        else if (settlement.IsStarving)
        {
            float starvation = -5;
            starvation += (int) (data.TotalPop * -0.007f);
            result.Add(starvation, new TextObject("Starvation"));
            if (settlement.OwnerClan.Leader == Hero.MainHero)
            {
                InformationManager.DisplayMessage(
                    new InformationMessage(string.Format("Population is starving at {0}!", settlement.Name)));
            }
        }

        if (!settlement.IsVillage)
        {
            if (BannerKingsConfig.Instance.PolicyManager.IsPolicyEnacted(settlement, "draft",
                    (int) DraftPolicy.Demobilization))
            {
                result.AddFactor(0.05f, new TextObject("Draft policy"));
            }
        }

        return result;
    }

    public ExplainedNumber CalculateEffect(Settlement settlement)
    {
        return new ExplainedNumber();
    }

    public void CalculateHearthGrowth(Village village, ref ExplainedNumber baseResult)
    {
        /*
        PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(village.Settlement);
        bool boost = BannerKingsConfig.Instance.PolicyManager.IsPolicyEnacted(village.Settlement, PolicyManager.PolicyType.POP_GROWTH);
        int growthFactor = GetDataGrowthFactor(village.Settlement, data, boost, false);
        float taxFactor = 1f;
        TaxType tax = BannerKingsConfig.Instance.PolicyManager.GetSettlementTax(village.Settlement);
        if (tax == TaxType.High)
            taxFactor = 0.8f;
        else if (tax == TaxType.Low)
            taxFactor = 1.2f;
        else if (tax == TaxType.Exemption)
            taxFactor = 1.25f;

        float hearths = MBRandom.RandomFloatRanged(growthFactor / 3, growthFactor / 6) * taxFactor;
        data.UpdatePopulation(village.Settlement, (int)MBRandom.RandomFloatRanged(hearths * 3f, hearths * 6f), PopType.None);
        baseResult.Add(hearths, null);
        */
    }

    public int CalculateSettlementCap(Settlement settlement)
    {
        return settlement.IsTown ? 50000 : settlement.IsCastle ? 8000 : 4000;
    }

    public float GetSettlementFilledCapacity(Settlement settlement)
    {
        var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
        return data.TotalPop / (float) CalculateSettlementCap(settlement);
    }
}