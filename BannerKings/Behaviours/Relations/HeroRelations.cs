using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.SaveSystem;

namespace BannerKings.Behaviours.Relations
{
    public class HeroRelations
    {
        [SaveableProperty(1)] public Hero Hero { get; private set; }
        [SaveableProperty(2)] private Dictionary<Hero, List<RelationsModifier>> Relations { get; set; }

        public HeroRelations(Hero hero) 
        { 
            Hero = hero;
            Relations = new Dictionary<Hero, List<RelationsModifier>>();
        }

        public void AddModifier(Hero hero2, RelationsModifier modifier)
        {
            Relations[hero2].Add(modifier);
        }

        public int GetRelationsFinalTarget(List<RelationsModifier> modifiers)
        {
            int result = 0;
            foreach (RelationsModifier modifier in modifiers)
            {
                result += modifier.Relation;
            }

            return result;
        }

        public int GetRelationsTarget(Hero hero)
        {
            List<RelationsModifier> modifiers = BannerKingsConfig.Instance.RelationsModel.CalculateModifiers(this, hero);
            int result = 0;
            foreach (RelationsModifier modifier in modifiers)
            {
                result += modifier.Relation;
            }

            return result;
        }

        public List<RelationsModifier> GetRelations(Hero target)
        {
            List<RelationsModifier> relations;
            if (!Relations.TryGetValue(target, out relations)) relations = new List<RelationsModifier>();

            return relations;
        }

        public void UpdateRelations()
        {
            CleanRelations();
            foreach (Hero hero2 in GetHeroesToUpdate())
            {
                if (hero2 == Hero) continue;

                int target = GetRelationsTarget(hero2);
                int relation = Hero.GetRelation(hero2);
                if (relation > target) ChangeRelationAction.ApplyRelationChangeBetweenHeroes(Hero, hero2, -2, false);
                else if (relation < target) ChangeRelationAction.ApplyRelationChangeBetweenHeroes(Hero, hero2, 2, false);
            }
        }

        private HashSet<Hero> GetHeroesToUpdate()
        {
            HashSet<Hero> heroes = new HashSet<Hero>(20);
            if (Hero.CurrentSettlement != null)
            {
                foreach (Hero notable in Hero.CurrentSettlement.Notables)
                    heroes.Add(notable);

                foreach (var party in Hero.CurrentSettlement.Parties)
                    if (party.LeaderHero != null)
                        heroes.Add(party.LeaderHero);
            }

            if (Hero.MapFaction != null && Hero.MapFaction.IsKingdomFaction)
            {
                Kingdom kingdom = (Kingdom)Hero.MapFaction;
                foreach (var clan in kingdom.Clans)
                    heroes.Add(clan.Leader);

                foreach (Kingdom k in Kingdom.All)
                    heroes.Add(k.Leader);
            }

            if (Hero.Clan != null)
            {
                foreach (Hero hero in Hero.Clan.Heroes) heroes.Add(hero); 
                foreach (Hero hero in Hero.Clan.Companions) heroes.Add(hero);
            }
            else if (Hero.IsNotable)
            {
                Clan clan = Hero.CurrentSettlement.OwnerClan;
                foreach (Hero hero in clan.Heroes) heroes.Add(hero);
                foreach (Hero hero in clan.Companions) heroes.Add(hero);
            }

            return heroes;
        }

        public void CleanRelations()
        {
            Dictionary<Hero, List<RelationsModifier>> cache = new Dictionary<Hero, List<RelationsModifier>>(Relations);
            List<RelationsModifier> modifiersCache = new List<RelationsModifier>();
            foreach (var relation in cache)
            {
                if (relation.Key.IsDead) Relations.Remove(relation.Key);
                else if (Relations.ContainsKey(relation.Key))
                {
                    modifiersCache.Clear();
                    modifiersCache.AddRange(Relations[relation.Key]);
                    foreach (RelationsModifier modifier in modifiersCache)
                        if (modifier.Expiry.IsPast)
                            Relations[relation.Key].Remove(modifier);
                }
            }
        }
    }
}
