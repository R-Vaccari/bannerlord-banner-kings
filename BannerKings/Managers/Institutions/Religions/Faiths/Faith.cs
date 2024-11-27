using System;
using System.Collections.Generic;
using BannerKings.Managers.Institutions.Religions.Doctrines;
using BannerKings.Managers.Institutions.Religions.Doctrines.Marriage;
using BannerKings.Managers.Institutions.Religions.Doctrines.War;
using BannerKings.Managers.Institutions.Religions.Faiths.Groups;
using BannerKings.Managers.Institutions.Religions.Faiths.Rites;
using BannerKings.Managers.Institutions.Religions.Faiths.Societies;
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
        protected List<Divinity> pantheon;
        protected Dictionary<int, CharacterObject> presets;
        protected List<Rite> rites;

        [SaveableField(4)] protected Dictionary<Faith, FaithStance> stances;
        [SaveableField(5)] protected FaithGroup faithGroup;

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
        public FaithGroup FaithGroup
        {
            get => faithGroup;
            private set => faithGroup = value;
        }

        public Divinity MainGod { get; private set; }
        public FeastType FeastType { get; private set; }
        public List<Doctrine> Doctrines { get; private set; }
        public WarDoctrine WarDoctrine { get; private set; }
        public MarriageDoctrine MarriageDoctrine { get; private set; }
        public List<Society> Societies { get; private set; }
        public bool Active { get; set; } = true;

        public void Initialize(Divinity mainGod,
            List<Divinity> pantheon,
            Dictionary<TraitObject, bool> traits, 
            FaithGroup faithGroup,
            List<Doctrine> doctrines,
            MarriageDoctrine marriageDoctrine,
            WarDoctrine warDoctrine,
            List<Rite> rites,
            List<Society> societies,
            FeastType feastType = FeastType.None)
        {
            if (FaithGroup == null) FaithGroup = faithGroup;
            else FaithGroup.Initialize(faithGroup.Name, faithGroup.Title, faithGroup.Description);

            MainGod = mainGod;
            this.pantheon = pantheon;
            this.traits = traits;
            this.rites = rites;
            Doctrines = doctrines;
            FeastType = feastType;
            MarriageDoctrine = marriageDoctrine;
            WarDoctrine = warDoctrine;
            Societies = societies;
        }

        public FaithStance GetStance(Faith otherFaith)
        {
            if (otherFaith == null) return FaithStance.Untolerated;
            if (FaithGroup.Equals(otherFaith.FaithGroup) || otherFaith == this) return FaithStance.Tolerated;
            if (stances.ContainsKey(otherFaith)) return stances[otherFaith];
            if (Doctrines.Contains(DefaultDoctrines.Instance.Tolerant)) return FaithStance.Tolerated;

            return FaithStance.Untolerated;
        }

        public void AddStance(Faith faith, FaithStance stance)
        {
            if (faith == this) return;
            stances[faith] = stance;
        }

        public void AddPreset(int rank, CharacterObject preset) => presets[rank] = preset;
            

        public CharacterObject GetPreset(int rank)
        {
            if (presets.ContainsKey(rank)) return presets[rank];
            return null;
        }

        public abstract float JoinSocietyCost { get; }
        public abstract float FaithStrengthFactor { get; }
        public abstract float BlessingCostFactor { get; }
        public abstract float VirtueFactor { get; }
        public abstract float ConversionCost { get; }
        public abstract TextObject GetFaithTypeName();
        public abstract TextObject GetFaithTypeExplanation();
        public abstract TextObject GetZealotsGroupName();
        public abstract TextObject GetInductionExplanationText();
        public abstract bool IsCultureNaturalFaith(CultureObject culture);
        public abstract bool IsHeroNaturalFaith(Hero hero);
        public abstract TextObject GetFaithName();
        public abstract TextObject GetFaithDescription();
        public Divinity GetMainDivinity() => MainGod;
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