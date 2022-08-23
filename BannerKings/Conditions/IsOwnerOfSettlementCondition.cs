using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Conditions
{
    internal sealed class IsOwnerOfSettlementCondition : BKCondition
    {
        public IsOwnerOfSettlementCondition(string settlementId) : base($"condition_is_owner_of_settlement_{settlementId}")
        {
            Settlement = Campaign.Current.Settlements.First(s => s.StringId == settlementId);
        }

        public IsOwnerOfSettlementCondition(Settlement settlement) : base($"condition_is_owner_of_settlement_{settlement.StringId}")
        {
            Settlement = settlement;
        }

        private new static TextObject FailedReasonText => GameTexts.FindText("str_bk_condition.is_owner_of_settlement");

        public Settlement Settlement { get; }

        public bool Apply(Hero hero, out TextObject? failedReasonText)
        {
            return ApplyInternal(hero, false, out failedReasonText);
        }

        public bool Apply(Clan clan, out TextObject? failedReasonText)
        {
            return ApplyInternal(clan.Leader, true, out failedReasonText);
        }

        private bool ApplyInternal(Hero hero, bool considerClan, out TextObject? failedReasonText)
        {
            failedReasonText = null;

            var isOwnerOfSettlement = Settlement.All.FirstOrDefault(s => s.StringId == Settlement.StringId && (s.Owner.StringId == hero.StringId || considerClan && s.OwnerClan.StringId == hero.Clan?.StringId)) != null;
            if (!isOwnerOfSettlement)
            {
                failedReasonText = FailedReasonText;
                failedReasonText.SetCharacterProperties("HERO", hero.CharacterObject);
                failedReasonText.SetTextVariable("SETTLEMENT_LINK", Settlement.EncyclopediaLinkWithName);
            }

            return isOwnerOfSettlement;
        }
    }
}