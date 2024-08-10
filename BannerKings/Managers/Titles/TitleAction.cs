using BannerKings.CampaignContent.Traits;
using BannerKings.Managers.Helpers;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

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

        public bool IsWilling
        {
            get
            {
                bool result = true;
                if (IsHostile())
                {
                    Hero target = Title.deJure;
                    int ambitious = ActionTaker.GetTraitLevel(BKTraits.Instance.Ambitious);
                    if (ambitious >= 1)
                    {
                        if (ambitious == 2)
                        {
                            result = true;
                        }
                        else if (ActionTaker.GetRelation(target) < 50)
                        {
                            result = true;
                        }
                    }

                    if (ActionTaker.MapFaction != target.MapFaction)
                    {
                        result = true;
                    }

                    if (ActionTaker.IsFriend(target))
                    {
                        result = false;
                    }
                    else if (ActionTaker.IsEnemy(target))
                    {
                        result = true;
                    }

                    if (result && target.MapFaction == ActionTaker.MapFaction)
                    {
                        result = false;
                        if (MBRandom.RandomFloat < 0.01f) result = true;
                    }
                }

                if (Gold >= ActionTaker.Gold * 0.5f)
                {
                    result = false;
                }
               
                return result;
            }
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
                case ActionType.Create:
                    BannerKingsConfig.Instance.TitleManager.CreateTitle(this);
                    break;
                case ActionType.Found:
                    TitleGenerator.FoundKingdom(this);
                    break;
                case ActionType.Grant:
                case ActionType.Destroy:
               
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