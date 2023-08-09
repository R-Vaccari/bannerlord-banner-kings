using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace BannerKings.Managers.Innovations.Eras
{
    public class BKTroopAdvancement
    {
        public CharacterObject Character { get; private set; }
        public CultureObject Culture => Character.Culture;
        public MBEquipmentRoster UpgradeEquipment { get; private set; }

        internal void SetEquipment()
        {

        }
    }
}
