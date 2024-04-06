using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;

namespace BannerKings.Campaign.Culture
{
    public class DefaultCulturalStandings : DefaultTypeInitializer<DefaultCulturalStandings, CulturalStanding>
    {
        public CulturalStanding Vlandia { get; set; }
        public CulturalStanding Battania { get; set; }
        public CulturalStanding Khuzait { get; set; }
        public CulturalStanding Aserai { get; set; }
        public CulturalStanding Empire { get; set; }
        public CulturalStanding Sturgia { get; set; }
        public override IEnumerable<CulturalStanding> All
        {
            get
            {
                yield return Vlandia;
                yield return Battania;
                yield return Khuzait;
                yield return Aserai;
                yield return Empire;
                yield return Sturgia;
                foreach (var item in ModAdditions)   
                    yield return item;
            }
        }

        public int GetRelation(CultureObject culture, CultureObject target)
        {
            int relation = 0;
            CulturalStanding standing = All.FirstOrDefault(x => x.Culture.StringId == culture.StringId);
            if (standing != null) relation = standing.GetStanding(target);

            return relation;
        }

        public override void Initialize()
        {
            var vlandia = Utils.Helpers.GetCulture("vlandia");
            var khuzait = Utils.Helpers.GetCulture("khuzait");
            var battania = Utils.Helpers.GetCulture("battania");
            var sturgia = Utils.Helpers.GetCulture("sturgia");
            var aserai = Utils.Helpers.GetCulture("aserai");
            var empire = Utils.Helpers.GetCulture("empire");

            Vlandia = new CulturalStanding("Vlandia", vlandia);
            Vlandia.AddStanding(battania, -5);
            Vlandia.AddStanding(sturgia, -5);
            Vlandia.AddStanding(aserai, -10);
            Vlandia.AddStanding(empire, -15);
            Vlandia.AddStanding(khuzait, -10);

            Aserai = new CulturalStanding("Aserai", aserai);
            Aserai.AddStanding(battania, -5);
            Aserai.AddStanding(sturgia, -5);
            Aserai.AddStanding(vlandia, -5);
            Aserai.AddStanding(empire, -5);
            Aserai.AddStanding(khuzait, -10);

            Empire = new CulturalStanding("Empire", empire);
            Empire.AddStanding(battania, -10);
            Empire.AddStanding(sturgia, -10);
            Empire.AddStanding(aserai, -10);
            Empire.AddStanding(vlandia, -15);
            Empire.AddStanding(khuzait, -20);

            Battania = new CulturalStanding("Battania", battania);
            Battania.AddStanding(vlandia, -10);
            Battania.AddStanding(sturgia, -5);
            Battania.AddStanding(aserai, -5);
            Battania.AddStanding(empire, -20);
            Battania.AddStanding(khuzait, -10);

            Khuzait = new CulturalStanding("Khuzait", khuzait);
            Khuzait.AddStanding(battania, -5);
            Khuzait.AddStanding(sturgia, -5);
            Khuzait.AddStanding(aserai, -10);
            Khuzait.AddStanding(empire, -15);
            Khuzait.AddStanding(vlandia, -10);

            Sturgia = new CulturalStanding("Sturgia", sturgia);
            Sturgia.AddStanding(battania, -5);
            Sturgia.AddStanding(vlandia, -10);
            Sturgia.AddStanding(aserai, -10);
            Sturgia.AddStanding(empire, -15);
            Sturgia.AddStanding(khuzait, -5);
        }
    }
}
