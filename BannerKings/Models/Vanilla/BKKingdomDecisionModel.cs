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
                    var succession = title.Contract.Succession;
                    return succession.ElectedSuccession;
                }
            }

            return base.IsKingSelectionDecisionAllowed(kingdom);
        }

        public bool IsTradePactAllowed(Kingdom kingdom1, Kingdom kingdom2, out TextObject reason)
        {
            reason = new TextObject("{=!}A trade pact is possible.");
            if (kingdom1 == kingdom2)
            {
                reason = TextObject.Empty;
                return false;
            }

            StanceLink stance = kingdom1.GetStanceWith(kingdom2);
            if (stance.IsAtWar)
            {
                reason = new TextObject("{=!}Kingdoms are in war.");
                return false;
            }

            if (!BannerKingsConfig.Instance.DiplomacyModel.IsTradeAcceptable(kingdom1, kingdom2))
            {
                reason = new TextObject("{=!}{KINGDOM} is not interested in a trade pact with your realm.")
                    .SetTextVariable("KINGDOM", kingdom2.Name);
                return false;
            }

            var diplomacy = TaleWorlds.CampaignSystem.Campaign.Current.GetCampaignBehavior<BKDiplomacyBehavior>().GetKingdomDiplomacy(kingdom1);
            if (diplomacy != null && diplomacy.HasTradePact(kingdom2))
            {
                reason = new TextObject("{=!}Kingdoms are already in a trade pact.");
                return false;
            }

            float influence = BannerKingsConfig.Instance.InfluenceModel.CalculateInfluenceCap(kingdom1.RulingClan)
                .ResultNumber;
            if (influence < BannerKingsConfig.Instance.DiplomacyModel.TRADE_PACT_INFLUENCE_CAP)
            {
                reason = new TextObject("{=!}You do not have enough influence cap to sustain another pact.");
                return false;
            }

            return true;
        }

        public bool IsTruceAllowed(Kingdom kingdom1, Kingdom kingdom2, out TextObject reason)
        {
            reason = new TextObject("{=!}A truce is possible.");
            if (kingdom1 == kingdom2)
            {
                reason = TextObject.Empty;
                return false;
            }

            if (!BannerKingsConfig.Instance.DiplomacyModel.IsTruceAcceptable(kingdom1, kingdom2))
            {
                reason = new TextObject("{=!}{KINGDOM} is not interested in a truce with your realm.")
                    .SetTextVariable("KINGDOM", kingdom2.Name);
                return false;
            }

            StanceLink stance = kingdom1.GetStanceWith(kingdom2);
            if (stance.IsAtWar)
            {
                reason = new TextObject("{=!}Kingdoms are in war.");
                return false;
            }

            var diplomacy = TaleWorlds.CampaignSystem.Campaign.Current.GetCampaignBehavior<BKDiplomacyBehavior>().GetKingdomDiplomacy(kingdom1);
            if (diplomacy != null && diplomacy.HasValidTruce(kingdom2))
            {
                reason = new TextObject("{=!}Kingdoms are already in truce.");
                return false;
            }

            return true;
        }

        public bool IsAllianceAllowed(Kingdom kingdom1, Kingdom kingdom2, out TextObject reason)
        {
            reason = new TextObject("{=!}An alliance is possible.");
            if (kingdom1 == kingdom2)
            {
                reason = TextObject.Empty;
                return false;
            }

            if (!BannerKingsConfig.Instance.DiplomacyModel.IsTruceAcceptable(kingdom1, kingdom2))
            {
                reason = new TextObject("{=!}{KINGDOM} is not interested in a truce with your realm.")
                    .SetTextVariable("KINGDOM", kingdom2.Name);
                return false;
            }

            StanceLink stance = kingdom1.GetStanceWith(kingdom2);
            if (stance.IsAtWar)
            {
                reason = new TextObject("{=!}Kingdoms are in war.");
                return false;
            }

            if (stance.IsAllied)
            {
                reason = new TextObject("{=!}Kingdoms are already allies.");
                return false;
            }

            foreach (StanceLink s in kingdom1.Stances)
            {
                IFaction other = stance.Faction1 == kingdom1 ? stance.Faction2 : stance.Faction1;
                if (other.IsKingdomFaction && stance.IsAllied)
                {
                    reason = new TextObject("{=!}{KINGDOM} is already in an alliance.")
                        .SetTextVariable("KINGDOM", kingdom1.Name);
                    return false;
                }
            }

            foreach (StanceLink s in kingdom2.Stances)
            {
                IFaction other = stance.Faction1 == kingdom2 ? stance.Faction2 : stance.Faction1;
                if (other.IsKingdomFaction && stance.IsAllied)
                {
                    reason = new TextObject("{=!}{KINGDOM} is already in an alliance.")
                        .SetTextVariable("KINGDOM", kingdom2.Name);
                    return false;
                }
            }

            bool allianceWilling = BannerKingsConfig.Instance.DiplomacyModel.WillAcceptAlliance(kingdom1, kingdom2);
            if (!allianceWilling)
            {
                reason = new TextObject("{=!}{KINGDOM} is not willing to have an alliance with you.")
                                        .SetTextVariable("KINGDOM", kingdom2.Name);
                return false;
            }

            return true;
        }

        public override bool IsWarDecisionAllowedBetweenKingdoms(Kingdom kingdom1, Kingdom kingdom2, out TextObject reason)
        {
            reason = new TextObject("{=!}Declaring war is possible.");
            if (kingdom1 == kingdom2)
            {
                reason = TextObject.Empty;
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

            var diplomacy = TaleWorlds.CampaignSystem.Campaign.Current.GetCampaignBehavior<BKDiplomacyBehavior>().GetKingdomDiplomacy(kingdom1);
            if (diplomacy != null && diplomacy.HasValidTruce(kingdom2))
            {
                reason = new TextObject("{=!}Kingdoms are in truce.");
                return false;
            }

            return true;
        }
    }
}
