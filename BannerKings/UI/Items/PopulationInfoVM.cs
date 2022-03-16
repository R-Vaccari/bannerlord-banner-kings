using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings
{
    namespace UI
    {
        public class PopulationInfoVM : ViewModel
        {
            private string _name, _count;
            private HintViewModel _hint { get; set; }

            public PopulationInfoVM(string name, int count, string hintText) 
            {
                _name = name;
                _count = count.ToString();
                this.Hint = new HintViewModel(new TextObject(hintText));
            }


            [DataSourceProperty]
            public string Name
            {
                get => _name;
                set
                {
                    if (value != _name)
                    {
                        _name = value;
                        base.OnPropertyChangedWithValue(value, "Name");
                    }
                }
            }

            [DataSourceProperty]
            public string Count
            {
                get => _count;
                set
                {
                    if (value != _count)
                    {
                        _count = value;
                        base.OnPropertyChangedWithValue(value, "Count");
                    }
                }
            }

            [DataSourceProperty]
            public HintViewModel Hint
            {
                get => _hint;
                set
                {
                    if (value != _hint)
                    {
                        _hint = value;
                        base.OnPropertyChangedWithValue(value, "Hint");
                    }
                }
            }
        }
    }
}
