using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Conditions
{
    internal sealed class IsPartOfKingdomCondition : BKCondition
    {
        public IsPartOfKingdomCondition(string kingdomId) : base($"condition_is_part_of_kingdom_{kingdomId}")
        {
            Kingdom = Campaign.Current.Kingdoms.First(k => k.StringId == kingdomId);
        }

        public IsPartOfKingdomCondition(Kingdom kingdom) : base($"condition_is_part_of_kingdom_{kingdom.StringId}")
        {
            Kingdom = kingdom;
        }

        private new static TextObject FailedReasonText => GameTexts.FindText("str_bk_condition.is_part_of_kingdom");

        public Kingdom Kingdom { get; }

        public bool Apply(CharacterObject character, out TextObject failedReasonText)
        {
            return ApplyInternal(character.HeroObject, out failedReasonText);
        }

        public bool Apply(Hero hero, out TextObject failedReasonText)
        {
            return ApplyInternal(hero, out failedReasonText);
        }

        public bool Apply(Clan clan, out TextObject failedReasonText)
        {
            return ApplyInternal(clan.Leader, out failedReasonText);
        }

        public bool Apply(IFaction faction, out TextObject failedReasonText)
        {
            return ApplyInternal(faction.Leader, out failedReasonText);
        }

        private bool ApplyInternal(Hero hero, out TextObject failedReasonText)
        {
            failedReasonText = null;

            var isPartOfKingdom = hero.Clan != null && hero.Clan.MapFaction.IsKingdomFaction;
            if (!isPartOfKingdom)
            {
                failedReasonText = FailedReasonText;
                failedReasonText.SetCharacterProperties("HERO", hero.CharacterObject);
                failedReasonText.SetTextVariable("KINGDOM_LINK", Kingdom.EncyclopediaLinkWithName);
            }

            return isPartOfKingdom;
        }
    }
}