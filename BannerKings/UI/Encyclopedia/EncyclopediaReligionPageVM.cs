using BannerKings.Managers.Institutions.Religions;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia;
using TaleWorlds.Library;

namespace BannerKings.UI.Encyclopedia
{
    [EncyclopediaViewModel(typeof(Religion))]
    public class EncyclopediaReligionPageVM : EncyclopediaContentPageVM
    {
		private Religion religion;
		private MBBindingList<HeroVM> clergymen;
		private MBBindingList<HeroVM> faithful;

		public EncyclopediaReligionPageVM(EncyclopediaPageArgs args) : base(args)
        {
			religion = (Religion?)Obj;
			Clergymen = new MBBindingList<HeroVM>();
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
			Clergymen.Clear();

			foreach (Clergyman clergyman in religion.Clergy.Values)
				Clergymen.Add(new HeroVM(clergyman.Hero));
        }

		[DataSourceProperty]
		public string Name => religion.Faith.GetFaithName().ToString();

		[DataSourceProperty]
		public string Description => religion.Faith.GetFaithDescription().ToString();

		[DataSourceProperty]
		public MBBindingList<HeroVM> Faithful
		{
			get => faithful;
			set
			{
				if (value != faithful)
				{
					faithful = value;
					OnPropertyChangedWithValue(value);
				}
			}
		}


		[DataSourceProperty]
		public MBBindingList<HeroVM> Clergymen
		{
			get => clergymen;
			set
			{
				if (value != clergymen)
				{
					clergymen = value;
					OnPropertyChangedWithValue(value);
				}
			}
		}
	}
}
