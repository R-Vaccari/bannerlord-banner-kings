using System.Collections.Generic;
using System.Linq;
using BannerKings.Behaviours;
using BannerKings.Managers.Court.Members;
using BannerKings.Managers.Kingdoms.Policies;
using BannerKings.Managers.Populations.Estates;
using BannerKings.Utils.Extensions;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Goals.Decisions
{
    public class CallBannersGoal : Goal
    {
        List<BannerOption> banners = new List<BannerOption>();
        List<BannerOption> vassalBanners = new List<BannerOption>();
        List<BannerOption> allBanners = new List<BannerOption>();
        List<InquiryElement> elements = new List<InquiryElement>();

        public CallBannersGoal(Hero fulfiller = null) : base("goal_found_kingdom", GoalCategory.Kingdom, GoalUpdateType.Hero, fulfiller)
        {
            var name = new TextObject("{=zzjbxN9h}Call Banners");
            var description = new TextObject("Stablish your own kingdom title. Your faction must be one that is not already represented by a kingdom title.");
            Initialize(name, description);
            Refresh();
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
                var councilMember = council.GetCouncilPosition(DefaultCouncilPositions.Instance.Marshal);

                if (fulfiller.Clan.Kingdom.HasPolicy(BKPolicies.Instance.LimitedArmyPrivilege))
                {
                    failedReasons.Add(new TextObject("{=0Yoz051M}You must be faction leader, {MARSHAL} for the {CLAN} or have a title superior to County level.")
                                            .SetTextVariable("MARSHAL", councilMember.Name)
                                            .SetTextVariable("CLAN", rulingClan.Name));
                }
                else
                {
                    failedReasons.Add(new TextObject("{=9ap6ssvZ}You must be faction leader, {MARSHAL} for the {CLAN} or have a title superior to Lordship level.")
                    .SetTextVariable("MARSHAL", councilMember.Name)
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
                failedReasons.Add(new TextObject("{=QHfkhG0b}Not in a party."));
            }
            else if (fulfiller.PartyBelongedTo.Army != null)
            {
                failedReasons.Add(GameTexts.FindText("str_in_army"));
            }

            var behavior = Campaign.Current.GetCampaignBehavior<BKArmyBehavior>();
            if (behavior.LastHeroArmy(fulfiller).ElapsedSeasonsUntilNow < 2f)
            {
                failedReasons.Add(new TextObject("{=yG6r0iaK}It has been less than 2 seasons since you last summoned your banners."));
            }
            
            return failedReasons.IsEmpty();
        }

        private void Refresh()
        {
            var hero = GetFulfiller();

            banners.Clear();
            allBanners.Clear();
            vassalBanners.Clear();
            elements.Clear();
            AddBanners(hero.Clan);
        }

        private void AddBanners(Clan suzerainClan)
        {
            var behavior = Campaign.Current.GetCampaignBehavior<BKGentryBehavior>();
            foreach (var vassal in BannerKingsConfig.Instance.TitleManager.CalculateAllVassals(suzerainClan))
            {
                var estates = BannerKingsConfig.Instance.PopulationManager.GetEstates(vassal);
                Estate estate = null;
                if (estates.Count > 0)
                {
                    estate = estates[0];
                }

                Clan clan = vassal.Clan;
                var influence = GetInfluenceCost(GetFulfiller(), vassal);
                BannerOption option = new BannerOption(vassal,
                    influence,
                    vassal.PartyBelongedTo,
                    estate);
                bool ready = false;
                TextObject hint = null;
                if (vassal.PartyBelongedTo != null && vassal.PartyBelongedTo.LeaderHero == vassal)
                {
                    var party = vassal.PartyBelongedTo;
                    var troops = party.MemberRoster.TotalManCount;
                    ready = party.IsReady && party.Army == null && party.SiegeEvent == null;
                    if (vassal.Clan == Clan.PlayerClan)
                    {
                        hint = new TextObject("{=5v0L5y5A}Summon {HERO} to your army. They are a knight in your household. They currently lead {TROOPS} troops. Calling them will cost {INFLUENCE} influence.")
                            .SetTextVariable("HERO", vassal.Name)
                            .SetTextVariable("INFLUENCE", influence)
                            .SetTextVariable("TROOPS", troops);
                    }
                    else
                    {
                        hint = new TextObject("{=M03ZVW56}Summon {HERO} to your army. They are a vassal and currently lead {TROOPS} troops. Calling them will cost {INFLUENCE} influence.")
                            .SetTextVariable("HERO", vassal.Name)
                            .SetTextVariable("INFLUENCE", influence)
                            .SetTextVariable("TROOPS", troops);
                    }
                }
                else if (estate != null)
                {
                    (bool, TextObject) readyTuple = behavior.IsAvailableForSummoning(clan, estate);
                    ready = readyTuple.Item1;
                    hint = new TextObject("{=djtn6LCe}Summon {HERO} to your army. They are landed gentry and will return to their property once the army is finished. Their estate can provide {TROOPS} troops. Calling them will cost {INFLUENCE} influence.\n\n{READY}")
                        .SetTextVariable("HERO", vassal.Name)
                        .SetTextVariable("INFLUENCE", influence)
                        .SetTextVariable("TROOPS", estate.GetManpower(PopulationManager.PopType.Serfs))
                        .SetTextVariable("READY", readyTuple.Item2);
                }

                if (hint != null && allBanners.FirstOrDefault(x => x.Hero == vassal) == null)
                {
                    allBanners.Add(option);
                    elements.Add(new InquiryElement(option,
                                                    vassal.Name.ToString(),
                                                    new ImageIdentifier(clan.Banner),
                                                    ready && Clan.PlayerClan.Influence >= option.Influence,
                                                    hint.ToString()));
                    AddBanners(vassal.Clan);
                }
            }
        }

        internal override void ShowInquiry()
        {
            Refresh();

            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                new TextObject("{=zzjbxN9h}Call Banners").ToString(),
                new TextObject("{=QDf3sOgR}Summon your vassals to fulfill their duties.").ToString(),
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
        }

        private float GetInfluenceCost(Hero fulfiller, Hero banner, Estate estate = null)
        {
            if (banner.IsPartyLeader)
            {
                return BannerKingsConfig.Instance.ArmyManagementModel.CalculatePartyInfluenceCost(fulfiller.PartyBelongedTo,
                    banner.PartyBelongedTo) * 0.75f;
            }
            else
            {
                float result = banner.Clan.Tier * 2f;
                result += banner.GetRelation(fulfiller) / -10f;
                if (estate != null)
                {
                    result += estate.GetManpower(PopulationManager.PopType.Serfs) * 0.3f;
                }
                return MathF.Clamp(result, 15f, 60f);
            }
        }

        internal override void ApplyGoal()
        {
            var hero = GetFulfiller();
            var mobileParty = hero.PartyBelongedTo;
            Army army = new Army(hero.Clan.Kingdom, mobileParty, Army.ArmyTypes.Patrolling)
            {
                AIBehavior = Army.AIBehaviorFlags.Gathering
            };
            Settlement settlement = hero.CurrentSettlement != null ? hero.CurrentSettlement :
                SettlementHelper.FindNearestSettlement(x => x.Town != null || x.IsVillage, hero.PartyBelongedTo);
            army.Gather(settlement);
            mobileParty.Army = army;

            var behavior = Campaign.Current.GetCampaignBehavior<BKGentryBehavior>();
            float influenceTotal = 0f;
            foreach (var option in banners)
            {
                if (hero.Clan.Influence >= influenceTotal + option.Influence)
                {
                    influenceTotal += option.Influence;
                    if (option.Party != null)
                    {
                        SetPartyAiAction.GetActionForEscortingParty(option.Party, army.LeaderParty);
                    }
                    else if (option.Estate != null)
                    {
                        behavior.SummonGentry(option.Hero.Clan, army, option.Estate);
                    }
                }
            }

            GainKingdomInfluenceAction.ApplyForDefault(hero, -influenceTotal);
            var armyBehavior = Campaign.Current.GetCampaignBehavior<BKArmyBehavior>();
            armyBehavior.AddRecord(hero);
            if (hero != Hero.MainHero && hero.MapFaction == Hero.MainHero.MapFaction)
            {
                int troops = hero.PartyBelongedTo.MemberRoster.TotalManCount;
                foreach (var option in banners)
                {
                    if (option.Hero.PartyBelongedTo != null)
                    {
                        troops += option.Hero.PartyBelongedTo.MemberRoster.TotalManCount;
                    }
                }

                InformationManager.DisplayMessage(new InformationMessage(
                    new TextObject("{=YZfBWynb}{HERO} has called his banners! {TROOPS} troops are gathering for war.")
                    .SetTextVariable("HERO", hero.Name)
                    .SetTextVariable("TROOPS", troops).ToString(),
                    Color.FromUint(4282569842U),
                    Utils.Helpers.GetKingdomDecisionSound()));
            }
        }

        public override void DoAiDecision()
        {
            Hero fulfiller = GetFulfiller();
            if (allBanners.Count < 5 || fulfiller.PartyBelongedTo.HasUnpaidWages > 0 || fulfiller.PartyBelongedTo.GetNumDaysForFoodToLast() < 10)
            {
                return;
            }

            if (IsFulfilled(out List<TextObject> reasons))
            {
                return;
            }

            float cost = 0f;
            int parties = 0;
            foreach (var banner in allBanners)
            {
                if (cost + banner.Influence <= fulfiller.Clan.Influence)
                {
                    banners.Add(banner);
                    parties++;
                    cost += banner.Influence;
                }
            }

            if (banners.Count < 5)
            {
                return;
            }

            ApplyGoal();
        }

        private class BannerOption
        {
            public BannerOption(Hero clan, float influence, MobileParty party, Estate estate = null)
            {
                Hero = clan;
                Estate = estate;
                Influence = influence;
                Party = party;
            }

            public Hero Hero { get; private set; }
            public MobileParty Party { get; private set; }
            public Estate Estate { get; private set; }
            public float Influence { get; private set; }
        }
    }
}