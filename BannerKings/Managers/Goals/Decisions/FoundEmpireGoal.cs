using System;
using System.Collections.Generic;
using BannerKings.Managers.Titles;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Goals.Decisions
{
    public class FoundEmpireGoal : Goal
    {
        public FoundEmpireGoal() : base("goal_found_kingdom", GoalCategory.Unique, GoalUpdateType.Hero)
        {
            var name = new TextObject("{=e0t4jZoO}Found Empire");
            var description = new TextObject("{=Zi7h8WK3}Found an Empire-level title. An Empire is the highest form of title, ruling over kingdoms. Empires may absorb kingdom titles as their vassals through the process of De Jure Drift.{newline}{newline}");
            Initialize(name, description);
        }

        public override bool IsAvailable() => true;      

        public override bool IsFulfilled(out List<TextObject> failedReasons)
        {
            failedReasons = new List<TextObject>();

            if (Clan.PlayerClan.Kingdom == null)
            {
                failedReasons.Add(new TextObject("{=JDFpx1eN}No kingdom."));
            }
            else
            {
                var title = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(Clan.PlayerClan.Kingdom);
                if (title == null)
                {
                    failedReasons.Add(new TextObject("{=eH3B6qgm}The realm {REALM} is not represented by a Kingdom-level title. Found a Kingdom first, and then an Empire.")
                        .SetTextVariable("REALM", Clan.PlayerClan.Kingdom.Name));
                }
                else if (title.TitleType == TitleType.Empire)
                {
                    failedReasons.Add(new TextObject("{=F5BEyddZ}Your realm, {REALM} is already attached to the Empire-level title {TITLE}.")
                            .SetTextVariable("REALM", Clan.PlayerClan.Kingdom.Name)
                            .SetTextVariable("TITLE", title.FullName));
                }
            }
            
            return failedReasons.IsEmpty();
        }

        public override void ShowInquiry()
        {
            var hero = GetFulfiller();
            var clan = hero.Clan;
            var kingdom = clan.Kingdom;
            var action = BannerKingsConfig.Instance.TitleModel.GetFoundKingdom(kingdom, hero);

            InformationManager.ShowInquiry
            (
                new InquiryData
                (
                    new TextObject("{=thijhbki}Establish a new Title").ToString(),
                    new TextObject("{=qjD2WwBH}Do you want to establish the title {TITLE}?\nThis will cost you {GOLD}{GOLD_ICON} and {INFLUENCE}{INFLUENCE_ICON}.\nAs a reward your clan will earn {RENOWN} renown.")
                        .SetTextVariable("TITLE", name)
                        .SetTextVariable("GOLD", (int)action.Gold)
                        .SetTextVariable("INFLUENCE", action.Influence)
                        .SetTextVariable("INFLUENCE_ICON", "<img src=\"General\\Icons\\Influence@2x\" extend=\"7\">")
                        .SetTextVariable("RENOWN", 100)
                        .ToString(),
                    true,
                    true,
                    GameTexts.FindText("str_accept").ToString(),
                    GameTexts.FindText("str_cancel").ToString(),
                    ApplyGoal,
                    null
                ),
                true
            );
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
                new TextObject("{=miyAGkb6}Founding Dukedom").ToString(),
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
            throw new NotImplementedException();
        }
    }
}