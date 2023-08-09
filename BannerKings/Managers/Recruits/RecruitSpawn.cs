using System.Linq;
using System.Text.RegularExpressions;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;
using static BannerKings.Managers.PopulationManager;

namespace BannerKings.Managers.Recruits
{
    public class RecruitSpawn : BannerKingsObject
    {
        public RecruitSpawn() : base("") 
        {
            Initialize(TextObject.Empty, TextObject.Empty);
        }

        public void Initialize(CharacterObject troop, CultureObject culture, float chance, PopType popType,
            string fief = null, Kingdom kingdom = null)
        {
            Troop = troop;
            PopType = popType;
            FiefString = fief;
            Culture = culture;
            Chance = chance;
            Kingdom = kingdom;
        }

        public CharacterObject Troop { get; private set; }
        public string FiefString { get; set; }
        public Town Fief => Town.AllFiefs.FirstOrDefault(x => x.StringId == FiefString);
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
