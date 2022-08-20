using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings
{
    namespace UI
    {
        public class InformationElement : ViewModel
        {
            private string description, value;
            private HintViewModel _hint;

            public InformationElement(string description, string value, string hintText) 
            {
                this.description = description;
                this.value = value;
                Hint = new HintViewModel(new TextObject(hintText));
            }


            [DataSourceProperty]
            public string Description
            {
                get => description;
                set
                {
                    if (value != description)
                    {
                        description = value;
                        OnPropertyChangedWithValue(value, "Description");
                    }
                }
            }

            [DataSourceProperty]
            public string Value
            {
                get => value;
                set
                {
                    if (value != this.value)
                    {
                        this.value = value;
                        OnPropertyChangedWithValue(value, "Value");
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
