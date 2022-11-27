using BannerKings.Behaviours.Feasts;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Goals.Decisions
{
    internal class OrganizeFeastDecision : Goal
    {
        private Town feastPlace;
        private List<Clan> guests;
        private float influenceCost;
        public OrganizeFeastDecision() : base("goal_organize_feast_decision", GoalUpdateType.Manual)
        {
            var name = new TextObject("{=RH2NC5ij}Organize Feast");
            var description = new TextObject("{=8XXOBM1L}Organize a feast. Summon lords of the realm to one of your towns or castles, and celebrate with bountiful food. Feasts are an opportunity to improve relations with your Peers. However, some planning is necessary - you don't want your guests out of food or alcohol! Despite all planning, some unfortunate events may occur...\n");

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
                failedReasons.Add(new TextObject("{=gn6WKs03}Cannot organize feasts during wars"));
            }

            CampaignTime lastFeast = Campaign.Current.GetCampaignBehavior<BKFeastBehavior>().LastHeroFeast(GetFulfiller());
            if (lastFeast.ElapsedYearsUntilNow < 1f)
            {
                failedReasons.Add(new TextObject("{=4Gqf2t8F}It has been less than a year since your last feast"));
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

        private float GuestInfluenceCost(Clan clan)
        {
            float cost = 5f;
            cost += clan.Tier * 2f;

            if (clan == clan.Kingdom.RulingClan)
            {
                cost *= 2f;
            }

            if (clan.IsUnderMercenaryService)
            {
                cost *= 0.5f;
            }

            return cost;
        }

        internal override void ShowInquiry()
        {
            var list = new List<InquiryElement>();
            foreach (var town in Clan.PlayerClan.Fiefs)
            {
                list.Add(new InquiryElement(town, town.Name.ToString(), null));
            }


            var clanList = new List<InquiryElement>();
            foreach (var clan in Clan.PlayerClan.Kingdom.Clans)
            {
                if (clan == Clan.PlayerClan)
                {
                    continue;
                }

                clanList.Add(new InquiryElement(clan, 
                    new TextObject("{=USJTkG5d}{CLAN} - {INFLUENCE} influence")
                    .SetTextVariable("CLAN", clan.Name)
                    .SetTextVariable("INFLUENCE", GuestInfluenceCost(clan))
                    .ToString(), 
                    new ImageIdentifier(clan.Banner)));
            }

            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                new TextObject("{=RH2NC5ij}Organize Feast").ToString(),
                new TextObject("{=s71kzA2t}Choose where to hold your feast. The feast food and alcohol will be taken from the settlement Stash.").ToString(),
                list,
                false,
                1,
                GameTexts.FindText("str_accept").ToString(),
                String.Empty,
                delegate (List<InquiryElement> list)
                {
                    feastPlace = (Town)list[0].Identifier;

                    MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                        new TextObject("{=Usb3B69i}Feast Guests").ToString(),
                        new TextObject("{=S9S5PCrP}Choose the guests for your feast. Pick at least 3 different clans from your realm. Mind you, guests will have expectations. They will want food in large variety, of good quality and plentiful. They will also expect a lot of alcohol, and the host to be present.").ToString(),
                        clanList,
                        false,
                        clanList.Count,
                        GameTexts.FindText("str_accept").ToString(),
                        String.Empty,
                        delegate (List<InquiryElement> list)
                        {
                            guests = new List<Clan>();

                            InformationManager.ShowInquiry(new InquiryData(null,
                                null,
                                true,
                                true,
                                null,
                                null,
                                () =>
                                {
                                    foreach (var element in list)
                                    {
                                        Clan clan = (Clan)element.Identifier;
                                        influenceCost += GuestInfluenceCost(clan);
                                        guests.Add(clan);
                                    }
                                    ApplyGoal();
                                },
                                null));
                        },
                        null));
                },
                null));
        }

        internal override void ApplyGoal()
        {
            if (guests.Count < 3)
            {
                InformationManager.DisplayMessage(new InformationMessage(
                    new TextObject("{=YR4uDhkZ}Too few guests! Invite at least 3 clans for a feast.").ToString()));
                return;
            }

            GainKingdomInfluenceAction.ApplyForDefault(GetFulfiller(), -influenceCost);
            Campaign.Current.GetCampaignBehavior<BKFeastBehavior>().LaunchFeast(feastPlace, guests);
        }

        public override void DoAiDecision()
        {
            throw new NotImplementedException();
        }
    }
}