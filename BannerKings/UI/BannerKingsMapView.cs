using System;
using BannerKings.Managers.Populations;
using BannerKings.UI.CampaignStart;
using BannerKings.UI.Estates;
using BannerKings.UI.Management;
using BannerKings.UI.Management.BannerKings.UI.Panels;
using BannerKings.UI.Marriages;
using BannerKings.UI.Panels;
using BannerKings.UI.Titles;
using SandBox.View.Map;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.ScreenSystem;
using ReligionVM = BannerKings.UI.Religion.ReligionVM;

namespace BannerKings.UI
{
    public class BannerKingsMapView : MapView
    {
        public string id;
        private GauntletLayer Layer { get; set; }
        private BannerKingsViewModel VM { get; set; }

        public BannerKingsMapView(string id)
        {
            this.id = id;
            CreateLayout();
        }

        protected override void CreateLayout()
        {
            base.CreateLayout();
            //UIManager.Instance.BKScreen.OnFinalize();
            var tuple = GetVM(id);
            Layer = new GauntletLayer(999);
            VM = tuple.Item1;
            Layer.LoadMovie(tuple.Item2, tuple.Item1);
            Layer.InputRestrictions.SetInputRestrictions(false);
            MapScreen.Instance.AddLayer(Layer);
            ScreenManager.TrySetFocus(Layer);
        }

        private (BannerKingsViewModel, string) GetVM(string id)
        {
            PopulationData data = null;
            if (Settlement.CurrentSettlement != null)
            {
                data = BannerKingsConfig.Instance.PopulationManager.GetPopData(Settlement.CurrentSettlement);
            }

            switch (id)
            {
                case "population":
                    return (new PopulationVM(data), "PopulationWindow");
                case "estates":
                    return (new EstatesVM(data), "EstatesWindow");
                case "guild":
                    return (new GuildVM(data), "GuildWindow");
                case "vilage_project":
                    return (new VillageProjectVM(data), "VillageProjectWindow");
                case "titles":
                {
                    var title = BannerKingsConfig.Instance.TitleManager.GetTitle(Settlement.CurrentSettlement);
                    if (title == null)
                    {
                        title = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(Clan.PlayerClan.Kingdom);
                    }

                    return (new DemesneHierarchyVM(title.sovereign ?? title, Clan.PlayerClan.Kingdom),
                        "TitlesWindow");
                }
                case "religions":
                    return (new ReligionVM(data), "ReligionPopup");
                case "campaignStart":
                    return new ValueTuple<BannerKingsViewModel, string>(new CampaignStartVM(), "CampaignStartPopup");
                case "marriage":
                    return new ValueTuple<BannerKingsViewModel, string>(new MarriageContractProposalVM(Hero.OneToOneConversationHero),
                        "MarriageProposalWindow");
                default:
                    return (new PopulationVM(data), "PopulationWindow");
            }
        }

        public void Close()
        {
            if (Layer != null)
            {
                MapScreen.Instance.RemoveLayer(Layer);
            }
        }

        public void Refresh()
        {
            if (VM != null)
            {
                VM.RefreshValues();
            }
        }
    }
}