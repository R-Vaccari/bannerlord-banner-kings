using System;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Engine.Options;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using static Populations.PolicyManager;

namespace Populations
{
    namespace UI
    {
        public class PopulationPolicyVM : ViewModel
        {
            private HintViewModel _hint;
            private string _description;

            public PopulationPolicyVM()
            {

            }

            public PopulationPolicyVM SetAction(PolicyElement element, Action<bool> onChange)
            {
                _description = element.description;
                _hint = new HintViewModel(new TextObject(element.hint), null);
                return this;
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
                        base.OnPropertyChangedWithValue(value, "Description");
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
