using BannerKings.Managers;
using BannerKings.Managers.Court;
using BannerKings.Managers.Policies;
using BannerKings.Managers.Titles;
using BannerKings.Populations;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents.Map;
using TaleWorlds.Localization;
using static BannerKings.Managers.Policies.BKCriminalPolicy;
using static BannerKings.Managers.Policies.BKWorkforcePolicy;
using static BannerKings.Managers.TitleManager;

namespace BannerKings.Models
{
    class BKSecurityModel : DefaultSettlementSecurityModel
    {
        public override ExplainedNumber CalculateSecurityChange(Town town, bool includeDescriptions = false)
        {
            ExplainedNumber baseResult = base.CalculateSecurityChange(town, includeDescriptions);

            if (town.IsCastle)
                baseResult.Add(0.5f, new TextObject("Castle security"));

            if (BannerKingsConfig.Instance.PopulationManager != null && BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(town.Settlement))
            {
                PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(town.Settlement);
                float assim = data.CultureData.GetAssimilation(town.Owner.Culture);
                float assimilation = assim - 1f + assim;
                baseResult.Add(assimilation, new TextObject("Cultural Assimilation"));

                if (BannerKingsConfig.Instance.PolicyManager.IsPolicyEnacted(town.Settlement, "workforce", (int)WorkforcePolicy.Martial_Law))
                {
                    float militia = town.Militia / 2;
                    baseResult.Add(militia * 0.01f, new TextObject("Martial Law policy"));
                }

                CriminalPolicy criminal = ((BKCriminalPolicy)BannerKingsConfig.Instance.PolicyManager.GetPolicy(town.Settlement, "criminal")).Policy;
                if (criminal == CriminalPolicy.Execution)
                    baseResult.Add(0.5f, new TextObject("Criminal policy"));
                else if (criminal == CriminalPolicy.Forgiveness)
                    baseResult.Add(1f, new TextObject("Criminal policy"));

                GovernmentType government = BannerKingsConfig.Instance.TitleManager.GetSettlementGovernment(town.Settlement);
                if (government == GovernmentType.Imperial)
                    baseResult.Add(1f, new TextObject("{=!}Government"));

                BannerKingsConfig.Instance.CourtManager.ApplyCouncilEffect(ref baseResult, town.OwnerClan.Leader, CouncilPosition.Spymaster, 1f, false);
            }

            this.GetHideoutBonus(town, ref baseResult);
            return baseResult;
        }

        private void GetHideoutBonus(Town town, ref ExplainedNumber explainedNumber)
        {
            float num = 40f;
            float num2 = num * num;
            int num3 = 0;
            foreach (Hideout hideout in Hideout.All)
            {
                if (hideout.IsInfested && town.Settlement.Position2D.DistanceSquared(hideout.Settlement.Position2D) < num2)
                {
                    num3++;
                    break;
                }
            }
            if (num3 == 0)
                explainedNumber.Add(0.5f, new TextObject("No hideouts nearby"), null);
            
        }

    }
}
