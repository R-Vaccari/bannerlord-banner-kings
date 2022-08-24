using System.Collections.Generic;
using BannerKings.Managers.Institutions.Religions.Faiths.Asera;
using BannerKings.Managers.Institutions.Religions.Faiths.Battania;
using BannerKings.Managers.Institutions.Religions.Faiths.Empire;
using BannerKings.Managers.Institutions.Religions.Faiths.Rites;
using BannerKings.Managers.Institutions.Religions.Faiths.Vlandia;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions.Faiths
{
    public class DefaultFaiths
    {
        private PolytheisticFaith amraFaith, canticlesFaith;
        private MonotheisticFaith aseraCode, darusosian;
        private FaithGroup aseraGroup, battaniaGroup, imperialGroup, vlandiaGroup;

        public Faith AseraCode => aseraCode;
        public Faith AmraOllahm => amraFaith;
        public Faith Darusosian => darusosian;
        public Faith Canticles => canticlesFaith;

        public FaithGroup ImperialGroup => imperialGroup;

        public IEnumerable<Faith> All
        {
            get
            {
                yield return AseraCode;
                yield return AmraOllahm;
                yield return Darusosian;
                yield return Canticles;
            }
        }

        public static DefaultFaiths Instance => ConfigHolder.CONFIG;

        public void Initialize()
        {
            aseraGroup = new FaithGroup(new TextObject("{=CvkBdY8X9}Aseran Faiths"),
                new TextObject("{=V7Sp1gkOY}Those that believe in Asera as the true and only prohpet."));
            aseraCode = new AseraFaith();

            Rite zabiha = new Zabiha();
            aseraCode.Initialize(DefaultDivinities.Instance.AseraMain,
                new List<Divinity>
                {
                    DefaultDivinities.Instance.AseraSecondary1, DefaultDivinities.Instance.AseraSecondary2,
                    DefaultDivinities.Instance.AseraSecondary3
                },
                new Dictionary<TraitObject, bool>
                {
                    {DefaultTraits.Honor, true},
                    {DefaultTraits.Valor, true}
                },
                aseraGroup, new List<Rite> {zabiha});

            battaniaGroup = new FaithGroup(new TextObject("{=ohDtt8sni}Derwyddon Faiths"),
                new TextObject("{=QjS9x19Dp}The faiths in the true old Calradian gods."));
            amraFaith = new AmraFaith();

            Rite ironOffering = new Offering(DefaultItems.IronOre, 100);
            amraFaith.Initialize(DefaultDivinities.Instance.AmraMain,
                new List<Divinity> {DefaultDivinities.Instance.AmraSecondary1, DefaultDivinities.Instance.AmraSecondary2},
                new Dictionary<TraitObject, bool>
                {
                    {DefaultTraits.Honor, false},
                    {DefaultTraits.Valor, true}
                },
                battaniaGroup, new List<Rite> {ironOffering});


            imperialGroup = new FaithGroup(new TextObject("{=bqrxWfP3F}Calradian Faiths"),
                new TextObject("{=GBFn0er0u}The Imperial Calradian faiths."));
            darusosian = new DarusosianFaith();

            darusosian.Initialize(DefaultDivinities.Instance.DarusosianMain,
                new List<Divinity>
                    {DefaultDivinities.Instance.DarusosianSecondary1, DefaultDivinities.Instance.DarusosianSecondary2},
                new Dictionary<TraitObject, bool>
                {
                    {DefaultTraits.Honor, true},
                    {DefaultTraits.Mercy, true}
                },
                imperialGroup);


            vlandiaGroup = new FaithGroup(new TextObject("{=pTJDrgBxV}Vlandic Faiths"),
                new TextObject("{=xwMkswcbh}The faiths brought through the seas by the Vlandic peoples."));
            canticlesFaith = new CanticlesFaith();
            canticlesFaith.Initialize(DefaultDivinities.Instance.VlandiaMain,
                new List<Divinity>
                    {DefaultDivinities.Instance.VlandiaSecondary1, DefaultDivinities.Instance.VlandiaSecondary2},
                new Dictionary<TraitObject, bool>
                {
                    {DefaultTraits.Mercy, false},
                    {DefaultTraits.Valor, true}
                },
                vlandiaGroup, new List<Rite>());
        }

        public Faith GetById(string id)
        {
            Faith faith = null;
            foreach (var f in All)
            {
                if (f.GetId() == id)
                {
                    faith = f;
                }
            }

            return faith;
        }

        internal struct ConfigHolder
        {
            public static DefaultFaiths CONFIG = new();
        }
    }
}