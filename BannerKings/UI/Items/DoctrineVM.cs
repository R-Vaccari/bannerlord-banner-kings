using BannerKings.Managers.Institutions.Religions.Doctrines;
using TaleWorlds.Library;

namespace BannerKings.UI.Items
{
    public class DoctrineVM : ViewModel
    {
        private Doctrine doctrine;

        public DoctrineVM(Doctrine doctrine)
        {
            this.doctrine = doctrine;
        }

        [DataSourceProperty]
        public string Name => doctrine.Name.ToString();

        [DataSourceProperty]
        public string Description => doctrine.Description.ToString();

        [DataSourceProperty]
        public string Effects => doctrine.Effects.ToString();
    }
}
