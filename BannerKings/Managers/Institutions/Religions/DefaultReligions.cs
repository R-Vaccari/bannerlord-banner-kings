using BannerKings.Managers.Institutions.Religions.Faiths;
using BannerKings.Managers.Institutions.Religions.Leaderships;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem;
using System.Linq;

namespace BannerKings.Managers.Institutions.Religions
{
    public class DefaultReligions : DefaultTypeInitializer<DefaultReligions, Religion>
    {
        public Religion AseraCode { get; } = new Religion("asera_code");
        public Religion Amra { get; } = new Religion("amra");
        public Religion Martyrdom { get; } = new Religion("darusosian_martyrdom");
        public Religion Canticles { get; } = new Religion("canticles");
        public Religion Treelore { get; } = new Religion("treelore");

        public override IEnumerable<Religion> All
        {
            get
            {
                yield return AseraCode;
                yield return Amra;
                yield return Martyrdom;
                yield return Canticles;
                yield return Treelore;
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

            AseraCode.Initialize(DefaultFaiths.Instance.AseraCode, 
                new KinshipLeadership(),
                new List<CultureObject> { aserai, khuzait, imperial },
                new List<string> { "literalism", "legalism", "heathen_tax" },
                null);

            Amra.Initialize(DefaultFaiths.Instance.AmraOllahm, 
                new AutonomousLeadership(),
                new List<CultureObject> { battania },
                new List<string> { "druidism", "animism" },
                null);

            Treelore.Initialize(DefaultFaiths.Instance.Treelore,
                new AutonomousLeadership(),
                new List<CultureObject> { sturgia, vakken },
                new List<string> { "druidism", "animism" },
                null);

            Martyrdom.Initialize(DefaultFaiths.Instance.Darusosian, 
                new HierocraticLeadership(),
                new List<CultureObject> { imperial },
                new List<string> { "legalism", "childbirth" },
                Settlement.All.First(x => x.StringId == "town_ES4"));

            Canticles.Initialize(DefaultFaiths.Instance.Canticles, 
                new HierocraticLeadership(),
                new List<CultureObject> { vlandia },
                new List<string> { "sacrifice", "literalism" },
                null);
        }
    }
}
