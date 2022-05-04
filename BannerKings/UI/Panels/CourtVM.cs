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
		private MBBindingList<CouncilMemberVM> courtMembers;

		public CourtVM(PopulationData data) : base(data, true)
        {
			GameTexts.SetVariable("PLAYER_NAME", Hero.MainHero.Name);
			courtInfo = new MBBindingList<InformationElement>();
			council = BannerKingsConfig.Instance.CourtManager.GetCouncil(Hero.MainHero);
			councilPosition = CouncilPosition.Marshall;
			courtMembers = new MBBindingList<CouncilMemberVM>();
			List<Hero> heroes = council.GetCourtMembers();
			foreach (Hero hero in heroes)
				courtMembers.Add(new CouncilMemberVM(hero, null,
									councilPosition, council.GetCompetence(hero, councilPosition)));
			CouncilMemberSelection = new CouncilVM(SetCouncilMember, council, councilPosition, heroes);	
		}

        public override void RefreshValues()
        {
            base.RefreshValues();
			CourtInfo.Clear();
			CouncilMemberSelection.RefreshValues();
			Marshall = new HeroVM(council.Marshall);
			Steward = new HeroVM(council.Steward);
			Chancellor = new HeroVM(council.Chancellor);
			Spymaster = new HeroVM(council.Spymaster);
			CourtInfo.Add(new InformationElement("Administrative costs:", FormatValue(council.AdministrativeCosts),
				"Costs associated with payment of council members, deducted on all your fiefs' revenues."));

		}

		private void SetId(string id)
		{
			CouncilPosition newPosition = (CouncilPosition)Enum.Parse(typeof(CouncilPosition), id);
			if (councilPosition != newPosition)
            {
				councilPosition = newPosition;
				CouncilMemberSelection.Position = newPosition;
				RefreshValues();
			}
		}

		private void SetCouncilMember(Hero member)
        {
			if (councilPosition == CouncilPosition.Marshall)
            {
				if (Marshall.Hero != member)
                {
					Marshall = new HeroVM(member);
					council.Marshall = member;
				}
					
			}
			else if (councilPosition == CouncilPosition.Steward)
            {
				if (Steward.Hero != member)
				{
					Steward = new HeroVM(member);
					council.Steward = member;
				}
			}
			else if (councilPosition == CouncilPosition.Chancellor)
            {
				if (Chancellor.Hero != member)
				{
					Chancellor = new HeroVM(member);
					council.Chancellor = member;
				}
			}		
			else if (councilPosition == CouncilPosition.Spymaster)
            {
				if (Spymaster.Hero != member)
				{
					Spymaster = new HeroVM(member);
					council.Spymaster = member;
				}
			}
			RefreshValues();			
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
					OnPropertyChangedWithValue(value);
				}
			}
		}

		[DataSourceProperty]
		public string Title => new TextObject("{=!}Council of {PLAYER_NAME}").ToString();

		[DataSourceProperty]
		public MBBindingList<CouncilMemberVM> CourtMembers
		{
			get => courtMembers;
			set
			{
				if (value != courtMembers)
				{
					courtMembers = value;
					OnPropertyChangedWithValue(value);
				}
			}
		}

		[DataSourceProperty]
		public CouncilVM CouncilMemberSelection
		{
			get => governorSelection;
			set
			{
				if (value != governorSelection)
				{
					governorSelection = value;
					OnPropertyChangedWithValue(value, "GovernorSelection");
				}
			}
		}

		[DataSourceProperty]
		public HeroVM Marshall
		{
			get => marshall;
			set
			{
				if (value != marshall)
				{
					marshall = value;
					OnPropertyChangedWithValue(value);
				}
			}
		}

		[DataSourceProperty]
		public HeroVM Steward
		{
			get => steward;
			set
			{
				if (value != steward)
				{
					steward = value;
					OnPropertyChangedWithValue(value);
				}
			}
		}

		[DataSourceProperty]
		public HeroVM Chancellor
		{
			get => chancellor;
			set
			{
				if (value != chancellor)
				{
					chancellor = value;
					OnPropertyChangedWithValue(value);
				}
			}
		}

		[DataSourceProperty]
		public HeroVM Spymaster
		{
			get => spymaster;
			set
			{
				if (value != spymaster)
				{
					spymaster = value;
					OnPropertyChangedWithValue(value);
				}
			}
		}
	}
}
