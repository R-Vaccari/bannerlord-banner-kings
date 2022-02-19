
using BannerKings.UI.Windows;
using SandBox.View.Map;

namespace BannerKings.UI
{
    class UIManager
    {
        private static UIManager instance;

        public static UIManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new UIManager();
                return instance;
            }
            set
            {
                instance = value;
            }
        }

        private BannerKingsMapView mapView;

        public void ShowWindow(string id)
        {
            if (MapScreen.Instance != null)
            {
                if (this.mapView == null) this.mapView = new BannerKingsMapView(id);
                else if (this.mapView.id != id) this.mapView = new BannerKingsMapView(id);
                this.mapView.Refresh();
            }
        }

        public void CloseUI()
        {
            if (mapView != null)
            {
                mapView.Close();
                mapView = null;
            }
        }
    }
}
