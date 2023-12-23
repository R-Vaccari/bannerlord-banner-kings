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

        public Inheritance GetKingdomIdealInheritance(string id, Government government)
        {
            if (id == "empire_s" || id == "aserai")
            {
                return Primogeniture;
            }

            return Seniority;
        }

        public override void Initialize()
        {
            Primogeniture.Initialize(new TextObject("{=!}Primogeniture"),
                new TextObject("{=!}Primogeniture inheritance gives precedence to the children of the deceased, from eldest to youngest. Whether the firstborn son or daughter will be chosen depends on the Gender Law in place."),
                300f,
                150f,
                100f,
                30f,
                1f,
                -0.2f,
                -0.8f);

            Ultimogeniture.Initialize(new TextObject("{=!}Ultimogeniture"),
                new TextObject("{=!}Ultimogeniture inheritance gives precedence to the children of the deceased, from youngest to eldest. Whether the youngest son or daughter will be chosen depends on the Gender Law in place."),
                300f,
                150f,
                100f,
                30f,
                0.5f,
                0.2f,
                -0.5f);

            Seniority.Initialize(new TextObject("{=!}Seniority"),
                new TextObject("{=!}Seniority inheritance gives no precedence to any particular member of a household. The main criteria is their age, meaning that household members without blood ties may take precedence over children or close relatives. Whether the eldest man or woman will be chosen depends on the Gender Law in place."),
                0f,
                0f,
                0f,
                0f,
                -0.4f,
                0.4f,
                1f);
        }
    }
}
