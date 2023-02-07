using BannerKings.Utils.Extensions;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions.Faiths.Rites
{
    public abstract class Festival : RecurrentRite
    {
        protected Settlement festivalPlace;

        public override TextObject GetRequirementsText(Hero hero)
        {
            return new TextObject("{=!}The current date must match the festival's traditional season and day of season.");
        }

        public override void Execute(Hero executor)
        {
            InformationManager.ShowInquiry(new InquiryData(
                new TextObject("{=!}Organize Festival").ToString(),
                new TextObject("{=!}A religious festival is a way to celebrate your faith and improve bonds with your fellow faithful. The festival will require a feast in a town or castle of yours. Piety will be awarded according to number of guests and their satisfaction with the feast, so be sure to provide plenty of good food and beverage.").ToString(),
                true,
                false,
                GameTexts.FindText("str_ok").ToString(),
                string.Empty,
                () =>
                {
                    SetDialogue();
                },
                null));
        }

        public override float GetPietyReward()
        {
            return 100;
        }

        public override bool MeetsCondition(Hero hero, out TextObject reason)
        {
            reason = new TextObject("{=!}This rite is available to be performed.");
            bool hasFief = hero.Clan != null && hero.IsClanLeader() &&
                hero.Clan.Fiefs.Count > 0;

            if (!hasFief)
            {
                reason = new TextObject("{=!}The festival needs a town or castle to be performed in.");
            }

            bool date = CampaignTime.Now.GetSeasonOfYear == SeasonOfTheYear &&
            CampaignTime.Now.GetDayOfSeason == DayOfTheSeason;

            if (!date)
            {
                reason = new TextObject("{=!}The festival may only be performed on the {ORDINAL} of {SEASON}.")
                    .SetTextVariable("ORDINAL", GameTexts.FindText("str_ordinal_number", DayOfTheSeason.ToString()))
                    .SetTextVariable("SEASON", GameTexts.FindText("str_season_" + SeasonOfTheYear.ToString()));
            }

            return hasFief && date;
        }
    }
}
