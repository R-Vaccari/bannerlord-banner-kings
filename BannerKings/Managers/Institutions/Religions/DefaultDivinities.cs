using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions
{
    public class DefaultDivinities
    {
        private Divinity asera;

        public static DefaultDivinities Instance => ConfigHolder.CONFIG;

        internal struct ConfigHolder
        {
            public static DefaultDivinities CONFIG = new DefaultDivinities();
        }

        public void Initialize()
        {
            asera = new Divinity(new TextObject("{=!}Asera"), new TextObject("{=!}The god of the Aserai."));
        }

        public Divinity Asera => asera;
    }
}
