using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;

namespace BannerKings.Managers.Court
{
    public class CouncilAction : BannerKingsAction
    {
        public CouncilAction(CouncilActionType type, Hero actionTaker, CouncilPosition target, CouncilPosition current,
            CouncilData council)
        {
            ActionTaker = actionTaker;
            TargetPosition = target;
            CurrentPosition = current;
            Type = type;
            Council = council;
        }

        public CouncilPosition TargetPosition { get; }
        public CouncilPosition CurrentPosition { get; }
        public CouncilActionType Type { get; }
        public CouncilData Council { get; }


        public override void TakeAction(Hero receiver = null)
        {
            switch (Type)
            {
                case CouncilActionType.REQUEST:
                    BannerKingsConfig.Instance.CourtManager.AddHeroToCouncil(this);
                    break;
                case CouncilActionType.SWAP:
                    BannerKingsConfig.Instance.CourtManager.SwapCouncilPositions(this);
                    break;
                default:
                    BannerKingsConfig.Instance.CourtManager.RelinquishCouncilPosition(this);
                    break;
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