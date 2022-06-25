using BannerKings.Managers.Court;
using BannerKings.UI.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.CharacterDeveloper;
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
		private bool isRoyal;
		private MBBindingList<CouncilPositionVM> corePositions, royalPositions;
		private MBBindingList<InformationElement> courtInfo, courtierInfo;
		private CharacterVM currentCharacter;
		private string positionName, positionDescription;

		public CourtVM(bool royal) : base(null, false)
        {
			if (!royal) council = BannerKingsConfig.Instance.CourtManager.GetCouncil(Hero.MainHero);
			else if (Clan.PlayerClan.Kingdom != null) council = BannerKingsConfig.Instance.CourtManager.GetCouncil(Clan.PlayerClan.Kingdom.Leader);
			family = new MBBindingList<ClanLordItemVM>();
			courtiers = new MBBindingList<ClanLordItemVM>();
			corePositions = new MBBindingList<CouncilPositionVM>();
			royalPositions = new MBBindingList<CouncilPositionVM>();
			courtInfo = new MBBindingList<InformationElement>();
			courtierInfo = new MBBindingList<InformationElement>();
			councilPosition = CouncilPosition.Marshall;
			isRoyal = royal;
			currentCharacter = new CharacterVM(Hero.MainHero, null);
		}

        public override void RefreshValues()
        {
            base.RefreshValues();
			Family.Clear();
			Courtiers.Clear();
			CourtInfo.Clear();
			CorePositions.Clear();
			RoyalPositions.Clear();
			CourtierInfo.Clear();
			List<Hero> heroes = council.GetCourtMembers();
			CouncilVM = new CouncilVM(new Action<Hero>(SetCouncilMember), council, councilPosition, heroes);

			foreach (Hero hero in heroes)
			{
				if (hero.Spouse == council.Owner || council.Owner.Children.Contains(hero) || council.Owner.Siblings.Contains(hero) ||
					council.Owner.Father == hero || council.Owner.Mother == hero)
					Family.Add(new ClanLordItemVM(hero, new Action<ClanLordItemVM>(SetCurrentCharacter)));
				else Courtiers.Add(new ClanLordItemVM(hero, new Action<ClanLordItemVM>(SetCurrentCharacter)));

			}

			CourtInfo.Add(new InformationElement("Administrative costs:", base.FormatValue(council.AdministrativeCosts),
				"Costs associated with payment of council members, deducted on all your fiefs' revenues."));

			foreach (CouncilMember position in council.Positions)
            {
				if (position.Clan == null) position.Clan = council.Owner.Clan;
				CorePositions.Add(new CouncilPositionVM(position, new Action<string>(SetId), new Action<string>(UpdatePositionTexts)));
			}
				

			if (isRoyal)
				foreach (CouncilMember position in council.RoyalPositions)
                {
					if (position.Clan == null) position.Clan = council.Owner.Clan;
					RoyalPositions.Add(new CouncilPositionVM(position, new Action<string>(SetId), new Action<string>(UpdatePositionTexts)));
				}

			CouncilMember member = council.GetMemberFromPosition(councilPosition);
			if (member != null)
			{
				positionName = member.GetName().ToString();
				positionDescription = member.GetDescription().ToString();
			}

			if (currentCharacter != null)
            {
				CourtierInfo.Add(new InformationElement(GameTexts.FindText("str_enc_sf_occupation").ToString(), 
					CampaignUIHelper.GetHeroOccupationName(currentCharacter.Hero).ToString(), string.Empty));
				CouncilMember heroPosition = council.GetHeroPosition(currentCharacter.Hero);
				if (heroPosition != null) CourtierInfo.Add(new InformationElement(new TextObject("{=!}Council Position:").ToString(), 
					heroPosition.GetName().ToString(), string.Empty));
			}
		}

		private void UpdatePositionTexts(string id)
        {
			CouncilMember member = council.GetMemberFromPosition((CouncilPosition)Enum.Parse(typeof(CouncilPosition), id));
			if (member != null)
            {
				CurrentPositionNameText = member.GetName().ToString();
				CurrentPositionDescriptionText = member.GetDescription().ToString();
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
			council.GetMemberFromPosition(councilPosition).Member = member;
			RefreshValues();
		}

		private void SetCurrentCharacter(ClanLordItemVM vm)
        {
			CurrentCharacter = new CharacterVM(vm.GetHero(), null);
			RefreshValues();
		}

		[DataSourceProperty]
		public string FamilyText => GameTexts.FindText("str_family_group", null).ToString();

		[DataSourceProperty]
		public string CourtiersText => new TextObject("{=!}Courtiers").ToString();

		[DataSourceProperty]
		public string CurrentPositionNameText
		{
			get => positionName;
			set
			{
				if (value != positionName)
				{
					positionName = value;
					OnPropertyChangedWithValue(value, "CurrentPositionNameText");
				}
			}
		}

		[DataSourceProperty]
		public string CurrentPositionDescriptionText
		{
			get => positionDescription;
			set
			{
				if (value != positionDescription)
				{
					positionDescription = value;
					OnPropertyChangedWithValue(value, "CurrentPositionDescriptionText");
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
		public CharacterVM CurrentCharacter
		{
			get => currentCharacter;
			set
			{
				if (value != currentCharacter)
				{
					currentCharacter = value;
					OnPropertyChangedWithValue(value, "CurrentCharacter");
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
		public MBBindingList<InformationElement> CourtierInfo
		{
			get => courtierInfo;
			set
			{
				if (value != courtierInfo)
				{
					courtierInfo = value;
					base.OnPropertyChangedWithValue(value, "CourtierInfo");
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
		public MBBindingList<CouncilPositionVM> CorePositions
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
		public MBBindingList<CouncilPositionVM> RoyalPositions
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
	}
}
