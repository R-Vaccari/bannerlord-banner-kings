using BannerKings.Managers.Populations;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Models.BKModels
{
    public class BKCultureAssimilationModel : ICultureModel
    {
        public ExplainedNumber CalculateEffect(Settlement settlement)
        {
            var ownerCulture = settlement.OwnerClan.Culture;
            var baseResult = new ExplainedNumber();

            if (settlement.Culture != ownerCulture)
            {
                baseResult.Add(-0.005f, new TextObject("Natural resistance"));
                var random1 = 0.001f * MBRandom.RandomFloat;
                var random2 = 0.001f * MBRandom.RandomFloat;
                baseResult.Add(random1 - random2, new TextObject("Random factors"));

                if (!settlement.IsVillage && settlement.Town != null)
                {
                    baseResult.Add(0.005f * (1f * (settlement.Town.Security * 0.01f)), new TextObject("Security effect"));
                }

                var governor = settlement.IsVillage ? settlement.Village.Bound.Town.Governor : settlement.Town.Governor;
                if (governor != null)
                {
                    var skill = governor.GetSkillValue(DefaultSkills.Steward);
                    var effect = skill * 0.00005f;
                    if (effect > 0.015f)
                    {
                        effect = 0.015f;
                    }

                    baseResult.Add(effect, new TextObject("Governor effect"));
                }
            }
            else
            {
                baseResult.Add(0f, new TextObject("Already assimilated"));
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
            if (!foreigner)
            {
                if (dataCulture == ownerCulture)
                {
                    var acceptance = data.Acceptance;

                    if (data.Assimilation < 1f - popData.Foreigner.ResultNumber)
                    {
                        result.Add(-0.005f, new TextObject("Natural resistance"));
                        var random1 = 0.001f * MBRandom.RandomFloat;
                        var random2 = 0.001f * MBRandom.RandomFloat;
                        result.Add(random1 - random2, new TextObject("Random factors"));
                        result.Add(0.005f * acceptance, new TextObject("Cultural acceptance"));

                        if (!settlement.IsVillage && settlement.Town != null)
                        {
                            result.Add(0.005f * (1f * (settlement.Town.Security * 0.01f)),
                                new TextObject("Security effect"));
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

                            result.Add(effect, new TextObject("Governor effect"));
                        }

                        if (dataCulture == popData.CultureData.DominantCulture)
                        {
                            result.Add(0.005f);
                        }
                    }
                    else if (data.Assimilation < 1f - popData.Foreigner.ResultNumber)
                    {
                        result.Add(-0.005f, new TextObject("Over limit"));
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
            }


            return result;
        }
    }
}