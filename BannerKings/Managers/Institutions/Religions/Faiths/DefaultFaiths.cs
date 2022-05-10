using BannerKings.Managers.Institutions.Religions.Faiths.Asera;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions.Faiths
{
    public class DefaultFaiths
    {
        private FaithGroup aseraGroup, battaniaGroup;
        private MonotheisticFaith aseraCode;
        private PolytheisticFaith amraFaith;

        public Faith AseraCode => aseraCode;
        public Faith AmraOllahm => amraFaith;


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

            battaniaGroup = new FaithGroup(new TextObject("{=!}Derwyddon Faiths"), new TextObject("{=!}The faith in the old Calradian gods."));
            amraFaith = new AmraFaith();

            amraFaith.Initialize(DefaultDivinities.Instance.AmraMain,
                new List<Divinity>() { DefaultDivinities.Instance.AmraSecondary1, DefaultDivinities.Instance.AmraSecondary2 },
                new Dictionary<TraitObject, bool>
                {
                    { DefaultTraits.Honor, false },
                    { DefaultTraits.Valor, true }
                },
                battaniaGroup,
                new Dictionary<int, CharacterObject>());

            
        }

        public static DefaultFaiths Instance => ConfigHolder.CONFIG;

        internal struct ConfigHolder
        {
            public static DefaultFaiths CONFIG = new DefaultFaiths();
        }
    }
}
