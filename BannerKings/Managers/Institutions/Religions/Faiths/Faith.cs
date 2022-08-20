using System;
using System.Collections.Generic;
using BannerKings.Managers.Institutions.Religions.Faiths.Rites;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Institutions.Religions.Faiths
{
    public abstract class Faith
    {
        protected FaithGroup faithGroup;
        protected Divinity mainGod;
        protected List<Divinity> pantheon;
        protected Dictionary<int, CharacterObject> presets;
        protected List<Rite> rites;

        [SaveableField(1)] protected Dictionary<Faith, FaithStance> stances;

        protected Dictionary<TraitObject, bool> traits;

        public Faith()
        {
            stances = new Dictionary<Faith, FaithStance>();
            presets = new Dictionary<int, CharacterObject>();
        }

        public MBReadOnlyList<Rite> Rites => rites.GetReadOnlyList();
        public MBReadOnlyDictionary<TraitObject, bool> Traits => traits.GetReadOnlyDictionary();
        public FaithGroup FaithGroup => faithGroup;
        public Divinity MainGod => mainGod;

        protected void Initialize(Divinity mainGod, Dictionary<TraitObject, bool> traits, FaithGroup faithGroup,
            List<Rite> rites = null)
        {
            this.mainGod = mainGod;
            this.traits = traits;
            this.faithGroup = faithGroup;
            if (rites == null)
            {
                rites = new List<Rite>();
            }

            this.rites = rites;
        }

        public FaithStance GetStance(Faith otherFaith)
        {
            if (otherFaith == null)
            {
                return FaithStance.Untolerated;
            }

            if (otherFaith == this)
            {
                return FaithStance.Tolerated;
            }

            if (stances.ContainsKey(otherFaith))
            {
                return stances[otherFaith];
            }

            return FaithStance.Untolerated;
        }

        public void AddStance(Faith faith, FaithStance stance)
        {
            if (faith == this)
            {
                return;
            }

            if (stances.ContainsKey(faith))
            {
                stances[faith] = stance;
            }
            else
            {
                stances.Add(faith, stance);
            }
        }

        public void AddPreset(int rank, CharacterObject preset)
        {
            if (!presets.ContainsKey(rank))
            {
                presets.Add(rank, preset);
            }
            else
            {
                presets[rank] = preset;
            }
        }

        public CharacterObject GetPreset(int rank)
        {
            if (presets.ContainsKey(rank))
            {
                return presets[rank];
            }

            return null;
        }

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

        public abstract TextObject GetBlessingAction();
        public abstract TextObject GetBlessingActionName();
        public abstract TextObject GetBlessingQuestion();
        public abstract TextObject GetBlessingConfirmQuestion();
        public abstract TextObject GetBlessingQuickInformation();
    }

    public enum FaithStance
    {
        Tolerated,
        Untolerated,
        Hostile
    }
}