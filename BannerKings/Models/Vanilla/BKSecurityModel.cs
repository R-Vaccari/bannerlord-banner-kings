using BannerKings.Behaviours;
using BannerKings.Managers.Court;
using BannerKings.Managers.Policies;
using BannerKings.Managers.Titles;
using CalradiaExpandedKingdoms.Models;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;
using static BannerKings.Managers.Policies.BKCriminalPolicy;
using static BannerKings.Managers.Policies.BKWorkforcePolicy;

namespace BannerKings.Models.Vanilla
{
    public class BKSecurityModel : CEKSettlementSecurityModel
    {
        public override ExplainedNumber CalculateSecurityChange(Town town, bool includeDescriptions = false)
        {
            var baseResult = base.CalculateSecurityChange(town, includeDescriptions);

            if (town.IsCastle)
            {
                baseResult.Add(0.5f, new TextObject("{=UnxSzSGt}Castle security"));
            }

            var capital = Campaign.Current.GetCampaignBehavior<BKCapitalBehavior>().GetCapital(town.OwnerClan.Kingdom);
            if (capital == town)
            {
                baseResult.Add(-1f, new TextObject("{=!}Capital"));
            }


            if (BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(town.Settlement))
            {
                var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(town.Settlement);
                var assim = data.CultureData.GetAssimilation(town.Owner.Culture);
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
                if (government == GovernmentType.Imperial)
                {
                    baseResult.Add(1f, new TextObject("{=PSrEtF5L}Government"));
                }

                BannerKingsConfig.Instance.CourtManager.ApplyCouncilEffect(ref baseResult, town.OwnerClan.Leader,
                    CouncilPosition.Spymaster, 1f, false);
            }

            GetHideoutBonus(town, ref baseResult);
            return baseResult;
        }

        private void GetHideoutBonus(Town town, ref ExplainedNumber explainedNumber)
        {
            var num = 40f;
            var num2 = num * num;
            var num3 = 0;
            foreach (var hideout in Hideout.All)
            {
                if (hideout.IsInfested && town.Settlement.Position2D.DistanceSquared(hideout.Settlement.Position2D) < num2)
                {
                    num3++;
                    break;
                }
            }

            if (num3 == 0)
            {
                explainedNumber.Add(0.5f, new TextObject("{=Zy76yFyk}No hideouts nearby"));
            }
        }
    }
}