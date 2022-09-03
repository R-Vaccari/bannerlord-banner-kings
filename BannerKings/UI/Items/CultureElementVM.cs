using BannerKings.Managers.Populations;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.Items
{
    namespace UI
    {
        public class CultureElementVM : BannerKingsViewModel
        {
            private readonly CultureDataClass dataClass;

            public CultureElementVM(PopulationData data, CultureDataClass dataClass) : base(data, false)
            {
                this.dataClass = dataClass;
            }


            [DataSourceProperty] public string Name => dataClass.Culture.Name.ToString();

            [DataSourceProperty] public string Acceptance => $"{dataClass.Acceptance:P}";

            [DataSourceProperty] public string Assimilation => $"{dataClass.Assimilation:P}";

            [DataSourceProperty] public HintViewModel Hint => new(new TextObject("{=!}"));
        }
    }
}