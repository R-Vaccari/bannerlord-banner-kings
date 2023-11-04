using BannerKings.Managers.Innovations.Eras;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using static BannerKings.Managers.PopulationManager;

namespace BannerKings.Managers.Recruits
{
    public class RecruitSpawn : BannerKingsObject
    {
        public RecruitSpawn() : base("") 
        {
            Initialize(TextObject.Empty, TextObject.Empty);
            FiefStrings = new HashSet<string>(1);
        }

        public void Initialize(CharacterObject troop, 
            CultureObject culture,
            Dictionary<PopType, float> chances,
            Kingdom kingdom = null)
        {
            Troop = troop;
            Chances = chances;
            Culture = culture;
            Kingdom = kingdom;
        }

        public void SetTroopAdvancement(Era era, string equipmentId)
        {
            era.AddTroopAdvancement(new BKTroopAdvancement(Troop, equipmentId));
        }
        
        public void AddFiefString(string id)
        {
            FiefStrings.Add(id);
        }

        public CharacterObject Troop { get; private set; }
        public HashSet<string> FiefStrings { get; set; }
        public Kingdom Kingdom { get; private set; }
        public CultureObject Culture { get; private set; }
        public float GetChance(PopType type) => Chances.ContainsKey(type) ? Chances[type] : 0f;
        public List<PopType> GetPossibleTypes() => Chances.Keys.ToList();

        public PopType GetTroopPopType()
        {
            if (Chances.Count == 1) return Chances.First().Key;
            else if (Chances.Count > 0)
            {
                while (true)
                {
                    foreach (var pair in Chances)
                    {
                        if (MBRandom.RandomFloat <= pair.Value)
                        {
                            return pair.Key;
                        }
                    }
                }
            }

            return PopType.None;
        }
        
        private Dictionary<PopType, float> Chances { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is RecruitSpawn)
            {
                RecruitSpawn r = (RecruitSpawn)obj;
                return r.Troop == Troop && r.Culture == Culture;
            }
            return base.Equals(obj);
        }
    }
}
