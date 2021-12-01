using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using static Populations.PopulationManager;

namespace Populations.Models
{
    class CultureModel : GameModel
    {

        public void CalculateAssimilationChange(Settlement settlement)
        {
            float result = GetAssimilationChange(settlement);
            if (PopulationConfig.Instance.PopulationManager != null && PopulationConfig.Instance.PopulationManager.IsSettlementPopulated(settlement))
            {
                PopulationData data = PopulationConfig.Instance.PopulationManager.GetPopData(settlement);
                data.Assimilation = Math.Max(data.Assimilation + result, 0);
            }
        }

        public float GetAssimilationChange(Settlement settlement)
        {
            CultureObject ownerCulture = settlement.OwnerClan.Culture;
 
            PopulationData data = PopulationConfig.Instance.PopulationManager.GetPopData(settlement);
            float change = -0.005f;
            if (settlement.Culture != ownerCulture)
            {
                if (!settlement.IsVillage && settlement.Town != null)
                    if (settlement.Town.Governor != null && settlement.Town.Governor.Culture == ownerCulture)
                    {
                        change += 0.005f;
                        int skill = settlement.Town.Governor.GetSkillValue(DefaultSkills.Steward);
                        change += (float)skill * 0.00005f;
                    }
                else if (settlement.IsVillage)
                        if (settlement.Village.MarketTown.Governor != null && settlement.Village.MarketTown.Governor.Culture == ownerCulture)
                        {
                            change += 0.005f;
                            int skill = settlement.Town.Governor.GetSkillValue(DefaultSkills.Steward);
                            change += (float)skill * 0.00005f;
                        }

            } else change = 0f;
            return change;
        }
    }
}
