using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Institutions.Religions.Faiths
{
    public abstract class Faith
    {
        protected FaithGroup faithGroup;

        protected Divinity mainGod;

        [SaveableField(1)]
        protected Dictionary<Faith, FaithStance> stances;

        protected Dictionary<TraitObject, bool> traits;

        protected Dictionary<int, CharacterObject> presets;

        public Faith() 
        {
            stances = new Dictionary<Faith, FaithStance>();
            presets = new Dictionary<int, CharacterObject>();
        }

        protected void Initialize(Divinity mainGod, Dictionary<TraitObject, bool> traits, FaithGroup faithGroup)
        {
            this.mainGod = mainGod;
            this.traits = traits;
            this.faithGroup = faithGroup;
        }

        public MBReadOnlyDictionary<TraitObject, bool> Traits => traits.GetReadOnlyDictionary();

        public FaithStance GetStance(Faith otherFaith)
        {
            if (otherFaith == null)
                return FaithStance.Untolerated;

            if (otherFaith == this)
                return FaithStance.Tolerated;

            if (stances.ContainsKey(otherFaith))
                return stances[otherFaith];

            return FaithStance.Untolerated;
        }

        public void AddStance(Faith faith, FaithStance stance)
        {
            if (faith == this) return;
            if (stances.ContainsKey(faith))
                stances[faith] = stance;
            else stances.Add(faith, stance);
        }

        public void AddPreset(int rank, CharacterObject preset)
        {
            if (!presets.ContainsKey(rank))
                presets.Add(rank, preset);
            else presets[rank] = preset;
        }

        public CharacterObject GetPreset(int rank)
        {
            if (presets.ContainsKey(rank))
                return presets[rank];

            return null;
        }
        public FaithGroup FaithGroup => faithGroup;
        public Divinity MainGod => mainGod;

        public abstract TextObject GetFaithName();
        public abstract TextObject GetFaithDescription();
        public abstract Divinity GetMainDivinity();
        public abstract MBReadOnlyList<Divinity> GetSecondaryDivinities();
        public abstract TextObject GetMainDivinitiesDescription();
        public abstract TextObject GetSecondaryDivinitiesDescription();
        public abstract int GetMaxClergyRank();
        public abstract TextObject GetClergyGreeting(int rank);
        public abstract TextObject GetClergyGreetingInducted(int rank);
        public abstract TextObject GetClergyPreachingAnswer(int rank);
        public abstract TextObject GetClergyPreachingAnswerLast(int rank);
        public abstract TextObject GetClergyProveFaith(int rank);
        public abstract TextObject GetClergyProveFaithLast(int rank);
        public abstract TextObject GetClergyForbiddenAnswer(int rank);
        public abstract TextObject GetClergyForbiddenAnswerLast(int rank);
        public abstract TextObject GetClergyInduction(int rank);
        public abstract TextObject GetClergyInductionLast(int rank);
        public abstract ValueTuple<bool, TextObject> GetInductionAllowed(Hero hero, int rank);
        public abstract int GetIdealRank(Settlement settlement, bool isCapital);
        public abstract TextObject GetRankTitle(int rank);
        public abstract string GetId();
    }

    public enum FaithStance
    {
        Tolerated,
        Untolerated,
        Hostile
    }
}
