using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Shipping
{
    public class ShippingLane : BannerKingsObject
    {
        public ShippingLane(string stringId) : base(stringId)
        {
        }

        public void Initialize(TextObject name, TextObject description, List<Settlement> ports,
            bool isRiver = false,
            CultureObject culture = null)
        {
            Initialize(name, description);
            Ports = ports;
            IsRiver = isRiver;
            Culture = culture;
        }

        public bool IsRiver { get; private set; }
        public CultureObject Culture { get; private set; }
        public List<Settlement> Ports { get; private set; }
    }
}
