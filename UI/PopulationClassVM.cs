using TaleWorlds.Library;

namespace Populations
{
    namespace UI
    {
        public class PopulationInfoVM : ViewModel
        {
            private string _name, _count;

            public PopulationInfoVM(string name, int count) 
            {
                _name = name;
                _count = count.ToString();
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
        }
    }
}
