using System;
using System.Collections.Generic;
using System.Linq;
using BannerKings.Managers.Titles;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using static System.Collections.Specialized.BitVector32;

namespace BannerKings.Managers.Goals.Decisions
{
    internal class FoundKingdomGoal : Goal
    {
        public FoundKingdomGoal() : base("goal_found_kingdom", GoalUpdateType.Hero)
        {
            var name = new TextObject("{=nbV21qZv}Found Kingdom");
            var description = new TextObject("{=Df3Fdnuw}Stablish your own kingdom title. Your faction must be one that is not already represented by a kingdom title.");
            Initialize(name, description);
        }

        internal override bool IsAvailable()
        {
            return true;
        }

        internal override bool IsFulfilled(out List<TextObject> failedReasons)
        {
            failedReasons = new List<TextObject>();

            if (Clan.PlayerClan.Kingdom == null)
            {
                failedReasons.Add(new TextObject("{=JDFpx1eN}No kingdom."));
            }
            else
            {
                var title = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(Clan.PlayerClan.Kingdom);
                if (title != null)
                {
                    failedReasons.Add(new TextObject("{=eTMvobFw}Faction sovereign title already exists."));
                }
            }
            
            return failedReasons.IsEmpty();
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

            var duchies = new List<InquiryElement>();
            foreach (var clan in kingdom.Clans)
            {
                var titles = BannerKingsConfig.Instance.TitleManager.GetAllDeJure(clan);
                foreach (var title in titles)
                {
                    if (title.type == TitleType.Dukedom)
                    {
                        duchies.Add(new InquiryElement(title, title.FullName.ToString(), null));
                    }
                }
            }

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