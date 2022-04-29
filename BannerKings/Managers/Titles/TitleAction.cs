﻿using TaleWorlds.CampaignSystem;
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
            Type = type;
            Title = title;
            ActionTaker = taker;
        }

        public TitleAction(bool possible, TextObject reason, float gold, float influence, float renown)
        {
            Possible = possible;
            Reason = reason;
            Gold = gold;
            Influence = influence;
            Renown = renown;
        }

        public void TakeAction(Hero receiver)
        {
            if (!Possible) return;

            if (Type == ActionType.Usurp)
                BannerKingsConfig.Instance.TitleManager.UsurpTitle(Title.deJure, this);
            else if (Type == ActionType.Claim)
                BannerKingsConfig.Instance.TitleManager.AddOngoingClaim(this);
            else BannerKingsConfig.Instance.TitleManager.GrantTitle(receiver, ActionTaker, Title, Influence);
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
