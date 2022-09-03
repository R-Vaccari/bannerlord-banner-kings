using SandBox.View.Map;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.ScreenSystem;
using TaleWorlds.TwoDimension;

namespace BannerKings.UI
{
    internal class BannerKingsScreen : GlobalLayer
    {
        public string Id { get; set; }
        private BannerKingsViewModel datasource;
        private GauntletLayer gauntletLayer;
        private IGauntletMovie gauntletMovie;
        private SpriteCategory categoryDeveloper, categoryEncyclopedia;

        public BannerKingsScreen()
        {
            var spriteData = UIResourceManager.SpriteData;
            var resourceContext = UIResourceManager.ResourceContext;
            var resourceDepot = UIResourceManager.UIResourceDepot;

            categoryDeveloper = spriteData.SpriteCategories["ui_characterdeveloper"];
            categoryDeveloper.Load(resourceContext, resourceDepot);

            categoryEncyclopedia = spriteData.SpriteCategories["ui_encyclopedia"];
            categoryEncyclopedia.Load(resourceContext, resourceDepot);

            gauntletLayer = new GauntletLayer(550);
            Layer = (ScreenLayer)gauntletLayer;
        }

        private void Load()
        {
            var resourceContext = UIResourceManager.ResourceContext;
            var resourceDepot = UIResourceManager.UIResourceDepot;
            categoryDeveloper.Load(resourceContext, resourceDepot);
            categoryEncyclopedia.Load(resourceContext, resourceDepot);
        }

        public void Refresh()
        {
            datasource.RefreshValues();
        }

        public void LoadLayer(BannerKingsViewModel vm, string xml)
        {
            datasource = vm;
            gauntletMovie = gauntletLayer.LoadMovie(xml, vm);

            gauntletLayer.InputRestrictions.SetInputRestrictions(false);
            ScreenManager.AddGlobalLayer(this, true);
            Load();
            MapScreen.Instance.AddLayer(gauntletLayer);     
        }

        public void CloseLayer()
        {
            MapScreen.Instance.RemoveLayer(gauntletLayer);
            ScreenManager.RemoveGlobalLayer(this);
        }

        public void OnFinalize()
        {
            gauntletMovie = null;
            categoryDeveloper.Unload();
            gauntletLayer = null;
        }
    }
}
