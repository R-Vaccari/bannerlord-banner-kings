using BannerKings.Behaviours.Diplomacy.Groups.Demands;
using BannerKings.Behaviours.Diplomacy.Wars;
using BannerKings.Managers.Court;
using BannerKings.Managers.Titles.Laws;
using BannerKings.Utils.Models;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using static BannerKings.Behaviours.Diplomacy.Groups.Demands.Demand;

namespace BannerKings.Behaviours.Diplomacy.Groups
{
    public class InterestGroup : BannerKingsObject
    {
        public InterestGroup(string stringId) : base(stringId)
        {
        }

        public void Initialize(TextObject name, TextObject description, TraitObject mainTrait,
            bool demandsCouncil, bool allowsCommoners, bool allowsNobles, List<Occupation> preferredOccupations, 
            List<PolicyObject> supportedPolicy, List<PolicyObject> shunnedPolicies, List<DemesneLaw> supportedLaws, 
            List<DemesneLaw> shunnedLaws, List<CasusBelli> supportedCasusBelli, List<Demand> possibleDemands,
            CouncilMember favoredPosition)
        {
            Initialize(name, description);
            MainTrait = mainTrait;
            DemandsCouncil = demandsCouncil;
            AllowsCommoners = allowsCommoners;
            AllowsNobles = allowsNobles;
            PreferredOccupations = preferredOccupations;
            SupportedPolicies = supportedPolicy;
            ShunnedPolicies = shunnedPolicies;
            SupportedLaws = supportedLaws;
            ShunnedLaws = shunnedLaws;
            SupportedCasusBelli = supportedCasusBelli;
            List<Demand> demands = new List<Demand>();
            foreach (var demand in possibleDemands)
            {
                demands.Add(demand.GetCopy(this));
            }
            PossibleDemands = demands;
            FavoredPosition = favoredPosition;

            Members = new List<Hero>();
            RecentOucomes = new List<DemandOutcome>();
        }

        public KingdomDiplomacy KingdomDiplomacy { get; private set; }
        public Hero FactionLeader => KingdomDiplomacy.Kingdom.Leader;

        public CouncilMember FavoredPosition { get; private set; }

        public InterestGroup GetCopy(KingdomDiplomacy diplomacy)
        {
            InterestGroup result = new InterestGroup(StringId);
            result.Initialize(Name, Description, MainTrait, DemandsCouncil, AllowsCommoners,
                AllowsNobles, PreferredOccupations, SupportedPolicies, ShunnedPolicies, SupportedLaws,
                ShunnedLaws, SupportedCasusBelli, PossibleDemands, FavoredPosition);
            result.KingdomDiplomacy = diplomacy;
            return result;
        }

        public void SetName(TextObject name) => this.name = name;

        public bool IsInterestGroup { get; private set; }
        public bool IsRadicalGroup => !IsInterestGroup;
        public Kingdom Kingdom { get; private set; }
        public TraitObject MainTrait { get; private set; }
        public Hero Leader { get; private set; }
        public List<Hero> Members { get; private set; }

        public bool CanHeroJoin(Hero hero, KingdomDiplomacy diplomacy) => hero.MapFaction == diplomacy.Kingdom && 
            hero.MapFaction.Leader != hero && diplomacy.GetHeroGroup(hero) == null;

        public void AddMember(Hero hero)
        {
            if (hero != null && !Members.Contains(hero))
            {
                Members.Add(hero);
                if (hero.Clan == Clan.PlayerClan)
                {
                    MBInformationManager.AddQuickInformation(new TextObject("{=!}{HERO} has joined the {GROUP} group.")
                        .SetTextVariable("HERO", hero.Name)
                        .SetTextVariable("GROUP", this.Name),
                        0,
                        hero.CharacterObject,
                        Utils.Helpers.GetKingdomDecisionSound());
                }
            }
        }

        public void RemoveMember(Hero hero, KingdomDiplomacy diplomacy, bool forced = false)
        {
            if (hero != null && Members.Contains(hero))
            {
                if (hero.Clan == Clan.PlayerClan)
                {
                    MBInformationManager.AddQuickInformation(new TextObject("{=!}{HERO} has left the {GROUP} group.")
                        .SetTextVariable("HERO", hero.Name)
                        .SetTextVariable("GROUP", this.Name),
                        0,
                        hero.CharacterObject,
                        Utils.Helpers.GetRelationDecisionSound());
                }

                if (forced)
                {
                    Members.Remove(hero);
                    return;
                }

                Members.Remove(hero);
                if (Leader == hero)
                {
                    foreach (var member in Members)
                    {
                        ChangeRelationAction.ApplyRelationChangeBetweenHeroes(hero, member, -10, false);
                    }

                    SetNewLeader(diplomacy);
                }
                else
                {
                    ChangeRelationAction.ApplyRelationChangeBetweenHeroes(hero, Leader, -20, false);
                    foreach (var member in Members)
                    {
                        if (MBRandom.RandomFloat < 0.3f)
                        {
                            ChangeRelationAction.ApplyRelationChangeBetweenHeroes(hero, member, -6, false);
                        }
                    }
                }
            }
        }

        public void SetNewLeader(KingdomDiplomacy diplomacy)
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

        public List<Hero> GetSortedMembers(KingdomDiplomacy diplomacy)
        {
            var list = new List<Hero>(Members);
            if (Leader != null)
            {
                list.Remove(Leader);
            }

            var dictionary = new Dictionary<Hero, float>();
            foreach (var member in Members)
            {
                dictionary.Add(member, BannerKingsConfig.Instance.InterestGroupsModel.CalculateHeroInfluence(this, diplomacy, member)
                    .ResultNumber);
            }

            list.Sort((x, y) => dictionary[x].CompareTo(dictionary[y]));
            return list;
        }

        public void AddOutcome(Demand demand, DemandResponse response, bool success)
        {
            RecentOucomes.Add(new DemandOutcome(demand,
                CampaignTime.YearsFromNow(1f),
                response.Explanation,
                success));
        }

        public (bool, TextObject) CanPushDemand(Demand demand)
        {
            DemandOutcome outcome = RecentOucomes.FirstOrDefault(x => x.Demand == demand);
            if (outcome != null)
            {
                return new(false, outcome.Explanation);
            }

            BKExplainedNumber influence = BannerKingsConfig.Instance.InterestGroupsModel
               .CalculateGroupInfluence(this, false);
            if (influence.ResultNumber < demand.MinimumGroupInfluence)
            {
                return new(false, new TextObject("{=!}This demand requires at least {INFLUENCE}% group influence.")
                    .SetTextVariable("INFLUENCE", (demand.MinimumGroupInfluence * 100f).ToString("0.0")));
            }

            return demand.IsDemandCurrentlyAdequate();
        }

        public bool DemandsCouncil { get; private set; }
        public bool AllowsCommoners { get; private set; }
        public bool AllowsNobles { get; private set; }
        public List<Occupation> PreferredOccupations { get; private set; }
        public List<PolicyObject> SupportedPolicies { get; private set; }
        public List<PolicyObject> ShunnedPolicies { get; private set; }
        public List<DemesneLaw> SupportedLaws { get; private set; }
        public List<DemesneLaw> ShunnedLaws { get; private set; }
        public List<CasusBelli> SupportedCasusBelli { get; private set; }
        public List<Demand> PossibleDemands { get; private set; }
        public List<DemandOutcome> RecentOucomes { get; private set; }

        public override bool Equals(object obj)
        {
            if (obj is InterestGroup)
            {
                return (obj as InterestGroup).StringId == StringId;
            }
            return base.Equals(obj);
        }

        public class DemandOutcome
        {
            public Demand Demand { get; private set; }
            public CampaignTime EndDate { get; private set; }
            public TextObject Explanation { get; private set; }
            public bool Success { get; private set; }

            public DemandOutcome(Demand demand, CampaignTime endDate, TextObject explanation, bool success)
            {
                Demand = demand;
                EndDate = endDate;
                Explanation = explanation;
                Success = success;
            }
        }
    }
}
