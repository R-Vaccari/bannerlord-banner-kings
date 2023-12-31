using BannerKings.Managers.Populations;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.Estates
{
    internal class EstatesVM : BannerKingsViewModel
    {
        private MBBindingList<EstateVM> estatesList;
        public EstatesVM(PopulationData data) : base(data, true)
        {
            EstatesList = new MBBindingList<EstateVM>();
        }

        [DataSourceProperty]
        public string EstatesText => new TextObject("{=DHG67WAy}{SETTLEMENT} Estates")
            .SetTextVariable("SETTLEMENT", data.Settlement.Name).ToString();

        [DataSourceProperty]
        public MBBindingList<EstateVM> EstatesList
        {
            get => estatesList;
            set
            {
                estatesList = value;
                OnPropertyChanged("EstatesList");
            }
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            var estateData = data.EstateData;
            foreach (var estate in estateData.Estates)
            {
                EstatesList.Add(new EstateVM(estate, data));
            }
        }
    }
}
