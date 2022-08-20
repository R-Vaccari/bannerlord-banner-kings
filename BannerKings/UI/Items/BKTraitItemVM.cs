using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;

namespace BannerKings.UI.Items
{
    public class BKTraitItemVM : ViewModel
    {
        private readonly TraitObject _traitObj;
        private HintViewModel _hint;
        private string _traitId;
        private int _value;

        public BKTraitItemVM(TraitObject traitObj, bool positive)
        {
            _traitObj = traitObj;
            TraitId = traitObj.StringId;
            Value = positive ? 2 : -2;
            Hint = new HintViewModel(traitObj.Description);
        }

        [DataSourceProperty] public string Name => _traitObj.Name.ToString();

        [DataSourceProperty]
        public string TraitId
        {
            get => _traitId;
            set
            {
                if (value != _traitId)
                {
                    _traitId = value;
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

        [DataSourceProperty]
        public int Value
        {
            get => _value;
            set
            {
                if (value != _value)
                {
                    _value = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }
    }
}