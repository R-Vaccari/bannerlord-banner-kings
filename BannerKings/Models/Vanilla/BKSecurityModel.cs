using BannerKings.Behaviours;
using BannerKings.Managers.Court.Members;
using BannerKings.Managers.Court.Members.Tasks;
using BannerKings.Managers.Policies;
using BannerKings.Managers.Titles.Governments;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;
using static BannerKings.Managers.Policies.BKCriminalPolicy;
using static BannerKings.Managers.Policies.BKWorkforcePolicy;

namespace BannerKings.Models.Vanilla
{
    public class BKSecurityModel : DefaultSettlementSecurityModel
    {
        public override ExplainedNumber CalculateSecurityChange(Town town, bool includeDescriptions = false)
        {
            var baseResult = base.CalculateSecurityChange(town, includeDescriptions);
            if (town.IsCastle)
            {
                baseResult.Add(0.5f, new TextObject("{=UnxSzSGt}Castle security"));
            }

            var capital = TaleWorlds.CampaignSystem.Campaign.Current.GetCampaignBehavior<BKCapitalBehavior>().GetCapital(town.OwnerClan.Kingdom);
            if (capital == town)
            {
                baseResult.Add(-1f, new TextObject("{=fQVyeiJb}Capital"));
            }

            var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(town.Settlement);
            var assim = data.CultureData.GetAssimilation(town.OwnerClan.Leader.Culture);
            var assimilation = assim - 1f + assim;
            baseResult.Add(assimilation, new TextObject("{=D3trXTDz}Cultural Assimilation"));

            if (BannerKingsConfig.Instance.PolicyManager.IsPolicyEnacted(town.Settlement, "workforce",
                    (int) WorkforcePolicy.Martial_Law))
            {
                var militia = town.Militia / 2;
                baseResult.Add(militia * 0.01f, new TextObject("{=7cFbhefJ}Martial Law policy"));
            }

            var criminal =
                ((BKCriminalPolicy) BannerKingsConfig.Instance.PolicyManager.GetPolicy(town.Settlement, "criminal"))
                .Policy;
            switch (criminal)
            {
                case CriminalPolicy.Execution:
                    baseResult.Add(0.5f, new TextObject("Criminal policy"));
                    break;
                case CriminalPolicy.Forgiveness:
                    baseResult.Add(1f, new TextObject("Criminal policy"));
                    break;
            }

            var government = BannerKingsConfig.Instance.TitleManager.GetSettlementGovernment(town.Settlement);
            if (government == DefaultGovernments.Instance.Imperial)
            {
                baseResult.Add(1f, new TextObject("{=PSrEtF5L}Government"));
            }

            BannerKingsConfig.Instance.CourtManager.ApplyCouncilEffect(ref baseResult, town.OwnerClan.Leader,
                DefaultCouncilPositions.Instance.Spymaster,
                DefaultCouncilTasks.Instance.OverseeSecurity,
                1f, false);

            BannerKingsConfig.Instance.CourtManager.ApplyCouncilEffect(ref baseResult,
                town.OwnerClan.Leader, DefaultCouncilPositions.Instance.Constable,
                DefaultCouncilTasks.Instance.EnforceLaw,
                0.3f, false);

            return baseResult;
        }
    }
}