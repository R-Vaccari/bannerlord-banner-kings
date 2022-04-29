using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace BannerKings.Managers.Institutions
{
    public class Guild : LandedInstitution
    {
        private IEnumerable<ValueTuple<ItemObject, float>> productions;
        private GuildType type;
        private List<Hero> members;
        private int capital;
        public Guild(Settlement settlement, GuildType type, IEnumerable<ValueTuple<ItemObject, float>> productions) : base(settlement)
        {
            capital = 10000;
            leader = GenerateLeader();
            members = new List<Hero>();
            this.type = type;
            this.productions = productions;
        }

        public override void Destroy()
        {
            KillCharacterAction.ApplyByRemove(leader);     
        }

        public int Capital => capital;
        public MBReadOnlyList<Hero> Members => new MBReadOnlyList<Hero>(members);

        public void AddMemer(Hero hero)
        {
            if (settlement.Notables.Contains(hero) && !members.Contains(hero))
                members.Add(hero);
        }

        public void RemoveMember(Hero hero)
        {
            if (members.Contains(hero))
                members.Remove(hero);
        }

        public override Hero GenerateLeader()
        {
            CultureObject culture = Settlement.Culture;
            IEnumerable<CharacterObject> templates = from x in culture.NotableAndWandererTemplates 
                                                     where (x.Occupation == Occupation.Merchant || x.Occupation == Occupation.Artisan)
                                                     select x;
            if (templates.Count() > 0)
            {
                CharacterObject template = templates.GetRandomElementInefficiently();
                Settlement born = Settlement.All.GetRandomElementWithPredicate(x => x.Culture == culture);
                return HeroCreator.CreateSpecialHero(template, born);
            }

            return null;
        }
    }

    public enum GuildType
    {
        Merchants,
        Masons,
        Metalworkers
    }
}
