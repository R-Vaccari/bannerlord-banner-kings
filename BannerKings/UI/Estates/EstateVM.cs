using BannerKings.Managers.Populations.Estates;
using BannerKings.UI.Items.UI;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using static BannerKings.Managers.PopulationManager;

namespace BannerKings.UI.Estates
{
    internal class EstateVM : ViewModel
    {
        private ImageIdentifierVM imageIdentifier;
        private MBBindingList<InformationElement> generalInfo;
        public EstateVM(Estate estate)
        {
            Estate = estate;
            generalInfo = new MBBindingList<InformationElement>();
            ImageIdentifier = new ImageIdentifierVM(new ImageIdentifier(CampaignUIHelper.GetCharacterCode(estate.Owner.CharacterObject)));
            RefreshValues();
        }

        public Estate Estate { get; private set; }

        public override void RefreshValues()
        {
            base.RefreshValues();
            GeneralInfo.Clear();
            GeneralInfo.Add(new InformationElement(new TextObject("{=!}Total Population:").ToString(),
                PopulationText, ""));

            var incomeExplained = Estate.Income;
            GeneralInfo.Add(new InformationElement(new TextObject("{=!}Income:").ToString(),
               incomeExplained.ResultNumber.ToString("0"), incomeExplained.GetExplanations()));

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
