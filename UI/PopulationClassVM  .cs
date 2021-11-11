using TaleWorlds.Library;

namespace Populations
{
    namespace UI
    {
        public class PopulationInfoVM : ViewModel
        {
            private string _name, _count, _percentage;

            public PopulationInfoVM(string name, int count, float percentage) 
            {
                _name = name;
                _count = count.ToString();
                _percentage = FormatPercentage(percentage);
            }

            private string FormatPercentage(float percentage) => (percentage * 100).ToString() + '%';

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
            public string Percentage
            {
                get => _percentage;
                set
                {
                    if (value != _percentage)
                    {
                        _percentage = value;
                        base.OnPropertyChangedWithValue(value, "Percentage");
                    }
                }
            }

        }
    }
}
