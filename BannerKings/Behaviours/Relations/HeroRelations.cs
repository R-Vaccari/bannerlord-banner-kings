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
            foreach (Hero hero2 in Hero.AllAliveHeroes)
            {
                if (hero2 == Hero) continue;

                int target = GetRelationsTarget(hero2);
                int relation = Hero.GetRelation(hero2);
                if (relation > target) ChangeRelationAction.ApplyRelationChangeBetweenHeroes(Hero, hero2, -2, false);
                else if (relation < target) ChangeRelationAction.ApplyRelationChangeBetweenHeroes(Hero, hero2, 2, false);
            }
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
