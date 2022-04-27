using BannerKings.Managers.Institutions.Religions.Faiths;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

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

        public Hero GenerateClergyman(Settlement settlement)
        {
            Hero clergyman = null;
            return clergyman;
        }

        public bool IsFavoredCulture(CultureObject culture) => this.favoredCultures.Contains(culture);

        public override void Destroy()
        {
            throw new System.NotImplementedException();
        }

        public MBReadOnlyList<CultureObject> FavoredCultures => this.favoredCultures.GetReadOnlyList();

    }
}
