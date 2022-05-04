using System;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI
{
	// Token: 0x02000020 RID: 32
	public class DecisionElement : ViewModel
	{
		public int OptionTypeID { get; set; }
		private HintViewModel hint { get; set; }
		public string description { get; set; }
		public bool IsDiscrete { get; set; }

		[DataSourceProperty] 
		public string ButtonName { get; set; }

		public Action OnPressAction { get; set; }

		private bool booleanValue;
		private Action<bool> booleanAction;
		public bool show, enabled;

		public bool OptionValueAsBoolean
		{
			get => booleanValue;
			set
			{
				bool flag = value != booleanValue;
				if (flag)
				{
					booleanValue = value;
					OnPropertyChanged();
					booleanAction(value);
				}
			}
		}


		public DecisionElement SetAsBooleanOption(string desc, bool initialValue, Action<bool> onChange, TextObject hintText)
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
			if (OnPressAction != null) OnPressAction();
		}

		public DecisionElement SetAsTitle(string title, TextObject hintText = null)
		{
			OptionTypeID = 0;
			Description = title;
			return this;
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
	}
}
