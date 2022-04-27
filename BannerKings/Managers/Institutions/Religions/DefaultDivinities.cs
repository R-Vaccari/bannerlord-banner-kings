using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            this.asera = new Divinity(new TextObject("{=!}Asera"), new TextObject("{=!}The god of the Aserai."));
        }

        public Divinity Asera => this.asera;
    }
}
