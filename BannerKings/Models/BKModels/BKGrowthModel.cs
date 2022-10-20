using BannerKings.Managers.Populations;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Buildings;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using static BannerKings.Managers.Policies.BKDraftPolicy;
using static BannerKings.Managers.PopulationManager;

namespace BannerKings.Models.BKModels
{
    public class BKGrowthModel : IGrowthModel
    {
        private const float POP_GROWTH_FACTOR = 0.005f;

        public ExplainedNumber CalculateEffect(Settlement settlement, PopulationData data)
        {
            var result = new ExplainedNumber(5f, true);

            var filledCapacity = data.TotalPop / CalculateSettlementCap(settlement, data).ResultNumber;
            data.Classes.ForEach(popClass =>
            {
                if (popClass.type != PopType.Slaves)
                {
                    result.Add(popClass.count * POP_GROWTH_FACTOR, new TextObject("{=!}{POPULATION_CLASS} growth")
                        .SetTextVariable("POPULATION_CLASS", Utils.Helpers.GetClassName(popClass.type, settlement.Culture)));
                }
            });

            result.AddFactor(-filledCapacity, new TextObject("{=!}Filled capacity"));
            if (settlement.IsStarving)
            {
                result.AddFactor(-2f, GameTexts.FindText("str_starvation_morale"));
            }
 

            if (settlement.IsVillage)
            {
                return result;
            }

            if (BannerKingsConfig.Instance.PolicyManager.IsPolicyEnacted(settlement, "draft", (int) DraftPolicy.Demobilization))
            {
                result.AddFactor(0.05f, new TextObject("{=!}Drafting policy"));
            }

            return result;
        }

        public ExplainedNumber CalculateSettlementCap(Settlement settlement, PopulationData data)
        {
            var result = new ExplainedNumber(0f, true);

            if (settlement.IsVillage)
            {
                result.Add(settlement.Village.Hearth * 4f, GameTexts.FindText("str_hearths"));
            }

            var land = data.LandData;
            var farmland = land.GetAcreOutput("farmland") * 20f;
            result.Add(land.Farmland * farmland, new TextObject("{=zMPm162W}Farmlands"));

            var pasture = land.GetAcreOutput("pasture") * 20f;
            result.Add(land.Pastureland * pasture, new TextObject("{=ngRhXYj1}Pasturelands"));

            var woods = land.GetAcreOutput("woodland") * 20f;
            result.Add(land.Woodland * woods, new TextObject("{=qPQ7HKgG}Woodlands"));

            var town = settlement.Town;
            if (town != null)
            {
                if (settlement.IsTown)
                {
                    var walls = settlement.Town.Buildings.First(x => x.BuildingType == DefaultBuildingTypes.Fortifications);
                    result.Add(walls.CurrentLevel * 5000f, DefaultBuildingTypes.Fortifications.Name);
                }
                else
                {
                    var walls = settlement.Town.Buildings.First(x => x.BuildingType == DefaultBuildingTypes.Wall);
                    result.Add(walls.CurrentLevel * 1500f, DefaultBuildingTypes.Fortifications.Name);
                }

                result.Add(settlement.Prosperity / 5f, GameTexts.FindText("str_map_tooltip_prosperity"));
            }


            return result;
        }

        public ExplainedNumber CalculateEffect(Settlement settlement)
        {
            return new ExplainedNumber();
        }
    }
}