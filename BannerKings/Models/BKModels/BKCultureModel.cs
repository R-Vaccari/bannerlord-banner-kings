using BannerKings.Managers.Buildings;
using BannerKings.Managers.Populations;
using BannerKings.Managers.Skills;
using BannerKings.Managers.Titles;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Models.BKModels
{
    public class BKCultureModel : ICultureModel
    {

        public ExplainedNumber GetConversionCost(Hero notable, Hero converter)
        {
            var result = new ExplainedNumber(30f, false);
            result.LimitMin(30f);
            result.LimitMax(150f);

            if (!notable.IsNotable || notable.CurrentSettlement == null)
            {
                return new ExplainedNumber(0f);
            }

            result.Add(notable.GetRelation(converter) * -0.1f);
            result.Add(GetNotableFactor(notable, notable.CurrentSettlement));

            var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(notable.CurrentSettlement);
            var acceptance = data.CultureData.GetAcceptance(converter.Culture);
            result.AddFactor(1f - acceptance);

            return result;
        }

        public ExplainedNumber CalculateCultureWeight(Settlement settlement, CultureDataClass data, float baseWeight = 0f)
        {
            var result = new ExplainedNumber(baseWeight, true);

            foreach (var notable in settlement.Notables)
            {
                if (notable.Culture == data.Culture)
                {
                    result.Add(GetNotableFactor(notable, settlement), notable.Name);
                }
            }

            if (data.Culture == settlement.Culture)
            {
                result.Add(30f, new TextObject("{=2wOt5txz}Natural resistance"));
            }

            result.Add(data.Assimilation * 50f, new TextObject("{=D3trXTDz}Cultural Assimilation"));

            var owner = settlement.Owner;
            if (owner != null)
            {
                if (data.Culture == owner.Culture)
                {
                    result.Add(10f, new TextObject("{=LHFoaUGo}Owner Culture"));
                }
            }

            if (settlement.IsTown || settlement.IsCastle)
            {
                foreach (var village in settlement.BoundVillages)
                {
                    if (village.Settlement.Culture == data.Culture)
                    {
                        result.Add(15f, village.Name);
                    }
                }

                if (settlement.Town.Governor != null)
                {
                    var governor = settlement.Town.Governor;
                    if (data.Culture == governor.Culture)
                    {
                        result.Add(10f, new TextObject("{=gafTzKhz}Governor effect"));
                    }
                }

                var theater = settlement.Town.Buildings.FirstOrDefault(x => x.BuildingType == BKBuildings.Instance.Theater);
                if (theater != null && theater.CurrentLevel > 0 && data.Culture == owner.Culture)
                {
                    result.Add(theater.CurrentLevel * 5f, BKBuildings.Instance.Theater.Name);
                }
            }
            else if (settlement.IsVillage)
            {
                var village = settlement.Village;
                if (village != null && village.TradeBound != null)
                {
                    if (data.Culture == settlement.Village.TradeBound.Culture)
                    {
                        result.Add(20f, settlement.Village.TradeBound.Name);
                    }
                }
            }

            LegitimacyType legitimacy = (LegitimacyType)BannerKingsConfig.Instance.LegitimacyModel.CalculateEffect(settlement).ResultNumber;
            if (legitimacy == LegitimacyType.Unlawful || legitimacy == LegitimacyType.Unlawful_Foreigner)
            {
                result.Add(-5f, new TextObject("{=!}Unlawful owner"));
            }

            return result;
        }


        public float GetNotableFactor(Hero notable, Settlement settlement)
        {
            var totalPower = 0f;
            foreach (var hero in settlement.Notables)
            {
                totalPower += hero.Power;
            }

            return (settlement.Notables.Count * 15f) * (notable.Power / totalPower);
        }

        public ExplainedNumber CalculateEffect(Settlement settlement)
        {
            var ownerCulture = settlement.OwnerClan.Culture;
            var baseResult = new ExplainedNumber();

            if (settlement.Culture != ownerCulture)
            {
                baseResult.Add(-0.005f, new TextObject("{=2wOt5txz}Natural resistance"));
                var random1 = 0.001f * MBRandom.RandomFloat;
                var random2 = 0.001f * MBRandom.RandomFloat;
                baseResult.Add(random1 - random2, new TextObject("{=wJV3Gdc1}Random factors"));

                if (!settlement.IsVillage && settlement.Town != null)
                {
                    baseResult.Add(0.005f * (1f * (settlement.Town.Security * 0.01f)), new TextObject("{=a2GE4xwy}Security effect"));
                }

                var governor = settlement.IsVillage 
                    ? settlement.Village.Bound.Town.Governor 
                    : settlement.Town.Governor;
                if (governor != null)
                {
                    var skill = governor.GetSkillValue(DefaultSkills.Steward);
                    var effect = skill * 0.00005f;
                    if (effect > 0.015f)
                    {
                        effect = 0.015f;
                    }

                    baseResult.Add(effect, new TextObject("{=gafTzKhz}Governor effect"));

                    var lordshipTraditionalistPerk = BKPerks.Instance.LordshipTraditionalist;
                    if (governor.GetPerkValue(BKPerks.Instance.LordshipTraditionalist))
                    {
                        baseResult.AddFactor(0.1f, lordshipTraditionalistPerk.Name);
                    }
                }
            }
            else
            {
                baseResult.Add(0f, new TextObject("{=uHDDG1Vq}Already assimilated"));
            }

            ;
            return baseResult;
        }

        public ExplainedNumber CalculateEffect(Settlement settlement, CultureDataClass data)
        {
            var popData = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
            var ownerCulture = settlement.OwnerClan.Culture;
            var dataCulture = data.Culture;
            var result = new ExplainedNumber(0f);

            var foreigner = dataCulture != settlement.Culture && dataCulture != ownerCulture;
            if (foreigner)
            {
                return result;
            }

            if (dataCulture == ownerCulture)
            {
                var acceptance = data.Acceptance;

                if (data.Assimilation < 1f - popData.Foreigner.ResultNumber)
                {
                    result.Add(-0.005f, new TextObject("{=2wOt5txz}Natural resistance"));
                    var random1 = 0.001f * MBRandom.RandomFloat;
                    var random2 = 0.001f * MBRandom.RandomFloat;
                    result.Add(random1 - random2, new TextObject("{=wJV3Gdc1}Random factors"));
                    result.Add(0.005f * acceptance, new TextObject("{=2qB0s9H9}Cultural acceptance"));

                    if (!settlement.IsVillage && settlement.Town != null)
                    {
                        result.Add(0.005f * (1f * (settlement.Town.Security * 0.01f)),
                            new TextObject("{=a2GE4xwy}Security effect"));
                    }

                    var governor = settlement.IsVillage
                        ? settlement.Village.Bound.Town.Governor
                        : settlement.Town.Governor;
                    if (governor != null)
                    {
                        var skill = governor.GetSkillValue(DefaultSkills.Steward);
                        var effect = skill * 0.00005f;
                        if (effect > 0.015f)
                        {
                            effect = 0.015f;
                        }

                        result.Add(effect, new TextObject("{=gafTzKhz}Governor effect"));

                        var lordshipTraditionalistPerk = BKPerks.Instance.LordshipTraditionalist;
                        if (governor.GetPerkValue(BKPerks.Instance.LordshipTraditionalist))
                        {
                            result.AddFactor(0.1f, lordshipTraditionalistPerk.Name);
                        }
                    }

                    if (dataCulture == popData.CultureData.DominantCulture)
                    {
                        result.Add(0.005f);
                    }
                }
                else if (data.Assimilation < 1f - popData.Foreigner.ResultNumber)
                {
                    result.Add(-0.005f, new TextObject("{=HuoR2sGE}Over limit"));
                }

                else
                {
                    return result;
                }
            }
            else
            {
                CultureDataClass ownerClass = null;
                foreach (var dataClass in popData.CultureData.Cultures)
                {
                    if (dataClass.Culture == ownerCulture)
                    {
                        ownerClass = dataClass;
                    }
                }

                if (ownerClass != null)
                {
                    var assimChange = CalculateEffect(settlement, ownerClass).ResultNumber;
                    result.Add(assimChange);
                }
            }


            return result;
        }
    }
}