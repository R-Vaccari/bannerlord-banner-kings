using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;

namespace BannerKings.Managers.Court
{
    public class CouncilAction : BannerKingsAction
    {
        public CouncilAction(CouncilActionType type, Hero actionTaker, CouncilMember target, CouncilMember current,
            CouncilData council)
        {
            ActionTaker = actionTaker;
            TargetPosition = target;
            CurrentPosition = current;
            Type = type;
            Council = council;
        }

        public CouncilMember TargetPosition { get; }
        public CouncilMember CurrentPosition { get; }
        public CouncilActionType Type { get; }
        public CouncilData Council { get; }


        public override void TakeAction(Hero receiver = null)
        {
            if (Type == CouncilActionType.REQUEST)
            {
                BannerKingsConfig.Instance.CourtManager.AddHeroToCouncil(this);
            }
            else if (Type == CouncilActionType.SWAP)
            {
                BannerKingsConfig.Instance.CourtManager.SwapCouncilPositions(this);
            }
            else
            {
                BannerKingsConfig.Instance.CourtManager.RelinquishCouncilPosition(this);
            }
        }

        public void Reject(Hero rejector)
        {
            ChangeRelationAction.ApplyRelationChangeBetweenHeroes(ActionTaker, rejector, CourtManager.ON_REJECTED_RELATION);
        }
    }

    public enum CouncilActionType
    {
        REQUEST,
        SWAP,
        RELINQUISH
    }
}