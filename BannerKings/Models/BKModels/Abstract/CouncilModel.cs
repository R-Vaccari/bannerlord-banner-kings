using BannerKings.Managers.Court;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;

namespace BannerKings.Models.BKModels.Abstract
{
    public abstract class CouncilModel
    {
        public abstract ExplainedNumber CalculateAdmCosts(CouncilData data, bool explanations = false);
        public abstract ExplainedNumber CalculateGraceTarget(CouncilData data, bool explanations = false);
        public abstract ExplainedNumber CalculateExpectedGrace(CouncilData data, bool explanations = false);
        public abstract ExplainedNumber CalculateHeroCompetence(Hero hero, CouncilMember position, bool ignoreTask = false, bool explanations = false);
        public abstract ExplainedNumber CalculateRelocateCourtPrice(Clan clan, Town target, bool explanations = false);
        public abstract CouncilAction GetAction(CouncilActionType type, CouncilData council, Hero requester,
            CouncilMember targetPosition, CouncilMember currentPosition = null,
            bool appointed = false);

        public bool WillAcceptAction(CouncilAction action, Hero hero)
        {
            if (action.Type != CouncilActionType.REQUEST)
            {
                return true;
            }

            return action.Possible;
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
    }
}
