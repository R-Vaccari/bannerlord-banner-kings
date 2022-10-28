using BannerKings.Managers.Populations;
using BannerKings.Managers.Populations.Estates;
using BannerKings.UI.Items;
using BannerKings.UI.Items.UI;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu.TownManagement;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Core.ViewModelCollection.Selector;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;
using static BannerKings.Managers.PopulationManager;
using static BannerKings.Managers.Populations.Estates.Estate;

namespace BannerKings.UI.Estates
{
    internal class EstateVM : BannerKingsViewModel
    {
        private MBBindingList<TownManagementDescriptionItemVM> mainInfo;
        private MBBindingList<MBBindingList<InformationElement>> extraInfos;
        private ImageIdentifierVM imageIdentifier;
        private SelectorVM<BKItemVM> taskSelector;

        public EstateVM(Estate estate, PopulationData data) : base(data, true)
        {
            Estate = estate;
            LandInfo = new MBBindingList<InformationElement>();
            WorkforceInfo = new MBBindingList<InformationElement>();
            StatsInfo = new MBBindingList<InformationElement>();
            MainInfo = new MBBindingList<TownManagementDescriptionItemVM>();
            ExtraInfos = new MBBindingList<MBBindingList<InformationElement>>();
            if (!IsDisabled) 
            {
                ImageIdentifier = new ImageIdentifierVM(new ImageIdentifier(CampaignUIHelper.GetCharacterCode(estate.Owner.CharacterObject)));
            }

            RefreshValues();
        }

        [DataSourceProperty]
        public bool IsDisabled => Estate.IsDisabled;

        [DataSourceProperty]
        public bool IsEnabled => !Estate.IsDisabled;

        [DataSourceProperty]
        public bool PlayerOwned => Estate.Owner == Hero.MainHero && IsEnabled;

        public Estate Estate { get; private set; }

        public override void RefreshValues()
        {
            base.RefreshValues();
            LandInfo.Clear();
            WorkforceInfo.Clear();
            StatsInfo.Clear();
            MainInfo.Clear();
            ExtraInfos.Clear();

            MainInfo.Add(new TownManagementDescriptionItemVM(new TextObject("{=!}Population:"), 
                Estate.Population, 
                0,
                TownManagementDescriptionItemVM.DescriptionType.Loyalty));

            var income = Estate.Income;
            MainInfo.Add(new TownManagementDescriptionItemVM(new TextObject("{=!}Income:"),
               (int)income.ResultNumber,
               0,
               TownManagementDescriptionItemVM.DescriptionType.Gold,
               new BasicTooltipViewModel(() => income.GetExplanations())));

            var production = Estate.Production;
            MainInfo.Add(new TownManagementDescriptionItemVM(new TextObject("{=!}Production:"),
               (int)production.ResultNumber,
               0,
               TownManagementDescriptionItemVM.DescriptionType.Production,
               new BasicTooltipViewModel(() => production.GetExplanations())));


            var acreage = Estate.AcreageGrowth;
            MainInfo.Add(new TownManagementDescriptionItemVM(new TextObject("{=!}Acreage:"),
               (int)Estate.Acreage,
               (int)acreage.ResultNumber,
               TownManagementDescriptionItemVM.DescriptionType.Prosperity,
               new BasicTooltipViewModel(() => acreage.GetExplanations())));


            /*var value = Estate.EstateValue;
            MainInfo.Add(new TownManagementDescriptionItemVM(new TextObject("{=!}Estate Value:"),
               (int)value.ResultNumber,
               0,
               TownManagementDescriptionItemVM.DescriptionType.Gold,
               new BasicTooltipViewModel(() => value.GetExplanations())));*/


            if (IsEnabled)
            {
                TaskSelector = new SelectorVM<BKItemVM>(0, OnTaskChange);
                TaskSelector.AddItem(new BKItemVM(EstateTask.Prodution, true, "",
                    GameTexts.FindText("str_bk_estate_task", EstateTask.Prodution.ToString())));

                TaskSelector.AddItem(new BKItemVM(EstateTask.Land_Expansion, true, "",
                    GameTexts.FindText("str_bk_estate_task", EstateTask.Land_Expansion.ToString())));

                TaskSelector.SelectedIndex = (int)Estate.Task;
                TaskSelector.SetOnChangeAction(OnTaskChange);



                LandInfo.Add(new InformationElement(new TextObject("{=!}Farmland:").ToString(),
                   new TextObject("{=!}{ACRES} acres").SetTextVariable("ACRES", Estate.Farmland.ToString("0.00")).ToString(),
                   new TextObject("{=ABrCGWep}Acres in this region used as farmland, the main source of food in most places")
                    .ToString()));

                LandInfo.Add(new InformationElement(new TextObject("{=RsRkc9dF}Pastureland:").ToString(),
                   new TextObject("{=!}{ACRES} acres").SetTextVariable("ACRES", Estate.Pastureland.ToString("0.00")).ToString(),
                   new TextObject("{=864UHkZw}Acres in this region used as pastureland, to raise cattle and other animals. These output meat and animal products such as butter and cheese")
                    .ToString()));

                LandInfo.Add(new InformationElement(new TextObject("{=bwEtOiYF}Woodland:").ToString(),
                   new TextObject("{=!}{ACRES} acres").SetTextVariable("ACRES", Estate.Woodland.ToString("0.00")).ToString(), 
                   new TextObject("{=MJYam3iu}Acres in this region used as woodland, kept for hunting, foraging of berries and materials like wood")
                    .ToString()));

                ExtraInfos.Add(LandInfo);


                WorkforceInfo.Add(new InformationElement(new TextObject("{=!}Goods Production:").ToString(),
                    new TextObject("{=!}{POINTS} (Daily)")
                    .SetTextVariable("POINTS", production.ResultNumber.ToString("0.00"))
                    .ToString(),
                    new TextObject("{=ez3NzFgO}{TEXT}\n{EXPLANATIONS}")
                        .SetTextVariable("TEXT",
                            new TextObject("{=!}Sum of goods produced on a daily basis, including all the types produced here."))
                        .SetTextVariable("EXPLANATIONS", production.GetExplanations())
                        .ToString()));

                WorkforceInfo.Add(new InformationElement(new TextObject("{=p7yrSOcC}Available Workforce:").ToString(),
                    Estate.AvailableWorkForce.ToString(),
                    new TextObject("{=1mJgkKHB}The amount of productive workers in this region, able to work the land").ToString()));

                WorkforceInfo.Add(new InformationElement(new TextObject("{=vaT0rnKq}Workforce Saturation:").ToString(),
                    FormatValue(Estate.WorkforceSaturation),
                    new TextObject("{=1KB6Hbpm}Represents how many workers there are in correlation to the amount needed to fully utilize the acreage. Saturation over 100% indicates more workers than the land needs, while under 100% means not all acres are producing output")
                    .ToString()));

                ExtraInfos.Add(WorkforceInfo);


                StatsInfo.Add(new InformationElement(GameTexts.FindText("str_clan_finance_total_income") + ':'.ToString(),
                    income.ResultNumber.ToString("0"),
                    income.GetExplanations()));

                StatsInfo.Add(new InformationElement(GameTexts.FindText("str_total_influence").ToString(),
                    FormatFloatGain(Estate.Influence),
                    new TextObject("{=!}Influence from local nobles.").ToString()));

                var value = Estate.EstateValue;
                StatsInfo.Add(new InformationElement(new TextObject("{=!}Estate Value:").ToString(),
                    value.ResultNumber.ToString("0"),
                    value.GetExplanations()));


                ExtraInfos.Add(StatsInfo);
            }
        }

        private void OnTaskChange(SelectorVM<BKItemVM> obj)
        {
            if (obj.SelectedItem != null)
            {
                var vm = obj.GetCurrentItem();
                Estate.ChangeTask((EstateTask)vm.value);
            }
        }

        public MBBindingList<InformationElement> LandInfo { get; set; }
        public MBBindingList<InformationElement> WorkforceInfo { get; set; }
        public MBBindingList<InformationElement> StatsInfo { get; set; }


        [DataSourceProperty]
        public SelectorVM<BKItemVM> TaskSelector
        {
            get => taskSelector;
            set
            {
                if (value != taskSelector)
                {
                    taskSelector = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<MBBindingList<InformationElement>> ExtraInfos
        {
            get => extraInfos;
            set
            {
                if (value != extraInfos)
                {
                    extraInfos = value;
                    OnPropertyChanged("ExtraInfos");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<TownManagementDescriptionItemVM> MainInfo
        {
            get => mainInfo;
            set
            {
                if (value != mainInfo)
                {
                    mainInfo = value;
                    OnPropertyChanged("MainInfo");
                }
            }
        }

        [DataSourceProperty]
        public ImageIdentifierVM ImageIdentifier
        {
            get => imageIdentifier;
            set
            {
                imageIdentifier = value;
                OnPropertyChanged("ImageIdentifier");
            }
        }


        [DataSourceProperty]
        public string NameText => IsDisabled ? new TextObject("{=!}Vacant Estate").ToString() : Estate.Name.ToString();


        [DataSourceProperty]
        public string AcreageText => new TextObject("{=!}{ACRES} acres")
            .SetTextVariable("ACRES", Estate.Farmland + Estate.Pastureland + Estate.Woodland)
            .ToString();

        [DataSourceProperty]
        public string FarmlandText => new TextObject("{=!}{ACRES} acres").SetTextVariable("ACRES", Estate.Farmland).ToString();

        [DataSourceProperty]
        public string PasturelandText => new TextObject("{=!}{ACRES} acres").SetTextVariable("ACRES", Estate.Pastureland).ToString();

        [DataSourceProperty]
        public string WoodlandText => new TextObject("{=!}{ACRES} acres").SetTextVariable("ACRES", Estate.Woodland).ToString();


        [DataSourceProperty]
        public string PopulationText => new TextObject("{=!}{QUANTITY} people")
         .SetTextVariable("QUANTITY", Estate.Nobles + Estate.Craftsmen + Estate.Serfs + Estate.Slaves)
         .ToString();

        [DataSourceProperty]
        public string WorkforceText => new TextObject("{=!}{QUANTITY} workforce")
        .SetTextVariable("QUANTITY", Estate.Serfs + Estate.Slaves)
        .ToString();



        [DataSourceProperty]
        public string NoblesText => new TextObject("{=!}{QUANTITY} {POPULATION}")
            .SetTextVariable("QUANTITY", Estate.Nobles)
            .SetTextVariable("POPULATION", Utils.Helpers.GetClassName(PopType.Nobles, Estate.EstatesData.Settlement.Culture))
            .ToString();

        [DataSourceProperty]
        public string CraftsmenText => new TextObject("{=!}{QUANTITY} {POPULATION}")
           .SetTextVariable("QUANTITY", Estate.Craftsmen)
           .SetTextVariable("POPULATION", Utils.Helpers.GetClassName(PopType.Craftsmen, Estate.EstatesData.Settlement.Culture))
           .ToString();

        [DataSourceProperty]
        public string SerfsText => new TextObject("{=!}{QUANTITY} {POPULATION}")
           .SetTextVariable("QUANTITY", Estate.Serfs)
           .SetTextVariable("POPULATION", Utils.Helpers.GetClassName(PopType.Serfs, Estate.EstatesData.Settlement.Culture))
           .ToString();

        [DataSourceProperty]
        public string SlavesText => new TextObject("{=!}{QUANTITY} {POPULATION}")
           .SetTextVariable("QUANTITY", Estate.Slaves)
           .SetTextVariable("POPULATION", Utils.Helpers.GetClassName(PopType.Slaves, Estate.EstatesData.Settlement.Culture))
           .ToString();


    }
}
