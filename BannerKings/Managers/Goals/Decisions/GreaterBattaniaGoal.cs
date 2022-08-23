using System.Collections.Generic;
using System.Linq;
using BannerKings.Managers.Titles;
using BannerKings.Utils.Extensions;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Goals.Decisions
{
    internal class GreaterBattaniaGoal : Goal
    {
        private readonly List<Settlement> settlements;

        public GreaterBattaniaGoal() : base("goal_greater_battania", GoalUpdateType.Settlement)
        {
            var name = new TextObject("{=!}Greater Battania");
            var description = new TextObject("{!=}Found Greater Battania");

            Initialize(name, description);

            var settlementStringIds = new List<string>
            {
                "town_B1",
                "town_B2",
                "town_B3",
                "town_B4",
                "town_B5",
                "castle_B1",
                "castle_B2",
                "castle_B3",
                "castle_B4",
                "castle_B5",
                "castle_B6",
                "castle_B7",
                "castle_B8",
                "town_V2",
                "town_EN1"
            };

            settlements = Campaign.Current.Settlements.Where(s => settlementStringIds.Contains(s.StringId)).ToList();
        }

        internal override bool IsAvailable()
        {
            return BannerKingsConfig.Instance.TitleManager.GetTitleByStringId("title_greater_battania") == null;
        }

        internal override bool IsFulfilled(out List<TextObject> failedReasons)
        {
            failedReasons = new List<TextObject>();

            var referenceSettlement = settlements.First();
            var referenceHero = referenceSettlement.Owner;
            var (gold, influence) = GetCosts(referenceHero);

            if (!IsAvailable())
            {
                var title = BannerKingsConfig.Instance.TitleManager.GetTitleByStringId("title_greater_battania");

                var failedReason = new TextObject("{=!}This title is already founded! de Jure is {DE_JURE.LINK} and de Facto is {DE_FACTO.LINK}.");
                failedReason.SetCharacterProperties("DE_JURE", title.deJure.CharacterObject);
                failedReason.SetCharacterProperties("DE_FACTO", title.DeFacto.CharacterObject);

                failedReasons.Add(failedReason);
            }

            if (!referenceHero.IsKingdomLeader() || referenceHero.Clan.Kingdom.StringId != "battania")
            {
                var kingdom = Campaign.Current.Kingdoms.FirstOrDefault(k => k.StringId == "battania");
                if (kingdom == null)
                {
                    failedReasons.Add(new TextObject("{!=}The {KINGDOM} kingdom does not exist")
                    .SetTextVariable("KINGDOM", GameTexts.FindText("str_adjective_for_faction", "battania")));
                }
                else
                {
                    failedReasons.Add(new TextObject("{!=}You're not the leader of {KINGDOM}")
                    .SetTextVariable("KINGDOM", kingdom.EncyclopediaLinkWithName));
                }
            }

            failedReasons.AddRange
            (
                from settlement in settlements
                let title = BannerKingsConfig.Instance.TitleManager.GetTitle(settlement)
                where title.deFacto.MapFaction != referenceHero.MapFaction
                select new TextObject("{=!}No one of your kingdom is de Facto for {SETTLEMENT}")
                    .SetTextVariable("SETTLEMENT", settlement.EncyclopediaLinkWithName)
            );

            if (referenceHero.Gold < gold)
            {
                failedReasons.Add(new TextObject("{=!}You need at least {GOLD}{GOLD_ICON}.")
                    .SetTextVariable("GOLD", gold));
            }

            if (referenceHero.Clan.Influence < influence)
            {
                failedReasons.Add(new TextObject("{=!}You need at least {INFLUENCE}{INFLUENCE_ICON}.")
                    .SetTextVariable("INFLUENCE", influence));
            }

            return failedReasons.IsEmpty();
        }

        internal override Hero GetFulfiller()
        {
            return settlements.First().Owner;
        }

        internal override void ShowInquiry()
        {
            var (gold, influence) = GetCosts(GetFulfiller());
            
            InformationManager.ShowInquiry
            (
                new InquiryData
                (
                    "Founding a new Title",
                    new TextObject("Do you want to found the title {TITLE}?\nThis will cost you {GOLD}{GOLD_ICON} and {INFLUENCE}{INFLUENCE_ICON}.\nAs a reward your clan will earn {RENOWN} renown.")
                        .SetTextVariable("TITLE", name)
                        .SetTextVariable("GOLD", gold)
                        .SetTextVariable("INFLUENCE", influence)
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

        internal override void ApplyGoal()
        {
            var founder = GetFulfiller();
            var (gold, influence) = GetCosts(founder);

            var foundAction = new TitleAction(ActionType.Found, null, founder)
            {
                Gold = gold,
                Influence = influence,
                Renown = 100
            };

            BannerKingsConfig.Instance.TitleManager.FoundEmpire(foundAction, "title_greater_battania");
        }

        public override void DoAiDecision()
        {
            //TODO: Implement the AI decision for this goal.
            ApplyGoal();
        }

        private static (float Gold, float Influence) GetCosts(Hero hero)
        {
            return
            (
                500000 + BannerKingsConfig.Instance.ClanFinanceModel.CalculateClanIncome(hero.Clan).ResultNumber * CampaignTime.DaysInYear,
                1000 + BannerKingsConfig.Instance.InfluenceModel.CalculateInfluenceChange(hero.Clan).ResultNumber * CampaignTime.DaysInYear * 0.1f
            );
        }
    }
}