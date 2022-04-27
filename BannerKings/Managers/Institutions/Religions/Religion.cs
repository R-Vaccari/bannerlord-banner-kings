using BannerKings.Managers.Institutions.Religions.Faiths;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using System.Linq;

namespace BannerKings.Managers.Institutions.Religions
{
    public class Religion : LandedInstitution
    {
        private Dictionary<Settlement, Hero> clergy;
        private Faith faith;
        private ReligiousLeadership leadership;
        private List<CultureObject> favoredCultures;

        public Religion(Settlement settlement, Faith faith, ReligiousLeadership leadership,
            List<CultureObject> favoredCultures) : base(settlement)
        {
            this.clergy = new Dictionary<Settlement, Hero>();
            this.leadership = leadership;
            this.faith = faith;
            this.favoredCultures = favoredCultures;
        }

        public Divinity MainGod => this.faith.MainGod;
        public Hero Leader => this.leadership.GetLeader();
        public Faith Faith => this.faith;

        public Clergyman GenerateClergyman(Settlement settlement)
        {
            int rank = faith.GetIdealRank(settlement);
            CharacterObject character = CharacterObject.All.FirstOrDefault(x => x.StringId == "bannerkings_preacher_aserai_1");
            if (character != null)
            {
                Clergyman clergyman = new Clergyman(HeroCreator.CreateSpecialHero(character, settlement), rank);
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
