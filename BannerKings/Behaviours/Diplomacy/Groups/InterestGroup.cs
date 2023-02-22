using BannerKings.Behaviours.Diplomacy.Groups.Demands;
using BannerKings.Behaviours.Diplomacy.Wars;
using BannerKings.Managers.Titles.Laws;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using static SandBox.CampaignBehaviors.LordConversationsCampaignBehavior;

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
            List<DemesneLaw> shunnedLaws, List<CasusBelli> supportedCasusBelli, List<Demand> possibleDemands)
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
            PossibleDemands = possibleDemands;

            Members = new List<Hero>();
        }

        public KingdomDiplomacy KingdomDiplomacy { get; private set; }
        public Hero FactionLeader => KingdomDiplomacy.Kingdom.Leader;

        public InterestGroup GetCopy(KingdomDiplomacy diplomacy)
        {
            InterestGroup result = new InterestGroup(StringId);
            result.Initialize(Name, Description, MainTrait, DemandsCouncil, AllowsCommoners,
                AllowsNobles, PreferredOccupations, SupportedPolicies, ShunnedPolicies, SupportedLaws,
                ShunnedLaws, SupportedCasusBelli, PossibleDemands);
            KingdomDiplomacy = diplomacy;
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

        public override bool Equals(object obj)
        {
            if (obj is InterestGroup)
            {
                return (obj as InterestGroup).StringId == StringId;
            }
            return base.Equals(obj);
        }
    }
}
