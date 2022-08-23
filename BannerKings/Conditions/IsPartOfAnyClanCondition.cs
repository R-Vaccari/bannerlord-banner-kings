using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Conditions
{
    internal sealed class IsPartOfAnyClanCondition : BKCondition
    {
        public IsPartOfAnyClanCondition() : base("condition_is_part_of_any_clan")
        {
        }

        private new static TextObject FailedReasonText => GameTexts.FindText("str_bk_condition.is_part_of_any_clan");

        public bool Apply(CharacterObject character, out TextObject failedReasonText)
        {
            return ApplyInternal(character.HeroObject, out failedReasonText);
        }

        public bool Apply(Hero hero, out TextObject failedReasonText)
        {
            return ApplyInternal(hero, out failedReasonText);
        }

        private bool ApplyInternal(Hero hero, out TextObject failedReasonText)
        {
            failedReasonText = null;

            var isPartOfClan = hero.Clan != null;
            if (!isPartOfClan)
            {
                failedReasonText = FailedReasonText;
                failedReasonText.SetCharacterProperties("HERO", hero.CharacterObject);
            }

            return isPartOfClan;
        }
    }
}