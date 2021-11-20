
using SandBox.View.Map;

namespace Populations.UI
{
    class UIManager
    {
        private static UIManager _instance;

        public static UIManager instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new UIManager();
                }
                return _instance;
            }
            set
            {
                _instance = value;
            }
        }

        public PopulationWindow populationWindow;

        public void InitializePopulationWindow()
        {
            if (MapScreen.Instance != null)
            {
                if (this.populationWindow == null) this.populationWindow = new PopulationWindow();
                this.populationWindow.UpdateUi();
            }
        }

        public void CloseUI()
        {
            if (populationWindow != null)
            {
                populationWindow.CloseUi();
                populationWindow = null;
            }
        }
    }
}
