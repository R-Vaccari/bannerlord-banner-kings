using BannerKings.Managers;
using BannerKings.Managers.Policies;
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

            BKWorkforcePolicy work = (BKWorkforcePolicy)BannerKingsConfig.Instance.PolicyManager.GetPolicy(settlement, "workforce");
            if (work.Policy != BKWorkforcePolicy.WorkforcePolicy.None)
                baseResult += 0.075f;

            float decisions = BannerKingsConfig.Instance.PolicyManager.GetActiveDecisionsNumber(settlement);
            baseResult += (0.05f * decisions);

            return Math.Max(baseResult, 0f);
        }

       
        public ExplainedNumber CalculateEffect(Settlement settlement)
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


            BKWorkforcePolicy work = (BKWorkforcePolicy)BannerKingsConfig.Instance.PolicyManager.GetPolicy(settlement, "workforce");
            if (work.Policy != BKWorkforcePolicy.WorkforcePolicy.None)
                baseResult.Add(0.075f, new TextObject(""));

            float decisions = BannerKingsConfig.Instance.PolicyManager.GetActiveDecisionsNumber(settlement);
            baseResult.Add(0.05f * decisions, new TextObject("{=!} Active decisions"));

            return baseResult;
        }
    }
}
