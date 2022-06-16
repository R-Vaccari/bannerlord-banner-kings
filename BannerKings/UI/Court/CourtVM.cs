using BannerKings.Managers.Court;
using BannerKings.UI.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.ClanManagement;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.Court
{
    class CourtVM : BannerKingsViewModel
    {
        private MBBindingList<ClanLordItemVM> family, courtiers;
		private CouncilVM councilVM;
		private CouncilData council;
		private CouncilPosition councilPosition;
		private bool isSelected, isRoyal;
		private HeroVM marshall;
		private HeroVM steward;
		private HeroVM chancellor;
		private HeroVM spymaster;
		private HeroVM spiritual;
		private MBBindingList<RoyalPositionVM> corePositions, royalPositions;
		private MBBindingList<InformationElement> courtInfo;

		public CourtVM(bool royal) : base(null, false)
        {
			if (!royal) council = BannerKingsConfig.Instance.CourtManager.GetCouncil(Hero.MainHero);
			else council = BannerKingsConfig.Instance.CourtManager.GetCouncil(Clan.PlayerClan.Kingdom.Leader);
			isSelected = false;
			family = new MBBindingList<ClanLordItemVM>();
			courtiers = new MBBindingList<ClanLordItemVM>();
			corePositions = new MBBindingList<RoyalPositionVM>();
			royalPositions = new MBBindingList<RoyalPositionVM>();
			courtInfo = new MBBindingList<InformationElement>();
			councilPosition = CouncilPosition.Marshall;
			isRoyal = royal;
		}

        public override void RefreshValues()
        {
            base.RefreshValues();
			Family.Clear();
			Courtiers.Clear();
			CourtInfo.Clear();
			CorePositions.Clear();
			RoyalPositions.Clear();
			List<Hero> heroes = council.GetCourtMembers();
			CouncilVM = new CouncilVM(new Action<Hero>(SetCouncilMember), council, councilPosition, heroes);

			foreach (Hero hero in heroes)
			{
				if (hero.Spouse == Hero.MainHero || Hero.MainHero.Children.Contains(hero) || Hero.MainHero.Siblings.Contains(hero))
					Family.Add(new ClanLordItemVM(hero, null));
				else Courtiers.Add(new ClanLordItemVM(hero, null));

			}

			Marshall = new HeroVM(council.Marshall);
			Steward = new HeroVM(council.Steward);
			Chancellor = new HeroVM(council.Chancellor);
			Spymaster = new HeroVM(council.Spymaster);
			Spiritual = new HeroVM(council.Spiritual);

			CourtInfo.Add(new InformationElement("Administrative costs:", base.FormatValue(council.AdministrativeCosts),
				"Costs associated with payment of council members, deducted on all your fiefs' revenues."));

			foreach (CouncilMember position in council.Positions)
            {
				if (position.Clan == null) position.Clan = council.Owner.Clan;
				CorePositions.Add(new RoyalPositionVM(position, new Action<string>(SetId)));
			}
				

			if (isRoyal)
				foreach (CouncilMember position in council.RoyalPositions)
                {
					if (position.Clan == null) position.Clan = council.Owner.Clan;
					RoyalPositions.Add(new RoyalPositionVM(position, new Action<string>(SetId)));
				}

		}

		private void SetId(string id)
		{
			CouncilPosition newPosition = (CouncilPosition)Enum.Parse(typeof(CouncilPosition), id);
			if (this.councilPosition != newPosition)
			{
				this.councilPosition = newPosition;
				this.CouncilVM.Position = newPosition;
				RefreshValues();
			}
			CouncilVM.ShowOptions();
		}

		private void SetCouncilMember(Hero member)
		{
			if (councilPosition == CouncilPosition.Marshall)
			{
				if (this.Marshall.Hero != member)
				{
					this.Marshall = new HeroVM(member);
					this.council.Marshall = member;
				}
			}
			else if (councilPosition == CouncilPosition.Steward)
			{
				if (this.Steward.Hero != member)
				{
					this.Steward = new HeroVM(member);
					this.council.Steward = member;
				}
			}
			else if (councilPosition == CouncilPosition.Chancellor)
			{
				if (this.Chancellor.Hero != member)
				{
					this.Chancellor = new HeroVM(member);
					this.council.Chancellor = member;
				}
			}
			else if (councilPosition == CouncilPosition.Spymaster)
			{
				if (this.Spymaster.Hero != member)
				{
					this.Spymaster = new HeroVM(member);
					this.council.Spymaster = member;
				}
			}
			else if (councilPosition == CouncilPosition.Spiritual)
			{
				if (this.Spiritual.Hero != member)
				{
					this.Spiritual = new HeroVM(member);
					this.council.Spiritual = member;
				}
			}
			this.RefreshValues();
		}

		[DataSourceProperty]
		public string FamilyText => GameTexts.FindText("str_family_group", null).ToString();

		[DataSourceProperty]
		public string CourtiersText => new TextObject("{=!}Courtiers").ToString();



		[DataSourceProperty]
		public bool IsSelected
		{
			get => isSelected;
			set
			{
				if (value != isSelected)
				{
					isSelected = value;
					OnPropertyChangedWithValue(value, "IsSelected");
				}
			}
		}

		[DataSourceProperty]
		public bool IsRoyal
		{
			get => isRoyal;
			set
			{
				if (value != isRoyal)
				{
					isRoyal = value;
					OnPropertyChangedWithValue(value, "IsRoyal");
				}
			}
		}

		[DataSourceProperty]
		public CouncilVM CouncilVM
		{
			get => this.councilVM;
			set
			{
				if (value != this.councilVM)
				{
					this.councilVM = value;
					base.OnPropertyChangedWithValue(value, "CouncilVM");
				}
			}
		}

		[DataSourceProperty]
		public MBBindingList<ClanLordItemVM> Family
		{
			get => family;
			set
			{
				if (value != family)
				{
					family = value;
					OnPropertyChangedWithValue(value, "Family");
				}
			}
		}

		[DataSourceProperty]
		public MBBindingList<ClanLordItemVM> Courtiers
		{
			get => courtiers;
			set
			{
				if (value != courtiers)
				{
					courtiers = value;
					OnPropertyChangedWithValue(value, "Courtiers");
				}
			}
		}

		[DataSourceProperty]
		public MBBindingList<InformationElement> CourtInfo
		{
			get => courtInfo;
			set
			{
				if (value != courtInfo)
				{
					courtInfo = value;
					base.OnPropertyChangedWithValue(value, "CourtInfo");
				}
			}
		}

		[DataSourceProperty]
		public MBBindingList<RoyalPositionVM> CorePositions
		{
			get => corePositions;
			set
			{
				if (value != corePositions)
				{
					corePositions = value;
					base.OnPropertyChangedWithValue(value, "CorePositions");
				}
			}
		}

		[DataSourceProperty]
		public MBBindingList<RoyalPositionVM> RoyalPositions
		{
			get => royalPositions;
			set
			{
				if (value != royalPositions)
				{
					royalPositions = value;
					base.OnPropertyChangedWithValue(value, "RoyalPositions");
				}
			}
		}

		[DataSourceProperty]
		public HeroVM Marshall
		{
			get => this.marshall;
			set
			{
				if (value != this.marshall)
				{
					this.marshall = value;
					base.OnPropertyChangedWithValue(value, "Marshall");
				}
			}
		}

		[DataSourceProperty]
		public HeroVM Spiritual
		{
			get => this.spiritual;
			set
			{
				if (value != this.spiritual)
				{
					this.spiritual = value;
					base.OnPropertyChangedWithValue(value, "Spiritual");
				}
			}
		}

		[DataSourceProperty]
		public HeroVM Steward
		{
			get => this.steward;
			set
			{
				if (value != this.steward)
				{
					this.steward = value;
					base.OnPropertyChangedWithValue(value, "Steward");
				}
			}
		}

		[DataSourceProperty]
		public HeroVM Chancellor
		{
			get => this.chancellor;
			set
			{
				if (value != this.chancellor)
				{
					this.chancellor = value;
					base.OnPropertyChangedWithValue(value, "Chancellor");
				}
			}
		}

		[DataSourceProperty]
		public HeroVM Spymaster
		{
			get => this.spymaster;
			set
			{
				if (value != this.spymaster)
				{
					this.spymaster = value;
					base.OnPropertyChangedWithValue(value, "Spymaster");
				}
			}
		}
	}
}
