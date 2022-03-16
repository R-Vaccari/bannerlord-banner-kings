using BannerKings.Populations;
using BannerKings.UI.Items;
using TaleWorlds.Library;
using static BannerKings.Managers.TitleManager;

namespace BannerKings.UI.Windows
{
    public class TitleWindowVM : BannerKingsViewModel
    {
		private TitleElementVM tree;
		public TitleWindowVM(PopulationData data) : base(data, true)
        {
			FeudalTitle title = BannerKingsConfig.Instance.TitleManager.GetTitle(data.Settlement);
			if (title != null)
            {
				if (title.sovereign != null)
					this.Tree = new TitleElementVM(title.sovereign);
				else this.Tree = new TitleElementVM(title);
			}
			
        }


		[DataSourceProperty]
		public TitleElementVM Tree
		{
			get => this.tree;
			
			set
			{
				if (value != this.tree)
				{
					this.tree = value;
					base.OnPropertyChangedWithValue(value, "Tree");
				}
			}
		}
	}
}
