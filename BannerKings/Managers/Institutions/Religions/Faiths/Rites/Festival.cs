using BannerKings.Utils.Extensions;
using System.Collections.Generic;
using System.Drawing;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions.Faiths.Rites
{
    public abstract class Festival : RecurrentRite
    {
        protected Settlement festivalPlace;

        public override void Execute(Hero executor)
        {
            TextObject reason;
            if (!MeetsCondition(executor, out reason))
            {
                return;
            }

            var options = new List<InquiryElement>();
            foreach (var fief in executor.Clan.Fiefs)
            {
                options.Add(new InquiryElement(fief.Settlement, fief.Name.ToString(), null));
            }

            MBInformationManager.ShowMultiSelectionInquiry(
                new MultiSelectionInquiryData(
                    GetName().ToString(),
                    GetDescription().ToString(),
                    options,
                    false,
                    1,
                    GameTexts.FindText("str_done").ToString(),
                    string.Empty,
                    delegate (List<InquiryElement> x)
                    {
                        festivalPlace = (Settlement?)x[0].Identifier;
                    },
                    null,
                    string.Empty));
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
