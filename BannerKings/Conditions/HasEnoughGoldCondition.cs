using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Conditions
{
    internal sealed class HasEnoughGoldCondition : BKCondition
    {
        public HasEnoughGoldCondition(int gold) : base($"condition_has_enough_gold_{gold}")
        {
            Gold = gold;
        }

        private new static TextObject FailedReasonText => GameTexts.FindText("str_bk_condition.has_enough_gold");

        public int Gold { get; }

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

        public bool Apply(Kingdom kingdom, out TextObject? failedReasonText)
        {
            return ApplyInternal(kingdom.Leader, out failedReasonText);
        }

        public bool Apply(IFaction faction, out TextObject? failedReasonText)
        {
            return ApplyInternal(faction.Leader, out failedReasonText);
        }

        private bool ApplyInternal(Hero hero, out TextObject? failedReasonText)
        {
            failedReasonText = null;

            var hasEnoughGold = hero.Gold >= Gold;
            if (!hasEnoughGold)
            {
                failedReasonText = FailedReasonText;
                failedReasonText.SetCharacterProperties("HERO", hero.CharacterObject);
            }

            return hasEnoughGold;
        }
    }
}