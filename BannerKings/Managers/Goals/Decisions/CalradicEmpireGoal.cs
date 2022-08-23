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
    internal class CalradicEmpireGoal : Goal
    {
        private readonly List<Settlement> settlements;

        public CalradicEmpireGoal() : base("goal_calradic_empire", GoalUpdateType.Settlement)
        {
            var name = new TextObject("{=!}Reform the Calradic Empire");
            var description = new TextObject("{!=}Establish a new empire title. You must bring all imperial duchies under control of your kingdom.");

            Initialize(name, description);

            var duchyStringIds = new List<string>
            {
                "Lakonia",
                "Nevys",
                "Myzead",
                "Gavys",
                "Perassica",
                "Aria",
                "Ornia",
                "Sethys",
                "Calsea",
                "Tanaesis"
            };

            settlements = BannerKingsConfig.Instance.TitleManager.GetAllTitlesByType(TitleType.Dukedom)
                .Where(t => duchyStringIds.Contains(t.shortName.ToString()))
                .SelectMany(t => t.vassals.Select(v => v.fief))
                .ToList();
        }

        internal override bool IsAvailable()
        {
            return BannerKingsConfig.Instance.TitleManager.GetTitleByStringId("title_calradic_empire") == null;
        }

        internal override bool IsFulfilled(out List<TextObject> failedReasons)
        {
            failedReasons = new List<TextObject>();

            var referenceSettlement = settlements.First();
            var referenceHero = referenceSettlement.Owner;
            var (gold, influence) = GetCosts(referenceHero);

            if (!IsAvailable())
            {
                var title = BannerKingsConfig.Instance.TitleManager.GetTitleByStringId("title_calradic_empire");

                var failedReason = new TextObject("{=!}This title is already founded! de Jure is {DE_JURE.LINK} and de Facto is {DE_FACTO.LINK}.");
                failedReason.SetCharacterProperties("DE_JURE", title.deJure.CharacterObject);
                failedReason.SetCharacterProperties("DE_FACTO", title.DeFacto.CharacterObject);

                failedReasons.Add(failedReason);
            }

            var imperialKingdomsStringIds = new List<string>
            {
                "empire",
                "empire_w",
                "empire_s"
            };
            if (imperialKingdomsStringIds.All(k => k != referenceHero.Clan.Kingdom.StringId) || !referenceHero.IsKingdomLeader())
            {
                failedReasons.Add(new TextObject("{!=}You're not the leader of an Imperial Kingdom."));
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
                    "Establish a new Title",
                    new TextObject("Do you want to establish the title {TITLE}?\nThis will cost you {GOLD}{GOLD_ICON} and {INFLUENCE}{INFLUENCE_ICON}.\nAs a reward your clan will earn {RENOWN} renown.")
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

            BannerKingsConfig.Instance.TitleManager.FoundEmpire(foundAction, "title_calradic_empire");
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