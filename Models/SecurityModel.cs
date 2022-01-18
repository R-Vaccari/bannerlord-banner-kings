using BannerKings.Managers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents.Map;
using TaleWorlds.Localization;
using static BannerKings.Managers.PolicyManager;
using static BannerKings.Managers.PopulationManager;

namespace BannerKings.Models
{
    class SecurityModel : DefaultSettlementSecurityModel
    {
        public override ExplainedNumber CalculateSecurityChange(Town town, bool includeDescriptions = false)
        {
            ExplainedNumber baseResult = base.CalculateSecurityChange(town, includeDescriptions);

            if (town.IsCastle)
                baseResult.Add(0.5f, new TextObject("Castle security"));

            if (BannerKingsConfig.Instance.PopulationManager != null && BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(town.Settlement))
            {
                PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(town.Settlement);
                float assimilation = data.Assimilation - 1f + data.Assimilation;
                baseResult.Add(assimilation, new TextObject("Cultural Assimilation"));

                if (BannerKingsConfig.Instance.PolicyManager.GetSettlementWork(town.Settlement) == PolicyManager.WorkforcePolicy.Martial_Law)
                {
                    float militia = town.Militia;
                    baseResult.Add(militia * 0.01f, new TextObject("Martial Law policy"));
                }

                CriminalPolicy criminal = BannerKingsConfig.Instance.PolicyManager.GetCriminalPolicy(town.Settlement);
                if (criminal == CriminalPolicy.Execution)
                    baseResult.Add(0.5f, new TextObject("Criminal policy"));
                else if (criminal == CriminalPolicy.Forgiveness)
                    baseResult.Add(1f, new TextObject("Criminal policy"));
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
