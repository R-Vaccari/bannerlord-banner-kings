using BannerKings.Managers.Institutions.Religions;
using BannerKings.Managers.Institutions.Religions.Doctrines;
using BannerKings.Managers.Populations;
using BannerKings.UI.Items;
using BannerKings.UI.Items.UI;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Core.ViewModelCollection.Selector;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.Religion
{
    internal class ReligionVM : BannerKingsViewModel
    {
        private MBBindingList<ReligionElementVM> aspects;
        private MBBindingList<ReligionMemberVM> clergymen;
        private MBBindingList<InformationElement> courtInfo;
        private MBBindingList<ReligionElementVM> doctrines, rites;
        private MBBindingList<ReligionMemberVM> faithful;
        private Managers.Institutions.Religions.Religion heroReligion;
        private Managers.Institutions.Religions.Religion currentReligion;
        private MBBindingList<ReligionElementVM> secondaryDivinities;
        private MBBindingList<BKTraitItemVM> virtues;
        private SelectorVM<ReligionSelectorItemVM> selector;
        private string name, description, groupName, groupDescription, divinities, inductionExplanation;
        private Hero hero;

        public ReligionVM(Managers.Institutions.Religions.Religion heroReligion, Hero hero) : base(null, true)
        {
            this.hero = hero;
            this.heroReligion = heroReligion;
            currentReligion = heroReligion;
            courtInfo = new MBBindingList<InformationElement>();
            Clergymen = new MBBindingList<ReligionMemberVM>();
            Faithful = new MBBindingList<ReligionMemberVM>();
            Virtues = new MBBindingList<BKTraitItemVM>();
            Doctrines = new MBBindingList<ReligionElementVM>();
            Rites = new MBBindingList<ReligionElementVM>();
            Aspects = new MBBindingList<ReligionElementVM>();
            SecondaryDivinities = new MBBindingList<ReligionElementVM>();
            InitializeCurrentReligion();
        }

        public ReligionVM(PopulationData data) : base(data, true)
        {
            hero = Hero.MainHero;
            heroReligion = data.ReligionData.DominantReligion;
            currentReligion = heroReligion;
            courtInfo = new MBBindingList<InformationElement>();
            Clergymen = new MBBindingList<ReligionMemberVM>();
            Faithful = new MBBindingList<ReligionMemberVM>();
            Virtues = new MBBindingList<BKTraitItemVM>();
            Doctrines = new MBBindingList<ReligionElementVM>();
            Rites = new MBBindingList<ReligionElementVM>();
            Aspects = new MBBindingList<ReligionElementVM>();
            SecondaryDivinities = new MBBindingList<ReligionElementVM>();
            InitializeCurrentReligion();
        }

        private void InitializeCurrentReligion()
        {
            currentReligion = BannerKingsConfig.Instance.ReligionsManager.GetIdealReligion(hero.Culture);
            if (currentReligion == null)
            {
                currentReligion = BannerKingsConfig.Instance.ReligionsManager.GetReligions().GetRandomElement();
            }
        }

        [DataSourceProperty]
        public SelectorVM<ReligionSelectorItemVM> Selector
        {
            get => selector;
            set
            {
                if (value != selector)
                {
                    selector = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public string HeroFaithText
        {
            get
            {
                TextObject baseText = new TextObject("{=diqoFxAm}{HERO} {RELIGION_TEXT}.\n{CULT_TEXT}")
                    .SetTextVariable("HERO", hero.Name);

                TextObject relText = heroReligion == null ? new TextObject("{=JoZGpNrK} does not follow any faith") :
                    new TextObject("{=Ckk2iVKQ}is following the {FAITH}").SetTextVariable("FAITH", heroReligion.Faith.GetFaithName());

                var data = BannerKingsConfig.Instance.ReligionsManager.GetFaithfulData(hero);
                if (data != null)
                {
                    Divinity cult = null;
                    if (data != null)
                    {
                        cult = data.Blessing;
                    }

                    string cultText = "";
                    bool indefinitely = data.BlessingEndDate == CampaignTime.Never;
                    if (cult != null)
                    {
                        if (indefinitely)
                        {
                            cultText = new TextObject("{=1O1b2s03}They are pledged to {CULT} indefinitely.")
                                .SetTextVariable("CULT", cult.Name).ToString();
                        }
                        else
                        {
                            cultText = new TextObject("{=wA81PG4d}They are pledged to {CULT} until {DATE}.")
                                .SetTextVariable("CULT", cult.Name)
                                .SetTextVariable("DATE", data.BlessingEndDate.ToString()).ToString();
                        }
                    }

                    return baseText.SetTextVariable("RELIGION_TEXT", relText)
                        .SetTextVariable("CULT_TEXT", cultText)
                        .ToString();
                }

                return string.Empty;
            }
        }

        [DataSourceProperty] public string ClergymenText => new TextObject("{=GbMZ6V8B}Clergymen").ToString();
        [DataSourceProperty] public string FaithfulText => new TextObject("{=mnpTkVYf}Faithful").ToString();
        [DataSourceProperty] public string GroupText => new TextObject("{=OKw2P9m1}Faith Group").ToString();
        [DataSourceProperty] public string VirtuesText => new TextObject("{=p6itQbf8}Virtues").ToString();
        [DataSourceProperty] public string DoctrinesText => new TextObject("{=BKLacKdC}Doctrines").ToString();
        [DataSourceProperty] public string AspectsText => new TextObject("{=1sKJS1JR}Aspects").ToString();
        [DataSourceProperty] public string RitesText => new TextObject("{=Yy2s38FQ}Rites").ToString();
        [DataSourceProperty] public string InductionText => new TextObject("{=M3CdwrTZ}Induction").ToString();

        [DataSourceProperty] public HintViewModel InductionHint => new HintViewModel(new TextObject("{=yS81FhXP}The criteria for being inducted into this faith. Induction may be done through preachers. Depending on the preacher's rank, they may require different things."));

        [DataSourceProperty]
        public string InductionExplanationText
        {
            get => inductionExplanation;
            set
            {
                inductionExplanation = value;
                OnPropertyChangedWithValue(value, "InductionExplanationText");
            }
        }

        [DataSourceProperty]
        public string SecondaryDivinitiesText
        {
            get => divinities;
            set
            {
                divinities = value;
                OnPropertyChangedWithValue(value, "SecondaryDivinitiesText");
            }
        }

        [DataSourceProperty] public string Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChangedWithValue(value, "Name");
            }
        }

        [DataSourceProperty] public string Description 
        { 
            get => description;
            set 
            {
                description = value;
                OnPropertyChangedWithValue(value, "Description"); 
            } 
        }

        [DataSourceProperty] public string GroupName
        {
            get => groupName;
            set
            {
                groupName = value;
                OnPropertyChangedWithValue(value, "GroupName");
            }
        }

        [DataSourceProperty] public string GroupDescription
        {
            get => groupDescription;
            set
            {
                groupDescription = value;
                OnPropertyChangedWithValue(value, "GroupDescription");
            }
        }


        [DataSourceProperty]
        public MBBindingList<BKTraitItemVM> Virtues
        {
            get => virtues;
            set
            {
                if (value != virtues)
                {
                    virtues = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<ReligionElementVM> Aspects
        {
            get => aspects;
            set
            {
                if (value != aspects)
                {
                    aspects = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<ReligionElementVM> SecondaryDivinities
        {
            get => secondaryDivinities;
            set
            {
                if (value != secondaryDivinities)
                {
                    secondaryDivinities = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<ReligionElementVM> Doctrines
        {
            get => doctrines;
            set
            {
                if (value != doctrines)
                {
                    doctrines = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<ReligionElementVM> Rites
        {
            get => rites;
            set
            {
                if (value != rites)
                {
                    rites = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<ReligionMemberVM> Clergymen
        {
            get => clergymen;
            set
            {
                if (value != clergymen)
                {
                    clergymen = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<ReligionMemberVM> Faithful
        {
            get => faithful;
            set
            {
                if (value != faithful)
                {
                    faithful = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }


        [DataSourceProperty]
        public MBBindingList<InformationElement> CourtInfo
        {
            get => courtInfo;
            set
            {
                if (value != courtInfo)
                {
                    courtInfo = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            Clergymen.Clear();
            Faithful.Clear();
            Virtues.Clear();
            Doctrines.Clear();
            Aspects.Clear();
            Rites.Clear();
            SecondaryDivinities.Clear();

            Name = currentReligion.Faith.GetFaithName().ToString();
            Description = currentReligion.Faith.GetFaithDescription().ToString();
            GroupName = currentReligion.Faith.FaithGroup.Name.ToString();
            GroupDescription = currentReligion.Faith.FaithGroup.Description.ToString();
            SecondaryDivinitiesText = currentReligion.Faith.GetCultsDescription().ToString();

            var selectedIndex = 0;
            Selector = new SelectorVM<ReligionSelectorItemVM>(0, null);
            var i = 0;
            foreach (var religion in BannerKingsConfig.Instance.ReligionsManager.GetReligions())
            {
                var item = new ReligionSelectorItemVM(religion, religion.Faith != this.currentReligion?.Faith);
                if (religion.Faith == this.currentReligion.Faith) selectedIndex = i;
                selector.AddItem(item);
                i++;
            }
            Selector.SelectedIndex = selectedIndex;
            Selector.SetOnChangeAction(delegate (SelectorVM<ReligionSelectorItemVM> obj)
            {
                currentReligion = obj.SelectedItem.Religion;
                RefreshValues();
            });

            InductionExplanationText = currentReligion.Faith.GetInductionExplanationText().ToString();

            foreach (var pair in currentReligion.Clergy)
            {
                var clgm = pair.Value;
                if (clgm != null && clgm.Hero != null)
                {
                    Clergymen.Add(new ReligionMemberVM(clgm, null));
                }
            }

            foreach (var hero in BannerKingsConfig.Instance.ReligionsManager.GetFaithfulHeroes(currentReligion))
            {
                Faithful.Add(new ReligionMemberVM(hero, null));
            }

            foreach (var pair in currentReligion.Faith.Traits)
            {
                Virtues.Add(new BKTraitItemVM(pair.Key, pair.Value));
            }

            foreach (var docString in currentReligion.Faith.Doctrines)
            {
                var doctrine = DefaultDoctrines.Instance.GetById(docString);
                if (doctrine != null)
                {
                    Doctrines.Add(new ReligionElementVM(doctrine.Name, doctrine.Effects, doctrine.Description));
                }
            }

            foreach (var rite in currentReligion.Rites)
            {
                Rites.Add(new ReligionElementVM(rite.GetName(), rite.GetDescription(), rite.GetRequirementsText(hero)));
            }

            var mainDivinity = currentReligion.Faith.GetMainDivinity();
            SecondaryDivinities.Add(new ReligionElementVM(mainDivinity.SecondaryTitle, mainDivinity.Name,
                    new TextObject("{=77isPS24}{EFFECTS}\nPiety cost: {COST}")
                    .SetTextVariable("EFFECTS", mainDivinity.Description)
                    .SetTextVariable("COST", mainDivinity.BlessingCost(hero)), mainDivinity.Effects));

            foreach (var divinity in currentReligion.Faith.GetSecondaryDivinities())
            {
                SecondaryDivinities.Add(new ReligionElementVM(divinity.SecondaryTitle, divinity.Name,
                    new TextObject("{=77isPS24}{EFFECTS}\nPiety cost: {COST}")
                    .SetTextVariable("EFFECTS", divinity.Description)
                    .SetTextVariable("COST", divinity.BlessingCost(hero)), divinity.Effects));
            }

            //Aspects.Add(new ReligionElementVM(currentReligion.Faith.GetMainDivinitiesDescription(),
            //    currentReligion.Faith.GetMainDivinity().Name, currentReligion.Faith.GetMainDivinity().Description));
            Aspects.Add(new ReligionElementVM(new TextObject("{=OKw2P9m1}Faith Group"), currentReligion.Faith.FaithGroup.Name,
                currentReligion.Faith.FaithGroup.Description));
            //Aspects.Add(new ReligionElementVM(new TextObject("{=OKw2P9m1}Faith"), UIHelper.GetFaithTypeName(currentReligion.Faith),
            //    UIHelper.GetFaithTypeDescription(currentReligion.Faith)));
            Aspects.Add(new ReligionElementVM(new TextObject("{=EjTxnGJp}Culture"), currentReligion.MainCulture.Name,
                new TextObject("{=6NYxLhjH}The main culture associated with this faith.")));
        }
    }
}