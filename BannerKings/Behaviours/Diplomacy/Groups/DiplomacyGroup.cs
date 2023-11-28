using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.SaveSystem;

namespace BannerKings.Behaviours.Diplomacy.Groups
{
    public abstract class DiplomacyGroup : BannerKingsObject
    {
        public DiplomacyGroup(string stringId) : base(stringId)
        {
            Members = new List<Hero>();
            JoinTime = new Dictionary<Hero, CampaignTime>();
        }

        [SaveableProperty(10)] public KingdomDiplomacy KingdomDiplomacy { get; protected set; }
        [SaveableProperty(11)] public Hero Leader { get; protected set; }
        [SaveableProperty(12)] public List<Hero> Members { get; protected set; }
        [SaveableProperty(9)] public Dictionary<Hero, CampaignTime> JoinTime { get; protected set; }

        public Hero FactionLeader => KingdomDiplomacy.Kingdom.Leader;

        public abstract bool IsInterestGroup { get; }
        public bool IsRadicalGroup => !IsInterestGroup;

        public abstract void SetNewLeader(KingdomDiplomacy diplomacy);
        public abstract bool CanHeroJoin(Hero hero, KingdomDiplomacy diplomacy);
        public abstract bool CanHeroLeave(Hero hero, KingdomDiplomacy diplomacy);
        public abstract void AddMember(Hero hero);
        public abstract void RemoveMember(Hero hero, bool forced = false);

        protected void AddMemberInternal(Hero hero)
        {
            Members.Add(hero);
            JoinTime.Add(hero, CampaignTime.Now);
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

        public abstract void Tick();

        protected void TickInternal()
        {
            var toRemove = new List<Hero>();
            foreach (var hero in Members)
            {
                if (hero.IsDead || hero.MapFaction != KingdomDiplomacy.Kingdom)
                {
                    toRemove.Add(hero);
                }
            }

            foreach (var hero in toRemove)
            {
                Members.Remove(hero);
            }

            if (Leader == null)
            {
                SetNewLeader(KingdomDiplomacy);
            }
        }
    }
}
