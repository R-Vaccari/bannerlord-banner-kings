using BannerKings.Managers.Institutions.Religions;
using BannerKings.Managers.Institutions.Religions.Doctrines;
using BannerKings.Managers.Populations;
using BannerKings.UI.Items;
using BannerKings.UI.Items.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Core.ViewModelCollection.Selector;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.VanillaTabs.Character.Religion
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
        private ImageIdentifierVM banner;
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
                currentReligion = BannerKingsConfig.Instance.ReligionsManager.GetReligions()
                    .GetRandomElementWithPredicate(x => x.Faith.Active);
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

            if (currentReligion == null) return;

            Banner = new ImageIdentifierVM(BannerCode.CreateFrom(currentReligion.Faith.GetBanner()), true);
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
                if (religion.Faith.Active)
                {
                    var item = new ReligionSelectorItemVM(religion, religion.Faith != currentReligion?.Faith);
                    if (religion.Faith == currentReligion.Faith) selectedIndex = i;
                    selector.AddItem(item);
                    i++;
                }
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

            Dictionary<Settlement, Divinity> sites = new Dictionary<Settlement, Divinity>(SecondaryDivinities.Count);
            if (mainDivinity.Shrine != null)
            {
                sites.Add(mainDivinity.Shrine, mainDivinity);
            }

            foreach (var divinity in currentReligion.Faith.GetSecondaryDivinities())
            {
                SecondaryDivinities.Add(new ReligionElementVM(divinity.SecondaryTitle, divinity.Name,
                    new TextObject("{=77isPS24}{EFFECTS}\nPiety cost: {COST}")
                    .SetTextVariable("EFFECTS", divinity.Description)
                    .SetTextVariable("COST", divinity.BlessingCost(hero)), divinity.Effects));

                if (divinity.Shrine != null)
                {
                    sites.Add(divinity.Shrine, divinity);
                }
            }

            //Aspects.Add(new ReligionElementVM(currentReligion.Faith.GetMainDivinitiesDescription(),
            //    currentReligion.Faith.GetMainDivinity().Name, currentReligion.Faith.GetMainDivinity().Description));
            Aspects.Add(new ReligionElementVM(new TextObject("{=OKw2P9m1}Faith Group"), 
                currentReligion.Faith.FaithGroup.Name,
                currentReligion.Faith.FaithGroup.Description));

            Aspects.Add(new ReligionElementVM(new TextObject("{=gL6y1Pgr}Faith Seat"),
                currentReligion.Faith.FaithSeat != null ? currentReligion.Faith.FaithSeat.Name : new TextObject("{=!}None"),
                new TextObject("{=ZvpbQfPP}The Faith Seat is the most religiously important fief within the faith. When the Seat is not held by a member of the faith, it loses a great deal of fervor. The holder of the Seat is given extra influence limit and piety according to the stability of the Seat, and thus is encouraged to give it good management.")));

            Hero leader = currentReligion.Faith.FaithGroup.Leader;
            TextObject leaderName = leader != null ? leader.Name : new TextObject("{=!}None");
            if (currentReligion.Faith.FaithGroup.ShouldHaveLeader) leaderName = new TextObject("{=!}Not Possible");
            Aspects.Add(new ReligionElementVM(new TextObject("{=!}Head of Faith"),
                leaderName,
                currentReligion.Faith.FaithGroup.Explanation));

            var marriage = currentReligion.Faith.MarriageDoctrine;
            Aspects.Add(new ReligionElementVM(new TextObject("{=!}Marriage Doctrine"),
                marriage.Name,
                new TextObject("{=!}{EXPLANATION}{newline}{newline}{newline}Consorts:{newline}{CONSORTS}{newline}{newline}Consanguinity:{newline}{CONSANGUINITY}{newline}{newline}Tolerance:{newline}{TOLERANCE}")
                .SetTextVariable("EXPLANATION", marriage.Description)
                .SetTextVariable("CONSORTS", marriage.ConsortsExplanation)
                .SetTextVariable("CONSANGUINITY", marriage.ConsanguinityExplanation)
                .SetTextVariable("TOLERANCE", marriage.UntoleratedExplanation)
                ));

            Aspects.Add(new ReligionElementVM(new TextObject("{=!}Warfare Doctrine"),
                currentReligion.Faith.WarDoctrine.Name,
                currentReligion.Faith.WarDoctrine.Description));

            //Aspects.Add(new ReligionElementVM(new TextObject("{=OKw2P9m1}Faith"), UIHelper.GetFaithTypeName(currentReligion.Faith),
            //    UIHelper.GetFaithTypeDescription(currentReligion.Faith)));
            Aspects.Add(new ReligionElementVM(new TextObject("{=EjTxnGJp}Culture"), currentReligion.MainCulture.Name,
                new TextObject("{=6NYxLhjH}The main culture associated with this faith.")));

            if (sites.Count > 0)
            {
                Aspects.Add(new ReligionElementVM(new TextObject("{=g0Pc7Awk}Holy Sites"),
                    new TextObject("{=!}" + sites.Count),
                    new TextObject("{=wprHBRFq}Holy sites are the fiefs directly connected to the religion's divinities or cults. Holding such sites is important for religious Fervor. In addition, being blessed by a Divinity in its holy site adds double the blessing duration.{newline}{newline}Sites:{SITES}")
                    .SetTextVariable("SITES", sites.Aggregate("", (current, site) => current + Environment.NewLine +
                        new TextObject("{=s1CgckUZ}{HOLY_SITE}: {DIVINITY}")
                        .SetTextVariable("HOLY_SITE", site.Key.Name)
                        .SetTextVariable("DIVINITY", site.Value.Name)
                        .ToString()))
                ));
            }

            var fervor = BannerKingsConfig.Instance.ReligionModel.CalculateFervor(currentReligion);
            Aspects.Add(new ReligionElementVM(new TextObject("Fervor"),
                new TextObject("{=!}" + FormatValue(fervor.ResultNumber)),
                new TextObject("{=ez3NzFgO}{TEXT}\n{EXPLANATIONS}")
                    .SetTextVariable("TEXT",
                        new TextObject("{=a98ihEMD}The faith's fervor. A faith's fervor makes its populations and heroes harder to convert. In settlements, fervor grealy contributes to the faith's presence. Heroes instead are less likely and/or require more resources to convert. Fervor is based on doctrines, settlements and clans that follow the faith. Additionaly, holding the Faith Seat and the faith's Holy Sites are important factors to fervor."))
                    .SetTextVariable("EXPLANATIONS", fervor.GetExplanations())
                    ));

        }

        [DataSourceProperty]
        public ImageIdentifierVM Banner
        {
            get => banner;
            set
            {
                if (value != banner)
                {
                    banner = value;
                    OnPropertyChangedWithValue(value);
                }
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

        [DataSourceProperty]
        public string Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChangedWithValue(value, "Name");
            }
        }

        [DataSourceProperty]
        public string Description
        {
            get => description;
            set
            {
                description = value;
                OnPropertyChangedWithValue(value, "Description");
            }
        }

        [DataSourceProperty]
        public string GroupName
        {
            get => groupName;
            set
            {
                groupName = value;
                OnPropertyChangedWithValue(value, "GroupName");
            }
        }

        [DataSourceProperty]
        public string GroupDescription
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
    }
}