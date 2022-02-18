using BannerKings.UI.Panels;
using SandBox.View.Map;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Engine.Screens;
using TaleWorlds.Library;

namespace BannerKings.UI
{
    public class GuildWindow : MapView
    {

        private GauntletLayer layer;
        private ViewModel datasource;

        protected override void CreateLayout()
        {
            base.CreateLayout();
            layer = new GauntletLayer(550, "GauntletLayer", false);
            Settlement current = Settlement.CurrentSettlement;
            datasource = new GuildVM(BannerKingsConfig.Instance.PopulationManager.GetPopData(current));
            layer.LoadMovie("GuildWindow", datasource);

            layer.InputRestrictions.SetInputRestrictions(false, InputUsageMask.All);
            MapScreen.Instance.AddLayer(layer);
            ScreenManager.TrySetFocus(layer);
        }

        public GuildWindow() => CreateLayout();

        public void CloseUi()
        {
            MapScreen.Instance.RemoveLayer(layer);
        }

        public void UpdateUi()
        {
            this.datasource.RefreshValues();
        }
    }
}
