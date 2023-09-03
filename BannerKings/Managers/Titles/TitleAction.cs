using BannerKings.Managers.Helpers;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;

namespace BannerKings.Managers.Titles
{
    public class TitleAction : BannerKingsAction
    {
        public TitleAction(ActionType type, FeudalTitle title, Hero actionTaker)
        {
            Type = type;
            Title = title;
            ActionTaker = actionTaker;
            Vassals = new List<FeudalTitle>();
        }

        public float Gold { get; set; }
        public float Renown { get; set; }
        public ActionType Type { get; }
        public FeudalTitle Title { get; private set; }
        public List<FeudalTitle> Vassals { get; private set; }

        public bool IsHostile()
        {
            return Type is ActionType.Usurp or ActionType.Claim or ActionType.Revoke;
        }

        public void SetTile(FeudalTitle title)
        {
            Title = title;
        }

        public void SetVassals(List<FeudalTitle> vassals)
        {
            Vassals = vassals;
        }

        public override void TakeAction(Hero receiver)
        {
            if (!Possible)
            {
                return;
            }

            switch (Type)
            {
                case ActionType.Usurp:
                    BannerKingsConfig.Instance.TitleManager.UsurpTitle(Title.deJure, this);
                    break;
                case ActionType.Claim:
                    BannerKingsConfig.Instance.TitleManager.AddOngoingClaim(this);
                    break;
                case ActionType.Revoke:
                    BannerKingsConfig.Instance.TitleManager.RevokeTitle(this);
                    break;
                case ActionType.Found:
                    TitleGenerator.FoundKingdom(this);
                    break;
                case ActionType.Grant:
                case ActionType.Destroy:
                case ActionType.Create:
                default:
                    BannerKingsConfig.Instance.TitleManager.GrantTitle(this, receiver);
                    break;
            }
        }
    }

    public enum ActionType
    {
        Usurp,
        Revoke,
        Grant,
        Destroy,
        Create,
        Claim,
        Found
    }
}