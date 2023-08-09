using System.Collections.Generic;
using System.Linq;
using BannerKings.Managers.Titles;
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

        public GreaterBattaniaGoal() : base("goal_greater_battania", GoalCategory.Unique, GoalUpdateType.Settlement)
        {
            var name = new TextObject("{=BLugLsWR}Unite Greater Battania");
            var description = new TextObject("{=EKBkrvse}Unite the old Battanian lands back into a greater realm. To the West, the rascal Vlandians have taken the valley of Llyn Modris and called it 'Ocs Hall'. To the East, the bloodthristy Imperials submitted Epicrotea to their domination. The threat of Battanian extermination grows stronger with enemies all around aiming for it's lands. You must bring all battanian and formerly battanian towns and castles under control of your realm. The new empire will have a feudal contract with hereditary succcession.\n\n");

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

        public override bool IsAvailable()
        {
            return BannerKingsConfig.Instance.TitleManager.GetTitleByStringId("title_greater_battania") == null;
        }

        public override bool IsFulfilled(out List<TextObject> failedReasons)
        {
            failedReasons = new List<TextObject>();

            var referenceSettlement = settlements.First();
            var referenceHero = Hero.MainHero;
            var (gold, influence) = GetCosts(referenceHero);
            var culture = Utils.Helpers.GetCulture("battania");

            if (!IsAvailable())
            {
                var title = BannerKingsConfig.Instance.TitleManager.GetTitleByStringId("title_greater_battania");

                var failedReason = new TextObject("{=jHzaifoJ}This title is already founded! de Jure is {DE_JURE.LINK} and de Facto is {DE_FACTO.LINK}.");
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

                var battaniaKingdom = Campaign.Current.Kingdoms.FirstOrDefault(k => k.StringId == "battania");
                if (referenceHero.Clan.Kingdom != null)
                {
                    if (referenceHero.Clan.Kingdom.Culture != culture)
                    {
                        //If Battania does not exist, culture must be Battanian.
                        failedReasons.Add(new TextObject("{=4jUw7j4u}Your kingdom is not part of {CULTURE} culture.")
                            .SetTextVariable("CULTURE", culture.Name));
                    }

                    if (BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(referenceHero.Clan.Kingdom) == null)
                    {
                        failedReasons.Add(new TextObject("{=DnbZbcT7}Your kingdom has no title associated with it. Found a de Jure kingdom title for your faction."));
                    }
                } 
                else
                {
                    failedReasons.Add(new TextObject("{=YQhz7MP4}You are not a faction leader."));
                }

                var religion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(referenceHero);
                if (religion == null || religion.Faith.GetId() != "amra")
                {
                    var amra = BannerKingsConfig.Instance.ReligionsManager.GetReligionById("amra");
                    failedReasons.Add(new TextObject("{=NVcg68Lz}You do not adhere to the {RELIGION} faith.")
                        .SetTextVariable("RELIGION", amra.Faith.GetFaithName()));
                }

                if (BannerKingsConfig.Instance.TitleManager != null)
                {
                    foreach (var settlement in settlements)
                    {
                        var title = BannerKingsConfig.Instance.TitleManager.GetTitle(settlement);
                        if (title != null && (title.deFacto == null || title.deFacto.MapFaction != referenceHero.MapFaction))
                        {
                            failedReasons.Add(new TextObject("{=btzaJMMD}Your kingdom is not de facto ruler of {SETTLEMENT}")
                                .SetTextVariable("SETTLEMENT", settlement.Name));
                        }
                    }
                }
            }

            return failedReasons.IsEmpty();
        }

        public override void ShowInquiry()
        {
            var (gold, influence) = GetCosts(GetFulfiller());
            
            InformationManager.ShowInquiry
            (
                new InquiryData
                (
                    "Establish a new Empire",
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

        public override void ApplyGoal()
        {
            var founder = Hero.MainHero;
            var (gold, influence) = GetCosts(founder);

            var foundAction = new TitleAction(ActionType.Found, null, founder)
            {
                Gold = gold,
                Influence = influence,
                Renown = 100
            };

            var vassals = new List<FeudalTitle>();
            Kingdom battania = Kingdom.All.ToList().FirstOrDefault(x => x.StringId == "battania");

            if (battania != null)
            {
                var title = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(battania);
                if (title != null)
                {
                    vassals.Add(title);
                }
            } 
            else
            {
                var title = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(founder.Clan.Kingdom);
                if (title != null)
                {
                    vassals.Add(title);
                }
            }

            foundAction.SetVassals(vassals);

            BannerKingsConfig.Instance.TitleManager.FoundEmpire(foundAction, new TextObject("{=5M28g8TK}Greater Battania"), 
                "title_greater_battania");
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