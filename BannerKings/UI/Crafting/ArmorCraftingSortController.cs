using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.Crafting
{
    public class ArmorCraftingSortController : ViewModel
    {
		public ArmorCraftingSortController()
		{
			this._yieldComparer = new ItemYieldComparer();
			this._typeComparer = new ItemTypeComparer();
			this._nameComparer = new ItemNameComparer();
			this.RefreshValues();
		}

		public override void RefreshValues()
		{
			base.RefreshValues();
			this.SortNameText = new TextObject("{=PDdh1sBj}Name", null).ToString();
			this.SortTypeText = new TextObject("{=zMMqgxb1}Type", null).ToString();
			this.SortYieldText = new TextObject("{=v3OF6vBg}Yield", null).ToString();
		}

		public void SetListToControl(MBBindingList<ArmorItemVM> listToControl)
		{
			this._listToControl = listToControl;
		}


		public void SortByCurrentState()
		{
			if (this.IsNameSelected)
			{
				this._listToControl.Sort(this._nameComparer);
				return;
			}
			if (this.IsYieldSelected)
			{
				this._listToControl.Sort(this._yieldComparer);
				return;
			}
			if (this.IsTypeSelected)
			{
				this._listToControl.Sort(this._typeComparer);
			}
		}


		public void ExecuteSortByName()
		{
			int nameState = this.NameState;
			this.SetAllStates(SortState.Default);
			this.NameState = (nameState + 1) % 3;
			if (this.NameState == 0)
			{
				this.NameState++;
			}
			this._nameComparer.SetSortMode(this.NameState == 1);
			this._listToControl.Sort(this._nameComparer);
			this.IsNameSelected = true;
		}


		public void ExecuteSortByYield()
		{
			int yieldState = this.YieldState;
			this.SetAllStates(SortState.Default);
			this.YieldState = (yieldState + 1) % 3;
			if (this.YieldState == 0)
			{
				this.YieldState++;
			}
			this._yieldComparer.SetSortMode(this.YieldState == 1);
			this._listToControl.Sort(this._yieldComparer);
			this.IsYieldSelected = true;
		}

		public void ExecuteSortByType()
		{
			int typeState = this.TypeState;
			this.SetAllStates(SortState.Default);
			this.TypeState = (typeState + 1) % 3;
			if (this.TypeState == 0)
			{
				this.TypeState++;
			}
			this._typeComparer.SetSortMode(this.TypeState == 1);
			this._listToControl.Sort(this._typeComparer);
			this.IsTypeSelected = true;
		}


		private void SetAllStates(SortState state)
		{
			this.NameState = (int)state;
			this.TypeState = (int)state;
			this.YieldState = (int)state;
			this.IsNameSelected = false;
			this.IsTypeSelected = false;
			this.IsYieldSelected = false;
		}


		[DataSourceProperty]
		public int NameState
		{
			get
			{
				return this._nameState;
			}
			set
			{
				if (value != this._nameState)
				{
					this._nameState = value;
					base.OnPropertyChangedWithValue(value, "NameState");
				}
			}
		}


		[DataSourceProperty]
		public int TypeState
		{
			get
			{
				return this._typeState;
			}
			set
			{
				if (value != this._typeState)
				{
					this._typeState = value;
					base.OnPropertyChangedWithValue(value, "TypeState");
				}
			}
		}


		[DataSourceProperty]
		public int YieldState
		{
			get
			{
				return this._yieldState;
			}
			set
			{
				if (value != this._yieldState)
				{
					this._yieldState = value;
					base.OnPropertyChangedWithValue(value, "YieldState");
				}
			}
		}


		[DataSourceProperty]
		public bool IsNameSelected
		{
			get
			{
				return this._isNameSelected;
			}
			set
			{
				if (value != this._isNameSelected)
				{
					this._isNameSelected = value;
					base.OnPropertyChangedWithValue(value, "IsNameSelected");
				}
			}
		}

	
		[DataSourceProperty]
		public bool IsTypeSelected
		{
			get
			{
				return this._isTypeSelected;
			}
			set
			{
				if (value != this._isTypeSelected)
				{
					this._isTypeSelected = value;
					base.OnPropertyChangedWithValue(value, "IsTypeSelected");
				}
			}
		}


		[DataSourceProperty]
		public bool IsYieldSelected
		{
			get
			{
				return this._isYieldSelected;
			}
			set
			{
				if (value != this._isYieldSelected)
				{
					this._isYieldSelected = value;
					base.OnPropertyChangedWithValue(value, "IsYieldSelected");
				}
			}
		}


		[DataSourceProperty]
		public string SortTypeText
		{
			get
			{
				return this._sortTypeText;
			}
			set
			{
				if (value != this._sortTypeText)
				{
					this._sortTypeText = value;
					base.OnPropertyChangedWithValue(value, "SortTypeText");
				}
			}
		}

	
		[DataSourceProperty]
		public string SortNameText
		{
			get
			{
				return this._sortNameText;
			}
			set
			{
				if (value != this._sortNameText)
				{
					this._sortNameText = value;
					base.OnPropertyChangedWithValue(value, "SortNameText");
				}
			}
		}


		[DataSourceProperty]
		public string SortYieldText
		{
			get
			{
				return this._sortYieldText;
			}
			set
			{
				if (value != this._sortYieldText)
				{
					this._sortYieldText = value;
					base.OnPropertyChangedWithValue(value, "SortYieldText");
				}
			}
		}

		private MBBindingList<ArmorItemVM> _listToControl;


		private readonly ItemNameComparer _nameComparer;


		private readonly ItemYieldComparer _yieldComparer;


		private readonly ItemTypeComparer _typeComparer;


		private int _nameState;


		private int _yieldState;


		private int _typeState;


		private bool _isNameSelected;

		private bool _isYieldSelected;

		private bool _isTypeSelected;


		private string _sortTypeText;

		private string _sortNameText;

		private string _sortYieldText;


		private enum SortState
		{

			Default,
			Ascending,
			Descending
		}


		public abstract class ItemComparerBase : IComparer<ArmorItemVM>
		{
			// Token: 0x06001E37 RID: 7735 RVA: 0x00069C1D File Offset: 0x00067E1D
			public void SetSortMode(bool isAscending)
			{
				this._isAscending = isAscending;
			}


			public abstract int Compare(ArmorItemVM x, ArmorItemVM y);

			protected int ResolveEquality(ArmorItemVM x, ArmorItemVM y)
			{
				return x.ItemName.CompareTo(y.ItemName);
			}

			protected bool _isAscending;
		}


		public class ItemNameComparer : ItemComparerBase
		{

			public override int Compare(ArmorItemVM x, ArmorItemVM y)
			{
				if (this._isAscending)
				{
					return y.ItemName.CompareTo(x.ItemName) * -1;
				}
				return y.ItemName.CompareTo(x.ItemName);
			}
		}

		public class ItemYieldComparer : ItemComparerBase
		{

			public override int Compare(ArmorItemVM x, ArmorItemVM y)
			{
				/*int num = y.Yield.Count.CompareTo(x.Yield.Count);
				if (num != 0)
				{
					return num * (this._isAscending ? -1 : 1);
				} */
				return base.ResolveEquality(x, y);
			}
		}


		public class ItemTypeComparer : ItemComparerBase
		{

			public override int Compare(ArmorItemVM x, ArmorItemVM y)
			{
				/*int itemObjectTypeSortIndex = CampaignUIHelper.GetItemObjectTypeSortIndex(x.EquipmentElement.Item);
				int num = CampaignUIHelper.GetItemObjectTypeSortIndex(y.EquipmentElement.Item).CompareTo(itemObjectTypeSortIndex);
				if (num != 0)
				{
					return num * (this._isAscending ? -1 : 1);
				}*/
				return base.ResolveEquality(x, y);
			}
		}
	}
}
