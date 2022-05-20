using BannerKings.Managers.Institutions.Religions.Faiths.Asera;
using BannerKings.Managers.Institutions.Religions.Faiths.Battania;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions.Faiths
{
    public class DefaultFaiths
    {
        private FaithGroup aseraGroup, battaniaGroup, imperialGroup;
        private MonotheisticFaith aseraCode, darusosian;
        private PolytheisticFaith amraFaith;

        public Faith AseraCode => aseraCode;
        public Faith AmraOllahm => amraFaith;
        public Faith Darusosian => darusosian;

        public void Initialize()
        {
            aseraGroup = new FaithGroup(new TextObject("{=!}Aseran Faiths"), new TextObject("{=!}Those that believe in Asera as the true and only prohpet."));
            aseraCode = new AseraFaith();

            aseraCode.Initialize(DefaultDivinities.Instance.AseraMain,
                new List<Divinity>() { DefaultDivinities.Instance.AseraSecondary1, DefaultDivinities.Instance.AseraSecondary2, DefaultDivinities.Instance.AseraSecondary3 },
                new Dictionary<TraitObject, bool>
                { 
                    { DefaultTraits.Honor, true }, 
                    { DefaultTraits.Valor, true } 
                }, 
                aseraGroup);

            battaniaGroup = new FaithGroup(new TextObject("{=!}Derwyddon Faiths"), new TextObject("{=!}The faiths in the true old Calradian gods."));
            amraFaith = new AmraFaith();

            amraFaith.Initialize(DefaultDivinities.Instance.AmraMain,
                new List<Divinity>() { DefaultDivinities.Instance.AmraSecondary1, DefaultDivinities.Instance.AmraSecondary2 },
                new Dictionary<TraitObject, bool>
                {
                    { DefaultTraits.Honor, false },
                    { DefaultTraits.Valor, true }
                },
                battaniaGroup);


            imperialGroup = new FaithGroup(new TextObject("{=!}Calradian Faiths"), new TextObject("{=!}The Imperial Calradian faiths."));
            darusosian = new DarusosianFaith();

            darusosian.Initialize(DefaultDivinities.Instance.DarusosianMain,
                new List<Divinity>() { DefaultDivinities.Instance.DarusosianSecondary1, DefaultDivinities.Instance.DarusosianSecondary2 },
                new Dictionary<TraitObject, bool>
                {
                    { DefaultTraits.Honor, false },
                    { DefaultTraits.Valor, true }
                },
                imperialGroup);

        }

        public Faith GetById(string id)
        {
            Faith faith = null;
            foreach (Faith f in All)
                if (f.GetId() == id)
                    faith = f;
            return faith;
        }

        public IEnumerable<Faith> All
        {
            get
            {
                yield return AseraCode;
                yield return AmraOllahm;
                yield return Darusosian;
            }
        }

        public static DefaultFaiths Instance => ConfigHolder.CONFIG;

        internal struct ConfigHolder
        {
            public static DefaultFaiths CONFIG = new DefaultFaiths();
        }
    }
}
