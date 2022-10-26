using System;
using TaleWorlds.CampaignSystem;

namespace BannerKings.Managers.Populations.Estates
{
    public class EstateAction : BannerKingsAction
    {

        public EstateAction(Estate estate, Hero actionTaker, ActionType type, Hero actionTarget = null)
        {
            Estate = estate;
            ActionTaker = actionTaker;
            Type = type;
            ActionTarget = actionTarget;
        }

        public Estate Estate { get; private set; }

        public ActionType Type { get; private set; }

        public Hero ActionTarget { get; private set; }

        public override void TakeAction(Hero receiver = null)
        {
            throw new NotImplementedException();
        }
    }

    public enum ActionType
    {
        Buy,
        Grant,
        Reclaim
    }
}
