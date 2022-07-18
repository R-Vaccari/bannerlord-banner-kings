using BannerKings.Managers.Court;
using BannerKings.Managers.Populations.Villages;
using BannerKings.Populations;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using TaleWorlds.Localization;
using static BannerKings.Managers.PopulationManager;
using BannerKings.Populations;
using BannerKings.Managers.Populations.Villages;
using TaleWorlds.Library;
using BannerKings.Managers.Titles;

namespace BannerKings.Models
{
    public class BKInfluenceModel : DefaultClanPoliticsModel
    {

        public float GetRejectKnighthoodCost(Clan clan) => 10f + (CalculateInfluenceChange(clan, false).ResultNumber * 0.025f * (float)CampaignTime.DaysInYear);
        

        public override ExplainedNumber CalculateInfluenceChange(Clan clan, bool includeDescriptions = false)
        {
            ExplainedNumber baseResult = base.CalculateInfluenceChange(clan, includeDescriptions);

            float generalSupport = 0f;
            float generalAutonomy = 0f;
            float i = 0;
            foreach (Settlement settlement in clan.Settlements)
            {
                if (BannerKingsConfig.Instance.PopulationManager != null && BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(settlement))
                {
                    PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
                    if (BannerKingsConfig.Instance.AI.AcceptNotableAid(clan, data))
                        foreach (Hero notable in data.Settlement.Notables)
                            if (notable.SupporterOf == clan && notable.Gold > 5000)
                                baseResult.Add(-1f, new TextObject("{=!}Aid from {NOTABLE}").SetTextVariable("NOTABLE", notable.Name));

                    generalSupport  += data.NotableSupport.ResultNumber - 0.5f;
                    generalAutonomy += -0.5f * data.Autonomy;
                    i++;

                    ExplainedNumber settlementResult = CalculateSettlementInfluence(settlement, data);
                    baseResult.Add(settlementResult.ResultNumber, settlement.Name);
                    if (settlement.IsVillage && BannerKingsConfig.Instance.TitleManager != null)
                    {
                        FeudalTitle title = BannerKingsConfig.Instance.TitleManager.GetTitle(settlement);
                        if (title.deJure != null)
                        {
                            Clan deJureClan = title.deJure.Clan;
                            if (title.deJure != deJureClan.Leader && settlement.OwnerClan == deJureClan) 
                                BannerKingsConfig.Instance.TitleManager.AddKnightInfluence(title.deJure, settlementResult.ResultNumber * 0.1f);
                        }
                    }
                }
            }

            if (BannerKingsConfig.Instance.CourtManager != null)
            {
                CouncilMember position = BannerKingsConfig.Instance.CourtManager.GetHeroPosition(clan.Leader);
                if (position != null)
                    baseResult.Add(position.IsCorePosition(position.Position) ? 1f : 0.5f, new TextObject("{=!}Councillor role"));
            }

            if (i > 0)
            {
                float finalSupport = MBMath.ClampFloat(generalSupport / i, -0.5f, 0.5f);
                float finalAutonomy = MBMath.ClampFloat(generalAutonomy / i, -0.5f, 0f);
                if (finalSupport != 0f) baseResult.AddFactor(finalSupport, new TextObject("{=!}Overall notable support"));
                if (finalAutonomy != 0f) baseResult.AddFactor(finalAutonomy, new TextObject("{=!}Overall settlement autonomy"));
            }

            return baseResult;
        }

        public ExplainedNumber CalculateSettlementInfluence(Settlement settlement, PopulationData data)
        {
            ExplainedNumber settlementResult = new ExplainedNumber(0f, true);
            float nobles = data.GetTypeCount(PopType.Nobles);
            settlementResult.Add(MBMath.ClampFloat(nobles * 0.01f, 0f, 12f), new TextObject(string.Format("Nobles influence from {0}", settlement.Name)));

            VillageData villageData = data.VillageData;
            if (villageData != null)
            {
                float manor = villageData.GetBuildingLevel(DefaultVillageBuildings.Instance.Manor);
                if (manor > 0)
                    settlementResult.AddFactor(manor == 3 ? 0.5f : manor * 0.15f, new TextObject("{=!}Manor"));
            }

            if (BannerKingsConfig.Instance.PopulationManager.PopSurplusExists(settlement, PopType.Nobles, true))
            {
                float result = settlementResult.ResultNumber;
                float extra = BannerKingsConfig.Instance.PopulationManager.GetPopCountOverLimit(settlement, PopType.Nobles);
                settlementResult.Add(MBMath.ClampFloat(extra * -0.01f, result * -0.5f, -0.1f), new TextObject(string.Format("Excess noble population at {0}", settlement.Name)));
            }

            return settlementResult;
        }
    }
}
