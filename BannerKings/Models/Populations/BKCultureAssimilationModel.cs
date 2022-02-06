using BannerKings.Populations;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Models
{
    class BKCultureAssimilationModel : ICultureModel
    {
        public ExplainedNumber CalculateEffect(Settlement settlement)
        {
            CultureObject ownerCulture = settlement.OwnerClan.Culture;
            ExplainedNumber baseResult = new ExplainedNumber();

            if (settlement.Culture != ownerCulture)
            {
                baseResult.Add(-0.005f, new TextObject("Natural resistance"));
                float random1 = 0.001f * MBRandom.RandomFloat;
                float random2 = 0.001f * MBRandom.RandomFloat;
                baseResult.Add(random1 - random2, new TextObject("Random factors"));

                if (!settlement.IsVillage && settlement.Town != null)
                    baseResult.Add(0.005f * (1f * (settlement.Town.Security * 0.01f)), new TextObject("Security effect"));

                Hero governor = settlement.IsVillage ? settlement.Village.TradeBound.Town.Governor : settlement.Town.Governor;
                if (governor != null)
                {
                    int skill = settlement.Town.Governor.GetSkillValue(DefaultSkills.Steward);
                    float effect = (float)skill * 0.00005f;
                    if (effect > 0.015f)
                        effect = 0.015f;
                    baseResult.Add(effect, new TextObject("Governor effect"));
                }
            }
            else baseResult.Add(0f, new TextObject("Already assimilated")); ;
            return baseResult;
        }

        public ExplainedNumber CalculateEffect(Settlement settlement, CultureDataClass data)
        {
            CultureObject ownerCulture = settlement.OwnerClan.Culture;
            CultureObject culture = data.Culture;
            ExplainedNumber baseResult = new ExplainedNumber();

            if (culture == ownerCulture && settlement.Culture != ownerCulture)
            {
                baseResult.Add(-0.005f, new TextObject("Natural resistance"));
                float random1 = 0.001f * MBRandom.RandomFloat;
                float random2 = 0.001f * MBRandom.RandomFloat;
                baseResult.Add(random1 - random2, new TextObject("Random factors"));

                if (!settlement.IsVillage && settlement.Town != null)
                    baseResult.Add(0.005f * (1f * (settlement.Town.Security * 0.01f)), new TextObject("Security effect"));

                Hero governor = settlement.IsVillage ? settlement.Village.TradeBound.Town.Governor : settlement.Town.Governor;
                if (governor != null)
                {
                    int skill = settlement.Town.Governor.GetSkillValue(DefaultSkills.Steward);
                    float effect = (float)skill * 0.00005f;
                    if (effect > 0.015f)
                        effect = 0.015f;
                    baseResult.Add(effect, new TextObject("Governor effect"));
                }
            }
            else if (culture == ownerCulture) baseResult.Add(0f, new TextObject("Already assimilated"));
            else 
            {

            }

            return baseResult;
        }
    }
}
