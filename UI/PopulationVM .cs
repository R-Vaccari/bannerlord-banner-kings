using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using static Populations.Population;

namespace Populations
{
    namespace UI
    {
        public class PopulationVM : ViewModel
        {

            private MBBindingList<PopulationClassVM> _classes;

            public PopulationVM(Settlement settlement)
            {
                PopulationData data = Population.GetPopData(settlement);
                if (data != null)
                {
                    int nobles = data.GetTypeCount(PopType.Nobles);
                    int craftsmen = data.GetTypeCount(PopType.Craftsmen);
                    int serfs = data.GetTypeCount(PopType.Serfs);
                    int slaves = data.GetTypeCount(PopType.Slaves);
                    if (nobles > 0)
                        AddClassVM(PopType.Nobles, nobles, data);
                    if (craftsmen > 0)
                        AddClassVM(PopType.Craftsmen, craftsmen, data);
                    if (serfs > 0)
                        AddClassVM(PopType.Serfs, serfs, data);
                    if (slaves > 0)
                        AddClassVM(PopType.Slaves, slaves, data);

                }
            }

            private void AddClassVM(PopType type, int count, PopulationData data) => Classes.Add(new PopulationClassVM(type.ToString(), count, data.GetCurrentTypeFraction(type)));
            
            [DataSourceProperty]
            public MBBindingList<PopulationClassVM> Classes
            {
                get
                {
                    return this._classes;
                }
                set
                {
                    if (value != this._classes)
                    {
                        this._classes = value;
                        base.OnPropertyChangedWithValue(value, "Classes");
                    }
                }
            }

        }
    }
    
}
