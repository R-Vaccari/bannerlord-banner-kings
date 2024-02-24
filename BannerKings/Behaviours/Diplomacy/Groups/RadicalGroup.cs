using BannerKings.Behaviours.Diplomacy.Groups.Demands;
using BannerKings.Utils.Extensions;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using static BannerKings.Behaviours.Diplomacy.Groups.InterestGroup;

namespace BannerKings.Behaviours.Diplomacy.Groups
{
    public class RadicalGroup : DiplomacyGroup
    {
        private Demand demand;
        public ViewModel ViewModel { get; private set; }
        public RadicalGroup(string stringId) : base(stringId)
        {
        }

        public void Initialize(TextObject name, TextObject description, Demand demand)
        {
            Initialize(name, description);
            this.demand = demand;
        }

        public override DiplomacyGroup GetCopy(KingdomDiplomacy diplomacy)
        {
            RadicalGroup group = new RadicalGroup(StringId);
            group.Initialize(Name, Description, CurrentDemand);
            group.KingdomDiplomacy = diplomacy;
            group.KingdomName = diplomacy.Kingdom.Name;
            return group;
        }

        public void SetupRadicalGroup(Hero leader, ViewModel viewModel)
        {
            AddMember(leader);
            SetLeader(leader);
            ViewModel = viewModel;
            demand = CurrentDemand.GetCopy(this);
            CurrentDemand.ShowPlayerDemandOptions();   
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
        public float Radicalism { get; private set; } = 0.25f;
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

        public override bool CanHeroJoin(Hero hero, KingdomDiplomacy diplomacy) => hero.MapFaction == diplomacy.Kingdom &&
            hero.MapFaction.Leader != hero && hero.IsClanLeader();

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
            if (Leader == null || FactionLeader == null || !CurrentDemand.Active)
            {
                return;
            }

            float proportion = PowerProportion;
            if (proportion >= 0.5f) Radicalism += 0.1f;
            else Radicalism -= 0.01f;

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
            else if (Radicalism < 0f) Radicalism = 0f;

            foreach (Hero member in Members)
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
                return new(false, new TextObject("{=!}This demand requires at least {INFLUENCE}% group radicalism.")
                    .SetTextVariable("INFLUENCE", (demand.MinimumGroupInfluence * 100f).ToString("0.0")));
            }

            return demand.IsDemandCurrentlyAdequate();
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
