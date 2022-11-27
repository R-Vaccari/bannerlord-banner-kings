using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.Items
{
    namespace UI
    {
        public class InformationElement : ViewModel
        {
            private HintViewModel hint;
            private string description, value;

            public InformationElement(string description, string value, string hintText)
            {
                Description = description;
                Value = value;
                Hint = new HintViewModel(new TextObject("{=!}" + hintText));
            }

            public InformationElement(TextObject description, TextObject value, TextObject hintText)
            {
                Description = description.ToString();
                Value = value.ToString();
                Hint = new HintViewModel(hintText);
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
                        OnPropertyChangedWithValue(value);
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
                        OnPropertyChangedWithValue(value);
                    }
                }
            }

            [DataSourceProperty]
            public HintViewModel Hint
            {
                get => hint;
                set
                {
                    if (value != hint)
                    {
                        hint = value;
                        OnPropertyChangedWithValue(value);
                    }
                }
            }
        }
    }
}