using System.Collections.Generic;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.ObjectSystem;

namespace BannerKings.Managers.Innovations.Eras
{
    public class BKTroopAdvancement
    {
        public BKTroopAdvancement(CharacterObject character, string equipmentId)
        {
            Character = character;
            UpgradeEquipment = MBObjectManager.Instance.GetObject<MBEquipmentRoster>(equipmentId);
        }

        public CharacterObject Character { get; private set; }
        public CultureObject Culture => Character.Culture;
        public MBEquipmentRoster UpgradeEquipment { get; private set; }

        internal void SetEquipment()
        {
            MBEquipmentRoster equipmentRoster = (MBEquipmentRoster)Character.GetType().BaseType
                           .GetField("_equipmentRoster", BindingFlags.Instance | BindingFlags.NonPublic)
                           .GetValue(Character);
            List<Equipment> rosterEquipment = (List<Equipment>)equipmentRoster.GetType().GetField("_equipments", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(equipmentRoster);
            rosterEquipment.Clear();

            List<Equipment> equipmentsToAdd = (List<Equipment>)equipmentRoster.GetType().GetField("_equipments", BindingFlags.Instance | BindingFlags.NonPublic)
               .GetValue(UpgradeEquipment);
            foreach (Equipment equipment in equipmentsToAdd)
                rosterEquipment.Add(equipment);
        }
    }
}
