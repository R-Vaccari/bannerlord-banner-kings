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
        public MakeCamp(Hero fulfiller = null) : base("goal_camp", fulfiller)
        {
        }

        public override bool TickClanLeaders => true;

        public override bool TickClanMembers => true;

        public override bool TickNotables => false;

        public override GoalCategory Category => GoalCategory.Personal;

        public override Goal GetCopy(Hero fulfiller)
        {
            MakeCamp copy = new MakeCamp(fulfiller);
            copy.Initialize(Name, Description);
            return copy;
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