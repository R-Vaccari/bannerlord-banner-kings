using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using System.Linq;
using TaleWorlds.Library;
using TaleWorlds.CampaignSystem.Actions;

namespace BannerKings.Managers.Institutions
{
    public class Guild : LandedInstitution
    {
        private IEnumerable<ValueTuple<ItemObject, float>> productions;
        private GuildType type;
        private List<Hero> members;
        private int capital;
        private Hero guildMaster;
        public Guild(Settlement settlement, GuildType type, IEnumerable<ValueTuple<ItemObject, float>> productions) : base(settlement)
        {
            this.capital = 10000;
            this.guildMaster = this.GenerateLeader();
            this.members = new List<Hero>();
            this.type = type;
            this.productions = productions;
        }

        public override void Destroy()
        {
            KillCharacterAction.ApplyByRemove(this.guildMaster);     
        }

        public int Capital => this.capital;
        public MBReadOnlyList<Hero> Members => new MBReadOnlyList<Hero>(this.members);

        public void AddMemer(Hero hero)
        {
            if (this.settlement.Notables.Contains(hero) && !this.members.Contains(hero))
                this.members.Add(hero);
        }

        public void RemoveMember(Hero hero)
        {
            if (this.members.Contains(hero))
                this.members.Remove(hero);
        }

        public Hero Leader
        {
            get
            {
                if (this.guildMaster == null || !this.guildMaster.IsAlive || this.guildMaster.IsActive)
                    this.guildMaster = GenerateLeader();
                return this.guildMaster;
            }
        }

        public Hero GenerateLeader()
        {
            CultureObject culture = base.Settlement.Culture;
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
