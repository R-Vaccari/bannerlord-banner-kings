using BannerKings.Managers.Institutions.Religions.Faiths;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.CampaignSystem.Actions;

namespace BannerKings.Managers.Institutions.Religions
{
    public class Religion : LandedInstitution
    {
        private Dictionary<Settlement, Clergyman> clergy;
        private Faith faith;
        private ReligiousLeadership leadership;
        private List<CultureObject> favoredCultures;

        public Religion(Settlement settlement, Faith faith, ReligiousLeadership leadership,
            List<CultureObject> favoredCultures) : base(settlement)
        {
            this.clergy = new Dictionary<Settlement, Clergyman>();
            this.leadership = leadership;
            this.faith = faith;
            this.favoredCultures = favoredCultures;
        }

        public Divinity MainGod => this.faith.MainGod;
        public Hero Leader => this.leadership.GetLeader();
        public Faith Faith => this.faith;

        public MBReadOnlyDictionary<Settlement, Clergyman> Clergy => this.clergy.GetReadOnlyDictionary();

        public Clergyman GenerateClergyman(Settlement settlement)
        {
            int rank = faith.GetIdealRank(settlement);
            TextObject title = this.faith.GetRankTitle(rank);
            CharacterObject character = this.faith.GetPreset(rank);
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
                this.clergy.Add(settlement, clergyman);
                return clergyman;
            } else
            {
                throw new BannerKingsException("");
            }
        }

        public bool IsFavoredCulture(CultureObject culture) => this.favoredCultures.Contains(culture);

        public override void Destroy()
        {
            throw new System.NotImplementedException();
        }

        public MBReadOnlyList<CultureObject> FavoredCultures => this.favoredCultures.GetReadOnlyList();

    }
}
