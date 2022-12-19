using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers.Populations
{
    public class TournamentData : BannerKingsData
    {
        public TournamentData(Town town)
        {
            active = true;
        }
        [SaveableProperty(2)] public ItemObject Prize { get; private set; }

        [SaveableProperty(3)] private bool active { get; set; }

        public void SetPrize(ItemObject item) => Prize = item;

        public bool Active
        {
            get => active;
            set => active = value;
        }

        internal override void Update(PopulationData data)
        {
            if (!data.Settlement.Town.HasTournament)
            {
                active = false;
            }
        }
    }
}