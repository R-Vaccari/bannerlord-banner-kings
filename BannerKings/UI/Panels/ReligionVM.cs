using BannerKings.Managers.Institutions.Religions;
using BannerKings.Managers.Institutions.Religions.Doctrines;
using BannerKings.Populations;
using BannerKings.UI.Items;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.Panels
{
    class ReligionVM : BannerKingsViewModel
    {
		private MBBindingList<InformationElement> courtInfo;
		private MBBindingList<ReligionMemberVM> clergymen;
		private MBBindingList<ReligionMemberVM> faithful;
		private MBBindingList<BKTraitItemVM> virtues;
		private MBBindingList<ReligionElementVM> doctrines;
		private MBBindingList<ReligionElementVM> aspects;
		private Religion religion;

		public ReligionVM(PopulationData data) : base(data, true)
        {
			religion = data.ReligionData.Religion;
			courtInfo = new MBBindingList<InformationElement>();
			Clergymen = new MBBindingList<ReligionMemberVM>();
			Faithful = new MBBindingList<ReligionMemberVM>();
			Virtues = new MBBindingList<BKTraitItemVM>();
			Doctrines = new MBBindingList<ReligionElementVM>();
			Aspects = new MBBindingList<ReligionElementVM>();
		}

        public override void RefreshValues()
        {
            base.RefreshValues();
			Clergymen.Clear();
			Faithful.Clear();
			Virtues.Clear();
			Doctrines.Clear();
			Aspects.Clear();

			foreach (Clergyman clgm in religion.Clergy.Values)
				Clergymen.Add(new ReligionMemberVM(clgm, null));

			foreach (Hero hero in BannerKingsConfig.Instance.ReligionsManager.GetFaithfulHeroes(religion))
				Faithful.Add(new ReligionMemberVM(hero, null));

			foreach (KeyValuePair<TraitObject, bool> pair in religion.Faith.Traits)
				Virtues.Add(new BKTraitItemVM(pair.Key, pair.Value));

			foreach (string docString in religion.Doctrines)
            {
				Doctrine doctrine = DefaultDoctrines.Instance.GetById(docString);
				if (doctrine != null)
					Doctrines.Add(new ReligionElementVM(doctrine.Name, doctrine.Effects, doctrine.Description));
			}

			Aspects.Add(new ReligionElementVM(new TextObject("{=!}Leadership"), religion.Leadership.GetName(), religion.Leadership.GetHint()));
			Aspects.Add(new ReligionElementVM(new TextObject("{=!}Faith"), UIHelper.GetFaithTypeName(religion.Faith), UIHelper.GetFaithTypeDescription(religion.Faith)));
			Aspects.Add(new ReligionElementVM(new TextObject("{=!}Culture"), religion.MainCulture.Name, new TextObject("{=!}The main culture associated with this faith.")));
		}

		[DataSourceProperty]
		public string ClergymenText => new TextObject("{=!}Clergymen").ToString();

		[DataSourceProperty]
		public string FaithfulText => new TextObject("{=!}Faithful").ToString();

		[DataSourceProperty]
		public string GroupText => new TextObject("{=!}Faith Group").ToString();

		[DataSourceProperty]
		public string VirtuesText => new TextObject("{=!}Virtues").ToString();

		[DataSourceProperty]
		public string DoctrinesText => new TextObject("{=!}Doctrines").ToString();

		[DataSourceProperty]
		public string AspectsText => new TextObject("{=!}Aspects").ToString();

		[DataSourceProperty]
		public string Name => religion.Faith.GetFaithName().ToString();

		[DataSourceProperty]
		public string Description => religion.Faith.GetFaithDescription().ToString();

		[DataSourceProperty]
		public string GroupName => religion.Faith.FaithGroup.Name.ToString();

		[DataSourceProperty]
		public string GroupDescription => religion.Faith.FaithGroup.Description.ToString();


		[DataSourceProperty]
		public MBBindingList<BKTraitItemVM> Virtues
		{
			get => virtues;
			set
			{
				if (value != virtues)
				{
					virtues = value;
					base.OnPropertyChangedWithValue(value, "Virtues");
				}
			}
		}

		[DataSourceProperty]
		public MBBindingList<ReligionElementVM> Aspects
		{
			get => aspects;
			set
			{
				if (value != aspects)
				{
					aspects = value;
					base.OnPropertyChangedWithValue(value, "Aspects");
				}
			}
		}

		[DataSourceProperty]
		public MBBindingList<ReligionElementVM> Doctrines
		{
			get => doctrines;
			set
			{
				if (value != doctrines)
				{
					doctrines = value;
					base.OnPropertyChangedWithValue(value, "Doctrines");
				}
			}
		}

		[DataSourceProperty]
		public MBBindingList<ReligionMemberVM> Clergymen
		{
			get => clergymen;
			set
			{
				if (value != clergymen)
				{
					clergymen = value;
					base.OnPropertyChangedWithValue(value, "Clergymen");
				}
			}
		}

		[DataSourceProperty]
		public MBBindingList<ReligionMemberVM> Faithful
		{
			get => faithful;
			set
			{
				if (value != faithful)
				{
					faithful = value;
					base.OnPropertyChangedWithValue(value, "Faithful");
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
	}
}
