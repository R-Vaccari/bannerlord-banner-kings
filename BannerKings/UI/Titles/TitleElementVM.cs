using BannerKings.Managers.Titles;
using TaleWorlds.Library;

namespace BannerKings.UI.Items
{
    public class TitleElementVM : ViewModel
    {
		private MBBindingList<TitleElementVM> branch;
		private TitleVM title;
		public TitleElementVM(FeudalTitle title)
		{
			Branch = new MBBindingList<TitleElementVM>();
			Title = new TitleVM(title);
			if (title.vassals != null)
				foreach (FeudalTitle vassal in title.vassals)
					Branch.Add(new TitleElementVM(vassal));
				
		}

		public override void RefreshValues()
		{
			base.RefreshValues();
			Branch.ApplyActionOnAllItems(delegate (TitleElementVM x)
			{
				x.RefreshValues();
			});
			Title.RefreshValues();
		}

		[DataSourceProperty]
		public MBBindingList<TitleElementVM> Branch
		{
			get => branch;
			set
			{
				if (value != branch)
				{
					branch = value;
					OnPropertyChangedWithValue(value);
				}
			}
		}

		[DataSourceProperty]
		public TitleVM Title
		{
			get => title;
			set
			{
				if (value != title)
				{
					title = value;
					OnPropertyChangedWithValue(value);
				}
			}
		}
	}
}

