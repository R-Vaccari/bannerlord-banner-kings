using BannerKings.Managers.Institutions.Religions;
using BannerKings.Managers.Titles;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Goals
{
    public abstract class EmpireGoal : Goal
    {
        protected EmpireGoal(string stringId, Hero fulfiller = null) : base(stringId, fulfiller)
        {
        }

        public override bool TickClanLeaders => true;

        public override bool TickClanMembers => false;

        public override bool TickNotables => false;

        public override GoalCategory Category => GoalCategory.Unique;

        public abstract List<CultureObject> Cultures { get; }
        public abstract List<Religion> Religions { get; }
        public abstract List<string> SettlementIds { get; }
        public List<Settlement> Settlements => TaleWorlds.CampaignSystem.Campaign.Current.Settlements.Where(s => SettlementIds.Contains(s.StringId)).ToList();
        public abstract string TitleId { get; }

        public abstract (float Gold, float Influence) GetCosts(Hero hero);

        public override bool IsAvailable() => BannerKingsConfig.Instance.TitleManager.GetTitleByStringId(TitleId) == null;

        public override void ShowInquiry()
        {
            var (gold, influence) = GetCosts(GetFulfiller());

            InformationManager.ShowInquiry
            (
                new InquiryData
                (
                    new TextObject("{=!}Establish a new Empire").ToString(),
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

        public override bool IsFulfilled(out List<TextObject> failedReasons)
        {
            failedReasons = new List<TextObject>();
            var referenceHero = GetFulfiller();
            var (gold, influence) = GetCosts(referenceHero);

            if (!IsAvailable())
            {
                var title = BannerKingsConfig.Instance.TitleManager.GetTitleByStringId(TitleId);

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

                if (!Cultures.Any(x => x.StringId == referenceHero.Culture.StringId))
                {
                    failedReasons.Add(new TextObject("{=!}You are not part of any of the ({CULTURES}) cultures.")
                        .SetTextVariable("CULTURES", string.Join(", ", Cultures)));
                }

                if (referenceHero.Clan.Kingdom != null)
                {
                    if (!Cultures.Any(x => x.StringId == referenceHero.Clan.Kingdom.Culture.StringId))
                    {
                        failedReasons.Add(new TextObject("{=!}You are not part of any of the ({CULTURES}) cultures.")
                            .SetTextVariable("CULTURES", string.Join(", ", Cultures)));
                    }

                    var title = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(referenceHero.Clan.Kingdom);
                    if (title == null)
                    {
                        failedReasons.Add(new TextObject("{=DnbZbcT7}Your kingdom has no title associated with it. Found a de Jure kingdom title for your faction."));
                    }
                    else if (title.TitleType == TitleType.Empire)
                    {
                        failedReasons.Add(new TextObject("{=!}Your realm, {REALM} is already attached to the Empire-level title {TITLE}.")
                            .SetTextVariable("REALM", referenceHero.Clan.Kingdom.Name)
                            .SetTextVariable("TITLE", title.FullName));
                    }
                }
                else
                {
                    failedReasons.Add(new TextObject("{=YQhz7MP4}You are not a faction leader."));
                }

                var religion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(referenceHero);
                if (religion == null || !Religions.Any(x => x.Faith.StringId == religion.Faith.StringId))
                {
                    failedReasons.Add(new TextObject("{=!}You do not adhere to an adequate faith ({FAITHS})")
                        .SetTextVariable("FAITHS", string.Join(", ", Religions)));
                }

                if (BannerKingsConfig.Instance.TitleManager != null)
                {
                    var missing = new List<Settlement>(20);
                    foreach (var settlement in Settlements)
                    {
                        var title = BannerKingsConfig.Instance.TitleManager.GetTitle(settlement);
                        if (title != null && (title.deFacto == null || title.deFacto.MapFaction != referenceHero.MapFaction))
                        {
                            missing.Add(settlement);
                        }
                    }

                    failedReasons.Add(new TextObject("{=!}Your kingdom is not de facto ruler of ({SETTLEMENTS})")
                               .SetTextVariable("SETTLEMENTS", string.Join(", ", missing)));
                }
            }

            return failedReasons.IsEmpty();
        }

        public override void DoAiDecision()
        {
            //TODO: Implement the AI decision for this goal.
            //ApplyGoal();
        }
    }
}
