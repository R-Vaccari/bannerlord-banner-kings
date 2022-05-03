using BannerKings.Managers.Court;
using BannerKings.Populations;
using BannerKings.UI.Items;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.Panels
{
    class CourtVM : BannerKingsViewModel
    {
		private CouncilVM governorSelection;
		private MBBindingList<InformationElement> courtInfo;
		private CouncilData council;
		private CouncilPosition councilPosition;
		private HeroVM marshall;
		private HeroVM steward;
		private HeroVM chancellor;
		private HeroVM spymaster;
		private HeroVM spiritual;
		private MBBindingList<RoyalPositionVM> royalPositions;
		private MBBindingList<CouncilMemberVM> courtMembers;

		public CourtVM(PopulationData data) : base(data, true)
        {
			GameTexts.SetVariable("PLAYER_NAME", Hero.MainHero.Name);
			this.courtInfo = new MBBindingList<InformationElement>();
			royalPositions = new MBBindingList<RoyalPositionVM>();
			this.council = BannerKingsConfig.Instance.CourtManager.GetCouncil(Hero.MainHero);
			councilPosition = CouncilPosition.Marshall;
			this.courtMembers = new MBBindingList<CouncilMemberVM>();
			List<Hero> heroes = council.GetCourtMembers();
			foreach (Hero hero in heroes)
				this.courtMembers.Add(new CouncilMemberVM(hero, null,
									councilPosition, council.GetCompetence(hero, councilPosition)));
			CouncilMemberSelection = new CouncilVM(new Action<Hero>(this.SetCouncilMember), this.council, councilPosition, heroes);	
		}

        public override void RefreshValues()
        {
            base.RefreshValues();
			this.CourtInfo.Clear();
			RoyalPositions.Clear();
			this.CouncilMemberSelection.RefreshValues();
			this.Marshall = new HeroVM(council.Marshall);
			this.Steward = new HeroVM(council.Steward);
			this.Chancellor = new HeroVM(council.Chancellor);
			this.Spymaster = new HeroVM(council.Spymaster);
			Spiritual = new HeroVM(council.Spiritual);
			CourtInfo.Add(new InformationElement("Administrative costs:", base.FormatValue(council.AdministrativeCosts),
				"Costs associated with payment of council members, deducted on all your fiefs' revenues."));

			foreach (CouncilMember position in council.RoyalPositions)
				RoyalPositions.Add(new RoyalPositionVM(position));

		}

		private void SetId(string id)
		{
			CouncilPosition newPosition = (CouncilPosition)Enum.Parse(typeof(CouncilPosition), id);
			if (this.councilPosition != newPosition)
            {
				this.councilPosition = newPosition;
				this.CouncilMemberSelection.Position = newPosition;
				RefreshValues();
			}
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
			} else if (councilPosition == CouncilPosition.Spiritual)
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
		public string Title => new TextObject("{=!}Council of {PLAYER_NAME}").ToString();

		[DataSourceProperty]
		public bool IsRoyal => council.IsRoyal;

		[DataSourceProperty]
		public MBBindingList<CouncilMemberVM> CourtMembers
		{
			get => this.courtMembers;
			set
			{
				if (value != this.courtMembers)
				{
					this.courtMembers = value;
					base.OnPropertyChangedWithValue(value, "CourtMembers");
				}
			}
		}

		[DataSourceProperty]
		public CouncilVM CouncilMemberSelection
		{
			get => this.governorSelection;
			set
			{
				if (value != this.governorSelection)
				{
					this.governorSelection = value;
					base.OnPropertyChangedWithValue(value, "GovernorSelection");
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
