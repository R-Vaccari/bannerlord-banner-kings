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
			this.Branch = new MBBindingList<TitleElementVM>();
			this.Title = new TitleVM(title);
			if (title.vassals != null)
				foreach (FeudalTitle vassal in title.vassals)
					this.Branch.Add(new TitleElementVM(vassal));
				
		}

		public override void RefreshValues()
		{
			base.RefreshValues();
			this.Branch.ApplyActionOnAllItems(delegate (TitleElementVM x)
			{
				x.RefreshValues();
			});
			this.Title.RefreshValues();
		}

		[DataSourceProperty]
		public MBBindingList<TitleElementVM> Branch
		{
			get => this.branch;
			set
			{
				if (value != this.branch)
				{
					this.branch = value;
					base.OnPropertyChangedWithValue(value, "Branch");
				}
			}
		}

		[DataSourceProperty]
		public TitleVM Title
		{
			get => this.title;
			set
			{
				if (value != this.title)
				{
					this.title = value;
					base.OnPropertyChangedWithValue(value, "Title");
				}
			}
		}
	}
}

