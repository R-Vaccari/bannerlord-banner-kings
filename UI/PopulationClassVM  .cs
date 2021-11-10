using TaleWorlds.Library;

namespace Populations
{
    namespace UI
    {
        public class PopulationClassVM : ViewModel
        {
            private string _name;
            private int _count;
            private string _percentage;

            public PopulationClassVM(string name, int count, float percentage) 
            {
                _name = name;
                _count = count;
                _percentage = FormatPercentage(percentage);
            }

            private string FormatPercentage(float percentage) => (percentage * 100).ToString() + '%';

            [DataSourceProperty]
            public string Name
            {
                get
                {
                    return _name;
                }
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
            public int Count
            {
                get
                {
                    return _count;
                }
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
                get
                {
                    return _percentage;
                }
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
