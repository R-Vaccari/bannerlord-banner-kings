using BannerKings.Managers.Innovations.Eras;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
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
            float chance, 
            PopType popType, 
            Kingdom kingdom = null)
        {
            Troop = troop;
            PopType = popType;
            Culture = culture;
            Chance = chance;
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
        public float Chance { get; private set; }
        public PopType PopType { get; private set; }

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
