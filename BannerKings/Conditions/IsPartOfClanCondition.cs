using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Conditions
{
    internal sealed class IsPartOfClanCondition : BKCondition
    {
        public IsPartOfClanCondition(string clanId) : base($"condition_is_part_of_clan_{clanId}")
        {
            Clan = Campaign.Current.Clans.First(c => c.StringId == clanId);
        }

        public IsPartOfClanCondition(Clan clan) : base($"condition_is_part_of_clan_{clan.StringId}")
        {
            Clan = clan;
        }

        private new static TextObject FailedReasonText => GameTexts.FindText("str_bk_condition.is_part_of_clan");

        public Clan Clan { get; }

        public bool Apply(CharacterObject character, out TextObject? failedReasonText)
        {
            return ApplyInternal(character.HeroObject, out failedReasonText);
        }

        public bool Apply(Hero hero, out TextObject? failedReasonText)
        {
            return ApplyInternal(hero, out failedReasonText);
        }

        private bool ApplyInternal(Hero hero, out TextObject? failedReasonText)
        {
            failedReasonText = null;

            var isPartOfClan = hero.Clan != null && hero.Clan.Leader.StringId == hero.StringId;
            if (!isPartOfClan)
            {
                failedReasonText = FailedReasonText;
                failedReasonText.SetCharacterProperties("HERO", hero.CharacterObject);
                failedReasonText.SetTextVariable("CLAN_LINK", Clan.EncyclopediaLinkWithName);
            }

            return isPartOfClan;
        }
    }
}