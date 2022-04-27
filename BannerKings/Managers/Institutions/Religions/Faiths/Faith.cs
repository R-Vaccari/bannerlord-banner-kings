using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions.Faiths
{
    public abstract class Faith
    {
        private FaithGroup faithGroup;
        private Divinity mainGod;
        private Dictionary<TraitObject, bool> traits;
        private Dictionary<Religion, ReligiousStance> stances;

        public Faith() 
        {
            this.stances = new Dictionary<Religion, ReligiousStance>();
        }

        protected void Initialize(Divinity mainGod, Dictionary<TraitObject, bool> traits, FaithGroup faithGroup)
        {
            this.mainGod = mainGod;
            this.traits = traits;
            this.faithGroup = faithGroup;
        }

        public Dictionary<TraitObject, bool> Traits => this.traits;
        public FaithGroup FaithGroup => this.faithGroup;
        public Divinity MainGod => this.mainGod;
        public abstract List<Divinity> GetMainDivinities();
        public abstract List<Divinity> GetSecondaryDivinities();
        public abstract TextObject GetMainGodDescription();
        public abstract TextObject GetMainDivinitiesDescription();
        public abstract TextObject GetSecondaryDivinitiesDescription();
        public abstract int GetMaxClergyRank();
        public abstract TextObject GetClergyGreeting(int rank);
        public abstract TextObject GetClergyGreetingInducted(int rank);
        public abstract TextObject GetClergyPreachingAnswer(int rank);
        public abstract TextObject GetClergyProveFaith(int rank);
        public abstract TextObject GetClergyForbiddenAnswer(int rank);
        public abstract TextObject GetClergyInduction(int rank);
    }

    public enum ReligiousStance
    {
        Tolerated,
        Untolerated,
        Hostile
    }
}
