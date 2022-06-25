using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Titles
{
    public class TitleAction : BannerKingsAction
    {
        public float Gold { get; set; }
        public float Renown { get; set; }
        public ActionType Type { get; private set; }
        public FeudalTitle Title { get; private set; }
        

        public TitleAction(ActionType type, FeudalTitle title, Hero taker)
        {
            Type = type;
            Title = title;
            ActionTaker = taker;
        }

        public override void TakeAction(Hero receiver)
        {
            if (!Possible) return;

            if (Type == ActionType.Usurp)
                BannerKingsConfig.Instance.TitleManager.UsurpTitle(Title.deJure, this);
            else if (Type == ActionType.Claim)
                BannerKingsConfig.Instance.TitleManager.AddOngoingClaim(this);
            else if (Type == ActionType.Revoke)
                BannerKingsConfig.Instance.TitleManager.RevokeTitle(this);
            else BannerKingsConfig.Instance.TitleManager.GrantTitle(receiver, this.ActionTaker, this.Title, this.Influence);
        }
    }

    public enum ActionType
    {
        Usurp,
        Revoke,
        Grant,
        Destroy,
        Create,
        Claim
    }
}
