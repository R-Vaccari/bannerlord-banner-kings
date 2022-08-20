using BannerKings.Populations;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings
{
    namespace UI
    {
        public class CultureElementVM : BannerKingsViewModel
        {
            private CultureDataClass dataClass;

            public CultureElementVM(PopulationData data, CultureDataClass dataClass) : base(data, false)
            {
                this.dataClass = dataClass;
            }


            [DataSourceProperty]
            public string Name => dataClass.Culture.Name.ToString();

            [DataSourceProperty]
            public string Acceptance => FormatValue(dataClass.Acceptance);

            [DataSourceProperty]
            public string Assimilation => FormatValue(dataClass.Assimilation);

            [DataSourceProperty]
            public HintViewModel Hint => new HintViewModel(new TextObject());
        }
    }
}
