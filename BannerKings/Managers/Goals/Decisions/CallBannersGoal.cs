using System;
using System.Collections.Generic;
using BannerKings.Behaviours;
using BannerKings.Managers.Court;
using BannerKings.Managers.Kingdoms.Policies;
using BannerKings.Managers.Populations.Estates;
using BannerKings.Utils.Extensions;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Goals.Decisions
{
    public class CallBannersGoal : Goal
    {
        List<BannerOption> banners = new List<BannerOption>();
        public CallBannersGoal() : base("goal_found_kingdom", GoalCategory.Kingdom, GoalUpdateType.Hero)
        {
            var name = new TextObject("{=!}Call Banners");
            var description = new TextObject("{=!}Stablish your own kingdom title. Your faction must be one that is not already represented by a kingdom title.");
            Initialize(name, description);
        }

        internal override bool IsAvailable()
        {
            return Clan.PlayerClan.Kingdom != null;
        }

        internal override bool IsFulfilled(out List<TextObject> failedReasons)
        {
            failedReasons = new List<TextObject>();

            Hero fulfiller = GetFulfiller();
            if (!BannerKingsConfig.Instance.ArmyManagementModel.CanCreateArmy(fulfiller))
            {
                var rulingClan = fulfiller.Clan.Kingdom.RulingClan;
                var council = BannerKingsConfig.Instance.CourtManager.GetCouncil(rulingClan);
                var councilMember = council.GetMemberFromPosition(CouncilPosition.Marshall);

                if (fulfiller.Clan.Kingdom.HasPolicy(BKPolicies.Instance.LimitedArmyPrivilege))
                {
                    failedReasons.Add(new TextObject("{=0Yoz051M}You must be faction leader, {MARSHAL} for the {CLAN} or have a title superior to County level.")
                                            .SetTextVariable("MARSHAL", councilMember.GetName())
                                            .SetTextVariable("CLAN", rulingClan.Name));
                }
                else
                {
                    failedReasons.Add(new TextObject("{=9ap6ssvZ}You must be faction leader, {MARSHAL} for the {CLAN} or have a title superior to Lordship level.")
                    .SetTextVariable("MARSHAL", councilMember.GetName())
                    .SetTextVariable("CLAN", rulingClan.Name));
                }
            }

            if (fulfiller.IsPrisoner)
            {
                failedReasons.Add(new TextObject("{=!}"));
            }

            if (!fulfiller.IsClanLeader())
            {
                failedReasons.Add(new TextObject("{=PxhHMJXb}Not clan leader."));
            }

            if (fulfiller.PartyBelongedTo == null)
            {
                failedReasons.Add(new TextObject("{=!}Not in a party."));
            }
            else if (fulfiller.PartyBelongedTo.Army != null)
            {
                failedReasons.Add(GameTexts.FindText("str_in_army"));
            }
            
            return failedReasons.IsEmpty();
        }

        internal override void ShowInquiry()
        {
            banners.Clear();
            var hero = GetFulfiller();
            var kingdom = Clan.PlayerClan.Kingdom;

            var behavior = Campaign.Current.GetCampaignBehavior<BKGentryBehavior>();
            var elements = new List<InquiryElement>();
            foreach (var vassal in BannerKingsConfig.Instance.TitleManager.CalculateAllVassals(Clan.PlayerClan))
            {
                var estates = BannerKingsConfig.Instance.PopulationManager.GetEstates(vassal);
                Estate estate = null;
                if (estates.Count > 0)
                {
                    estate = estates[0];
                }

                Clan clan = vassal.Clan;
                BannerOption option = new BannerOption(vassal,
                    GetInfluenceCost(Hero.MainHero, vassal.Clan),
                    hero.PartyBelongedTo,
                    estate);
                bool ready = false;
                TextObject hint = null;
                if (vassal.PartyBelongedTo != null && vassal.PartyBelongedTo.LeaderHero == vassal)
                {
                    var party = vassal.PartyBelongedTo;
                    ready = party.IsReady && party.Army == null && party.SiegeEvent == null;
                    if (vassal.Clan == Clan.PlayerClan)
                    {
                        hint = new TextObject("{=!}Summon {HERO} to your army. They are a knight in your household. They currently lead {TROOPS} troops.");
                    }
                    else
                    {
                        hint = new TextObject("{=!}Summon {HERO} to your army. They are a vassal and currently lead {TROOPS} troops. Calling them will cost {INFLUENCE}{INFLUENCE_ICON}");
                    }
                }
                else if (estate != null)
                {
                    ready = behavior.IsAvailableForSummoning(clan, estate);
                    hint = new TextObject("{=!}Summon {HERO} to your army. They are landed gentry and will return to their property once the army is finished. Calling them will cost {INFLUENCE}{INFLUENCE_ICON}");
                }

                if (hint != null)
                {
                    elements.Add(new InquiryElement(option,
                                                    vassal.Name.ToString(),
                                                    new ImageIdentifier(clan.Banner),
                                                    ready && Clan.PlayerClan.Influence >= option.Influence,
                                                    hint.ToString()));
                }
            }

            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                new TextObject("{=!}Call Banners").ToString(),
                new TextObject("{=!}Summon your vassals to fulfill their duties.").ToString(),
                elements,
                true,
                elements.Count,
                GameTexts.FindText("str_accept").ToString(),
                GameTexts.FindText("str_reject").ToString(),
                delegate (List<InquiryElement> list)
                {
                    foreach (InquiryElement element in list)
                    {
                        BannerOption option = (BannerOption)element.Identifier;
                        banners.Add(option);
                    }

                    ApplyGoal();
                },
                null));

            var action = BannerKingsConfig.Instance.TitleModel.GetFoundKingdom(kingdom, hero);
        }

        private float GetInfluenceCost(Hero fulfiller, Clan clan) => 5f;

        internal override void ApplyGoal()
        {
            var hero = GetFulfiller();
            var mobileParty = hero.PartyBelongedTo;
            Army army = new Army(hero.Clan.Kingdom, mobileParty, Army.ArmyTypes.Patrolling)
            {
                AIBehavior = Army.AIBehaviorFlags.Gathering
            };
            mobileParty.Army = army;
            GatherArmyAction.Apply(mobileParty,
                SettlementHelper.FindNearestSettlement(x => x.IsFortification || x.IsVillage, hero.PartyBelongedTo));

            var behavior = Campaign.Current.GetCampaignBehavior<BKGentryBehavior>();
            float influenceTotal = 0f;
            foreach (var option in banners)
            {
                if (hero.Clan.Influence >= influenceTotal + option.Influence)
                {
                    influenceTotal += option.Influence;
                    if (option.Party != null)
                    {
                        SetPartyAiAction.GetActionForEscortingParty(option.Party, mobileParty);
                    }
                    else if (option.Estate != null)
                    {
                        behavior.SummonGentry(option.Hero.Clan, army, option.Estate);
                    }
                }
            }

            GainKingdomInfluenceAction.ApplyForDefault(hero, -influenceTotal);
        }

        public override void DoAiDecision()
        {
            throw new NotImplementedException();
        }

        private class BannerOption
        {
            public BannerOption(Hero clan, float influence, MobileParty party, Estate estate = null)
            {
                Hero = clan;
                Estate = estate;
                Influence = influence;
            }

            public Hero Hero { get; private set; }
            public MobileParty Party { get; private set; }
            public Estate Estate { get; private set; }
            public float Influence { get; private set; }
        }
    }
}