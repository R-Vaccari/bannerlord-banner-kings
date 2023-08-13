using System.Collections.Generic;
using BannerKings.Managers.Institutions.Religions.Faiths.Asera;
using BannerKings.Managers.Institutions.Religions.Faiths.Battania;
using BannerKings.Managers.Institutions.Religions.Faiths.Empire;
using BannerKings.Managers.Institutions.Religions.Faiths.Northern;
using BannerKings.Managers.Institutions.Religions.Faiths.Rites;
using BannerKings.Managers.Institutions.Religions.Faiths.Rites.Battania;
using BannerKings.Managers.Institutions.Religions.Faiths.Rites.Empire;
using BannerKings.Managers.Institutions.Religions.Faiths.Rites.Northern;
using BannerKings.Managers.Institutions.Religions.Faiths.Rites.Southern;
using BannerKings.Managers.Institutions.Religions.Faiths.Rites.Vlandia;
using BannerKings.Managers.Institutions.Religions.Faiths.Vlandia;
using BannerKings.Managers.Traits;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Religions.Faiths
{
    public class DefaultFaiths
    {
        private FaithGroup aseraGroup, battaniaGroup, imperialGroup, vlandiaGroup, sturgiaGroup;

        public MonotheisticFaith AseraCode { get; private set; }
        public PolytheisticFaith AmraOllahm { get; private set; }
        public MonotheisticFaith Darusosian { get; private set; }
        public PolytheisticFaith Canticles { get; private set; }
        public PolytheisticFaith Treelore { get; private set; }
        public PolytheisticFaith Osfeyd { get; private set; } = new Osfeyd();

        public FaithGroup ImperialGroup => imperialGroup;

        public IEnumerable<Faith> All
        {
            get
            {
                yield return AseraCode;
                yield return AmraOllahm;
                yield return Darusosian;
                yield return Canticles;
                yield return Treelore;
                yield return Osfeyd;
            }
        }

        public static DefaultFaiths Instance => ConfigHolder.CONFIG;

        public void Initialize()
        {
            aseraGroup = new FaithGroup(new TextObject("{=UAeorLSO}Aseran Faiths"),
                new TextObject("{=9bKowx3B}Those that believe in Asera as the true and only prohpet."));
            AseraCode = new AseraFaith();

            ContextualRite zabiha = new Zabiha();
            AseraCode.Initialize(DefaultDivinities.Instance.AseraMain,
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

            battaniaGroup = new FaithGroup(new TextObject("{=GbQpgQat}Derwyddon Faiths"),
                new TextObject("{=ZonhX1rf}The faiths in the true old Calradian gods."));
            AmraOllahm = new AmraFaith();
            AmraOllahm.Initialize(DefaultDivinities.Instance.AmraMain,
                new List<Divinity> {DefaultDivinities.Instance.AmraSecondary1, DefaultDivinities.Instance.AmraSecondary2},
                new Dictionary<TraitObject, bool>
                {
                    {DefaultTraits.Honor, false},
                    {DefaultTraits.Valor, true}
                },
                battaniaGroup, 
                new List<Rite> 
                { 
                    new IronOffering(), 
                    new GreatSwordOffering()
                });

            imperialGroup = new FaithGroup(new TextObject("{=NWqkTdMt}Calradian Faiths"),
                new TextObject("{=eqvyNpT8}The Imperial Calradian faiths."));
            Darusosian = new DarusosianFaith();
            DarusosianExecution darusosianExecution = new DarusosianExecution();
            Darusosian.Initialize(DefaultDivinities.Instance.DarusosianMain,
                new List<Divinity>
                    {DefaultDivinities.Instance.DarusosianSecondary1, DefaultDivinities.Instance.DarusosianSecondary2},
                new Dictionary<TraitObject, bool>
                {
                    {DefaultTraits.Honor, true},
                    {DefaultTraits.Mercy, true}
                },
                imperialGroup, new List<Rite>() { darusosianExecution },
                Behaviours.Feasts.Feast.FeastType.Astaronia);


            vlandiaGroup = new FaithGroup(new TextObject("{=6FGQ31TM}Vlandic Faiths"),
                new TextObject("{=U4yTbB0J}The faiths brought through the seas by the Vlandic peoples."));
            Canticles = new CanticlesFaith();
            Canticles.Initialize(DefaultDivinities.Instance.VlandiaMain,
                new List<Divinity>
                    {DefaultDivinities.Instance.VlandiaSecondary1, DefaultDivinities.Instance.VlandiaSecondary2},
                new Dictionary<TraitObject, bool>
                {
                    {DefaultTraits.Mercy, false},
                    {DefaultTraits.Valor, true}
                },
                vlandiaGroup, new List<Rite>());
            Canticles.Active = false;

            Osfeyd.Initialize(DefaultDivinities.Instance.Wilund,
                new List<Divinity>
                {
                   DefaultDivinities.Instance.Osric, 
                   DefaultDivinities.Instance.Horsa
                },
                new Dictionary<TraitObject, bool>
                {
                    { DefaultTraits.Mercy, false} ,
                    { DefaultTraits.Valor, true }
                },
                vlandiaGroup,
                new List<Rite>()
                {
                    new VlandiaHorse()
                });

            sturgiaGroup = new FaithGroup(new TextObject(),
                new TextObject());
            Treelore = new TreeloreFaith();
            Treelore.Initialize(DefaultDivinities.Instance.TreeloreMain,
                new List<Divinity>
                {
                    DefaultDivinities.Instance.TreeloreMoon
                },
                new Dictionary<TraitObject, bool>
                {
                    {DefaultTraits.Generosity, true},
                    {DefaultTraits.Valor, true}
                },
                sturgiaGroup,
                new List<Rite>()
                {
                    new TreeloreFestival()
                },
                Behaviours.Feasts.Feast.FeastType.Treelore);
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