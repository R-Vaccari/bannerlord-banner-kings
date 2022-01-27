using System;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI
{
	// Token: 0x02000020 RID: 32
	public class PopulationOptionVM : ViewModel
	{
		public int OptionTypeID { get; set; }
		private HintViewModel _hint { get; set; }

		public string _description { get; set; }
		public bool IsDiscrete { get; set; }

		[DataSourceProperty] 
		public string ButtonName { get; set; }

		public Action OnPressAction { get; set; }


		public bool OptionValueAsBoolean
		{
			get
			{
				return this._optionBooleanValue;
			}
			set
			{
				bool flag = value != this._optionBooleanValue;
				if (flag)
				{
					this._optionBooleanValue = value;
					base.OnPropertyChanged("OptionValueAsBoolean");
					this._onBooleanChangeAction(value);
				}
			}
		}


		public PopulationOptionVM SetAsBooleanOption(string desc, bool initialValue, Action<bool> onChange, TextObject hintText)
		{
			try
			{
				this.Hint = new HintViewModel(hintText);
				this.OptionTypeID = 1;
				this.Description = desc;
				this._optionBooleanValue = initialValue;
				this._onBooleanChangeAction = onChange;
			}
			catch (Exception ex)
			{
			}
			return this;
		}

		public PopulationOptionVM SetAsButtonOption(string buttonName, Action onPress, TextObject hintText = null)
		{
			try
			{
				this.OptionTypeID = 3;
				this.ButtonName = buttonName;
				this.OnPressAction = onPress;
				this.Hint = new HintViewModel(hintText);
			}
			catch (Exception ex)
			{
			}
			return this;
		}


		public void OnPress()
		{
			bool flag = this.OnPressAction != null;
			if (flag)
			{
				this.OnPressAction();
			}
		}

		// Token: 0x060001CF RID: 463 RVA: 0x0000E1BC File Offset: 0x0000C3BC
		public PopulationOptionVM SetAsTitle(string title, TextObject hintText = null)
		{
			this.OptionTypeID = 0;
			this.Description = title;
			return this;
		}

		private bool _optionBooleanValue = false;
		private Action<bool> _onBooleanChangeAction;

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
	}
}
