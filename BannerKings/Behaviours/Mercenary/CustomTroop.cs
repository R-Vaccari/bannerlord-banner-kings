using HarmonyLib;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace BannerKings.Behaviours.Mercenary
{
    internal class CustomTroop
    {
        internal CustomTroop(CharacterObject character)
        {
            Character = character;
            SetEquipment(character);
        }

        public void PostInitialize()
        {
            SetEquipment(Character, Equipments);
            SetName(Name);
        }

        public void SetEquipment(CharacterObject character, List<Equipment> equipments = null)
        {
            Name = character.Name;
            if (equipments == null)
            {
                MBEquipmentRoster roster = (MBEquipmentRoster)AccessTools.Field((character as BasicCharacterObject).GetType(), "_equipmentRoster")
                       .GetValue(character);
                Equipments = (List<Equipment>)AccessTools.Field(roster.GetType(), "_equipments")
                    .GetValue(roster);
            }

            if (Equipments.Count != 5)
            {
                var diff = Equipments.Count - 5;
                if (diff > 0)
                {
                    Equipments.RemoveRange(0, diff);
                }
                else for (int i = 0; i < diff; i++)
                {
                    Equipments.Add(new Equipment());
                }
            }
        }

        public void SetName(TextObject text)
        {
            AccessTools.Method((Character as BasicCharacterObject).GetType(), "SetName")
                            .Invoke(Character, new object[] { text });
            Name = text;
        }

        [SaveableProperty(1)] public CharacterObject Character { get; set; }
        [SaveableProperty(2)] public TextObject Name { get; set; }
        [SaveableProperty(3)] public List<Equipment> Equipments { get; set; }
    }
}
