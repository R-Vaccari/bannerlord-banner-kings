using SandBox.View.Map;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Engine.Screens;
using TaleWorlds.Library;

namespace Populations
{
    namespace UI
    {
        public class PopulationUI : MapView
        {
            private GauntletLayer layer;
            private PopulationVM datasource;

            public PopulationUI()
            {
                base.CreateLayout();
                layer = new GauntletLayer(550, "GauntletLayer", false);
                datasource = new PopulationVM();
                layer.LoadMovie("ImprovedGarrisonsMenu", datasource);
                layer.InputRestrictions.SetInputRestrictions(false, InputUsageMask.All);
                MapScreen.Instance.AddLayer(layer);
                ScreenManager.TrySetFocus(layer);
            }

        }
    } 
}
