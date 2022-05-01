using BannerKings.Managers.Institutions.Religions;
using BannerKings.Populations;
using BannerKings.UI.Items;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.Panels
{
    class ReligionVM : BannerKingsViewModel
    {
		private MBBindingList<InformationElement> courtInfo;
		private MBBindingList<ReligionMemberVM> clergymen, faithful;
		private Religion religion;

		public ReligionVM(PopulationData data) : base(data, true)
        {
			religion = data.ReligionData.Religion;
			courtInfo = new MBBindingList<InformationElement>();
			clergymen = new MBBindingList<ReligionMemberVM>();
			faithful = new MBBindingList<ReligionMemberVM>();
			foreach (Clergyman clgm in religion.Clergy.Values)
				clergymen.Add(new ReligionMemberVM(clgm, null));

			foreach (Hero hero in BannerKingsConfig.Instance.ReligionsManager.GetFaithfulHeroes(religion))
				faithful.Add(new ReligionMemberVM(hero, null));
		}

        public override void RefreshValues()
        {
            base.RefreshValues();
			this.CourtInfo.Clear();
			//CourtInfo.Add(new InformationElement("Administrative costs:", base.FormatValue(council.AdministrativeCosts),
			//	"Costs associated with payment of council members, deducted on all your fiefs' revenues."));

		}

		[DataSourceProperty]
		public string Name => religion.Faith.GetFaithName().ToString();

		[DataSourceProperty]
		public string Description => religion.Faith.GetFaithDescription().ToString();


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
		public string Title => new TextObject("{=!}Council of {PLAYER_NAME}").ToString();

		[DataSourceProperty]
		public MBBindingList<ReligionMemberVM> Clergymen
		{
			get => this.clergymen;
			set
			{
				if (value != this.clergymen)
				{
					this.clergymen = value;
					base.OnPropertyChangedWithValue(value, "Clergymen");
				}
			}
		}

		[DataSourceProperty]
		public MBBindingList<ReligionMemberVM> Faithful
		{
			get => this.faithful;
			set
			{
				if (value != this.faithful)
				{
					this.faithful = value;
					base.OnPropertyChangedWithValue(value, "Faithful");
				}
			}
		}
	}
}
