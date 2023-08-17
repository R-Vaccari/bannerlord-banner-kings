using BannerKings.Managers.Innovations;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.Titles
{
    public class InnovationElementVM : BannerKingsViewModel
    {
        private MBBindingList<InnovationElementVM> branch;
        private Innovation innovation;

        public InnovationElementVM(Innovation innovation) : base(null, true)
        {
            Branch = new MBBindingList<InnovationElementVM>();
            this.innovation = innovation;

            foreach (var i in DefaultInnovations.Instance.All)
            {
                if (i.Requirement == innovation)
                {
                    Branch.Add(new InnovationElementVM(i));
                }
            }
        }

        [DataSourceProperty]
        public string NameText => innovation.Name.ToString();

        [DataSourceProperty]
        public string ProgressText => new TextObject("{=0ABQRW1Z}{CURRENT}/{REQUIRED} ({PERCENTAGE})")
                            .SetTextVariable("CURRENT", innovation.CurrentProgress.ToString("0.00"))
                            .SetTextVariable("REQUIRED", innovation.RequiredProgress)
                            .SetTextVariable("PERCENTAGE",
                                FormatValue(innovation.CurrentProgress / innovation.RequiredProgress))
                            .ToString();

        [DataSourceProperty]
        public string EffectsText => innovation.Effects.ToString();

        [DataSourceProperty]
        public BasicTooltipViewModel Hint => new BasicTooltipViewModel(() => innovation.Description.ToString());

        [DataSourceProperty]
        public MBBindingList<InnovationElementVM> Branch
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

        public override void RefreshValues()
        {
            base.RefreshValues();
            Branch.ApplyActionOnAllItems(delegate(InnovationElementVM x) { x.RefreshValues(); });
        }
    }
}