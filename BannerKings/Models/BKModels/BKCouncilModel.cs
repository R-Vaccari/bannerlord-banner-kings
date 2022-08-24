using System;
using BannerKings.Managers.Court;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;

namespace BannerKings.Models.BKModels
{
    public class BKCouncilModel : IBannerKingsModel
    {
        public ExplainedNumber CalculateEffect(Settlement settlement)
        {
            return new ExplainedNumber();
        }


        public (bool, string) IsCouncilRoyal(Clan clan)
        {
            var explanation = new TextObject("{=!}Legal crown council.");

            var kingdom = clan.Kingdom;
            if (kingdom == null)
            {
                explanation = new TextObject("{=!}No kingdom.");
                return new ValueTuple<bool, string>(false, explanation.ToString());
            }

            if (clan.Kingdom.RulingClan != clan)
            {
                explanation = new TextObject("{=!}Not the ruling clan.");
                return new ValueTuple<bool, string>(false, explanation.ToString());
            }

            var sovereign = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(kingdom);
            if (sovereign == null)
            {
                explanation = new TextObject("{=!}Does not hold faction's sovereign title.");
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
                action.Reason = new TextObject("{=!}Not part of the council.");
                return action;
            }

            if (!targetPosition.IsValidCandidate(requester))
            {
                action.Possible = false;
                action.Reason = new TextObject("{=!}Not a valid candidate.");
                return action;
            }

            if (requester.Clan != null && requester.Clan.Influence < action.Influence)
            {
                action.Possible = false;
                action.Reason = new TextObject("{=!}Not enough influence.");
                return action;
            }

            if (targetPosition.IsCorePosition(targetPosition.Position))
            {
                if (requester.Clan != null && !requester.Clan.Kingdom.Leader.IsFriend(requester))
                {
                    action.Possible = false;
                    action.Reason = new TextObject("{=!}Not trustworthy enough for this position.");
                    return action;
                }

                if (council.GetCompetence(requester, targetPosition.Position) < 0.5f)
                {
                    action.Possible = false;
                    action.Reason = new TextObject("{=!}Not competent enough for this position.");
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
                    action.Reason = new TextObject("{=!}Not a better candidate than current councillor.");
                    return action;
                }
            }

            action.Possible = true;
            action.Reason = new TextObject("{=!}Action can be taken.");
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
                    action.Reason = new TextObject("{=!}No position to be relinquished.");
                    return action;
                }

                if (targetPosition.Member != requester)
                {
                    action.Possible = false;
                    action.Reason = new TextObject("{=!}Not current councilman of the position.");
                    return action;
                }
            }

            action.Possible = true;
            action.Reason = new TextObject("{=!}Action can be taken.");
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
                action.Reason = new TextObject("{=!}Already part of the council.");
                return action;
            }

            if (!targetPosition.IsValidCandidate(requester))
            {
                action.Possible = false;
                action.Reason = new TextObject("{=!}Not a valid candidate.");
                return action;
            }

            if (requester.Clan != null && requester.Clan.Influence < action.Influence)
            {
                action.Possible = false;
                action.Reason = new TextObject("{=!}Not enough influence.");
                return action;
            }


            if (!appointed)
            {
                if (targetPosition.IsCorePosition(targetPosition.Position))
                {
                    if (requester.Clan != null && !requester.Clan.Kingdom.Leader.IsFriend(requester))
                    {
                        action.Possible = false;
                        action.Reason = new TextObject("{=!}Not trustworthy enough for this position.");
                        return action;
                    }

                    if (council.GetCompetence(requester, targetPosition.Position) < 0.5f)
                    {
                        action.Possible = false;
                        action.Reason = new TextObject("{=!}Not competent enough for this position.");
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
                        action.Reason = new TextObject("{=!}Not a better candidate than current councillor.");
                        return action;
                    }
                }
            }

            action.Possible = true;
            action.Reason = new TextObject("{=!}Action can be taken.");
            return action;
        }

        public float GetDesirability(Hero candidate, CouncilData council, CouncilMember position)
        {
            float titleWeight = 0;
            var competence = council.GetCompetence(candidate, position.Position);
            var relation = council.Owner.GetRelation(candidate) * 0.01f;
            if (candidate.Clan == council.Owner.Clan)
            {
                relation -= 0.2f;
            }

            var title = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(candidate);
            if (title != null)
            {
                titleWeight = 4 - (int) title.type;
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