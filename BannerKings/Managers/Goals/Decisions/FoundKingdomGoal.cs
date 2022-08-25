using System;
using System.Collections.Generic;
using System.Linq;
using BannerKings.Managers.Titles;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Goals.Decisions
{
    internal class FoundKingdomGoal : Goal
    {
        public FoundKingdomGoal() : base("goal_found_kingdom", GoalUpdateType.Hero)
        {
            var name = new TextObject("{=nbV21qZv}Found Kingdom");
            var description = new TextObject("{=XbV2136v}Review this kingdom's contract, signed by lords that join it.\n\n");
            Initialize(name, description);
        }

        internal override bool IsAvailable()
        {
            return true;
        }

        internal override bool IsFulfilled(out List<TextObject> failedReasons)
        {
            failedReasons = new List<TextObject>();

            var hero = GetFulfiller();
            var action = BannerKingsConfig.Instance.TitleModel.GetFoundKingdom(hero.Clan.Kingdom, hero);

            failedReasons.Add(action.Reason);

            return action.Possible;
        }

        internal override Hero GetFulfiller()
        {
            return Hero.MainHero;
        }

        internal override void ShowInquiry()
        {
            var hero = GetFulfiller();
            var clan = hero.Clan;
            var kingdom = clan.Kingdom;
            var action = BannerKingsConfig.Instance.TitleModel.GetFoundKingdom(kingdom, hero);

            InformationManager.ShowInquiry(new InquiryData(
                new TextObject("{=ztoYKWVA}Founding a new Kingdom").ToString(),
                new TextObject("{=5VhaJ732}Found a new title for your kingdom. The title will legitimize your position and allow the de Jure domain of the kingdom to expand through de Jure drift of dukedoms, as well as extend your influence as a suzerain. Founding a title would increase your clan's renown by {RENOWN}. \n \nCosts: {GOLD} {GOLD_ICON}, {INFLUENCE} {INFLUENCE_ICON} \n\nCan form kingdom: {POSSIBLE} \n\nExplanation: {REASON}")
                    .SetTextVariable("POSSIBLE", GameTexts.FindText(action.Possible ? "str_yes" : "str_no"))
                    .SetTextVariable("GOLD", $"{(int) action.Gold:n0}")
                    .SetTextVariable("INFLUENCE", (int) action.Influence)
                    .SetTextVariable("INFLUENCE_ICON", "<img src=\"General\\Icons\\Influence@2x\" extend=\"7\">")
                    .SetTextVariable("RENOWN", action.Renown)
                    .SetTextVariable("REASON", action.Reason)
                    .ToString(),
                action.Possible,
                true,
                GameTexts.FindText("str_accept").ToString(),
                GameTexts.FindText("str_cancel").ToString(),
                ApplyGoal,
                null));
        }

        internal override void ApplyGoal()
        {
            var hero = GetFulfiller();
            var kingdom = hero.Clan.Kingdom;
            var action = BannerKingsConfig.Instance.TitleModel.GetFoundKingdom(kingdom, hero);

            var duchies =
            (
                from clan
                    in kingdom.Clans
                from dukedom
                    in BannerKingsConfig.Instance.TitleManager.GetAllDeJure(hero.Clan.Leader)
                        .FindAll(x => x.type == TitleType.Dukedom)
                select new InquiryElement(dukedom, dukedom.FullName.ToString(), null)
            ).ToList();

            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                new TextObject("{=CSRMOcCm}Founding Dukedoms").ToString(),
                new TextObject("{=JgLZ27UL}Select up to 3 dukedoms that will compose your kingdom. The kingdom's contract will follow the first dukedom's contract. Dukedom titles from other clans in the faction may be included as well.")
                    .ToString(),
                duchies,
                true,
                3,
                GameTexts.FindText("str_done").ToString(),
                string.Empty,
                delegate(List<InquiryElement> list)
                {
                    var firstDukedom = (FeudalTitle) list[0].Identifier;
                    var vassals = (from element in list
                        where (FeudalTitle) list[0].Identifier != firstDukedom
                        select (FeudalTitle) element.Identifier).ToList();

                    action.SetTile(firstDukedom);
                    action.SetVassals(vassals);
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