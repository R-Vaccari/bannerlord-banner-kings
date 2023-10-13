using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
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

        public void Start(Town town)
        {
            var tournamentManager = TaleWorlds.CampaignSystem.Campaign.Current.TournamentManager;
            tournamentManager.AddTournament(TaleWorlds.CampaignSystem.Campaign.Current.Models.TournamentModel.CreateTournament(town));
            Hero.MainHero.ChangeHeroGold(-5000);
            InformationManager.DisplayMessage(new InformationMessage(new TextObject("{=CyF16uSZ}Tournament started with prize: {PRIZE}")
                .SetTextVariable("PRIZE", Prize.Name).ToString(),
                "event:/ui/notification/coins_negative"));
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