using SandBox.View.Map;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Engine.Screens;
using TaleWorlds.Library;

namespace BannerKings
{
    namespace UI
    {
        public class PopulationWindow : MapView
        {
            private GauntletLayer layer;
            private ViewModel datasource;

            protected override void CreateLayout()
            {
                base.CreateLayout();
                layer = new GauntletLayer(550, "GauntletLayer", false);
                Settlement current = Settlement.CurrentSettlement;
                if (!current.IsVillage)
                {
                    datasource = new PopulationVM(current);
                    layer.LoadMovie("PopulationWindow", datasource);

                } else
                {
                    datasource = new PopulationVillageVM(current);
                    layer.LoadMovie("PopulationVilageWindow", datasource);
                }
                
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
