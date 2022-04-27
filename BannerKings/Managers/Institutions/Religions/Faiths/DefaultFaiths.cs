using BannerKings.Managers.Institutions.Religions.Faiths.Asera;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;
using System.Linq;

namespace BannerKings.Managers.Institutions.Religions.Faiths
{
    public class DefaultFaiths
    {
        private FaithGroup aseraGroup;
        private MonotheisticFaith aseraCode;

        public static DefaultFaiths Instance => ConfigHolder.CONFIG;

        internal struct ConfigHolder
        {
            public static DefaultFaiths CONFIG = new DefaultFaiths();
        }

        public void Initialize()
        {
            this.aseraGroup = new FaithGroup(new TextObject("{=!}Code of Asera"), new TextObject("{=!}Those that believe in Asera as the true and only prohpet."));
            this.aseraCode = new AseraFaith();

            this.aseraCode.Initialize(DefaultDivinities.Instance.Asera, 
                new Dictionary<TraitObject, bool>() 
                { 
                    { DefaultTraits.Honor, true }, 
                    { DefaultTraits.Valor, true } 
                }, 
                this.aseraGroup,
                new Dictionary<int, CharacterObject>());
        }

        public Faith AseraCode => this.aseraCode;
    }
}
