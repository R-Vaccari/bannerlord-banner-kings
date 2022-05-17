
using TaleWorlds.CampaignSystem;

namespace BannerKings.Managers.Court
{
    public class CouncilAction : BannerKingsAction
    {
        public CouncilMember TargetPosition { get; private set; }
        public CouncilMember CurrentPosition { get; private set; }
        public CouncilActionType Type { get; private set; }

        public CouncilAction(CouncilActionType type, Hero actionTaker, CouncilMember target, CouncilMember current)
        {
            ActionTaker = actionTaker;
            TargetPosition = target;
            CurrentPosition = current;
            Type = type;
        }


        public override void TakeAction(Hero receiver = null)
        {
            if (Type == CouncilActionType.REQUEST)
                BannerKingsConfig.Instance.CourtManager.AddHeroToCouncil(this);
            else if (Type == CouncilActionType.SWAP)
                BannerKingsConfig.Instance.CourtManager.SwapCouncilPositions(this);
        }
    }

    public enum CouncilActionType
    {
        REQUEST,
        SWAP,
        RELINQUISH
    }
}
