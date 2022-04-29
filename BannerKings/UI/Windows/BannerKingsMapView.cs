﻿using BannerKings.Populations;
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
            PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(Settlement.CurrentSettlement);
            if (id == "population")
                return (new PopulationVM(data), "PopulationWindow");
            if (id == "guild")
                return (new GuildVM(data), "GuildWindow");
            if (id == "vilage_project")
                return (new VillageProjectVM(data), "VillageProjectWindow");
            if (id == "court")
                return (new CourtVM(data), "CourtWindow");
            if (id == "titles")
                return (new TitleWindowVM(data), "TitlesWindow");
            return (new PopulationVM(data), "PopulationWindow");
        }

        public void Close() => MapScreen.Instance.RemoveLayer(layer);
        public void Refresh() => datasource.RefreshValues();
    }
}
