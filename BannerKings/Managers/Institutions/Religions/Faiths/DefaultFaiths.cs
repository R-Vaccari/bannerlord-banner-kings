using BannerKings.Managers.Institutions.Religions.Faiths.Asera;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

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
            aseraGroup = new FaithGroup(new TextObject("{=!}Aseran Faiths"), new TextObject("{=!}Those that believe in Asera as the true and only prohpet."));
            aseraCode = new AseraFaith();

            aseraCode.Initialize(DefaultDivinities.Instance.Asera, 
                new Dictionary<TraitObject, bool>
                { 
                    { DefaultTraits.Honor, true }, 
                    { DefaultTraits.Valor, true } 
                }, 
                aseraGroup,
                new Dictionary<int, CharacterObject>());
        }

        public Faith AseraCode => aseraCode;
    }
}
