using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using static Populations.PopulationManager;

namespace Populations.Models
{
    class CultureModel
    {

        public float CalculateAssimilationChange(Settlement settlement)
        {
            CultureObject ownerCulture = settlement.OwnerClan.Culture;
            if (PopulationConfig.Instance.PopulationManager != null && PopulationConfig.Instance.PopulationManager.IsSettlementPopulated(settlement))
            {
                PopulationData data = PopulationConfig.Instance.PopulationManager.GetPopData(settlement);

                if (settlement.Culture != ownerCulture)
                {
                    float change = -0.005f;

                    if (settlement.Town.Governor != null && settlement.Town.Governor.Culture == ownerCulture)
                    {
                        change += 0.005f;
                        int skill = settlement.Town.Governor.GetSkillValue(DefaultSkills.Steward);
                        change += (float)skill * 0.00005f;
                    }

                    data.Assimilation = Math.Max(data.Assimilation + change, 0);
                    return data.Assimilation;

                } else
                {
                    data.Assimilation = 1f;
                    return data.Assimilation;
                }
            }
            else return 1f;
        }
    }
}
