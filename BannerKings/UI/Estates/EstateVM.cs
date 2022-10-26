using BannerKings.Managers.Populations.Estates;
using BannerKings.UI.Items.UI;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu.TownManagement;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using static BannerKings.Managers.PopulationManager;

namespace BannerKings.UI.Estates
{
    internal class EstateVM : ViewModel
    {
        private MBBindingList<TownManagementDescriptionItemVM> mainInfo;
        private ImageIdentifierVM imageIdentifier;
        private MBBindingList<InformationElement> generalInfo;
        public EstateVM(Estate estate)
        {
            Estate = estate;
            generalInfo = new MBBindingList<InformationElement>();
            MainInfo = new MBBindingList<TownManagementDescriptionItemVM>();
            ImageIdentifier = new ImageIdentifierVM(new ImageIdentifier(CampaignUIHelper.GetCharacterCode(estate.Owner.CharacterObject)));
            RefreshValues();
        }

        public Estate Estate { get; private set; }

        public override void RefreshValues()
        {
            base.RefreshValues();
            GeneralInfo.Clear();
            MainInfo.Clear();

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


            var acreage = Estate.AcreageGrowth;
            MainInfo.Add(new TownManagementDescriptionItemVM(new TextObject("{=!}Acreage:"),
               (int)Estate.Acreage,
               (int)acreage.ResultNumber,
               TownManagementDescriptionItemVM.DescriptionType.Prosperity,
               new BasicTooltipViewModel(() => acreage.GetExplanations())));


            var value = Estate.EstateValue;
            MainInfo.Add(new TownManagementDescriptionItemVM(new TextObject("{=!}Estate Value:"),
               (int)value.ResultNumber,
               0,
               TownManagementDescriptionItemVM.DescriptionType.Gold,
               new BasicTooltipViewModel(() => value.GetExplanations())));


            GeneralInfo.Add(new InformationElement(new TextObject("{=!}Total Population:").ToString(),
                PopulationText, ""));

            var incomeExplained = Estate.Income;
            GeneralInfo.Add(new InformationElement(new TextObject("{=!}Income:").ToString(),
               incomeExplained.ResultNumber.ToString("0"), incomeExplained.GetExplanations()));

        }


        [DataSourceProperty]
        public MBBindingList<TownManagementDescriptionItemVM> MainInfo
        {
            get => mainInfo;
            set
            {
                if (value != this.mainInfo)
                {
                    this.mainInfo = value;
                    base.OnPropertyChanged("MainInfo");
                }
            }
        }


        [DataSourceProperty]
        public MBBindingList<InformationElement> GeneralInfo
        {
            get => generalInfo;
            set
            {
                generalInfo = value;
                OnPropertyChanged("GeneralInfo");
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
        public string NameText => Estate.Name.ToString();


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
