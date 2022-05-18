using BannerKings.Managers.Institutions.Religions.Faiths;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Institutions.Religions
{
    public class Religion : LandedInstitution
    {
        [SaveableField(1)]
        private Dictionary<Settlement, Clergyman> clergy;

        [SaveableField(2)]
        private Faith faith;

        [SaveableField(3)]
        private ReligiousLeadership leadership;

        [SaveableField(4)]
        private List<CultureObject> favoredCultures;

        [SaveableField(5)]
        private List<string> doctrineIds;

        public Religion(Settlement settlement, Faith faith, ReligiousLeadership leadership,
            List<CultureObject> favoredCultures, List<string> doctrineIds) : base(settlement)
        {
            clergy = new Dictionary<Settlement, Clergyman>();
            this.leadership = leadership;
            this.faith = faith;
            this.favoredCultures = favoredCultures;
            this.doctrineIds = doctrineIds;
            leadership.Initialize(this);
        }

        public CultureObject MainCulture => favoredCultures[0];
        public ReligiousLeadership Leadership => leadership;
        public Divinity MainGod => faith.MainGod;
        public Faith Faith => faith;
        public MBReadOnlyList<string> Doctrines => doctrineIds.GetReadOnlyList();
        public MBReadOnlyDictionary<Settlement, Clergyman> Clergy => clergy.GetReadOnlyDictionary();

        public void AddClergyman(Settlement settlement, Clergyman clergyman)
        {
            if (clergy.ContainsKey(settlement))
                clergy[settlement] = clergyman;
            else clergy.Add(settlement, clergyman);
        }

        public Clergyman GetClergyman(Settlement settlement)
        {
            if (clergy.ContainsKey(settlement))
                return clergy[settlement];

            return null;
        }

        public Clergyman GenerateClergyman(Settlement settlement)
        {
            int rank = faith.GetIdealRank(settlement);
            if (rank <= 0) return null;
            CharacterObject character = faith.GetPreset(rank);
            if (character != null)
            {
                Hero hero = GenerateClergymanHero(character, settlement, rank);
                EnterSettlementAction.ApplyForCharacterOnly(hero, settlement);
                Clergyman clergyman = new Clergyman(hero, rank);
                clergy.Add(settlement, clergyman);
                return clergyman;
            }

            throw new BannerKingsException("");
        }

        public Hero GenerateClergymanHero(CharacterObject preset, Settlement settlement, int rank)
        {
            Hero hero = HeroCreator.CreateSpecialHero(preset, settlement);
            TextObject firstName = hero.FirstName;
            TextObject fullName = new TextObject("{=!}{RELIGIOUS_TITLE} {NAME}")
                .SetTextVariable("RELIGIOUS_TITLE", faith.GetRankTitle(rank))
                .SetTextVariable("NAME", firstName);
            hero.SetName(fullName, firstName);
            return hero;
        }

        public bool IsFavoredCulture(CultureObject culture) => favoredCultures.Contains(culture);

        public override void Destroy()
        {
            throw new NotImplementedException();
        }

        public MBReadOnlyList<CultureObject> FavoredCultures => favoredCultures.GetReadOnlyList();

        public override bool Equals(object obj)
        {
            if (obj is Religion)
            {
                Religion rel = (Religion)obj;
                return faith.GetId() == rel.Faith.GetId();
            }
                
            return base.Equals(obj);
        }

    }
}
