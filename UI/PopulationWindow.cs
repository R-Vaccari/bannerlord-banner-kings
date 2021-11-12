using SandBox.View.Map;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Engine.Screens;
using TaleWorlds.Library;

namespace Populations
{
    namespace UI
    {
        public class PopulationWindow : MapView
        {
            private GauntletLayer layer;
            private PopulationVM datasource;

            protected override void CreateLayout()
            {
                base.CreateLayout();
                layer = new GauntletLayer(550, "GauntletLayer", false);
                datasource = new PopulationVM(Settlement.CurrentSettlement);
                layer.LoadMovie("PopulationWindow", datasource);
                layer.InputRestrictions.SetInputRestrictions(false, InputUsageMask.All);
                MapScreen.Instance.AddLayer(layer);
                ScreenManager.TrySetFocus(layer);
            }

            public PopulationWindow() => CreateLayout();

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
}
