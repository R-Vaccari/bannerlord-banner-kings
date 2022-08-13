using System.Collections.Generic;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.Crafting
{
    public class ArmorCraftingSortController : ViewModel
    {
		public ArmorCraftingSortController()
		{
			_yieldComparer = new ItemYieldComparer();
			_typeComparer = new ItemTypeComparer();
			_nameComparer = new ItemNameComparer();
			RefreshValues();
		}


		public void SetListToControl(MBBindingList<ArmorItemVM> listToControl)
		{
			_listToControl = listToControl;
		}


		public void SortByCurrentState()
		{
			if (IsNameSelected)
			{
				_listToControl.Sort(_nameComparer);
				return;
			}
			if (IsYieldSelected)
			{
				_listToControl.Sort(_yieldComparer);
				return;
			}
			if (IsTypeSelected)
				_listToControl.Sort(_typeComparer);
			
		}


		public void ExecuteSortByName()
		{
			int nameState = NameState;
			SetAllStates(SortState.Default);
			NameState = (nameState + 1) % 3;
			if (NameState == 0)
				NameState++;
			
			_nameComparer.SetSortMode(NameState == 1);
			_listToControl.Sort(_nameComparer);
			IsNameSelected = true;
		}


		public void ExecuteSortByYield()
		{
			int yieldState = YieldState;
			SetAllStates(SortState.Default);
			YieldState = (yieldState + 1) % 3;
			if (YieldState == 0)
				YieldState++;
			
			_yieldComparer.SetSortMode(YieldState == 1);
			_listToControl.Sort(_yieldComparer);
			IsYieldSelected = true;
		}

		public void ExecuteSortByType()
		{
			int typeState = TypeState;
			SetAllStates(SortState.Default);
			TypeState = (typeState + 1) % 3;
			if (TypeState == 0)
				TypeState++;
			
			_typeComparer.SetSortMode(TypeState == 1);
			_listToControl.Sort(_typeComparer);
			IsTypeSelected = true;
		}


		private void SetAllStates(SortState state)
		{
			NameState = (int)state;
			TypeState = (int)state;
			YieldState = (int)state;
			IsNameSelected = false;
			IsTypeSelected = false;
			IsYieldSelected = false;
		}


		[DataSourceProperty]
		public int NameState
		{
			get => nameState;	
			set
			{
				if (value != nameState)
				{
					nameState = value;
					OnPropertyChangedWithValue(value, "NameState");
				}
			}
		}


		[DataSourceProperty]
		public int TypeState
		{
			get => typeState;	
			set
			{
				if (value != typeState)
				{
					typeState = value;
					OnPropertyChangedWithValue(value, "TypeState");
				}
			}
		}


		[DataSourceProperty]
		public int YieldState
		{
			get => yieldState;		
			set
			{
				if (value != yieldState)
				{
					yieldState = value;
					OnPropertyChangedWithValue(value, "YieldState");
				}
			}
		}


		[DataSourceProperty]
		public bool IsNameSelected
		{
			get => isNameSelected;	
			set
			{
				if (value != isNameSelected)
				{
					isNameSelected = value;
					OnPropertyChangedWithValue(value, "IsNameSelected");
				}
			}
		}

	
		[DataSourceProperty]
		public bool IsTypeSelected
		{
			get => isTypeSelected;
			set
			{
				if (value != isTypeSelected)
				{
					isTypeSelected = value;
					OnPropertyChangedWithValue(value, "IsTypeSelected");
				}
			}
		}


		[DataSourceProperty]
		public bool IsYieldSelected
		{
			get => isYieldSelected;
			set
			{
				if (value != isYieldSelected)
				{
					isYieldSelected = value;
					OnPropertyChangedWithValue(value, "IsYieldSelected");
				}
			}
		}


		[DataSourceProperty]
		public string SortTypeText => new TextObject("{=zMMqgxb1}Type", null).ToString();

		[DataSourceProperty]
		public string SortNameText => new TextObject("{=PDdh1sBj}Name", null).ToString();

		[DataSourceProperty]
		public string SortYieldText => new TextObject("{=v3OF6vBg}Yield", null).ToString();

		private MBBindingList<ArmorItemVM> _listToControl;
		private readonly ItemNameComparer _nameComparer;
		private readonly ItemYieldComparer _yieldComparer;
		private readonly ItemTypeComparer _typeComparer;

		private int nameState;
		private int yieldState;
		private int typeState;

		private bool isNameSelected;
		private bool isYieldSelected;
		private bool isTypeSelected;



		private enum SortState
		{

			Default,
			Ascending,
			Descending
		}


		public abstract class ItemComparerBase : IComparer<ArmorItemVM>
		{
			public void SetSortMode(bool isAscending) => this.isAscending = isAscending;
			public abstract int Compare(ArmorItemVM x, ArmorItemVM y);
			protected int ResolveEquality(ArmorItemVM x, ArmorItemVM y) => x.ItemName.CompareTo(y.ItemName);

			protected bool isAscending;
		}


		public class ItemNameComparer : ItemComparerBase
		{

			public override int Compare(ArmorItemVM x, ArmorItemVM y)
			{
				if (isAscending) return y.ItemName.CompareTo(x.ItemName) * -1;		
				return y.ItemName.CompareTo(x.ItemName);
			}
		}

		public class ItemYieldComparer : ItemComparerBase
		{

			public override int Compare(ArmorItemVM x, ArmorItemVM y)
			{
				int num = y.ItemType.CompareTo(x.ItemType);
				if (num != 0) return num * (isAscending ? -1 : 1);
				
				return ResolveEquality(x, y);
			}
		}


		public class ItemTypeComparer : ItemComparerBase
		{

			public override int Compare(ArmorItemVM x, ArmorItemVM y)
			{
				int itemObjectTypeSortIndex = CampaignUIHelper.GetItemObjectTypeSortIndex(x.Item);
				int num = CampaignUIHelper.GetItemObjectTypeSortIndex(y.Item).CompareTo(itemObjectTypeSortIndex);
				if (num != 0)
					return num * (isAscending ? -1 : 1);
				
				return ResolveEquality(x, y);
			}
		}
	}
}
