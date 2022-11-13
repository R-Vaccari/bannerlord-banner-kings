using BannerKings.Behaviours.Feasts;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Goals.Decisions
{
    internal class OrganizeFeastDecision : Goal
    {
        private Town feastPlace;
        public OrganizeFeastDecision() : base("goal_organize_feast_decision", GoalUpdateType.Manual)
        {
            var name = new TextObject("{=!}Organize Feast");
            var description = new TextObject("{=!}Organize a feast.\n");

            Initialize(name, description);
        }

        internal override bool IsAvailable()
        {
            return Clan.PlayerClan.Kingdom != null && Clan.PlayerClan.Fiefs.Count > 0;
        }

        internal override bool IsFulfilled(out List<TextObject> failedReasons)
        {
            failedReasons = new List<TextObject>();

            if (!IsAvailable())
            {
                return false;
            }

            if (FactionManager.GetEnemyKingdoms(Clan.PlayerClan.Kingdom).Count() > 0)
            {
                failedReasons.Add(new TextObject("{=!}Cannot organize feasts during wars"));
            }

            /*
            if (Clan.PlayerClan.Influence < decision.GetProposalInfluenceCost())
            {
                failedReasons.Add(GameTexts.FindText("str_decision_not_enough_influence"));
            }*/

            return failedReasons.Count == 0;
        }

        internal override Hero GetFulfiller()
        {
            return Hero.MainHero;
        }

        internal override void ShowInquiry()
        {
            var list = new List<InquiryElement>();
            foreach (var town in Clan.PlayerClan.Fiefs)
            {
                list.Add(new InquiryElement(town, town.Name.ToString(), null));
            }

            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                new TextObject().ToString(),
                new TextObject().ToString(),
                list,
                true,
                1,
                GameTexts.FindText("str_confirm").ToString(),
                String.Empty,
                delegate (List<InquiryElement> list)
                {
                    feastPlace = (Town)list[0].Identifier;
                    ApplyGoal();
                },
                null));
        }

        internal override void ApplyGoal()
        {
            Campaign.Current.GetCampaignBehavior<BKFeastBehavior>().LaunchFeast(feastPlace);
        }

        public override void DoAiDecision()
        {
            throw new NotImplementedException();
        }
    }
}