using BannerKings.Models;
using BannerKings.Populations;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using static BannerKings.Managers.PopulationManager;

namespace BannerKings.UI
{
    public class MilitaryVM : BannerKingsViewModel
    {
        private MBBindingList<InformationElement> _defenseInfo;
        private Settlement _settlement;

        public MilitaryVM(PopulationData data, Settlement _settlement, bool selected) : base(data, selected)
        {
            _defenseInfo = new MBBindingList<InformationElement>();
            this._settlement = _settlement;
            this.RefreshValues();
        }

        public override void RefreshValues()
        {
            base.RefreshValues();

            DefenseInfo.Clear();
            DefenseInfo.Add(new InformationElement("Militia Cap:", new BKMilitiaModel().GetMilitiaLimit(data, _settlement.IsCastle).ToString(),
                "The maximum number of militiamen this settlement can support, based on it's population"));
            DefenseInfo.Add(new InformationElement("Militia Quality:", FormatValue(new BKMilitiaModel().CalculateEliteMilitiaSpawnChance(_settlement)),
                    "Chance of militiamen being spawned as veterans instead of recruits"));
            
        }


        [DataSourceProperty]
        public MBBindingList<InformationElement> DefenseInfo
        {
            get => _defenseInfo;
            set
            {
                if (value != _defenseInfo)
                {
                    _defenseInfo = value;
                    base.OnPropertyChangedWithValue(value, "DefenseInfo");
                }
            }
        }

        [DataSourceProperty]
        public string Militarism => base.FormatValue(this.data.MilitaryData.Militarism.ResultNumber);

        [DataSourceProperty]
        public string DraftEfficiency => base.FormatValue(this.data.MilitaryData.DraftEfficiency.ResultNumber);

        [DataSourceProperty]
        public string Manpower => base.FormatValue(this.data.MilitaryData.Manpower);

        [DataSourceProperty]
        public string PeasantManpower => base.FormatValue(this.data.MilitaryData.PeasantManpower);

        [DataSourceProperty]
        public string NobleManpower => base.FormatValue(this.data.MilitaryData.NobleManpower);
    }
}
