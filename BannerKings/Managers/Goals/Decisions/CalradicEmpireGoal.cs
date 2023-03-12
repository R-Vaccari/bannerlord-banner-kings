using System.Collections.Generic;
using System.Linq;
using BannerKings.Managers.Institutions.Religions.Faiths;
using BannerKings.Managers.Titles;
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

        public CalradicEmpireGoal() : base("goal_calradic_empire", GoalCategory.Unique, GoalUpdateType.Settlement)
        {
            var name = new TextObject("{=cZzO6kya}Reform the Imperium Calradium");
            var description = new TextObject("{=WCde02Um}Reestablish the former Calradian Empire. The Empire spanned most of the continent before emperor Arenicos died without a clear heir. By reforming the empire, you crush the validity of claimants, and ahead of you lies a new path for greatness. You must bring all imperial duchies under control of your realm.\n\n");

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
            return Clan.PlayerClan.Kingdom != null &&
                BannerKingsConfig.Instance.TitleManager.GetTitleByStringId("title_calradic_empire") == null;
        }

        internal override bool IsFulfilled(out List<TextObject> failedReasons)
        {
            failedReasons = new List<TextObject>();

            var referenceSettlement = settlements.FirstOrDefault();
            if (referenceSettlement == null)
            {
                return false;
            }

            var referenceHero = Hero.MainHero;
            var (gold, influence) = GetCosts(referenceHero);
            var culture = Utils.Helpers.GetCulture("empire");

            if (!IsAvailable())
            {
                var title = BannerKingsConfig.Instance.TitleManager.GetTitleByStringId("title_calradic_empire");

                var failedReason = new TextObject("{=1AwUwbfQ}This title is already founded! De Jure is {DE_JURE.LINK} and de Facto is {DE_FACTO.LINK}.");
                failedReason.SetCharacterProperties("DE_JURE", title.deJure.CharacterObject);
                failedReason.SetCharacterProperties("DE_FACTO", title.DeFacto.CharacterObject);

                failedReasons.Add(failedReason);
            }
            else
            {
                if (referenceHero.Gold < gold)
                {
                    failedReasons.Add(new TextObject("{=3KoOfniE}You need at least {GOLD}{GOLD_ICON}")
                        .SetTextVariable("GOLD", $"{gold:n0}"));
                }

                if (referenceHero.Clan.Influence < influence)
                {
                    failedReasons.Add(new TextObject("{=FWMoFfdT}You need at least {INFLUENCE}{INFLUENCE_ICON}")
                        .SetTextVariable("INFLUENCE", $"{influence:n0}")
                        .SetTextVariable("INFLUENCE_ICON", "<img src=\"General\\Icons\\Influence@2x\" extend=\"7\">"));
                }

                if (referenceHero.Culture != culture)
                {
                    failedReasons.Add(new TextObject("{=18Dv2nt4}You are not part of {CULTURE} culture.")
                            .SetTextVariable("CULTURE", culture.Name));
                }

                /*var imperialKingdomsStringIds = new List<string> { "empire", "empire_w", "empire_s" };
                var imperialKingdoms = Campaign.Current.Kingdoms.Where(k => imperialKingdomsStringIds.Contains(k.StringId)).ToList();
                if (imperialKingdoms.Any() && imperialKingdoms.All(ik => ik.Leader != referenceHero))
                {
                    failedReasons.Add(new TextObject("{=YFeW2BUA}You're not the leader of an Imperial Kingdom."));
                }
                else */
                
                if (referenceHero.Clan.Kingdom != null && referenceHero.Clan.Kingdom.Culture != culture)
                {
                    failedReasons.Add(new TextObject("{=4jUw7j4u}Your kingdom is not part of {CULTURE} culture.")
                        .SetTextVariable("CULTURE", culture.Name));
                }

                var religion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(referenceHero);
                if (religion == null || religion.Faith.FaithGroup != DefaultFaiths.Instance.ImperialGroup)
                {
                    var amra = BannerKingsConfig.Instance.ReligionsManager.GetReligionById("amra");
                    failedReasons.Add(new TextObject("{=gcdyXvXR}You do not adhere to a faith that is part of the {RELIGION} faith group.")
                        .SetTextVariable("RELIGION", DefaultFaiths.Instance.ImperialGroup.Name));
                }

                failedReasons.AddRange
                (
                    from settlement in settlements
                    let title = BannerKingsConfig.Instance.TitleManager.GetTitle(settlement)
                    where title.deFacto.MapFaction != referenceHero.MapFaction
                    select new TextObject("{=btzaJMMD}Your kingdom is not de facto ruler of {SETTLEMENT}")
                        .SetTextVariable("SETTLEMENT", settlement.EncyclopediaLinkWithName)
                );
            }

            return failedReasons.IsEmpty();
        }

        internal override void ShowInquiry()
        {
            var (gold, influence) = GetCosts(GetFulfiller());
            
            InformationManager.ShowInquiry
            (
                new InquiryData
                (
                    "Establish a new Title",
                    new TextObject("{=qjD2WwBH}Do you want to establish the title {TITLE}?\nThis will cost you {GOLD}{GOLD_ICON} and {INFLUENCE}{INFLUENCE_ICON}.\nAs a reward your clan will earn {RENOWN} renown.")
                        .SetTextVariable("TITLE", name)
                        .SetTextVariable("GOLD", gold)
                        .SetTextVariable("INFLUENCE", influence)
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

        internal override void ApplyGoal()
        {
            var founder = Hero.MainHero;
            var (gold, influence) = GetCosts(founder);

            var foundAction = new TitleAction(ActionType.Found, null, founder)
            {
                Gold = gold,
                Influence = influence,
                Renown = 100,
            };
            var vassals = new List<FeudalTitle>();
            IEnumerable<Kingdom> kingdoms = Kingdom.All.ToList().FindAll(x => x.StringId == "empire" ||
                x.StringId == "empire_w" || x.StringId == "empire_s");

            foreach (var kingdom in kingdoms)
            {
                var title = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(kingdom);
                if (title != null)
                {
                    vassals.Add(title);
                }
            }

            foundAction.SetVassals(vassals);

            BannerKingsConfig.Instance.TitleManager.FoundEmpire(foundAction, new TextObject("{=Tdi4brSb}Calradia"), 
                "title_calradic_empire", "imperial");
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