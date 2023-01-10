using BannerKings.Behaviours.Diplomacy;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.Localization;

namespace BannerKings.Models.Vanilla
{
    public class BKKingdomDecisionModel : DefaultKingdomDecisionPermissionModel
    {
        public override bool IsKingSelectionDecisionAllowed(Kingdom kingdom)
        {
            if (BannerKingsConfig.Instance.TitleManager != null)
            {
                var title = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(kingdom);
                if (title != null)
                {
                    var succession = title.contract.Succession;
                    return succession == Managers.Titles.SuccessionType.Elective_Monarchy || succession == Managers.Titles.SuccessionType.Republic;
                }
            }

            return base.IsKingSelectionDecisionAllowed(kingdom);
        }

        public override bool IsWarDecisionAllowedBetweenKingdoms(Kingdom kingdom1, Kingdom kingdom2, out TextObject reason)
        {
            reason = TextObject.Empty;
            if (kingdom1 == kingdom2)
            {
                return false;
            }

            StanceLink stance = kingdom1.GetStanceWith(kingdom2);
            if (stance.IsAllied)
            {
                reason = new TextObject("{=!}Kingdoms are allies.");
                return false;
            }

            var rulingClan1 = kingdom1.RulingClan;
            var rulingClan2 = kingdom2.RulingClan;
            StanceLink clanStance = rulingClan1.GetStanceWith(rulingClan2);
            if (clanStance.IsAllied)
            {
                reason = new TextObject("{=!}Ruling clans are allies.");
                return false;
            }

            var diplomacy = Campaign.Current.GetCampaignBehavior<BKDiplomacyBehavior>().GetKingdomDiplomacy(kingdom1);
            if (diplomacy != null && diplomacy.HasTradePact(kingdom2))
            {
                reason = new TextObject("{=!}Kingdoms are in truce.");
                return false;
            }

            return true;
        }
    }
}
