using System;
using System.Collections.Generic;
using BannerKings.Managers.Cultures;
using BannerKings.Managers.Titles;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Goals.Decisions
{
    public class FoundKingdomGoal : Goal
    {
        public FoundKingdomGoal(Hero fulfiller = null) : base("goal_found_kingdom", fulfiller)
        {
        }

        public override bool TickClanLeaders => true;

        public override bool TickClanMembers => false;

        public override bool TickNotables => false;

        public override GoalCategory Category => GoalCategory.Kingdom;

        public override Goal GetCopy(Hero fulfiller)
        {
            FoundKingdomGoal copy = new FoundKingdomGoal(fulfiller);
            copy.Initialize(Name, Description);
            return copy;
        }

        public override bool IsAvailable()
        {
            return true;
        }

        public override bool IsFulfilled(out List<TextObject> failedReasons)
        {
            failedReasons = new List<TextObject>();
            Hero fulfiller = GetFulfiller();
            TitleAction action = BannerKingsConfig.Instance.TitleModel.GetFoundKingdom(fulfiller.Clan.Kingdom, fulfiller);
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
            var action = BannerKingsConfig.Instance.TitleModel.GetFoundKingdom(kingdom, hero);

            CulturalTitleName culturalTitle = DefaultTitleNames.Instance.GetTitleName(hero.Culture, TitleType.Kingdom);
            InformationManager.ShowInquiry(new InquiryData(
                new TextObject("{=ztoYKWVA}Founding a new Kingdom").ToString(),
                new TextObject("{=5VhaJ732}Found a new title for your kingdom. The title will legitimize your position and allow the de Jure domain of the kingdom to expand through de Jure drift of dukedoms, as well as extend your influence as a suzerain. By founding an Empire, you shall be styled as {TITLE}. Founding a title would increase your clan's renown by {RENOWN}.{newline}{newline}Costs: {GOLD} {GOLD_ICON}, {INFLUENCE} {INFLUENCE_ICON}")
                .SetTextVariable("TITLE", hero.IsFemale ? culturalTitle.Female : culturalTitle.Name)
                .SetTextVariable("GOLD", $"{(int) action.Gold:n0}")
                .SetTextVariable("INFLUENCE", (int) action.Influence)
                .SetTextVariable("INFLUENCE_ICON", Utils.TextHelper.INFLUENCE_ICON)
                .SetTextVariable("RENOWN", action.Renown)
                .ToString(),
                action.Possible,
                true,
                GameTexts.FindText("str_accept").ToString(),
                GameTexts.FindText("str_cancel").ToString(),
                ApplyGoal,
                null));
        }

        public override void ApplyGoal()
        {
            var hero = GetFulfiller();
            var kingdom = hero.Clan.Kingdom;
            var action = BannerKingsConfig.Instance.TitleModel.GetFoundKingdom(kingdom, hero);

            var duchies = new List<InquiryElement>();
            foreach (var clan in kingdom.Clans)
            {
                var titles = BannerKingsConfig.Instance.TitleManager.GetAllDeJure(clan);
                foreach (var title in titles)
                {
                    if (title.TitleType == TitleType.Dukedom)
                    {
                        duchies.Add(new InquiryElement(title, title.FullName.ToString(), null));
                    }
                }
            }

            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                new TextObject("{=CSRMOcCm}Founding Dukedoms").ToString(),
                new TextObject("{=AVzvekuX}Select a dukedom that will compose your kingdom. The kingdom's contract will follow this dukedom's contract in terms of Succession, Inheritance and so on. Future dukedoms may be assimilated into the kingdom by the process of De Jure Drift.")
                    .ToString(),
                duchies,
                true,
                1,
                1,
                GameTexts.FindText("str_done").ToString(),
                string.Empty,
                delegate(List<InquiryElement> list)
                {
                    var firstDukedom = (FeudalTitle) list[0].Identifier;
                    action.SetTile(firstDukedom);
                    action.TakeAction(null);
                }, 
                null));
        }

        public override void DoAiDecision()
        {

        }
    }
}