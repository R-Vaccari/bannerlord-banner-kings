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
        private Dictionary<Faith, FaithStance> stances;
        private Dictionary<int, CharacterObject> presets;

        public Faith() 
        {
            this.stances = new Dictionary<Faith, FaithStance>();
        }

        protected void Initialize(Divinity mainGod, Dictionary<TraitObject, bool> traits, FaithGroup faithGroup,
            Dictionary<int, CharacterObject> presets)
        {
            this.mainGod = mainGod;
            this.traits = traits;
            this.faithGroup = faithGroup;
            this.presets = presets;
        }

        public void AddStance(Faith faith, FaithStance stance)
        {
            if (faith == this) return;
            if (this.stances.ContainsKey(faith))
                this.stances[faith] = stance;
            else this.stances.Add(faith, stance);
        }

        public void AddPreset(int rank, CharacterObject preset)
        {
            if (!this.presets.ContainsKey(rank))
                this.presets.Add(rank, preset);
            else this.presets[rank] = preset;
        }

        public CharacterObject GetPreset(int rank)
        {
            if (this.presets.ContainsKey(rank))
                return this.presets[rank];

            return null;
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
        public abstract int GetIdealRank(Settlement settlement);
        public abstract TextObject GetRankTitle(int rank);
    }

    public enum FaithStance
    {
        Tolerated,
        Untolerated,
        Hostile
    }
}
