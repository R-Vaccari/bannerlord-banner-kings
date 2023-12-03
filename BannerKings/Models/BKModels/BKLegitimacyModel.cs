using BannerKings.Behaviours.Diplomacy;
using BannerKings.Managers.Court;
using BannerKings.Managers.Institutions.Religions;
using BannerKings.Managers.Institutions.Religions.Faiths;
using BannerKings.Managers.Titles;
using BannerKings.Utils.Models;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace BannerKings.Models.BKModels
{
    public class BKLegitimacyModel
    {
        public BKExplainedNumber CalculateEffect(KingdomDiplomacy diplomacy, bool explanations = false)
        {
            var result = new BKExplainedNumber(0f, explanations);
            Kingdom kingdom = diplomacy.Kingdom;
            Hero leader = kingdom.Leader;

            FeudalTitle title = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(kingdom);
            if (title != null)
            {
                if (title.deJure == leader)
                {
                    result.Add(0.3f, new TextObject("{=!}De Jure holder of {KINGDOM}")
                    .SetTextVariable("KINGDOM", title.FullName));
                }

                if (title.DeFacto == leader)
                {
                    result.Add(0.1f, new TextObject("{=!}De Facto holder of {KINGDOM}")
                    .SetTextVariable("KINGDOM", title.FullName));
                }
            }
            else
            {
                FeudalTitle highestTitle = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(leader);
                if (highestTitle != null)
                {
                    TextObject titleDescription = Utils.TextHelper.GetTitlePrefix(highestTitle.TitleType, leader.Culture);
                    if (highestTitle.TitleType == TitleType.County)
                    {
                        result.Add(0.05f, new TextObject("{=!}Highest title is of {TITLE} level")
                        .SetTextVariable("TITLE", titleDescription));
                    }

                    if (highestTitle.TitleType == TitleType.Dukedom)
                    {
                        result.Add(0.1f, new TextObject("{=!}Highest title is of {TITLE} level")
                        .SetTextVariable("TITLE", titleDescription));
                    }

                    if (highestTitle.TitleType == TitleType.Kingdom)
                    {
                        result.Add(0.15f, new TextObject("{=!}Highest title is of {TITLE} level")
                        .SetTextVariable("TITLE", titleDescription));
                    }

                    if (highestTitle.TitleType == TitleType.Empire)
                    {
                        result.Add(0.3f, new TextObject("{=!}Highest title is of {TITLE} level")
                        .SetTextVariable("TITLE", titleDescription));
                    }
                }

                bool isHighest = true;
                foreach (Clan clan in kingdom.Clans)
                {
                    if (clan != leader.Clan && !clan.IsUnderMercenaryService)
                    {
                        FeudalTitle clantTitle = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(leader);
                        if (clantTitle != null && (highestTitle == null || clantTitle.TitleType <= highestTitle.TitleType))
                        {
                            isHighest = false;
                        }
                    }
                }

                if (isHighest)
                {
                    result.Add(0.1f, new TextObject("{=!}Holder of the highest title level within members of {KINGDOM}")
                    .SetTextVariable("KINGDOM", kingdom.Name));
                }
            }

            if (leader.Culture == kingdom.Culture)
            {
                result.Add(0.075f, new TextObject("{=!}Culture match with {KINGDOM}")
                    .SetTextVariable("KINGDOM", kingdom.Name));
            }

            Religion leaderRel = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(leader);
            if (leaderRel != null && diplomacy.Religion != null)
            {
                FaithStance stance = diplomacy.Religion.GetStance(leaderRel.Faith);
                if (stance == FaithStance.Hostile) result.Add(-0.25f, new TextObject("{=!}Hostile faith to {FAITH}")
                    .SetTextVariable("FAITH", diplomacy.Religion.Faith.GetFaithName()));
                else if (stance == FaithStance.Untolerated) result.Add(-0.10f, new TextObject("{=!}Untolerated faith to {FAITH}")
                    .SetTextVariable("FAITH", diplomacy.Religion.Faith.GetFaithName()));
                else result.Add(0.10f, new TextObject("{=!}Faith match with {KINGDOM}")
                    .SetTextVariable("KINGDOM", kingdom.Name));
            }

            CouncilData council = BannerKingsConfig.Instance.CourtManager.GetCouncil(leader.Clan);
            float expectedGrace = council.CourtGrace.ExpectedGrace.ResultNumber;
            float grace = council.CourtGrace.Grace;
            float graceFactor = grace / expectedGrace;
            result.Add(-0.2f * (1f - graceFactor), new TextObject("{=FFr56V5A}Grace ({GRACE}) correlation to expected grace ({EXPECTED})")
                .SetTextVariable("GRACE", grace)
                .SetTextVariable("EXPECTED", expectedGrace));

            int tier = leader.Clan.Tier;
            if (tier < 5)
            {
                result.Add(-10f * (5f / tier), new TextObject());
            }

            return result;
        }
    }
}