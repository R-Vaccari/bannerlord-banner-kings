using System;
using System.Collections.Generic;
using BannerKings.Managers.Institutions.Religions.Doctrines;
using BannerKings.Managers.Institutions.Religions.Faiths.Rites;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;
using static BannerKings.Behaviours.Feasts.Feast;

namespace BannerKings.Managers.Institutions.Religions.Faiths
{
    public abstract class Faith : MBObjectBase
    {
        protected FaithGroup faithGroup;
        protected Divinity mainGod;
        protected List<Divinity> pantheon;
        protected Dictionary<int, CharacterObject> presets;
        protected List<Rite> rites;

        [SaveableField(1)] protected Dictionary<Faith, FaithStance> stances;

        protected Dictionary<TraitObject, bool> traits;

        public Faith() : base()
        {
            StringId = GetId();
            stances = new Dictionary<Faith, FaithStance>();
            presets = new Dictionary<int, CharacterObject>();
            Doctrines = new List<Doctrine>();
        }

        public MBReadOnlyList<Rite> Rites => new MBReadOnlyList<Rite>(rites);
        public MBReadOnlyDictionary<TraitObject, bool> Traits => traits.GetReadOnlyDictionary();
        public FaithGroup FaithGroup => faithGroup;
        public Divinity MainGod => mainGod;
        public FeastType FeastType { get; private set; }
        public List<Doctrine> Doctrines { get; private set; }
        public bool Active { get; set; } = true;
        public List<Settlement> HolySites
        {
            get
            {
                List<Settlement> sites = new List<Settlement>(pantheon.Count);
                foreach (Divinity d in pantheon)
                {
                    if (d.Shrine != null) sites.Add(d.Shrine);
                }

                return sites;
            }
        }

        protected void Initialize(Divinity mainGod, 
            Dictionary<TraitObject, bool> traits, 
            FaithGroup faithGroup,
            List<Doctrine> doctrines,
            List<Rite> rites = null,
            FeastType feastType = FeastType.None)
        {
            this.mainGod = mainGod;
            this.traits = traits;
            this.faithGroup = faithGroup;
            rites ??= new List<Rite>();

            this.rites = rites;
            Doctrines = doctrines;
            FeastType = feastType;
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

            if (faithGroup == otherFaith.faithGroup)
            {
                return FaithStance.Tolerated;
            }

            if (Doctrines.Contains(DefaultDoctrines.Instance.Tolerant))
            {
                return FaithStance.Tolerated;
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

        public abstract TextObject GetInductionExplanationText();
        public abstract bool IsCultureNaturalFaith(CultureObject culture);
        public abstract bool IsHeroNaturalFaith(Hero hero);
        public abstract TextObject GetFaithName();
        public abstract TextObject GetFaithDescription();
        public Divinity GetMainDivinity() => mainGod;
        public MBReadOnlyList<Divinity> GetSecondaryDivinities() => new MBReadOnlyList<Divinity>(pantheon);
        public abstract TextObject GetCultsDescription();
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
        public abstract int GetIdealRank(Settlement settlement);
        public abstract TextObject GetRankTitle(int rank);
        public abstract string GetId();
        public abstract TextObject GetBlessingAction();
        public abstract TextObject GetBlessingActionName();
        public abstract TextObject GetBlessingQuestion();
        public abstract TextObject GetBlessingConfirmQuestion();
        public abstract TextObject GetBlessingQuickInformation();
        public abstract Banner GetBanner();
        public abstract TextObject GetDescriptionHint();
        public abstract Settlement FaithSeat { get; }

        public override bool Equals(object obj)
        {
            if (obj is Faith)
            {
                return GetId() == (obj as Faith).GetId();
            }
            return base.Equals(obj);
        }
    }

    public enum FaithStance
    {
        Tolerated,
        Untolerated,
        Hostile
    }
}