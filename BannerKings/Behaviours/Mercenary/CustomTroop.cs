using HarmonyLib;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.CharacterDeveloper;
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

        public void PostInitialize(CultureObject culture)
        {
            Skills.PostInitialize();
            if (Name == null)
            {
                Name = new TextObject("{=gaJDVkvHA}Placeholder").ToString();
            }
            SetName(new TextObject(Name));
            FillCharacter(culture.BasicTroop);
            SetEquipment(Character, Equipments);
            SetSkills(Character, Skills);
        }

        private void FillCharacter(CharacterObject reference)
        {
            Character.Initialize();
            Character.Culture = reference.Culture;

            Character.Age = reference.Age;  
            AccessTools.Method(reference.GetType(), "InitializeHeroBasicCharacterOnAfterLoad")
                .Invoke(Character, new object[] { (reference as BasicCharacterObject) });

            BasicCharacterObject basicCharacter = (BasicCharacterObject)Character;
            basicCharacter.Level = reference.Level; 
            AccessTools.Field(reference.GetType(), "_occupation").SetValue(Character, Occupation.Mercenary);

            AccessTools.Property(reference.GetType(), "UpgradeTargets").SetValue(Character, new CharacterObject[0]);
        }

        public void SetEquipment(CharacterObject character, List<Equipment> equipments = null)
        {
            Name = character.Name.ToString();
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

            if (equipments != null)
            {
                var roster = new MBEquipmentRoster();
                AccessTools.Field(roster.GetType(), "_equipments").SetValue(roster, equipments);
                AccessTools.Field((character as BasicCharacterObject).GetType(), "_equipmentRoster")
                    .SetValue((character as BasicCharacterObject), roster);
            }
        }

        public void SetName(TextObject text)
        {
            AccessTools.Method((Character as BasicCharacterObject).GetType(), "SetName")
                            .Invoke(Character, new object[] { text });
            Name = text.ToString();
        }

        public void SetSkills(CharacterObject character, CustomTroopPreset preset)
        {
            if (preset != null)
            {
                BasicCharacterObject basicCharacter = character;
                basicCharacter.Level = preset.Level;

                MBCharacterSkills skills = new MBCharacterSkills();
                skills.Skills.SetPropertyValue(DefaultSkills.OneHanded, preset.OneHanded);
                skills.Skills.SetPropertyValue(DefaultSkills.TwoHanded, preset.TwoHanded);
                skills.Skills.SetPropertyValue(DefaultSkills.Polearm, preset.Polearm);
                skills.Skills.SetPropertyValue(DefaultSkills.Riding, preset.Riding);
                skills.Skills.SetPropertyValue(DefaultSkills.Athletics, preset.Athletics);
                skills.Skills.SetPropertyValue(DefaultSkills.Bow, preset.Bow);
                skills.Skills.SetPropertyValue(DefaultSkills.Crossbow, preset.Crossbow);
                skills.Skills.SetPropertyValue(DefaultSkills.Throwing, preset.Throwing);

                AccessTools.Field((character as BasicCharacterObject).GetType(), "CharacterSkills")
                        .SetValue(character, skills);

                Skills = preset;
            }
        }

        [SaveableProperty(1)] public CharacterObject Character { get; set; }
        [SaveableProperty(2)] public string Name { get; set; }
        [SaveableProperty(3)] public List<Equipment> Equipments { get; set; }
        [SaveableProperty(4)] public CustomTroopPreset Skills { get; set; }
    }
}
