using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;

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
            if (Type == ActionType.Grant)
            {
                BannerKingsConfig.Instance.TitleManager.GrantEstate(this);
            }
            else if (Type == ActionType.Buy)
            {
                GiveGoldAction.ApplyBetweenCharacters(ActionTaker, Estate.Owner, (int)Estate.EstateValue.ResultNumber);
                Estate.SetOwner(ActionTaker);
            }
            else
            {
                ChangeRelationAction.ApplyPlayerRelation(Estate.Owner,
                           -BannerKingsConfig.Instance.EstatesModel.CalculateEstateGrantRelation(Estate, ActionTaker));
                Estate.SetOwner(ActionTaker);
            }
        }
    }

    public enum ActionType
    {
        Buy,
        Grant,
        Reclaim
    }
}
