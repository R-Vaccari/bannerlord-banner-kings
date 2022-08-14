using BannerKings.Managers.Titles;
using BannerKings.Populations;
using BannerKings.UI.CampaignStart;
using BannerKings.UI.Panels;
using SandBox.View.Map;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Engine.Screens;

namespace BannerKings.UI.Windows
{
    public class BannerKingsMapView : MapView
    {
        private BannerKingsViewModel datasource;
        private GauntletLayer layer;
        public string id;

        public BannerKingsMapView(string id)
        {
            this.id = id;
            CreateLayout();
        }

        protected override void CreateLayout()
        {
            base.CreateLayout();
            layer = new GauntletLayer(550);
            ValueTuple<BannerKingsViewModel, string> tuple = GetVM(id);
            datasource = tuple.Item1;
            layer.LoadMovie(tuple.Item2, datasource);

            layer.InputRestrictions.SetInputRestrictions(false);
            MapScreen.Instance.AddLayer(layer);
            ScreenManager.TrySetFocus(layer);
        }

        private (BannerKingsViewModel, string) GetVM(string id)
        {
            PopulationData data = null;
            if (Settlement.CurrentSettlement != null) data = BannerKingsConfig.Instance.PopulationManager.GetPopData(Settlement.CurrentSettlement);
            if (id == "population")
                return (new PopulationVM(data), "PopulationWindow");
            else if (id == "guild")
                return (new GuildVM(data), "GuildWindow");
            else if (id == "vilage_project")
                return (new VillageProjectVM(data), "VillageProjectWindow");
            else if (id == "titles")
            {

                FeudalTitle title = BannerKingsConfig.Instance.TitleManager.GetTitle(Settlement.CurrentSettlement);
                if (title == null) title = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(Clan.PlayerClan.Kingdom);
                return (new DemesneHierarchyVM(title.sovereign != null ? title.sovereign : title, Clan.PlayerClan.Kingdom), "TitlesWindow");
            }
            else if (id == "religions")
                return (new ReligionVM(data), "ReligionWindow");
            else if (id == "campaignStart")
                return new(new CampaignStartVM(), "CampaignStartPopup");
            else return (new PopulationVM(data), "PopulationWindow");
        }

        public void Close() => MapScreen.Instance.RemoveLayer(layer);
        public void Refresh() => datasource.RefreshValues();
    }
}
