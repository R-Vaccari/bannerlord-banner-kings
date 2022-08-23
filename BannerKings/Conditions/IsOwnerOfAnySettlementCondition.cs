using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Conditions
{
    internal sealed class IsOwnerOfAnySettlementCondition : BKCondition
    {
        public IsOwnerOfAnySettlementCondition() : base("condition_is_owner_of_any_settlement")
        {

        }

        private new static TextObject FailedReasonText => GameTexts.FindText("str_bk_condition.is_owner_of_any_settlement");

        public bool Apply(Hero hero, out TextObject failedReasonText)
        {
            return ApplyInternal(hero, out failedReasonText);
        }

        public bool Apply(Clan clan, out TextObject failedReasonText)
        {
            return ApplyInternal(clan.Leader, out failedReasonText);
        }

        private bool ApplyInternal(Hero hero, out TextObject failedReasonText)
        {
            failedReasonText = null;

            var isOwnerOfSettlement = Settlement.All.Any(s => s.Owner == hero || s.OwnerClan == hero.Clan);
            if (!isOwnerOfSettlement)
            {
                failedReasonText = FailedReasonText;
                failedReasonText.SetCharacterProperties("HERO", hero.CharacterObject);
            }

            return isOwnerOfSettlement;
        }
    }
}