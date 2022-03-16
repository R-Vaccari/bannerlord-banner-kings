using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings
{
    namespace UI
    {
        public class InformationElement : ViewModel
        {
            private string _description, _value;
            private HintViewModel _hint { get; set; }

            public InformationElement(string description, string value, string hintText) 
            {
                _description = description;
                _value = value;
                this.Hint = new HintViewModel(new TextObject(hintText));
            }


            [DataSourceProperty]
            public string Description
            {
                get => _description;
                set
                {
                    if (value != _description)
                    {
                        _description = value;
                        base.OnPropertyChangedWithValue(value, "Name");
                    }
                }
            }

            [DataSourceProperty]
            public string Value
            {
                get => _value;
                set
                {
                    if (value != _value)
                    {
                        _value = value;
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
