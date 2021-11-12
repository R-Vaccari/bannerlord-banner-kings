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
            private Settlement settlement;

            public PopulationVM(Settlement settlement)
            {
                this.settlement = settlement;
                _popInfo = new MBBindingList<PopulationInfoVM>();
            }

            public override void RefreshValues()
            {
                base.RefreshValues();
                PopInfo.Clear();
                PopulationData data = Population.GetPopData(Settlement.CurrentSettlement);
                if (data != null && data.Classes != null)
                {
                    data.Classes.ForEach(popClass => PopInfo.Add(new PopulationInfoVM(
                        popClass.type.ToString(), popClass.count)
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
