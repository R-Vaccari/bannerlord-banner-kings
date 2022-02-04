using BannerKings.Managers;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Models
{
    public class AdministrativeModel : IBannerKingsModel
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

            if (BannerKingsConfig.Instance.PolicyManager.GetSettlementWork(settlement) != PolicyManager.WorkforcePolicy.None)
                baseResult += 0.075f;

            if (BannerKingsConfig.Instance.PolicyManager.IsPolicyEnacted(settlement, PolicyManager.PolicyType.EXPORT_SLAVES))
                baseResult += 0.025f;

            if (BannerKingsConfig.Instance.PolicyManager.IsPolicyEnacted(settlement, PolicyManager.PolicyType.SUBSIDIZE_MILITIA))
                baseResult += 0.1f;

            if (BannerKingsConfig.Instance.PolicyManager.IsPolicyEnacted(settlement, PolicyManager.PolicyType.CONSCRIPTION))
                baseResult += 0.1f;

            return Math.Max(baseResult, 0f);
        }

        public override ExplainedNumber CalculateEffect(Settlement settlement)
        {
            ExplainedNumber baseResult = new ExplainedNumber(0.075f, true);
            baseResult.LimitMin(0f);

            Hero governor = settlement.IsVillage ? settlement.Village.MarketTown.Governor : settlement.Town.Governor;
            if (governor != null)
            {
                int skill = settlement.Town.Governor.GetSkillValue(DefaultSkills.Steward);
                float effect = (float)skill * -0.0005f;
                if (effect > 0.20f)
                    effect = 0.20f;
                baseResult.Add(effect, new TextObject("Governor stewardship"));
            }
            else baseResult.Add(0.05f, new TextObject("Absence of governor"));
            

            if (BannerKingsConfig.Instance.PolicyManager.GetSettlementWork(settlement) != PolicyManager.WorkforcePolicy.None)
                baseResult.Add(0.075f, new TextObject(""));

            if (BannerKingsConfig.Instance.PolicyManager.IsPolicyEnacted(settlement, PolicyManager.PolicyType.EXPORT_SLAVES))
                baseResult.Add(0.025f, new TextObject(""));

            if (BannerKingsConfig.Instance.PolicyManager.IsPolicyEnacted(settlement, PolicyManager.PolicyType.SUBSIDIZE_MILITIA))
                baseResult.Add(0.1f, new TextObject(""));

            if (BannerKingsConfig.Instance.PolicyManager.IsPolicyEnacted(settlement, PolicyManager.PolicyType.CONSCRIPTION))
                baseResult.Add(0.1f, new TextObject(""));

            return baseResult;
        }
    }
}
