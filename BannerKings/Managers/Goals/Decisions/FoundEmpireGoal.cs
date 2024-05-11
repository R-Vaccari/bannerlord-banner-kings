using System;
using System.Collections.Generic;
using BannerKings.Managers.Cultures;
using BannerKings.Managers.Helpers;
using BannerKings.Managers.Titles;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Goals.Decisions
{
    public class FoundEmpireGoal : Goal
    {
        public FoundEmpireGoal(Hero fulfiller = null) : base("goal_found_empire", fulfiller)
        {
        }

        public override bool TickClanLeaders => true;

        public override bool TickClanMembers => false;

        public override bool TickNotables => false;

        public override GoalCategory Category => GoalCategory.Unique;

        public override Goal GetCopy(Hero fulfiller)
        {
            FoundEmpireGoal copy = new FoundEmpireGoal(fulfiller);
            copy.Initialize(Name, Description);
            return copy;
        }

        public override bool IsAvailable() => true;      

        public override bool IsFulfilled(out List<TextObject> failedReasons)
        {
            failedReasons = new List<TextObject>();

            Hero fulfiller = GetFulfiller();
            TitleAction action = BannerKingsConfig.Instance.TitleModel.GetFoundEmpire(fulfiller.Clan.Kingdom, fulfiller);
            if (!action.Possible)
            {
                failedReasons.Add(action.Reason);
            }
            
            return failedReasons.IsEmpty();
        }

        public override void ShowInquiry()
        {
            var hero = GetFulfiller();
            var clan = hero.Clan;
            var kingdom = clan.Kingdom;
            var action = BannerKingsConfig.Instance.TitleModel.GetFoundEmpire(kingdom, hero);

            CulturalTitleName culturalTitle = DefaultTitleNames.Instance.GetTitleName(hero.Culture, TitleType.Empire);
            InformationManager.ShowInquiry(new InquiryData(
                new TextObject("{=!}Form your {EMPIRE} (1/2)")
                .SetTextVariable("EMPIRE", culturalTitle.Description)
                .ToString(),
                new TextObject("{=!}Empires are the highest possible types of title. Being an Empire holder maximizes your political leverage through increased influence cap, demesne limit, vassal limit, among other benefits. By founding an Empire, you shall be styled as {TITLE}.{newline}{newline}Additionally, founding an Empire-level title shall increase your renown by {RENOWN}.{newline}{newline}Costs: {GOLD} {GOLD_ICON}, {INFLUENCE} {INFLUENCE_ICON}")
                .SetTextVariable("EMPIRE", culturalTitle.Description)
                .SetTextVariable("TITLE", hero.IsFemale ? culturalTitle.Female : culturalTitle.Name)
                .SetTextVariable("GOLD", $"{(int)action.Gold:n0}")
                .SetTextVariable("INFLUENCE", (int)action.Influence)
                .SetTextVariable("INFLUENCE_ICON", Utils.TextHelper.INFLUENCE_ICON)
                .SetTextVariable("RENOWN", action.Renown)
                .ToString(),
                true,
                true,
                GameTexts.FindText("str_accept", null).ToString(),
                GameTexts.FindText("str_selection_widget_cancel", null).ToString(),
                () => ApplyGoal(),
                null));
        }

        public override void ApplyGoal()
        {
            var hero = GetFulfiller();
            var kingdom = hero.Clan.Kingdom;
            var title = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(kingdom);
            TitleAction action = BannerKingsConfig.Instance.TitleModel.GetFoundEmpire(hero.Clan.Kingdom, hero);
            action.SetVassals(new List<FeudalTitle>() { title });

            if (hero == Hero.MainHero)
            {
                CulturalTitleName culturalTitle = DefaultTitleNames.Instance.GetTitleName(hero.Culture, TitleType.Empire);
                InformationManager.ShowTextInquiry(new TextInquiryData(
                    new TextObject("{=!}Form your {EMPIRE} (2/2)")
                    .SetTextVariable("EMPIRE", culturalTitle.Description)
                    .ToString(),
                    new TextObject("{=!}By founding an Empire, you shall be styled as {TITLE}. Your empire will need a name. Write down its name, keeping in mind the title's full name will be '{EMPIRE} of [Chosen name]'. Founding an Empire-level title shall increase your renown by {RENOWN}.{newline}{newline}Costs: {GOLD} {GOLD_ICON}, {INFLUENCE} {INFLUENCE_ICON}")
                    .SetTextVariable("EMPIRE", culturalTitle.Description)
                    .SetTextVariable("TITLE", hero.IsFemale ? culturalTitle.Female : culturalTitle.Name)
                    .SetTextVariable("GOLD", $"{(int)action.Gold:n0}")
                    .SetTextVariable("INFLUENCE", (int)action.Influence)
                    .SetTextVariable("INFLUENCE_ICON", Utils.TextHelper.INFLUENCE_ICON)
                    .SetTextVariable("RENOWN", action.Renown)
                    .ToString(), 
                    true, 
                    true, 
                    GameTexts.FindText("str_accept", null).ToString(), 
                    GameTexts.FindText("str_selection_widget_cancel", null).ToString(), 
                    delegate (string name)
                    {
                        TitleGenerator.FoundEmpire(action, new TextObject("{=!}" + name));
                    }, 
                    null));
            }
            else TitleGenerator.FoundEmpire(action, title.shortName);
        }

        private static (float Gold, float Influence) GetCosts(Hero hero)
        {
            return
            (
                500000 + BannerKingsConfig.Instance.ClanFinanceModel.CalculateClanIncome(hero.Clan).ResultNumber * CampaignTime.DaysInYear,
                1000 + BannerKingsConfig.Instance.InfluenceModel.CalculateInfluenceChange(hero.Clan).ResultNumber * CampaignTime.DaysInYear * 0.1f
            );
        }

        public override void DoAiDecision()
        {
            throw new NotImplementedException();
        }
    }
}