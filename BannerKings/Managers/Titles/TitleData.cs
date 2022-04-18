using BannerKings.Populations;
using TaleWorlds.SaveSystem;
using static BannerKings.Managers.TitleManager;

namespace BannerKings.Managers.Titles
{
    public class TitleData : BannerKingsData
    {
        [SaveableProperty(1)]
        private FeudalTitle title { get; set; }

        public TitleData(FeudalTitle title)
        {
            this.title = title;
        }

        public FeudalTitle Title => this.title;
        internal override void Update(PopulationData data)
        {

        }
    }
}
