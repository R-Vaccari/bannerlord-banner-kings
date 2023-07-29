using BannerKings.Managers.Policies;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Models.BKModels
{
    public class BKAdministrativeModel : IBannerKingsModel
    {
        public ExplainedNumber CalculateEffect(Settlement settlement)
        {
            var baseResult = new ExplainedNumber(settlement.IsVillage ? 0.2f : 0.10f, true);
            baseResult.LimitMin(0f);

            var governor = settlement.IsVillage ? settlement.Village.Bound.Town.Governor : settlement.Town.Governor;
            if (governor != null)
            {
                var skill = governor.GetSkillValue(DefaultSkills.Steward);
                var effect = skill * -0.0005f;
                if (effect > 0.20f)
                {
                    effect = 0.20f;
                }

                baseResult.Add(effect, new TextObject("{=PBzKaQYS}Governor stewardship"));
            }

            var work = (BKWorkforcePolicy) BannerKingsConfig.Instance.PolicyManager.GetPolicy(settlement, "workforce");
            if (work.Policy != BKWorkforcePolicy.WorkforcePolicy.None)
            {
                baseResult.Add(0.04f, new TextObject("{=MBHftZmv}Workforce policy"));
            }

            var draft = (BKDraftPolicy) BannerKingsConfig.Instance.PolicyManager.GetPolicy(settlement, "draft");
            if (draft.Policy == BKDraftPolicy.DraftPolicy.Conscription)
            {
                baseResult.Add(0.04f, new TextObject("{=1aq83aPr}Conscription policy"));
            }

            var garrison = (BKGarrisonPolicy) BannerKingsConfig.Instance.PolicyManager.GetPolicy(settlement, "garrison");
            if (garrison.Policy == BKGarrisonPolicy.GarrisonPolicy.Enlistment)
            {
                baseResult.Add(0.04f, new TextObject("Garrison policy"));
            }

            float decisions = BannerKingsConfig.Instance.PolicyManager.GetActiveCostlyDecisionsNumber(settlement);
            baseResult.Add(0.025f * decisions, new TextObject("{=fBhajAND}Active decisions"));

            var council = BannerKingsConfig.Instance.CourtManager.GetCouncil(settlement.OwnerClan);
            if (council != null)
            {
                if (council.CourtGrace != null)
                {
                    foreach (var expense in council.CourtGrace.Expenses)
                    {
                        baseResult.Add(expense.AdministrativeCost, expense.Name);
                    }
                }
            }

            return baseResult;
        }
    }
}