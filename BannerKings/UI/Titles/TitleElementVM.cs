using BannerKings.Managers.Titles;
using TaleWorlds.Library;

namespace BannerKings.UI.Titles
{
    public class TitleElementVM : ViewModel
    {
        private MBBindingList<TitleElementVM> branch;
        private TitleVM title;

        public TitleElementVM(FeudalTitle title, DemesneHierarchyVM hierarchy)
        {
            Branch = new MBBindingList<TitleElementVM>();
            Title = new TitleVM(title);

            if (title.Fief != null)
            {
                var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(title.Fief);
                hierarchy.Population += data.TotalPop;
            }

            if (title.Vassals != null)
            {
                foreach (var vassal in title.Vassals)
                {
                    Branch.Add(new TitleElementVM(vassal, hierarchy));

                    if (vassal.Fief != null)
                    {
                        var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(vassal.Fief);
                        hierarchy.Population += data.TotalPop;
                    }
                }
            }
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

        public override void RefreshValues()
        {
            base.RefreshValues();
            Branch.ApplyActionOnAllItems(delegate(TitleElementVM x) { x.RefreshValues(); });
            Title.RefreshValues();
        }
    }
}