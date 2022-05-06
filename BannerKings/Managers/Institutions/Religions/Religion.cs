using BannerKings.Managers.Institutions.Religions.Faiths;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions
{
    public class Religion : LandedInstitution
    {
        private Dictionary<Settlement, Clergyman> clergy;
        private Faith faith;
        private ReligiousLeadership leadership;
        private List<CultureObject> favoredCultures;
        private List<string> doctrineIds;

        public Religion(Settlement settlement, Faith faith, ReligiousLeadership leadership,
            List<CultureObject> favoredCultures, List<string> doctrineIds) : base(settlement)
        {
            clergy = new Dictionary<Settlement, Clergyman>();
            this.leadership = leadership;
            this.faith = faith;
            this.favoredCultures = favoredCultures;
            this.doctrineIds = doctrineIds;
        }

        public CultureObject MainCulture => favoredCultures[0];
        public Divinity MainGod => faith.MainGod;
        public Faith Faith => faith;
        public MBReadOnlyList<string> Doctrines => doctrineIds.GetReadOnlyList();
        public MBReadOnlyDictionary<Settlement, Clergyman> Clergy => clergy.GetReadOnlyDictionary();

        public Clergyman GenerateClergyman(Settlement settlement)
        {
            int rank = faith.GetIdealRank(settlement);
            if (rank <= 0) return null;
            TextObject title = faith.GetRankTitle(rank);
            CharacterObject character = faith.GetPreset(rank);
            if (character != null)
            {
                Hero hero = HeroCreator.CreateSpecialHero(character, settlement);
                TextObject firstName = hero.FirstName;
                TextObject fullName = new TextObject("{=!}{RELIGIOUS_TITLE} {NAME}")
                    .SetTextVariable("RELIGIOUS_TITLE", title)
                    .SetTextVariable("NAME", firstName);
                hero.SetName(fullName, firstName);
                EnterSettlementAction.ApplyForCharacterOnly(hero, settlement);
                Clergyman clergyman = new Clergyman(hero, rank);
                clergy.Add(settlement, clergyman);
                return clergyman;
            }

            throw new BannerKingsException("");
        }

        public bool IsFavoredCulture(CultureObject culture) => favoredCultures.Contains(culture);

        public override void Destroy()
        {
            throw new NotImplementedException();
        }

        public MBReadOnlyList<CultureObject> FavoredCultures => favoredCultures.GetReadOnlyList();

    }
}
