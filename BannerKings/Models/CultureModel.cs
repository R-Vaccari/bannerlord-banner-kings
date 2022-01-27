using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using static BannerKings.Managers.PopulationManager;

namespace BannerKings.Models
{
    class CultureModel : GameModel
    {

        public void CalculateAssimilationChange(Settlement settlement)
        {
            
            if (BannerKingsConfig.Instance.PopulationManager != null && BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(settlement))
            {
                float result = GetAssimilationChange(settlement);
                PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
                float finalResult = data.Assimilation + result;
                if (finalResult > 1f)
                    finalResult = 1f;
                else if (finalResult < 0f)
                    finalResult = 0f;
                data.Assimilation = finalResult;

                if (data.Assimilation == 1f && settlement.Owner != null)
                    settlement.Culture = settlement.Owner.Culture;
            }
        }

        public float GetAssimilationChange(Settlement settlement)
        {
            CultureObject ownerCulture = settlement.OwnerClan.Culture;
            float random1 = 0.001f * MBRandom.RandomFloat;
            float random2 = 0.001f * MBRandom.RandomFloat;
            float change = -0.005f + random1 - random2;
            
            if (!settlement.IsVillage && settlement.Town != null)
            change += 0.005f * (1f * (settlement.Town.Security * 0.01f));

            if (settlement.Culture != ownerCulture)
            {
                if (!settlement.IsVillage && settlement.Town != null)
                    if (settlement.Town.Governor != null && settlement.Town.Governor.Culture == ownerCulture)
                    {
                        change += 0.005f;
                        int skill = settlement.Town.Governor.GetSkillValue(DefaultSkills.Steward);
                        float effect = (float)skill * 0.00005f;
                        if (effect > 0.015f)
                            effect = 0.015f;
                        change += effect;
                    }
                else if (settlement.IsVillage)
                        if (settlement.Village.MarketTown.Governor != null && settlement.Village.MarketTown.Governor.Culture == ownerCulture)
                        {
                            change += 0.005f;
                            int skill = settlement.Town.Governor.GetSkillValue(DefaultSkills.Steward);
                            float effect = (float)skill * 0.00005f;
                            if (effect > 0.015f)
                                effect = 0.015f;
                            change += effect;
                        }

            } else change = 0f;
            return change;
        }
    }
}
