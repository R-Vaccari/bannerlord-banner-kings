using BannerKings.Actions;
using BannerKings.Behaviours.Diplomacy.Groups.Demands;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace BannerKings.Behaviours.Diplomacy.Groups
{
    public class RadicalGroup : DiplomacyGroup
    {
        private RadicalDemand demand;
        public ViewModel ViewModel { get; private set; }
        public RadicalGroup(string stringId) : base(stringId)
        {
        }

        public void Initialize(TextObject name, TextObject description, RadicalDemand demand)
        {
            Initialize(name, description);
            this.demand = demand;
        }

        public void PostInitialize()
        {
            RadicalGroup r = DefaultRadicalGroups.Instance.GetById(this);
            Initialize(r.name, r.description, r.demand);
            CurrentDemand.SetTexts();
            CurrentDemand.Group = this;
        }

        public override DiplomacyGroup GetCopy(KingdomDiplomacy diplomacy)
        {
            RadicalGroup group = new RadicalGroup(StringId);
            group.Initialize(Name, Description, CurrentDemand as RadicalDemand);
            group.KingdomDiplomacy = diplomacy;
            group.KingdomName = diplomacy.Kingdom.Name;
            return group;
        }

        public void SetupRadicalGroup(Hero leader, ViewModel viewModel)
        {
            AddMember(leader);
            SetLeader(leader);
            ViewModel = viewModel;
            demand = (RadicalDemand)CurrentDemand.GetCopy(this);
            if (leader == Hero.MainHero) CurrentDemand.ShowPlayerDemandOptions();
            else CurrentDemand.SetUp();
        }

        public float TotalStrength
        {
            get
            {
                float power = 0f;
                foreach (Hero member in Members)
                    power += member.Clan.TotalStrength;

                return power;
            }
        }

        public float PowerProportion
        {
            get
            {
                float revoltPower = TotalStrength;
                float kingdomPower = 0f;
                foreach (Clan clan in KingdomDiplomacy.Kingdom.Clans)
                {
                    if (!Members.Contains(clan.Leader)) kingdomPower += clan.TotalStrength;
                }

                return revoltPower / kingdomPower;
            }
        }

        public TextObject KingdomName { get; private set; }
        [SaveableProperty(13)] public float Radicalism { get; private set; } = 0.25f;
        public override Demand CurrentDemand => demand;
        public override bool IsInterestGroup => false;

        public override void AddMember(Hero hero)
        {
            if (hero != null && !Members.Contains(hero) && CanHeroJoin(hero, KingdomDiplomacy))
            {
                AddMemberInternal(hero);
                if (hero.Clan == Clan.PlayerClan)
                {
                    MBInformationManager.AddQuickInformation(new TextObject("{=J7Yomhae}{HERO} has joined the {GROUP} group.")
                        .SetTextVariable("HERO", hero.Name)
                        .SetTextVariable("GROUP", this.Name),
                        0,
                        hero.CharacterObject,
                        Utils.Helpers.GetKingdomDecisionSound());
                }
                else if (hero.MapFaction.Leader == Hero.MainHero) 
                {
                    MBInformationManager.AddQuickInformation(new TextObject("{=oxwdse8t}{HERO} has joined the {GROUP} radical group against you!")
                        .SetTextVariable("HERO", hero.Name)
                        .SetTextVariable("GROUP", this.Name),
                        0,
                        hero.CharacterObject,
                        Utils.Helpers.GetKingdomDecisionSound());
                }
            }
        }

        public override bool CanHeroJoin(Hero hero, KingdomDiplomacy diplomacy) => 
            BannerKingsConfig.Instance.InterestGroupsModel.CanHeroJoinARadicalGroup(hero, KingdomDiplomacy);

        public override bool CanHeroLeave(Hero hero, KingdomDiplomacy diplomacy)
        {
            if (JoinTime.TryGetValue(hero, out var joinTime))
            {
                return joinTime.ElapsedYearsUntilNow >= 1f;
            }

            return true;
        }

        public override void RemoveMember(Hero hero, bool forced = false)
        {
            if (hero != null && Members.Contains(hero))
            {
                if (!forced && !CanHeroLeave(hero, KingdomDiplomacy)) return;

                Members.Remove(hero);
                if (hero.Clan == Clan.PlayerClan)
                {
                    MBInformationManager.AddQuickInformation(new TextObject("{=O9K6i3iT}{HERO} has left the {GROUP} group.")
                        .SetTextVariable("HERO", hero.Name)
                        .SetTextVariable("GROUP", this.Name),
                        0,
                        hero.CharacterObject,
                        Utils.Helpers.GetRelationDecisionSound());
                }

                if (!forced)
                {
                    if (Leader == hero)
                    {
                        foreach (var member in Members)
                            ChangeRelationAction.ApplyRelationChangeBetweenHeroes(hero, member, -10, false);

                        SetNewLeader(KingdomDiplomacy);
                    }
                    else
                    {
                        ChangeRelationAction.ApplyRelationChangeBetweenHeroes(hero, Leader, -20, false);
                        foreach (var member in Members)
                            if (MBRandom.RandomFloat < 0.3f)
                                ChangeRelationAction.ApplyRelationChangeBetweenHeroes(hero, member, -6, false);
                    }
                }
            }
        }

        public override void SetNewLeader(KingdomDiplomacy diplomacy)
        {
            var dictionary = new Dictionary<Hero, float>();
            foreach (var member in Members)
            {
                dictionary.Add(member, BannerKingsConfig.Instance.InterestGroupsModel.CalculateHeroInfluence(this, diplomacy, member)
                    .ResultNumber);
            }

            if (dictionary.Count > 0)
            {
                Hero hero = dictionary.FirstOrDefault(x => x.Value == dictionary.Values.Max()).Key;
                Leader = hero;
            }
        }

        public override void Tick()
        {
            TickInternal();

            if (!CurrentDemand.Active) CurrentDemand.SetUp();
            if (Leader == null || FactionLeader == null || !CurrentDemand.Active) return; 

            float proportion = PowerProportion;
            if (proportion >= 0.5f) Radicalism += 0.01f;
            else Radicalism -= 0.005f;

            if (Radicalism > 1f)
            {
                Radicalism = 1f;
                if (Leader != Hero.MainHero)
                {
                    if (CanPushDemand(CurrentDemand, Radicalism).Item1)
                    {
                        if (MBRandom.RandomFloat < 0.05f * Radicalism)
                        {
                            CurrentDemand.SetUp();
                        }
                    }
                }
                if (MBRandom.RandomFloat < 0.1f * Radicalism) CurrentDemand.PushForDemand();
            }
            else if (Radicalism <= 0f)
            {
                Radicalism = 0f;
                demand.Finish();
            }

            var members = new List<Hero>();
            members.AddRange(Members);
            foreach (Hero member in members)
            {
                float joinChance = BannerKingsConfig.Instance.InterestGroupsModel.CalculateHeroJoinChance(member, this, KingdomDiplomacy)
                    .ResultNumber;
                if (joinChance < 0f)
                {
                    RemoveMember(member);
                }
            }
        }

        public override (bool, TextObject) CanPushDemand(Demand demand, float radicalism)
        {
            if (radicalism < demand.MinimumGroupInfluence)
            {
                return new(false, new TextObject("{=EYqxJOKQ}This demand requires at least {INFLUENCE}% group radicalism.")
                    .SetTextVariable("INFLUENCE", (demand.MinimumGroupInfluence * 100f).ToString("0.0")));
            }

            return demand.IsDemandCurrentlyAdequate();
        }

        public void TriggerRevolt()
        {
            var rebels = Members.ConvertAll(x => x.Clan);
            /* foreach (Clan rebel in rebels)
             {
                 foreach (Clan clan in KingdomDiplomacy.Kingdom.Clans) 
                 {
                     if (!rebels.Contains(clan))
                     {
                         FactionManager.DeclareWar(rebel, clan);
                     }
                 }
             }*/

            Kingdom originalKingdom = KingdomDiplomacy.Kingdom;
            Kingdom rebelKingdom = RebellionActions.CreateRebelKingdom(Leader.Clan, rebels, KingdomDiplomacy.Kingdom);
            foreach (Clan clan in rebels)
            {
                foreach (var party in clan.WarPartyComponents)
                {
                    MobileParty mobileParty = party.MobileParty;
                    if (mobileParty.Army != null && mobileParty.Army.LeaderParty.MapFaction == originalKingdom) mobileParty.Army = null;
                    if (mobileParty.CurrentSettlement != null && mobileParty.CurrentSettlement.MapFaction == originalKingdom) LeaveSettlementAction.ApplyForParty(mobileParty);
                }
            }

            DeclareWarAction.ApplyByKingdomCreation(originalKingdom, rebelKingdom);
            TaleWorlds.CampaignSystem.Campaign.Current.GetCampaignBehavior<BKDiplomacyBehavior>().TriggerRebelWar(rebelKingdom, 
                KingdomDiplomacy.Kingdom, 
                (RadicalDemand)demand.GetCopy(null));
        }

        public override bool Equals(object obj)
        {
            if (obj is RadicalGroup)
            {
                return (obj as RadicalGroup).StringId == StringId && KingdomDiplomacy == (obj as RadicalGroup).KingdomDiplomacy;
            }
            return base.Equals(obj);
        }
    }
}
