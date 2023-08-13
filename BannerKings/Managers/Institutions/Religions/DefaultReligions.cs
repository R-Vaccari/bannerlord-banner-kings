using BannerKings.Managers.Institutions.Religions.Doctrines;
using BannerKings.Managers.Institutions.Religions.Faiths;
using BannerKings.Managers.Institutions.Religions.Leaderships;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;

namespace BannerKings.Managers.Institutions.Religions
{
    public class DefaultReligions : DefaultTypeInitializer<DefaultReligions, Religion>
    {
        public Religion AseraCode { get; } = new Religion("asera");
        public Religion Amra { get; } = new Religion("amra");
        public Religion Martyrdom { get; } = new Religion("darusosian");
        public Religion Canticles { get; } = new Religion("canticles");
        public Religion Treelore { get; } = new Religion("treelore");
        public Religion Osfeyd { get; } = new Religion("osfeyd");

        public override IEnumerable<Religion> All
        {
            get
            {
                yield return AseraCode;
                yield return Amra;
                yield return Martyrdom;
                yield return Canticles;
                yield return Treelore;
                yield return Osfeyd;
                foreach (Religion item in ModAdditions)
                {
                    yield return item;
                }
            }
        }

        public override void Initialize()
        {
            var aserai = Utils.Helpers.GetCulture("aserai");
            var khuzait = Utils.Helpers.GetCulture("khuzait");
            var imperial = Utils.Helpers.GetCulture("empire");
            var battania = Utils.Helpers.GetCulture("battania");
            var vlandia = Utils.Helpers.GetCulture("vlandia");
            var sturgia = Utils.Helpers.GetCulture("sturgia");
            var vakken = Utils.Helpers.GetCulture("vakken");
            var swadia = Utils.Helpers.GetCulture("swadia");
            var rhodok = Utils.Helpers.GetCulture("rhodok");
            var osrickin = Utils.Helpers.GetCulture("osrickin");

            AseraCode.Initialize(DefaultFaiths.Instance.AseraCode, 
                new KinshipLeadership(),
                new List<CultureObject> { aserai, khuzait, imperial },
                new List<Doctrine> 
                { 
                    DefaultDoctrines.Instance.Literalism,
                    DefaultDoctrines.Instance.Legalism,
                    DefaultDoctrines.Instance.HeathenTax
                },
                null);

            Amra.Initialize(DefaultFaiths.Instance.AmraOllahm, 
                new AutonomousLeadership(),
                new List<CultureObject> { battania },
                new List<Doctrine> 
                {
                    DefaultDoctrines.Instance.Druidism,
                    DefaultDoctrines.Instance.Animism
                },
                null);

            Treelore.Initialize(DefaultFaiths.Instance.Treelore,
                new AutonomousLeadership(),
                new List<CultureObject> { sturgia, vakken },
                new List<Doctrine> 
                {
                    DefaultDoctrines.Instance.Druidism,
                    DefaultDoctrines.Instance.Animism
                },
                null);

            Martyrdom.Initialize(DefaultFaiths.Instance.Darusosian, 
                new HierocraticLeadership(),
                new List<CultureObject> { imperial },
                new List<Doctrine> 
                {
                    DefaultDoctrines.Instance.Legalism,
                    DefaultDoctrines.Instance.Childbirth
                },
                null);

            Canticles.Initialize(DefaultFaiths.Instance.Canticles, 
                new HierocraticLeadership(),
                new List<CultureObject> { vlandia },
                new List<Doctrine> 
                {
                    DefaultDoctrines.Instance.Literalism
                },
                null);

            Osfeyd.Initialize(DefaultFaiths.Instance.Treelore,
                new AutonomousLeadership(),
                new List<CultureObject> { vlandia, vakken },
                new List<Doctrine>
                {
                    DefaultDoctrines.Instance.OsricsVengeance,
                    DefaultDoctrines.Instance.Animism
                },
                null);
        }
    }
}
