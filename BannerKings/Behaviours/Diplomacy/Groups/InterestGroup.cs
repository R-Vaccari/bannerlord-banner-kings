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
using TaleWorlds.SaveSystem;
using static BannerKings.Behaviours.Diplomacy.Groups.Demands.Demand;

namespace BannerKings.Behaviours.Diplomacy.Groups
{
    public class InterestGroup : DiplomacyGroup
    {
        public InterestGroup(string stringId) : base(stringId)
        {
            RecentOucomes = new List<DemandOutcome>();
        }

        public void Initialize(TextObject name, TextObject description, TraitObject mainTrait,
            bool demandsCouncil, bool allowsCommoners, bool allowsNobles, List<Occupation> preferredOccupations, 
            List<PolicyObject> supportedPolicy, List<PolicyObject> shunnedPolicies, List<DemesneLaw> supportedLaws, 
            List<DemesneLaw> shunnedLaws, List<CasusBelli> supportedCasusBelli, List<Demand> possibleDemands,
            CouncilMember favoredPosition, float legitimacyFactor)
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
            if (PossibleDemands == null)
            {
                List<Demand> demands = new List<Demand>();
                foreach (var demand in possibleDemands)
                {
                    demands.Add(demand.GetCopy(this));
                }
                PossibleDemands = demands;
            }
            else
            {
                foreach (Demand demand in possibleDemands)
                    if (!PossibleDemands.Any(x => x.StringId == demand.StringId))
                        PossibleDemands.Add(demand.GetCopy(this));
            }

            FavoredPosition = favoredPosition;
            LegitimacyFactor = legitimacyFactor;
        }

        public InterestGroup GetCopy(KingdomDiplomacy diplomacy)
        {
            InterestGroup result = new InterestGroup(StringId);
            result.Initialize(Name, Description, MainTrait, DemandsCouncil, AllowsCommoners,
                AllowsNobles, PreferredOccupations, SupportedPolicies, ShunnedPolicies, SupportedLaws,
                ShunnedLaws, SupportedCasusBelli, PossibleDemands, FavoredPosition, LegitimacyFactor);
            result.KingdomDiplomacy = diplomacy;
            return result;
        }

        public void PostInitialize()
        {
            base.PostInitialize();
            InterestGroup i = DefaultInterestGroup.Instance.GetById(this);
            Initialize(i.Name, i.Description, i.MainTrait, i.DemandsCouncil, i.AllowsCommoners,
                i.AllowsNobles, i.PreferredOccupations, i.SupportedPolicies, i.ShunnedPolicies, i.SupportedLaws,
                i.ShunnedLaws, i.SupportedCasusBelli, i.PossibleDemands, i.FavoredPosition, i.LegitimacyFactor);
            foreach (var demand in PossibleDemands)
            {
                demand.SetTexts();
                demand.Group = this;
            }

            if (RecentOucomes == null) RecentOucomes = new List<DemandOutcome>();
        }

        public override void Tick()
        {
            TickInternal();
            var current = CurrentDemand;
            if (current != null)
            {
                current.Tick();
            }

            if (Leader == Hero.MainHero || Leader == null || FactionLeader == null)
            {
                return;
            }

            var influence = BannerKingsConfig.Instance.InterestGroupsModel.CalculateGroupInfluence(this);
            foreach (Demand demand in PossibleDemands)
            {
                if (CanPushDemand(demand, influence.ResultNumber).Item1 && MBRandom.RandomFloat < MBRandom.RandomFloat)
                {
                    demand.SetUp();
                }
            }

            foreach (DemandOutcome outcome in RecentOucomes)
            {
                if (outcome.Enabled && outcome.EndDate.IsPast)
                {
                    outcome.Enabled = false;
                }
            }
        }
        
        [SaveableProperty(13)] public List<Demand> PossibleDemands { get; private set; }
        [SaveableProperty(14)] public List<DemandOutcome> RecentOucomes { get; private set; }

        public CouncilMember FavoredPosition { get; private set; }
        public TraitObject MainTrait { get; private set; }
        public Demand CurrentDemand => PossibleDemands.FirstOrDefault(x => x.Active);
        public bool DemandsCouncil { get; private set; }
        public bool AllowsCommoners { get; private set; }
        public bool AllowsNobles { get; private set; }
        public float LegitimacyFactor { get; private set; }
        public List<Occupation> PreferredOccupations { get; private set; }
        public List<PolicyObject> SupportedPolicies { get; private set; }
        public List<PolicyObject> ShunnedPolicies { get; private set; }
        public List<DemesneLaw> SupportedLaws { get; private set; }
        public List<DemesneLaw> ShunnedLaws { get; private set; }
        public List<CasusBelli> SupportedCasusBelli { get; private set; }

        public override bool IsInterestGroup => true;
        public BKExplainedNumber Influence => BannerKingsConfig.Instance.InterestGroupsModel
                .CalculateGroupInfluence(this, false);
        public BKExplainedNumber InfluenceExplained => BannerKingsConfig.Instance.InterestGroupsModel
                .CalculateGroupInfluence(this, true);
        public BKExplainedNumber Support => BannerKingsConfig.Instance.InterestGroupsModel
                .CalculateGroupSupport(this, false);
        public BKExplainedNumber SupportExplained => BannerKingsConfig.Instance.InterestGroupsModel
                .CalculateGroupSupport(this, true);

        public void SetName(TextObject name) => this.name = name;

        public override bool CanHeroJoin(Hero hero, KingdomDiplomacy diplomacy) => hero.MapFaction == diplomacy.Kingdom &&
            hero.MapFaction.Leader != hero && diplomacy.GetHeroGroup(hero) == null;

        public override bool CanHeroLeave(Hero hero, KingdomDiplomacy diplomacy)
        {
            if (JoinTime.TryGetValue(hero, out var joinTime))
            {
                return joinTime.ElapsedYearsUntilNow >= 1f;
            }

            return true;
        }

        public override void AddMember(Hero hero)
        {
            if (hero != null && !Members.Contains(hero) && CanHeroJoin(hero, KingdomDiplomacy))
            {
                AddMemberInternal(hero);
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

        public override void RemoveMember(Hero hero, bool forced = false)
        {
            if (hero != null && Members.Contains(hero))
            {
                if (!forced && !CanHeroLeave(hero, KingdomDiplomacy)) return;

                Members.Remove(hero);
                if (hero.Clan == Clan.PlayerClan)
                {
                    MBInformationManager.AddQuickInformation(new TextObject("{=!}{HERO} has left the {GROUP} group.")
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
                        {
                            ChangeRelationAction.ApplyRelationChangeBetweenHeroes(hero, member, -10, false);
                        }

                        SetNewLeader(KingdomDiplomacy);
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

        public void AddOutcome(Demand demand, DemandResponse response, bool success)
        {
            RecentOucomes.Add(new DemandOutcome(demand,
                CampaignTime.YearsFromNow(1f),
                response.Explanation.SetTextVariable("DATE", CampaignTime.Now.ToString()),
                success));
        }

        public (bool, TextObject) CanPushDemand(Demand demand, float influence)
        {
            DemandOutcome outcome = RecentOucomes.FirstOrDefault(x => x.Demand == demand);
            if (outcome != null)
            {
                return new(false, outcome.Explanation);
            }

            Demand active = CurrentDemand;
            if (active != null)
            {
                return new(false, new TextObject("{=!}The {DEMAND} demand is already being pushed.")
                    .SetTextVariable("DEMAND", active.Name));
            }

            if (influence < demand.MinimumGroupInfluence)
            {
                return new(false, new TextObject("{=!}This demand requires at least {INFLUENCE}% group influence.")
                    .SetTextVariable("INFLUENCE", (demand.MinimumGroupInfluence * 100f).ToString("0.0")));
            }

            return demand.IsDemandCurrentlyAdequate();
        }

        public override bool Equals(object obj)
        {
            if (obj is InterestGroup)
            {
                return (obj as InterestGroup).StringId == StringId && KingdomDiplomacy == (obj as InterestGroup).KingdomDiplomacy;
            }
            return base.Equals(obj);
        }

        public class DemandOutcome
        {
            [SaveableProperty(1)] public Demand Demand { get; private set; }
            [SaveableProperty(2)] public CampaignTime EndDate { get; private set; }
            [SaveableProperty(3)] public TextObject Explanation { get; private set; }
            [SaveableProperty(4)] public bool Success { get; private set; }
            [SaveableProperty(5)] public bool Enabled { get; set; }

            public DemandOutcome(Demand demand, CampaignTime endDate, TextObject explanation, bool success)
            {
                Demand = demand;
                EndDate = endDate;
                Explanation = explanation;
                Success = success;
                Enabled = true;
            }
        }
    }
}
