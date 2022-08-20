using BannerKings.Managers.Populations;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Institutions.Religions
{
    public class ReligionData : BannerKingsData
    {
        [SaveableField(2)] private Clergyman clergyman;

        public ReligionData(Religion religion, Settlement settlement)
        {
            Religion = religion;
            Settlement = settlement;
        }

        [field: SaveableField(3)] public Religion Religion { get; }

        [field: SaveableField(1)] public Settlement Settlement { get; }

        public Clergyman Clergyman
        {
            get
            {
                if (clergyman == null)
                {
                    clergyman = Religion.GenerateClergyman(Settlement);
                }

                return clergyman;
            }
        }

        internal override void Update(PopulationData data)
        {
            clergyman = Religion.GetClergyman(data.Settlement);
            if (clergyman == null)
            {
                clergyman = Religion.GenerateClergyman(Settlement);
            }
        }
    }
}