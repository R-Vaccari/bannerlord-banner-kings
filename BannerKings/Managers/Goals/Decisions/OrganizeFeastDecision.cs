using BannerKings.Behaviours.Feasts;
using BannerKings.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using static BannerKings.Behaviours.Feasts.Feast;

namespace BannerKings.Managers.Goals.Decisions
{
    public class OrganizeFeastDecision : Goal
    {
        private Town feastPlace;
        private List<Clan> guests;
        private float influenceCost;
        private FeastType type;

        public OrganizeFeastDecision(Hero fulfiller = null, Behaviours.Feasts.Feast.FeastType type = FeastType.Normal) : base("goal_organize_feast_decision", fulfiller)
        {
        }

        public override bool TickClanLeaders => true;

        public override bool TickClanMembers => false;

        public override bool TickNotables => false;

        public override GoalCategory Category => GoalCategory.Kingdom;

        public override Goal GetCopy(Hero fulfiller)
        {
            OrganizeFeastDecision copy = new OrganizeFeastDecision(fulfiller);
            copy.Initialize(Name, Description);
            return copy;
        }

        public override bool IsAvailable()
        {
            return Clan.PlayerClan.Kingdom != null && Clan.PlayerClan.Fiefs.Count > 0;
        }

        public override bool IsFulfilled(out List<TextObject> failedReasons)
        {
            failedReasons = new List<TextObject>();

            if (!IsAvailable())
            {
                return false;
            }

            if (GetFulfiller().Clan.Kingdom == null)
            {
                failedReasons.Add(new TextObject("{=JDFpx1eN}No kingdom."));
            }
            else if (FactionManager.GetEnemyKingdoms(GetFulfiller().Clan.Kingdom).Count() > 0)
            {
                failedReasons.Add(new TextObject("{=gn6WKs03}Cannot organize feasts during wars"));
            }

            CampaignTime lastFeast = TaleWorlds.CampaignSystem.Campaign.Current.GetCampaignBehavior<BKFeastBehavior>().LastHeroFeast(GetFulfiller());
            if (lastFeast.ElapsedYearsUntilNow < 1f)
            {
                failedReasons.Add(new TextObject("{=4Gqf2t8F}It has been less than a year since your last feast"));
            }

            if (GetFulfiller().Clan.Fiefs.Count == 0)
            {
                failedReasons.Add(new TextObject("{=MeYrOaFb}No adequate fief to make a feast"));
            }

            /*
            if (Clan.PlayerClan.Influence < decision.GetProposalInfluenceCost())
            {
                failedReasons.Add(GameTexts.FindText("str_decision_not_enough_influence"));
            }*/

            return failedReasons.Count == 0;
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

        public override void ShowInquiry()
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

                var busy = TaleWorlds.CampaignSystem.Campaign.Current.GetCampaignBehavior<BKFeastBehavior>().IsClanBusy(clan);
                if (busy)
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
                true,
                1,
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
                        true,
                        1,
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

        public override void ApplyGoal()
        {
            if (guests.Count < 3)
            {
                if (GetFulfiller() == Hero.MainHero)
                {
                    InformationManager.DisplayMessage(new InformationMessage(
                                       new TextObject("{=YR4uDhkZ}Too few guests! Invite at least 3 clans for a feast.").ToString()));
                }
               
                return;
            }

            GainKingdomInfluenceAction.ApplyForDefault(GetFulfiller(), -influenceCost);
            TaleWorlds.CampaignSystem.Campaign.Current.GetCampaignBehavior<BKFeastBehavior>().LaunchFeast(feastPlace, guests, null, type);
        }

        public override void DoAiDecision()
        {
            var reasons = new List<TextObject>();
            if (!BannerKingsSettings.Instance.Feasts) return;

            if (!IsFulfilled(out reasons)) return;

            var fulfiller = GetFulfiller();
            if (fulfiller.Gold < 80000) return;
            

            if (fulfiller.Clan.Kingdom.UnresolvedDecisions.Any(x => x is DeclareWarDecision))
                return;

            var behavior = TaleWorlds.CampaignSystem.Campaign.Current.GetCampaignBehavior<BKFeastBehavior>();
            if (behavior.KingdomHasFeast(fulfiller.Clan.Kingdom)) return;
            

            if (0.05f < MBRandom.RandomFloat) return;

            var guests = new List<Clan>();
            float influenceSum = 0f;
            foreach (var clan in fulfiller.Clan.Kingdom.Clans)
            {
                float cost = GuestInfluenceCost(clan);
                if (!clan.IsUnderMercenaryService && fulfiller.Clan.Influence >= influenceSum + cost)
                {
                    influenceSum += cost;
                    guests.Add(clan);
                }
            }

            this.guests = guests;
            feastPlace = fulfiller.Clan.Fiefs.GetRandomElementWithPredicate(x => x.IsTown);
            if (feastPlace == null)
            {
                return;
            }

            ApplyGoal();
        }
    }
}