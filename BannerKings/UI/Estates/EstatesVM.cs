using BannerKings.Managers.Populations;
using BannerKings.Managers.Populations.Estates;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.Estates
{
    internal class EstatesVM : BannerKingsViewModel
    {
        private MBBindingList<EstateVM> estatesList;
        private EstateVM demesneLands;
        public EstatesVM(PopulationData data) : base(data, true)
        {
            EstatesList = new MBBindingList<EstateVM>();
        }

        [DataSourceProperty]
        public string EstatesText => new TextObject("{=!}{SETTLEMENT} Estates")
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


        [DataSourceProperty]
        public EstateVM DemesneLands 
        { 
            get => demesneLands; 
            set
            {
                demesneLands = value;
                OnPropertyChanged("DemesneLands");
            }
        }


        [DataSourceProperty]
        public string DemesneLandsText => new TextObject("{=!}Demesne Lands").ToString();

        public override void RefreshValues()
        {
            base.RefreshValues();

            var landData = data.LandData;
            int nobles = data.GetTypeCount(Managers.PopulationManager.PopType.Nobles);
            int craftsmen = data.GetTypeCount(Managers.PopulationManager.PopType.Craftsmen);
            int serfs = data.GetTypeCount(Managers.PopulationManager.PopType.Serfs);
            int slaves = data.GetTypeCount(Managers.PopulationManager.PopType.Slaves);

            float farmland = landData.Farmland;
            float pastureLand = landData.Pastureland;
            float woodland = landData.Woodland;


            var estateData = data.EstateData;
            foreach (var estate in estateData.Estates)
            {
                farmland -= estate.Farmland;
                pastureLand -= estate.Pastureland;
                woodland -= estate.Woodland;

                EstatesList.Add(new EstateVM(estate, data));
            }

            var demesneLands = new Estate(data.Settlement.Owner, estateData, farmland, pastureLand, woodland, serfs,
                slaves, nobles, craftsmen);
            demesneLands.AddManpower(Managers.PopulationManager.PopType.Serfs, data.MilitaryData.PeasantManpower);
            demesneLands.AddManpower(Managers.PopulationManager.PopType.Nobles, data.MilitaryData.NobleManpower);

            DemesneLands = new EstateVM(demesneLands, data);
        }
    }
}
