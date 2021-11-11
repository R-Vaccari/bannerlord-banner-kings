
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

        public PopulationUI populationUI;

        public void InitializeReligionWindow()
        { 
            populationUI = new PopulationUI();
            this.populationUI.UpdateUi();
        }

        public void CloseUI()
        {
            if (populationUI != null)
            {
                populationUI.CloseUi();
            }
        }
    }
}
