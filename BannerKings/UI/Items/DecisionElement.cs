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

		private bool booleanValue = false;
		private Action<bool> booleanAction;
		public bool show, enabled;

		public bool OptionValueAsBoolean
		{
			get => this.booleanValue;
			set
			{
				bool flag = value != this.booleanValue;
				if (flag)
				{
					this.booleanValue = value;
					base.OnPropertyChanged("OptionValueAsBoolean");
					this.booleanAction(value);
				}
			}
		}


		public DecisionElement SetAsBooleanOption(string desc, bool initialValue, Action<bool> onChange, TextObject hintText)
		{
			this.Hint = new HintViewModel(hintText);
			this.OptionTypeID = 1;
			this.Description = desc;
			this.booleanValue = initialValue;
			this.booleanAction = onChange;
			this.Show = true;
			this.Enabled = true;
			return this;
		}

		public DecisionElement SetAsButtonOption(string buttonName, Action onPress, TextObject hintText = null)
		{
			this.OptionTypeID = 3;
			this.ButtonName = buttonName;
			this.OnPressAction = onPress;
			this.Hint = new HintViewModel(hintText);
			this.Show = true;
			this.Enabled = true;
			return this;
		}

		 
		public void OnPress()
		{
			if (this.OnPressAction != null) this.OnPressAction();
		}

		public DecisionElement SetAsTitle(string title, TextObject hintText = null)
		{
			this.OptionTypeID = 0;
			this.Description = title;
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
					base.OnPropertyChangedWithValue(value, "Hint");
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
					base.OnPropertyChangedWithValue(value, "Enabled");
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
					base.OnPropertyChangedWithValue(value, "Show");
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
					base.OnPropertyChangedWithValue(value, "Description");
				}
			}
		}
	}
}
