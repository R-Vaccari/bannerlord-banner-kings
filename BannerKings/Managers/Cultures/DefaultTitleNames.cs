using System.Collections.Generic;
using static BannerKings.Managers.PopulationManager;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;
using BannerKings.Managers.Titles;

namespace BannerKings.Managers.Cultures
{
    public class DefaultTitleNames : DefaultTypeInitializer<DefaultTitleNames, CulturalTitleName>
    {
        public CulturalTitleName DefaultEmpire { get; private set; }
        public CulturalTitleName DefaultKing { get; private set; }
        public CulturalTitleName DefaultDuke { get; private set; }
        public CulturalTitleName DefaultCount { get; private set; }
        public CulturalTitleName DefaultBaron { get; private set; }
        public CulturalTitleName DefaultLord { get; private set; }
        public override IEnumerable<CulturalTitleName> All
        {
            get
            {
                foreach (var item in ModAdditions)
                {
                    yield return item;
                }
            }
        }

        public CulturalTitleName GetPopulationName(CultureObject culture, TitleType titleType)
        {
            CulturalTitleName name = All.FirstOrDefault(x => x.Culture == culture && titleType == x.TitleType);
            return name != null ? name : All.First(x => x.Culture == null && titleType == x.TitleType);
        }

        public override void Initialize()
        {
            DefaultEmpire = CulturalTitleName.CreateEmpire("DefaultEmpire",
                null,
                new TextObject("{=9WOQTiBr}Emperor"),
                new TextObject("{=gbpokx6s}Empress"),
                new TextObject("{=!}Empire"));

            DefaultKing = CulturalTitleName.CreateKingdom("DefaultKing",
                null,
                new TextObject("{=zyKSROjQ}King"),
                new TextObject("{=JmdALFU2}Queen"),
                new TextObject("{=7x3HJ29f}Kingdom"));

            DefaultDuke = CulturalTitleName.CreateDuchy("DefaultDuke",
                null,
                new TextObject("{=vFJ7NjqE}Duke"),
                new TextObject("{=5uFw1EmO}Duchess"),
                new TextObject("{=HtWGKBDF}Dukedom"));

            DefaultCount = CulturalTitleName.CreateCounty("DefaultCount",
                null,
                new TextObject("{=hG2krbg9}Count"),
                new TextObject("{=o513XU29}Countess"),
                new TextObject("{=c6ggHVzS}County"));

            DefaultBaron = CulturalTitleName.CreateBarony("DefaultBaron",
                null,
                new TextObject("{=LvgTvjd1}Baron"),
                new TextObject("{=yxq4RV7E}Baroness"),
                new TextObject("{=qOLmvS0B}Barony"));

            DefaultLord = CulturalTitleName.CreateLordship("DefaultLord",
                null,
                new TextObject("{=Jd1iqDAX}Lord"),
                new TextObject("{=8V8i6QCm}Lady"),
                new TextObject("{=dwMA32rq}Lordship"));
        }
    }
}
