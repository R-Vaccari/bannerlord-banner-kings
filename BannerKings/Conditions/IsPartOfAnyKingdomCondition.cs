using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Conditions
{
    internal sealed class IsPartOfAnyKingdomCondition : BKCondition
    {
        public IsPartOfAnyKingdomCondition() : base("condition_is_part_of_any_kingdom")
        {
        }

        private new static TextObject FailedReasonText => GameTexts.FindText("str_bk_condition.is_part_of_any_kingdom");

        public bool Apply(CharacterObject character, out TextObject? failedReasonText)
        {
            return ApplyInternal(character.HeroObject, out failedReasonText);
        }

        public bool Apply(Hero hero, out TextObject? failedReasonText)
        {
            return ApplyInternal(hero, out failedReasonText);
        }

        public bool Apply(Clan clan, out TextObject? failedReasonText)
        {
            return ApplyInternal(clan.Leader, out failedReasonText);
        }

        public bool Apply(IFaction faction, out TextObject? failedReasonText)
        {
            return ApplyInternal(faction.Leader, out failedReasonText);
        }

        private bool ApplyInternal(Hero hero, out TextObject? failedReasonText)
        {
            failedReasonText = null;

            var isPartOfKingdom = hero.Clan?.Kingdom != null;
            if (!isPartOfKingdom)
            {
                failedReasonText = FailedReasonText;
                failedReasonText.SetCharacterProperties("HERO", hero.CharacterObject);
            }

            return isPartOfKingdom;
        }
    }
}