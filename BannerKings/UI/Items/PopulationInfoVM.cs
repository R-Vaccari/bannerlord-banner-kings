using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.Items
{
    namespace UI
    {
        public class PopulationInfoVM : ViewModel
        {
            private string _name;
            private string _count;

            public PopulationInfoVM(string name, int count, string hintText)
            {
                _name = name;
                _count = $"{count:n0}";
                Hint = new HintViewModel(new TextObject("{=!}" + hintText));
            }

            private HintViewModel _hint { get; set; }


            [DataSourceProperty]
            public string Name
            {
                get => _name;
                set
                {
                    if (value != _name)
                    {
                        _name = value;
                        OnPropertyChangedWithValue(value);
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
                        OnPropertyChangedWithValue(value);
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
                        OnPropertyChangedWithValue(value);
                    }
                }
            }
        }
    }
}