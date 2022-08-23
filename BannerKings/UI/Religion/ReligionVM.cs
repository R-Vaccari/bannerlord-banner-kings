using BannerKings.Managers.Institutions.Religions.Doctrines;
using BannerKings.Managers.Populations;
using BannerKings.UI.Items;
using BannerKings.UI.Items.UI;
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
        private MBBindingList<ReligionElementVM> doctrines;
        private MBBindingList<ReligionMemberVM> faithful;
        private Managers.Institutions.Religions.Religion religion;
        private MBBindingList<ReligionElementVM> secondaryDivinities;
        private MBBindingList<BKTraitItemVM> virtues;
        private SelectorVM<ReligionSelectorItemVM> selector;

        public ReligionVM(Managers.Institutions.Religions.Religion religion) : base(null, true)
        {
            this.religion = religion;
            courtInfo = new MBBindingList<InformationElement>();
            Clergymen = new MBBindingList<ReligionMemberVM>();
            Faithful = new MBBindingList<ReligionMemberVM>();
            Virtues = new MBBindingList<BKTraitItemVM>();
            Doctrines = new MBBindingList<ReligionElementVM>();
            Aspects = new MBBindingList<ReligionElementVM>();
            SecondaryDivinities = new MBBindingList<ReligionElementVM>();
        }

        public ReligionVM(PopulationData data) : base(data, true)
        {
            religion = data.ReligionData.Religion;
            courtInfo = new MBBindingList<InformationElement>();
            Clergymen = new MBBindingList<ReligionMemberVM>();
            Faithful = new MBBindingList<ReligionMemberVM>();
            Virtues = new MBBindingList<BKTraitItemVM>();
            Doctrines = new MBBindingList<ReligionElementVM>();
            Aspects = new MBBindingList<ReligionElementVM>();
            SecondaryDivinities = new MBBindingList<ReligionElementVM>();
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

        [DataSourceProperty] public string ClergymenText => new TextObject("{=!}Clergymen").ToString();

        [DataSourceProperty] public string FaithfulText => new TextObject("{=!}Faithful").ToString();

        [DataSourceProperty] public string GroupText => new TextObject("{=!}Faith Group").ToString();

        [DataSourceProperty] public string VirtuesText => new TextObject("{=!}Virtues").ToString();

        [DataSourceProperty]
        public string SecondaryDivinitiesText => religion.Faith.GetSecondaryDivinitiesDescription().ToString();

        [DataSourceProperty] public string DoctrinesText => new TextObject("{=!}Doctrines").ToString();

        [DataSourceProperty] public string AspectsText => new TextObject("{=!}Aspects").ToString();

        [DataSourceProperty] public string Name => religion.Faith.GetFaithName().ToString();

        [DataSourceProperty] public string Description => religion.Faith.GetFaithDescription().ToString();

        [DataSourceProperty] public string GroupName => religion.Faith.FaithGroup.Name.ToString();

        [DataSourceProperty] public string GroupDescription => religion.Faith.FaithGroup.Description.ToString();


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

            int selectedIndex = 0;
            Selector = new SelectorVM<ReligionSelectorItemVM>(0, null);
            int i = 0;
            foreach (var religion in BannerKingsConfig.Instance.ReligionsManager.GetReligions())
            {
                var item = new ReligionSelectorItemVM(religion, religion.Faith != this.religion?.Faith);
                if (religion.Faith == this.religion.Faith) selectedIndex = i;
                selector.AddItem(item);
                i++;
            }
            Selector.SelectedIndex = selectedIndex;
            Selector.SetOnChangeAction(delegate (SelectorVM<ReligionSelectorItemVM> obj)
            {
                religion = obj.SelectedItem.Religion;
                RefreshValues();
            });

            foreach (var clgm in religion.Clergy.Values)
            {
                Clergymen.Add(new ReligionMemberVM(clgm, null));
            }

            foreach (var hero in BannerKingsConfig.Instance.ReligionsManager.GetFaithfulHeroes(religion))
            {
                Faithful.Add(new ReligionMemberVM(hero, null));
            }

            foreach (var pair in religion.Faith.Traits)
            {
                Virtues.Add(new BKTraitItemVM(pair.Key, pair.Value));
            }

            foreach (var docString in religion.Doctrines)
            {
                var doctrine = DefaultDoctrines.Instance.GetById(docString);
                if (doctrine != null)
                {
                    Doctrines.Add(new ReligionElementVM(doctrine.Name, doctrine.Effects, doctrine.Description));
                }
            }

            foreach (var divinity in religion.Faith.GetSecondaryDivinities())
            {
                SecondaryDivinities.Add(new ReligionElementVM(divinity.SecondaryTitle, divinity.Name,
                    divinity.Description, divinity.Effects));
            }

            Aspects.Add(new ReligionElementVM(new TextObject("{=!}Leadership"), religion.Leadership.GetName(),
                religion.Leadership.GetHint()));
            Aspects.Add(new ReligionElementVM(religion.Faith.GetMainDivinitiesDescription(),
                religion.Faith.GetMainDivinity().Name, religion.Faith.GetMainDivinity().Description));
            Aspects.Add(new ReligionElementVM(new TextObject("{=!}Faith Group"), religion.Faith.FaithGroup.Name,
                religion.Faith.FaithGroup.Description));
            Aspects.Add(new ReligionElementVM(new TextObject("{=!}Faith"), UIHelper.GetFaithTypeName(religion.Faith),
                UIHelper.GetFaithTypeDescription(religion.Faith)));
            Aspects.Add(new ReligionElementVM(new TextObject("{=!}Culture"), religion.MainCulture.Name,
                new TextObject("{=!}The main culture associated with this faith.")));
        }
    }
}