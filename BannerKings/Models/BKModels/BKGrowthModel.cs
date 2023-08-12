using BannerKings.Behaviours;
using BannerKings.Extensions;
using BannerKings.Managers.Populations;
using BannerKings.Managers.Titles.Laws;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Buildings;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using static BannerKings.Managers.Policies.BKDraftPolicy;
using static BannerKings.Managers.PopulationManager;

namespace BannerKings.Models.BKModels
{
    public class BKGrowthModel : IGrowthModel
    {
        private const float POP_GROWTH_FACTOR = 0.005f;

        public ExplainedNumber CalculateEffect(Settlement settlement, PopulationData data, bool descriptions = false)
        {
            var result = new ExplainedNumber(5f, descriptions);

            var filledCapacity = data.TotalPop / CalculateSettlementCap(settlement, data).ResultNumber;
            foreach (var popClass in data.Classes)
            {
                var factor = POP_GROWTH_FACTOR;
                if (popClass.type != PopType.Slaves)
                {
                    factor *= 0.4f;
                }

                result.Add(popClass.count * factor, descriptions ? new TextObject("{=9XTWXhVi}{POPULATION_CLASS} growth")
                    .SetTextVariable("POPULATION_CLASS", Utils.Helpers.GetClassName(popClass.type, settlement.Culture)) : null);
            }

            result.AddFactor(-filledCapacity, new TextObject("{=MRQmKbko}Filled capacity"));
            if (settlement.IsStarving)
            {
                result.AddFactor(-2f, GameTexts.FindText("str_starvation_morale"));
            }

            if (filledCapacity <= 0.15f)
            {
                float factor = 1f;
                if (filledCapacity <= 0.05f) factor = 3f;
                if (filledCapacity <= 0.01f) factor = 2f;
                     
                result.AddFactor(factor, new TextObject("{=mZfEjDn5}Repopulation"));
            }
 
            if (settlement.IsVillage)
            {
                return result;
            }

            if (BannerKingsConfig.Instance.PolicyManager.IsPolicyEnacted(settlement, "draft", (int) DraftPolicy.Demobilization))
            {
                result.AddFactor(0.05f, new TextObject("{=T614zQR8}Drafting policy"));
            }

            return result;
        }

        public ExplainedNumber CalculateSettlementCap(Settlement settlement, PopulationData data, bool descriptions = false)
        {
            var result = new ExplainedNumber(0f, descriptions);

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

                var capital = Campaign.Current.GetCampaignBehavior<BKCapitalBehavior>().GetCapital(town.OwnerClan.Kingdom);
                if (capital == town)
                {
                    result.AddFactor(0.4f, new TextObject("{=fQVyeiJb}Capital"));
                }
            }

            return result;
        }

        public ExplainedNumber CalculatePopulationClassDemand(Settlement settlement, PopType type, bool explanations = false)
        {
            var result = new ExplainedNumber(1f, explanations);
            var faction = settlement.OwnerClan.Kingdom;
            if (faction != null)
            {
                var title = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(faction);
                if (type == PopType.Slaves)
                {
                    if (faction.ActivePolicies.Contains(DefaultPolicies.Serfdom))
                    {
                        result.AddFactor(-0.3f, DefaultPolicies.Serfdom.Name);
                    }

                    if (faction.ActivePolicies.Contains(DefaultPolicies.ForgivenessOfDebts))
                    {
                        result.AddFactor(-0.15f, DefaultPolicies.ForgivenessOfDebts.Name);
                    }

                    if (title != null)
                    {
                        if (title.Contract.IsLawEnacted(DefaultDemesneLaws.Instance.SlaveryManumission))
                        {
                            result.AddFactor(-1f, DefaultDemesneLaws.Instance.SlaveryManumission.Name);
                        }
                        else if (title.Contract.IsLawEnacted(DefaultDemesneLaws.Instance.SlaveryVlandia))
                        {
                            result.AddFactor(-0.3f, DefaultDemesneLaws.Instance.SlaveryVlandia.Name);
                        }
                        else if (title.Contract.IsLawEnacted(DefaultDemesneLaws.Instance.SlaveryAserai))
                        {
                            result.AddFactor(0.5f, DefaultDemesneLaws.Instance.SlaveryAserai.Name);
                        }

                        if (title.Contract.IsLawEnacted(DefaultDemesneLaws.Instance.SlavesAgricultureDuties))
                        {
                            float factor = -0.2f;
                            if (settlement.IsVillage)
                            {
                                if (settlement.Village.IsFarmingVillage())
                                {
                                    factor = 0.3f;
                                }
                            }

                            result.Add(result.ResultNumber * factor, DefaultDemesneLaws.Instance.SlavesAgricultureDuties.Name);
                        }
                        else if (title.Contract.IsLawEnacted(DefaultDemesneLaws.Instance.SlavesHardLabor)) 
                        {
                            float factor = -0.2f;
                            if (settlement.IsVillage)
                            {
                                if (settlement.Village.IsMiningVillage())
                                {
                                    factor = 0.3f;
                                }
                            }

                            result.Add(result.ResultNumber * factor, DefaultDemesneLaws.Instance.SlavesHardLabor.Name);
                        }
                        else
                        {
                            float factor = -0.2f;
                            if (settlement.IsTown)
                            {
                                factor = 0.3f;
                            }

                            result.Add(result.ResultNumber * factor, DefaultDemesneLaws.Instance.SlavesDomesticDuties.Name);
                        }
                    }
                }
                
                if (type == PopType.Nobles)
                {
                    if (faction.ActivePolicies.Contains(DefaultPolicies.Citizenship))
                    {
                        result.AddFactor(0.1f, DefaultPolicies.Citizenship.Name);
                    }
                }

                if (type == PopType.Serfs)
                {
                    if (faction.ActivePolicies.Contains(DefaultPolicies.Serfdom))
                    {
                        result.Add(0.25f, DefaultPolicies.Serfdom.Name);
                    }

                    if (title != null)
                    {
                        if (title.Contract.IsLawEnacted(DefaultDemesneLaws.Instance.TenancyFull))
                        {
                            result.AddFactor(-1f, DefaultDemesneLaws.Instance.TenancyFull.Name);
                        }
                        else if (title.Contract.IsLawEnacted(DefaultDemesneLaws.Instance.TenancyMixed))
                        {
                            result.AddFactor(-0.5f, DefaultDemesneLaws.Instance.TenancyMixed.Name);
                        }
                    }
                }

                if (type == PopType.Tenants)
                {
                    if (title != null)
                    {
                        if (title.Contract.IsLawEnacted(DefaultDemesneLaws.Instance.TenancyNone))
                        {
                            result.AddFactor(-0.9f, DefaultDemesneLaws.Instance.TenancyNone.Name);
                        }
                        else if (title.Contract.IsLawEnacted(DefaultDemesneLaws.Instance.TenancyMixed))
                        {
                            result.AddFactor(-0.5f, DefaultDemesneLaws.Instance.TenancyMixed.Name);
                        }
                    }
                }
            }

            return result;
        }

        public ExplainedNumber CalculateSlavePrice(Settlement settlement, bool explanations = false)
        {
            var result = new ExplainedNumber(150f, explanations);
            result.LimitMin(0f);

            result.AddFactor(CalculatePopulationClassDemand(settlement, PopType.Slaves).ResultNumber - 1f,
                new TextObject("{=Rq9KfmQ8}{FACTION} demand")
                .SetTextVariable("FACTION", settlement.MapFaction.Name));

            var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
            var dic = GetDesiredPopTypes(settlement);
            float fraction = MathF.Clamp(data.GetCurrentTypeFraction(PopType.Slaves), 0f, 1f);
            float medium = (dic[PopType.Slaves][0] + dic[PopType.Slaves][1]) / 2f;

            result.AddFactor(medium - fraction, new TextObject("{=G15w2C46}Local demand"));

            return result;
        }

        public ExplainedNumber CalculateEffect(Settlement settlement)
        {
            return new ExplainedNumber();
        }
    }
}