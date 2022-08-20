using System;
using BannerKings.Managers.Populations;
using BannerKings.UI.CampaignStart;
using BannerKings.UI.Management;
using BannerKings.UI.Management.BannerKings.UI.Panels;
using BannerKings.UI.Panels;
using BannerKings.UI.Religion;
using BannerKings.UI.Titles;
using SandBox.View.Map;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.ScreenSystem;

namespace BannerKings.UI
{
    public class BannerKingsMapView : MapView
    {
        private BannerKingsViewModel datasource;
        public string id;
        private GauntletLayer layer;

        public BannerKingsMapView(string id)
        {
            this.id = id;
            CreateLayout();
        }

        protected override void CreateLayout()
        {
            base.CreateLayout();
            layer = new GauntletLayer(550);
            var tuple = GetVM(id);
            datasource = tuple.Item1;
            layer.LoadMovie(tuple.Item2, datasource);

            layer.InputRestrictions.SetInputRestrictions(false);
            MapScreen.Instance.AddLayer(layer);
            ScreenManager.TrySetFocus(layer);
        }

        private (BannerKingsViewModel, string) GetVM(string id)
        {
            PopulationData data = null;
            if (Settlement.CurrentSettlement != null)
            {
                data = BannerKingsConfig.Instance.PopulationManager.GetPopData(Settlement.CurrentSettlement);
            }

            if (id == "population")
            {
                return (new PopulationVM(data), "PopulationWindow");
            }

            if (id == "guild")
            {
                return (new GuildVM(data), "GuildWindow");
            }

            if (id == "vilage_project")
            {
                return (new VillageProjectVM(data), "VillageProjectWindow");
            }

            if (id == "titles")
            {
                var title = BannerKingsConfig.Instance.TitleManager.GetTitle(Settlement.CurrentSettlement);
                if (title == null)
                {
                    title = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(Clan.PlayerClan.Kingdom);
                }

                return (new DemesneHierarchyVM(title.sovereign != null ? title.sovereign : title, Clan.PlayerClan.Kingdom),
                    "TitlesWindow");
            }

            if (id == "religions")
            {
                return (new ReligionVM(data), "ReligionWindow");
            }

            if (id == "campaignStart")
            {
                return new ValueTuple<BannerKingsViewModel, string>(new CampaignStartVM(), "CampaignStartPopup");
            }

            return (new PopulationVM(data), "PopulationWindow");
        }

        public void Close()
        {
            MapScreen.Instance.RemoveLayer(layer);
        }

        public void Refresh()
        {
            datasource.RefreshValues();
        }
    }
}