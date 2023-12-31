using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Shipping
{
    public class DefaultShippingLanes : DefaultTypeInitializer<DefaultShippingLanes, ShippingLane>
    {
        public ShippingLane Laconis { get; } = new ShippingLane("Laconis");
        public ShippingLane Perassic { get; } = new ShippingLane("Perassic");
        public ShippingLane Junme { get; } = new ShippingLane("Junme");
        public ShippingLane Western { get; } = new ShippingLane("Western");

        public override IEnumerable<ShippingLane> All
        {
            get
            {
                yield return Laconis;
                yield return Perassic;
                yield return Junme;
                yield return Western;
            }
        }

        public IEnumerable<ShippingLane> GetSettlementLanes(Settlement settlement)
        {
            foreach (ShippingLane lane in All)
                if (lane.Ports.Contains(settlement))
                    yield return lane;
        }

        public IEnumerable<ShippingLane> GetSettlementSeaLanes(Settlement settlement)
        {
            foreach (ShippingLane lane in All)
                if (!lane.IsRiver && lane.Ports.Contains(settlement))
                    yield return lane;
        }

        public IEnumerable<ShippingLane> GetSettlementRiverLanes(Settlement settlement)
        {
            foreach (ShippingLane lane in All)
                if (lane.IsRiver && lane.Ports.Contains(settlement))
                    yield return lane;
        }

        public override void Initialize()
        {
            Laconis.Initialize(new TextObject("{=ZJBYtrAB}Laconian Shipping Network"),
                new TextObject(),
                new List<Settlement>()
                {
                    Settlement.All.First(x => x.StringId == "town_S4"),
                    Settlement.All.First(x => x.StringId == "town_EN2"),
                    Settlement.All.First(x => x.StringId == "village_EN4_2"),
                    Settlement.All.First(x => x.StringId == "village_S3_2")
                });

            Western.Initialize(new TextObject("{=tySxydya}Western Sea Network"),
                new TextObject(),
                new List<Settlement>()
                {
                    Settlement.All.First(x => x.StringId == "town_V7"),
                    Settlement.All.First(x => x.StringId == "town_V8")
                });

            Junme.Initialize(new TextObject("{=FGXR8tdb}Junme Trade Network"),
                new TextObject(),
                new List<Settlement>()
                {
                    Settlement.All.First(x => x.StringId == "town_S2"),
                    Settlement.All.First(x => x.StringId == "town_V8")
                },
                false,
                Utils.Helpers.GetCulture("nord"));

            Perassic.Initialize(new TextObject("{=TFoGRBnG}Perassic Trade Network"),
                new TextObject(),
                new List<Settlement>()
                {
                    Settlement.All.First(x => x.StringId == "town_ES2"),
                    Settlement.All.First(x => x.StringId == "town_A4"),
                    Settlement.All.First(x => x.StringId == "town_A8"),
                    Settlement.All.First(x => x.StringId == "town_EW2"),
                    Settlement.All.First(x => x.StringId == "town_EW4"),
                    Settlement.All.First(x => x.StringId == "town_A1"),
                    Settlement.All.First(x => x.StringId == "town_A6")
                });
        }
    }
}
