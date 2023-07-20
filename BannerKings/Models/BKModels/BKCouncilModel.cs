using System;
using BannerKings.Managers.Court;
using BannerKings.Managers.Court.Grace;
using BannerKings.Managers.Education.Languages;
using BannerKings.Managers.Titles;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Models.BKModels
{
    public class BKCouncilModel : IBannerKingsModel
    {
        public ExplainedNumber CalculateEffect(Settlement settlement)
        {
            return new ExplainedNumber();
        }
        public ExplainedNumber CalculateAdmCosts(CouncilData data, bool explanations = false)
        {
            ExplainedNumber result = new ExplainedNumber(0, explanations);
            result.LimitMin(0f);
            result.LimitMax(0.9f);

            foreach (var councilMember in data.Positions)
            {
                if (councilMember.Member != null)
                {
                    result.Add(councilMember.AdministrativeCosts(), councilMember.GetCulturalName());
                }
            }

            if (data.CourtGrace != null)
            {
                foreach (var expense in data.CourtGrace.Expenses)
                {
                    result.Add(expense.AdministrativeCost, expense.Name);
                }
            }

            return result;
        }

        public ExplainedNumber CalculateGraceTarget(CouncilData data, bool explanations = false)
        {
            ExplainedNumber result = new ExplainedNumber(0, explanations);
            result.LimitMin(0f);
            result.LimitMax(1000f);
            if (data.Location != null)
            {
                result.Add(data.Location.Prosperity * 0.05f, data.Location.Name);
            }
            
            var lodgings = data.CourtGrace.GetExpense(CourtExpense.ExpenseType.Lodgings);
            var servants = data.CourtGrace.GetExpense(CourtExpense.ExpenseType.Servants);
            var security = data.CourtGrace.GetExpense(CourtExpense.ExpenseType.Security);
            var extravagance = data.CourtGrace.GetExpense(CourtExpense.ExpenseType.Extravagance);
            var supplies = data.CourtGrace.GetExpense(CourtExpense.ExpenseType.Supplies);

            result.Add(lodgings.Grace, lodgings.Name);
            result.Add(servants.Grace, servants.Name);
            //result.Add(security.Grace, security.Name);
            result.Add(extravagance.Grace, extravagance.Name);
            result.Add(supplies.Grace, supplies.Name);

            return result;
        }

        public ExplainedNumber CalculatePositionGrace(CouncilMember position, bool explanations = false)
        {
            ExplainedNumber result = new ExplainedNumber(0, explanations);
            if (position.Member == null)
            {
                return result;
            }

            result.Add(position.InfluenceCosts() * 5f, new TextObject("{=!}Position's influence"));
            result.AddFactor(position.Competence.ResultNumber, new TextObject("{=!}Competence"));

            return result;
        }

        public ExplainedNumber CalculateExpectedGrace(CouncilData data, bool explanations = false)
        {
            ExplainedNumber result = new ExplainedNumber(0, explanations);
            result.LimitMin(0f);
            result.LimitMax(1000f);

            if (data.Clan.Kingdom != null)
            {
                int tier = data.Clan.Tier;
                float tierGrace = 0f;
                if (tier == 2) tierGrace = 50f;
                else if (tier == 3) tierGrace = 100f;
                else if (tier == 4) tierGrace = 150f;
                else if (tier == 5) tierGrace = 200f;
                else if (tier >= 6) tierGrace = 250f;
                result.Add(tierGrace, new TextObject("{=!}Clan tier"));

                if (data.Clan.Kingdom != null && data.Clan.Kingdom.RulingClan == data.Clan)
                {
                    result.Add(300f, new TextObject("{=IcgVKFxZ}Ruler"));
                }

                FeudalTitle title = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(data.Clan.Leader);
                TitleType type = title != null ? title.TitleType : TitleType.Lordship;
                if (type <= TitleType.Barony) result.Add(300f / (float)type, new TextObject(Utils.Helpers.GetTitlePrefix(type,
                    title != null ? title.Contract.Government : GovernmentType.Feudal, data.Clan.Culture)));
            }

            return result;
        }

        public ExplainedNumber CalculateGuestCapacity(CouncilData council, bool explanations = false)
        {
            ExplainedNumber result = new ExplainedNumber(0, explanations);

            if (council.Location != null)
            {
                if (council.Location.IsCastle)
                {
                    result.Add(1f, new TextObject("{=UPhMZ859}Castle"));
                }
                else
                {
                    result.Add(2f, new TextObject("{=FO8mvaZJ}Town"));
                }
            }

            return result;
        }

        public ExplainedNumber CalculateRelocateCourtPrice(Clan clan, Town target, bool explanations = false)
        {
            ExplainedNumber result = new ExplainedNumber(BannerKingsConfig.Instance.ClanFinanceModel.CalculateClanIncome(clan).ResultNumber * 5f, 
                explanations);
            var council = BannerKingsConfig.Instance.CourtManager.GetCouncil(clan);

            if (council.Location != null)
            {
                float factor = Campaign.Current.Models.MapDistanceModel.GetDistance(target.Settlement,
                council.Location.Settlement) / Campaign.AverageDistanceBetweenTwoFortifications;
                result.AddFactor(factor, new TextObject("{=!}Distance between {TOWN1} and {TOWN2}")
                    .SetTextVariable("TOWN1", target.Name)
                    .SetTextVariable("TOWN2", council.Location.Name));
            }

            return result;
        }

        public ExplainedNumber CalculateHeroCompetence(Hero hero, CouncilMember position, bool ignoreTask = false, bool explanations = false)
        {
            ExplainedNumber result = new ExplainedNumber(0f, explanations);
            result.LimitMin(0f);

            if (hero == null)
            {
                return result;
            }

            result.Add(hero.GetSkillValue(position.PrimarySkill) / 200f, position.PrimarySkill.Name);
            if (position.SecondarySkill != null)
            {
                result.Add(hero.GetSkillValue(position.SecondarySkill) / 400f, position.SecondarySkill.Name);
            }

            result.AddFactor(0.15f * (hero.GetAttributeValue(DefaultCharacterAttributes.Intelligence) - 4), 
                DefaultCharacterAttributes.Intelligence.Name);

            Language courtLanguage = BannerKingsConfig.Instance.EducationManager.GetNativeLanguage(position.Culture);
            float fluency = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(hero).GetLanguageFluency(courtLanguage);
            if (fluency < 1f)
            {
                result.AddFactor(-0.3f * fluency, new TextObject("{=vRMD0fdw}{LANGUAGE} fluency")
                    .SetTextVariable("LANGUAGE", courtLanguage.Name));
            }

            if (position.Traits != null)
            {
                foreach (var pair in position.Traits)
                {
                    int trait = hero.GetTraitLevel(pair.Key);
                    result.AddFactor(trait * pair.Value, pair.Key.Name);
                }
            }   

            if (!ignoreTask)
            {
                if (position.CurrentTask != null && position.CurrentTask.Efficiency != 1f)
                {
                    result.AddFactor(position.CurrentTask.Efficiency - 1f, new TextObject("{=ARQYxT6t}Task Efficiency"));
                }
            }

            return result;
        }

        public (bool, string) IsCouncilRoyal(Clan clan)
        {
            var explanation = new TextObject("{=WJFzmFHu}Legal crown council.");

            var kingdom = clan.Kingdom;
            if (kingdom == null)
            {
                explanation = new TextObject("{=JDFpx1eN}No kingdom.");
                return new ValueTuple<bool, string>(false, explanation.ToString());
            }

            if (clan.Kingdom.RulingClan != clan)
            {
                explanation = new TextObject("{=RWoYaSfD}Not the ruling clan.");
                return new ValueTuple<bool, string>(false, explanation.ToString());
            }

            var sovereign = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(kingdom);
            if (sovereign == null)
            {
                explanation = new TextObject("{=CGOqUHZb}Does not hold faction's sovereign title.");
                return new ValueTuple<bool, string>(false, explanation.ToString());
            }

            return new ValueTuple<bool, string>(true, explanation.ToString());
        }

        public bool WillAcceptAction(CouncilAction action, Hero hero)
        {
            if (action.Type != CouncilActionType.REQUEST)
            {
                return true;
            }

            return action.Possible;
        }


        public CouncilAction GetAction(CouncilActionType type, CouncilData council, Hero requester,
            CouncilMember targetPosition, CouncilMember currentPosition = null,
            bool appointed = false)
        {
            return type switch
            {
                CouncilActionType.REQUEST => GetRequest(type, council, requester, targetPosition, currentPosition,
                    appointed),
                CouncilActionType.RELINQUISH => GetRelinquish(type, council, requester, currentPosition, targetPosition,
                    appointed),
                _ => GetSwap(type, council, requester, targetPosition, currentPosition, appointed)
            };
        }

        private CouncilAction GetSwap(CouncilActionType type, CouncilData council, Hero requester,
            CouncilMember targetPosition, CouncilMember currentPosition = null, bool appointed = false)
        {
            var action = new CouncilAction(type, requester, targetPosition, currentPosition, council)
            {
                Influence = GetInfluenceCost(type, targetPosition)
            };

            if (currentPosition == null || currentPosition.Member != requester)
            {
                action.Possible = false;
                action.Reason = new TextObject("{=RpBzpEfz}Not part of the council.");
                return action;
            }

            var adequate = targetPosition.IsValidCandidate(requester);
            if (!adequate.Item1)
            {
                action.Possible = false;
                action.Reason = adequate.Item2;
                return action;
            }

            if (requester.Clan != null && requester.Clan.Influence < action.Influence)
            {
                action.Possible = false;
                action.Reason = new TextObject("{=hVJNXynE}Not enough influence.");
                return action;
            }

            if (targetPosition.IsCorePosition(targetPosition.StringId))
            {
                if (requester.Clan != null && !requester.Clan.Kingdom.Leader.IsFriend(requester))
                {
                    action.Possible = false;
                    action.Reason = new TextObject("{=nCwf2baA}Not trustworthy enough for this position.");
                    return action;
                }

                if (council.GetCompetence(requester, targetPosition) < 0.5f)
                {
                    action.Possible = false;
                    action.Reason = new TextObject("{=opYJzphN}Not competent enough for this position.");
                    return action;
                }
            }

            if (targetPosition.Member != null)
            {
                var candidateDesire = GetDesirability(requester, council, targetPosition);
                var currentDesire = GetDesirability(targetPosition.Member, council, targetPosition);
                if (currentDesire > candidateDesire)
                {
                    action.Possible = false;
                    action.Reason = new TextObject("{=1yzWxjaj}Not a better candidate than current councillor.");
                    return action;
                }
            }

            action.Possible = true;
            action.Reason = new TextObject("{=bjJ99NEc}Action can be taken.");
            return action;
        }

        private CouncilAction GetRelinquish(CouncilActionType type, CouncilData council, Hero requester,
            CouncilMember currentPosition, CouncilMember targetPosition = null, bool appointed = false)
        {
            var action = new CouncilAction(type, requester, targetPosition, currentPosition, council)
            {
                Influence = GetInfluenceCost(type, targetPosition)
            };

            if (requester != null)
            {
                if (targetPosition == null)
                {
                    action.Possible = false;
                    action.Reason = new TextObject("{=B6NnSA7H}No position to be relinquished.");
                    return action;
                }

                if (targetPosition.Member != requester)
                {
                    action.Possible = false;
                    action.Reason = new TextObject("{=peS8Egz2}Not current councilman of the position.");
                    return action;
                }
            }

            action.Possible = true;
            action.Reason = new TextObject("{=bjJ99NEc}Action can be taken.");
            return action;
        }

        private CouncilAction GetRequest(CouncilActionType type, CouncilData council, Hero requester,
            CouncilMember targetPosition, CouncilMember currentPosition = null, bool appointed = false)
        {
            var action = new CouncilAction(type, requester, targetPosition, currentPosition, council)
            {
                Influence = appointed ? 0f : GetInfluenceCost(type, targetPosition)
            };

            if (currentPosition != null && currentPosition.Member == requester)
            {
                action.Possible = false;
                action.Reason = new TextObject("{=x5Wi8QSD}Already part of the council.");
                return action;
            }

            if (!targetPosition.CanMemberChange())
            {
                action.Possible = false;
                action.Reason = new TextObject("{=!}This position's councillor has recently been changed.");
                return action;
            }

            var adequate = targetPosition.IsValidCandidate(requester);
            if (!adequate.Item1)
            {
                action.Possible = false;
                action.Reason = adequate.Item2;
                return action;
            }

            if (!council.GetAvailableHeroes(targetPosition).Contains(requester))
            {
                action.Possible = false;
                action.Reason = new TextObject("{=!}{HERO} already fulfills a position of this type.")
                    .SetTextVariable("HERO", requester.Name);
                return action;
            }

            if (!appointed)
            {
                if (requester.Clan != null && requester.Clan.Influence < action.Influence)
                {
                    action.Possible = false;
                    action.Reason = new TextObject("{=hVJNXynE}Not enough influence.");
                    return action;
                }

                if (targetPosition.IsCorePosition(targetPosition.StringId))
                {
                    if (requester.Clan != null && !requester.Clan.Kingdom.Leader.IsFriend(requester))
                    {
                        action.Possible = false;
                        action.Reason = new TextObject("{=nCwf2baA}Not trustworthy enough for this position.");
                        return action;
                    }

                    if (council.GetCompetence(requester, targetPosition) < 0.5f)
                    {
                        action.Possible = false;
                        action.Reason = new TextObject("{=opYJzphN}Not competent enough for this position.");
                        return action;
                    }
                }

                if (targetPosition.Member != null)
                {
                    var candidateDesire = GetDesirability(requester, council, targetPosition);
                    var currentDesire = GetDesirability(targetPosition.Member, council, targetPosition);
                    if (currentDesire > candidateDesire)
                    {
                        action.Possible = false;
                        action.Reason = new TextObject("{=1yzWxjaj}Not a better candidate than current councillor.");
                        return action;
                    }
                }
            }

            action.Possible = true;
            action.Reason = new TextObject("{=bjJ99NEc}Action can be taken.");
            return action;
        }

        public float GetDesirability(Hero candidate, CouncilData council, CouncilMember position)
        {
            float titleWeight = 0;
            var competence = council.GetCompetence(candidate, position);
            var relation = council.Owner.GetRelation(candidate) * 0.01f;
            if (candidate.Clan == council.Owner.Clan)
            {
                relation -= 0.2f;
            }

            var title = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(candidate);
            if (title != null)
            {
                titleWeight = 4 - (int) title.TitleType;
            }

            return (titleWeight + competence + relation) / 3f;
        }

        public int GetInfluenceCost(CouncilActionType type, CouncilMember targetPosition)
        {
            switch (type)
            {
                case CouncilActionType.REQUEST when targetPosition.Member != null:
                    return 100;
                case CouncilActionType.REQUEST:
                    return 50;
                case CouncilActionType.RELINQUISH:
                    return 0;
            }

            if (targetPosition.Member != null)
            {
                return 50;
            }

            return 10;
        }
    }
}