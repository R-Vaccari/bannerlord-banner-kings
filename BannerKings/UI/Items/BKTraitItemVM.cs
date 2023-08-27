using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.Items
{
    public class BKTraitItemVM : ViewModel
    {
        private readonly TraitObject _traitObj;
        private HintViewModel _hint;
        private string _traitId, descriptionText;
        private int _value;

        public BKTraitItemVM(TraitObject traitObj, bool positive)
        {
            _traitObj = traitObj;
            TraitId = traitObj.StringId;
            Value = positive ? 2 : -2;
            Hint = new HintViewModel(traitObj.Description);
            descriptionText = new TextObject("{=!}{TRAIT} is considered virtuous")
                .SetTextVariable("TRAIT", GameTexts.FindText("str_trait_name_" + traitObj.StringId.ToLower(),
                        (Value + MathF.Abs(traitObj.MinValue)).ToString()))
                .ToString();
        }

        [DataSourceProperty] public string Name => _traitObj.Name.ToString();

        [DataSourceProperty]
        public string DescriptionText
        {
            get => descriptionText;
            set
            {
                if (value != descriptionText)
                {
                    descriptionText = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

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