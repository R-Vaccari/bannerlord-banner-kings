using System.Collections.Generic;
using BannerKings.Behaviours.Camping;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Goals.Decisions
{
    public class MakeCamp : Goal
    {
        public MakeCamp(Hero fulfiller = null) : base("MakeCamp", GoalCategory.Personal, GoalUpdateType.Hero, fulfiller)
        {
            Initialize(new TextObject("{=XRknzX3j}Make Camp"),
                new TextObject("{=C8eDVQJw}Levy Duty"));
        }

        public override bool IsAvailable() => true;

        public override bool IsFulfilled(out List<TextObject> failedReasons)
        {
            failedReasons = new List<TextObject>();
            Hero fulfiller = GetFulfiller();

            if (fulfiller.PartyBelongedTo == null)
            {
                failedReasons.Add(new TextObject("{=AvndY1C2}Not in a party"));
            }

            if (!fulfiller.IsPartyLeader)
            {
                failedReasons.Add(new TextObject("{=TFXtC96A}Not party leader"));
            }

            return failedReasons.IsEmpty();
        }
      
        public override void ShowInquiry()
        {
            Hero fulfiller = GetFulfiller();
            InformationManager.ShowInquiry(new InquiryData(new TextObject("{=XRknzX3j}Make Camp").ToString(),
                new TextObject("{=EcMwWGrw}Will you make camp in this position? Dismantling the camp will have the party disorganized for a couple hours.").ToString(),
                true,
                true,
                GameTexts.FindText("str_accept").ToString(),
                GameTexts.FindText("str_cancel").ToString(),
                () => ApplyGoal(),
                null));
        }

        public override void ApplyGoal()
        {
            var hero = GetFulfiller();
            if (hero.PartyBelongedTo != null)
            {
                TaleWorlds.CampaignSystem.Campaign.Current.GetCampaignBehavior<BKCampingBehavior>().MakeCamp(hero.PartyBelongedTo);
            }
        }

        public override void DoAiDecision()
        {
            ApplyGoal();
        }
    }
}