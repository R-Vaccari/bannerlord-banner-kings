﻿using System;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.Items
{
    // Token: 0x02000020 RID: 32
    public class DecisionElement : ViewModel
    {
        private Action<bool> booleanAction;

        private bool booleanValue;
        public bool show, enabled;
        public int OptionTypeID { get; set; }
        private HintViewModel hint { get; set; }
        public string description { get; set; }
        public bool IsDiscrete { get; set; }

        [DataSourceProperty] public string ButtonName { get; set; }

        public Action OnPressAction { get; set; }

        public bool OptionValueAsBoolean
        {
            get => booleanValue;
            set
            {
                var flag = value != booleanValue;
                if (flag)
                {
                    booleanValue = value;
                    OnPropertyChanged();
                    booleanAction(value);
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

        [DataSourceProperty]
        public bool Enabled
        {
            get => enabled;
            set
            {
                if (value != enabled)
                {
                    enabled = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public bool Show
        {
            get => show;
            set
            {
                if (value != show)
                {
                    show = value;
                    OnPropertyChangedWithValue(value);
                }
            }
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


        public DecisionElement SetAsBooleanOption(string desc, bool initialValue, Action<bool> onChange,
            TextObject hintText)
        {
            Hint = new HintViewModel(hintText);
            OptionTypeID = 1;
            Description = desc;
            booleanValue = initialValue;
            booleanAction = onChange;
            Show = true;
            Enabled = true;
            return this;
        }

        public DecisionElement SetAsButtonOption(string buttonName, Action onPress, TextObject hintText = null)
        {
            OptionTypeID = 3;
            ButtonName = buttonName;
            OnPressAction = onPress;
            Hint = new HintViewModel(hintText);
            Show = true;
            Enabled = true;
            return this;
        }


        public void OnPress()
        {
            OnPressAction?.Invoke();
        }
    }
}