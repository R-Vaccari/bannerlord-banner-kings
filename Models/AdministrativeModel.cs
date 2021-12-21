using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace Populations.Models
{
    public class AdministrativeModel
    {
        public float CalculateAdministrativeCost(Settlement settlement)
        {
            float baseResult = 0.075f;

            if (settlement.IsTown || settlement.IsCastle)
            {
                if (settlement.Town.Governor != null)
                {
                    int skill = settlement.Town.Governor.GetSkillValue(DefaultSkills.Steward);
                    float effect = (float)skill * -0.0005f;
                    if (effect > 0.20f)
                        effect = 0.20f;
                    baseResult += effect;
                }
                else baseResult += 0.05f;
            }
            else if (settlement.IsVillage)
            {
                if (settlement.Village.MarketTown.Governor != null)
                {
                    int skill = settlement.Village.MarketTown.Governor.GetSkillValue(DefaultSkills.Steward);
                    float effect = (float)skill * -0.0005f;
                    if (effect > 0.20f)
                        effect = 0.20f;
                    baseResult += effect;
                } else baseResult += 0.05f;
            }

            if (PopulationConfig.Instance.PolicyManager.GetSettlementWork(settlement) != PolicyManager.WorkforcePolicy.None)
                baseResult += 0.075f;

            if (PopulationConfig.Instance.PolicyManager.IsPolicyEnacted(settlement, PolicyManager.PolicyType.EXPORT_SLAVES))
                baseResult += 0.025f;

            if (PopulationConfig.Instance.PolicyManager.IsPolicyEnacted(settlement, PolicyManager.PolicyType.SUBSIDIZE_MILITIA))
                baseResult += 0.1f;

            if (PopulationConfig.Instance.PolicyManager.IsPolicyEnacted(settlement, PolicyManager.PolicyType.CONSCRIPTION))
                baseResult += 0.1f;

            return Math.Max(baseResult, 0f);
        }
    }
}
