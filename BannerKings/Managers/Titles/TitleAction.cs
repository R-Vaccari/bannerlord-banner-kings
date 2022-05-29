using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Titles
{
    public class TitleAction
    {
        public bool Possible { get; set; }
        public TextObject Reason { get; set; }
        public float Gold { get; set; }
        public float Influence { get; set; }
        public float Renown { get; set; }
        public ActionType Type { get; private set; }
        public FeudalTitle Title { get; private set; }
        public Hero ActionTaker { get; private set; }

        public TitleAction(ActionType type, FeudalTitle title, Hero taker)
        {
            this.Type = type;
            this.Title = title;
            this.ActionTaker = taker;
        }

        public TitleAction(bool possible, TextObject reason, float gold, float influence, float renown)
        {
            this.Possible = possible;
            this.Reason = reason;
            this.Gold = gold;
            this.Influence = influence;
            this.Renown = renown;
        }

        public void TakeAction(Hero receiver)
        {
            if (!this.Possible) return;

            if (this.Type == ActionType.Usurp)
                BannerKingsConfig.Instance.TitleManager.UsurpTitle(this.Title.deJure, this);
            else if (this.Type == ActionType.Claim)
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
