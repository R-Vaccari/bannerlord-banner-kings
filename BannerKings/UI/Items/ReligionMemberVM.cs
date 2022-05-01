using BannerKings.Managers.Institutions.Religions;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu.TownManagement;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;

namespace BannerKings.UI.Items
{
    class ReligionMemberVM : SettlementGovernorSelectionItemVM
    {
		private BasicTooltipViewModel religionHint;
		public ReligionMemberVM(Clergyman clergyman, Action<SettlementGovernorSelectionItemVM> onSelection) : base(clergyman.Hero, onSelection)
        { 

		}

		public ReligionMemberVM(Hero hero, Action<SettlementGovernorSelectionItemVM> onSelection) : base(hero, onSelection)
		{

		}


		[DataSourceProperty]
		public BasicTooltipViewModel ReligionHint
		{
			get => this.religionHint;
			set
			{
				if (value != this.religionHint)
				{
					this.religionHint = value;
					base.OnPropertyChangedWithValue(value, "ReligionHint");
				}
			}
		}
	}
}
