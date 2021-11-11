using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using static Populations.Population;

namespace Populations
{
    namespace UI
    {
        public class PopulationVM : ViewModel
        {

            private MBBindingList<PopulationInfoVM> _popInfo;

            public PopulationVM(Settlement settlement)
            {
                _popInfo = new MBBindingList<PopulationInfoVM>();
                PopulationData data = Population.GetPopData(settlement);
                if (data != null && data.Classes != null)
                {
                    data.Classes.ForEach(popClass => PopInfo.Add(new PopulationInfoVM(
                        popClass.type.ToString(), popClass.count, data.GetCurrentTypeFraction(popClass.type))
                        ));
                }
            }
            
            [DataSourceProperty]
            public MBBindingList<PopulationInfoVM> PopInfo
            {
                get => _popInfo;
                set
                {
                    if (value != _popInfo)
                    {
                        _popInfo = value;
                        base.OnPropertyChangedWithValue(value, "PopInfo");
                    }
                }
            }

            public void ExecuteClose()
            {
                UIManager.instance.CloseUI();
            }

        }
    }
    
}
