using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Titles.Governments
{
    public class DefaultInheritances : DefaultTypeInitializer<DefaultInheritances, Inheritance>
    {
        public Inheritance Primogeniture { get; } = new Inheritance("Primogeniture");
        public Inheritance Ultimogeniture { get; } = new Inheritance("Ultimogeniture");
        public Inheritance Seniority { get; } = new Inheritance("Seniority");
        public override IEnumerable<Inheritance> All
        {
            get
            {
                yield return Primogeniture;
                yield return Ultimogeniture;
                yield return Seniority;
            }
        }

        public Inheritance GetKingdomIdealInheritance(Kingdom kingdom, Government government)
        {
            string id = kingdom.StringId;
            if (id == "empire_s" || id == "aserai")
            {
                return Primogeniture;
            }

            return Seniority;
        }

        public override void Initialize()
        {
            Primogeniture.Initialize(new TextObject(),
                new TextObject(),
                300f,
                150f,
                100f,
                30f);

            Ultimogeniture.Initialize(new TextObject(),
                new TextObject(),
                300f,
                150f,
                100f,
                30f);

            Seniority.Initialize(new TextObject(),
                new TextObject(),
                300f,
                150f,
                100f,
                30f);
        }
    }
}
