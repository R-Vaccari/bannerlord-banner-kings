using BannerKings.Behaviours.Mercenary;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using static TaleWorlds.Core.ItemObject;

namespace BannerKings.UI.Mercenary
{
    internal class MercenaryCareerVM : BannerKingsViewModel
    {
        private string reputationText, pointsText, timeText,
            levyCharacterName, professionalCharacterName,
            editLevyText, editProfessionalText,
            privilegeAvailableText, dailyPointsGainText;
        private CharacterViewModel levyCharacter, professionalCharacter;
        private MBBindingList<MercenaryPrivilegeVM> privileges;
        private bool canEditLevy, canEditProfessional,
            canAskPrivilege, levyVisible, professionalVisible,
            showNoPrivilegesText;
        private HintViewModel dailyPointsGainHint;
        public MercenaryCareerVM() : base(null, false)
        {
            Career = Campaign.Current.GetCampaignBehavior<BKMercenaryCareerBehavior>().GetCareer(Clan.PlayerClan);
            Privileges = new MBBindingList<MercenaryPrivilegeVM>();
        }

        public MercenaryCareer Career { get; private set; }

        [DataSourceProperty] public string CareerText => new TextObject("{=!}{CLAN} Career in {KINGDOM}")
            .SetTextVariable("CLAN", Clan.PlayerClan.Name)
            .SetTextVariable("KINGDOM", Career?.Kingdom.Name)
            .ToString();
        [DataSourceProperty] public string RequestPrivilegeText => new TextObject("{=ZYyxmOv9}Request").ToString();
        [DataSourceProperty] public string PrivilegesText => new TextObject("{=!}Privileges").ToString();
        [DataSourceProperty] public string NoPrivilegesText => new TextObject("{=!}No privileges yet! Acquire Career Points through service time and merit. Request privileges by spending these points.").ToString();
        [DataSourceProperty] public string PointsHeaderText => new TextObject("{=!}Career Points").ToString();
        [DataSourceProperty] public string ReputationHeaderText => new TextObject("{=!}Reputation").ToString();
        [DataSourceProperty] public string TimeHeaderText => new TextObject("{=!}Service Time").ToString();
        [DataSourceProperty] public string LevyCharacterText => new TextObject("{=!}Levy Character").ToString();
        [DataSourceProperty] public string ProfessionalCharacterText => new TextObject("{=!}Professional Character").ToString();
        [DataSourceProperty] public string PrivilegeAvailableHeaderText => new TextObject("{=!}Privilege Available").ToString();
        [DataSourceProperty] public string DailyPointsGainHeaderText => new TextObject("{=!}Points Gain").ToString();

        [DataSourceProperty] public HintViewModel TimeHint => new HintViewModel(
            new TextObject("{=!}Your total time of service as mercenary, across all kingdoms. Every year of service, your clan gains Reputation as mercenaries."));

        [DataSourceProperty]
        public HintViewModel ReputationHint => new HintViewModel(
           new TextObject("{=!}Your reputation as mercenary, shared across all kingdoms. Reputation is awarded by service time (regardless of the contractor) & merit, such as gainning renown while serving."));

        [DataSourceProperty]
        public HintViewModel PointsHint => new HintViewModel(
           new TextObject("{=!}Your Career Points within this kingdom. Career Points are used for requesting Privileges. Points are gained daily based on your clan Reputation and Tier. Each clan party in an Army yields extra points. Leading an Army increases total gain by 20%."));

        [DataSourceProperty]
        public HintViewModel PrivilegeAvailableHint => new HintViewModel(
          new TextObject("{=!}Whether or not you are able to ask for another privilege in this realm. Once a privilege is requested, you need at least 2 seasons before requesting another."));

        public override void RefreshValues()
        {
            base.RefreshValues();

            if (Career != null)
            {
                Privileges.Clear();
                PointsText = Career.GetPoints(Career.Kingdom).ToString();
                ReputationText = FormatValue(Career.Reputation);

                TimeText = new TextObject("{=!}{DAYS} days served")
                    .SetTextVariable("DAYS", Career.ServiceDays)
                    .ToString();

                var pointsGain = Campaign.Current.GetCampaignBehavior<BKMercenaryCareerBehavior>().GetDailyCareerPointsGain(Career.Clan, true);
                DailyPointsGainText = FormatFloatGain(pointsGain.ResultNumber);
                DailyPointsGainHint = new HintViewModel(new TextObject("{=!}" + pointsGain.GetExplanations()));

                LevyCharacter = new CharacterViewModel(CharacterViewModel.StanceTypes.OnMount);
                ProfessionalCharacter = new CharacterViewModel(CharacterViewModel.StanceTypes.OnMount);

                LevyVisible = false;
                ProfessionalVisible = false;


                var privilegesList = Career.GetPrivileges(Career.Kingdom);
                foreach (var privilege in privilegesList)
                {
                    Privileges.Add(new MercenaryPrivilegeVM(privilege));
                }

                ShowNoPrivilegesText = privilegesList.Count == 0;
                
                CanEditLevy = privilegesList.Contains(DefaultMercenaryPrivileges.Instance.CustomTroop3);
                CanEditProfessional = privilegesList.Contains(DefaultMercenaryPrivileges.Instance.CustomTroop5);

                EditLevyText = new TextObject("{=!}Create").ToString();
                EditProfessionalText = new TextObject("{=!}Create").ToString();

                CanAskForPrivilege = Career.HasTimePassedForPrivilege(Career.Kingdom);
                PrivilegeAvailableText = new TextObject("{=!}{AVAILABLE} ({TIME})")
                    .SetTextVariable("AVAILABLE", GameTexts.FindText(CanAskForPrivilege ? "str_yes" : "str_no"))
                    .SetTextVariable("TIME", Career.GetPrivilegeTime(Career.Kingdom).ToString()).ToString();

                var levy = Career.GetTroop(Career.Kingdom);
                if (levy != null)
                {
                    LevyVisible = true;
                    EditLevyText = new TextObject("{=!}Edit").ToString();
                    LevyCharacterName = levy.Name != null ? levy.Name.ToString() : "";
                    LevyCharacter.FillFrom(levy.Character);
                    LevyCharacter.SetEquipment(levy.Character.BattleEquipments.First());
                }

                var professional = Career.GetTroop(Career.Kingdom, false);
                if (professional != null)
                {
                    ProfessionalVisible = true;
                    EditProfessionalText = new TextObject("{=!}Edit").ToString();
                    ProfessionalCharacterName = professional.Name != null ? professional.Name.ToString() : "";
                    ProfessionalCharacter.FillFrom(professional.Character);
                    ProfessionalCharacter.SetEquipment(professional.Character.BattleEquipments.First());
                }
            }
        }

        private void ShowPrivileges()
        {
            var list = new List<InquiryElement>();
            foreach (var privilege in DefaultMercenaryPrivileges.Instance.All)
            {
                bool enabled = Career.CanLevelUpPrivilege(privilege);
                var hint = enabled ? privilege.Description : privilege.UnAvailableHint;

                list.Add(new InquiryElement(privilege,
                    privilege.Name.ToString(),
                    null,
                    enabled,
                    hint.ToString()));
            }

            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                new TextObject("{=!}Select privilege").ToString(),
                new TextObject("{=!}").ToString(),
                list,
                true,
                1,
                GameTexts.FindText("str_accept").ToString(),
                string.Empty,
                delegate (List<InquiryElement> list)
                {
                    Career.AddPrivilege((MercenaryPrivilege)list[0].Identifier);
                    RefreshValues();
                },
                null));
        }

        private void EditLevy()
        {
            var customTroop = Career.GetTroop(Career.Kingdom);
            if (customTroop == null)
            {
                var preset = DefaultCustomTroopPresets.Instance.SargeantLevy;
                var character = CharacterObject.CreateFrom(Career.Kingdom.Culture.BasicTroop);

                InformationManager.ShowTextInquiry(new TextInquiryData(new TextObject("{=!}Custom Levy").ToString(),
                    new TextObject("{=!}Create a custom levy troop! This troop will be available in towns of {CULTURE} culture. They will only be available for your clan, and can be retrained or rearmed on demand - though these will incur costs. Their recruitment and upkeep costs will depend on the equipment you give them. First, give them a name.")
                    .SetTextVariable("CULTURE", Career.Kingdom.Culture.Name)
                    .ToString(),
                    true,
                    true,
                    GameTexts.FindText("str_accept").ToString(),
                    GameTexts.FindText("str_selection_widget_cancel").ToString(),
                    delegate (string name)
                    {
                        Career.AddTroop(Career.Kingdom, character);
                        customTroop = Career.GetTroop(Career.Kingdom);
                        (character as BasicCharacterObject).Level = preset.Level;
                        customTroop.SetName(new TextObject("{=!}" + name));
                       
                        RefreshValues();
                        ShowSkillEditing();
                    },
                    null));
            }
            else ShowEditingOptions();
        }

        private void EditProfessional()
        {
            var customTroop = Career.GetTroop(Career.Kingdom, false);
            if (customTroop == null)
            {
                var preset = DefaultCustomTroopPresets.Instance.SargeantLevy;
                var character = CharacterObject.CreateFrom(Career.Kingdom.Culture.BasicTroop);

                InformationManager.ShowTextInquiry(new TextInquiryData(new TextObject("{=!}Custom Professional").ToString(),
                    new TextObject("{=!}Create a custom professional troop! This troop will be available in towns of {CULTURE} culture. They will only be available for your clan, and can be retrained or rearmed on demand - though these will incur costs. Their recruitment and upkeep costs will depend on the equipment you give them. First, give them a name.")
                    .SetTextVariable("CULTURE", Career.Kingdom.Culture.Name)
                    .ToString(),
                    true,
                    true,
                    GameTexts.FindText("str_accept").ToString(),
                    GameTexts.FindText("str_selection_widget_cancel").ToString(),
                    delegate (string name)
                    {
                        Career.AddTroop(Career.Kingdom, character, false);
                        customTroop = Career.GetTroop(Career.Kingdom, false);
                        (character as BasicCharacterObject).Level = preset.Level;
                        customTroop.SetName(new TextObject("{=!}" + name));
                       
                        RefreshValues();
                        ShowSkillEditing(false);
                    },
                    null,
                    false,
                    new Func<string, Tuple<bool, string>>(CampaignUIHelper.IsStringApplicableForHeroName)));
            }
            else ShowEditingOptions(false);
        }

        private void ShowEditingOptions(bool levy = true)
        {
            var list = new List<InquiryElement>();
            list.Add(new InquiryElement("edit-name",
                GameTexts.FindText("str_sort_by_name_label").ToString(),
                null));

            list.Add(new InquiryElement("edit-skills",
                GameTexts.FindText("str_skills").ToString(),
                null));

            list.Add(new InquiryElement("edit-equipment",
                GameTexts.FindText("str_equipment").ToString(),
                null));

            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                new TextObject("{=!}Troop Editing").ToString(),
                new TextObject("{=!}Edit the {TROOP} to better fit your needs.")
                .SetTextVariable("TROOP", levy ? LevyCharacterName : ProfessionalCharacterName).ToString(),
                list,
                true,
                1,
                GameTexts.FindText("str_accept").ToString(),
                string.Empty,
                delegate (List<InquiryElement> list)
                {
                    string option = (string)list[0].Identifier;
                    if (option == "edit-name")
                    {
                        ShowNameEditing(levy);
                    }
                    else if (option == "edit-equipment")
                    {
                        ShowEquipmentOptions();
                    }
                    else
                    {
                        ShowSkillEditing(levy);
                    }
                }, 
                null));
        }

        private void ShowSkillEditing(bool levy = true)
        {
            var customTroop = Career.GetTroop(Career.Kingdom, levy);
            var list = new List<InquiryElement>();
            var items = Campaign.Current.ObjectManager.GetObjectTypeList<ItemObject>();
            foreach (var preset in DefaultCustomTroopPresets.Instance.GetAdequatePresets(levy ? 16 : 26))
            {
                list.Add(new InquiryElement(preset,
                    preset.Name.ToString(),
                    new ImageIdentifier(items.First(x => x.StringId == preset.ItemId)),
                    true,
                    preset.Description.ToString()));
            }

            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                new TextObject("{=!}Select Skill Set").ToString(),
                new TextObject("{=!}Choose a skill set that fits the function you want to give your troops, from melee infantry to mounted skirmishers. Equipment is edited separately, make sure to choose skills that match their equipment.").ToString(),
                list,
                true,
                1,
                GameTexts.FindText("str_accept").ToString(),
                GameTexts.FindText("str_cancel").ToString(),
                delegate (List<InquiryElement> list)
                {
                    int quantity = CalculateTroopAmount(customTroop.Character);
                    if (quantity == 0)
                    {
                        customTroop.SetSkills(customTroop.Character, (CustomTroopPreset)list[0].Identifier);
                        ShowEditingOptions();
                    }
                    else
                    {
                        int cost = (int)(quantity * (levy ? 750f : 1500f));
                        InformationManager.ShowInquiry(new InquiryData(new TextObject("{=!}Retrainning Costs").ToString(),
                            new TextObject("{=!}It seems there are {QUANTITY} of {CHARACTER} in your clan parties. Retrainning them to new skills will cost {COST}{GOLD_ICON}.")
                            .SetTextVariable("QUANTITY", quantity)
                            .SetTextVariable("CHARACTER", customTroop.Name)
                            .SetTextVariable("COST", cost)
                            .ToString(),
                            Hero.MainHero.Gold >= cost,
                            true,
                            GameTexts.FindText("str_accept").ToString(),
                            GameTexts.FindText("str_cancel").ToString(),
                            () =>
                            {
                                Hero.MainHero.ChangeHeroGold(-cost);
                                customTroop.SetSkills(customTroop.Character, (CustomTroopPreset)list[0].Identifier);
                                ShowEditingOptions();
                            },
                            () => ShowEditingOptions()));
                    }
                },
                delegate (List<InquiryElement> list)
                {
                    ShowEditingOptions(levy);
                }));
        }

        private void ShowNameEditing(bool levy = true)
        {
            var customTroop = Career.GetTroop(Career.Kingdom, levy);
            InformationManager.ShowTextInquiry(new TextInquiryData(new TextObject("{=!}Edit Name").ToString(),
                new TextObject("{=!}Change the name of {TROOP}.")
                .SetTextVariable("TROOP", customTroop.Name)
                .ToString(),
                true,
                true,
                GameTexts.FindText("str_accept").ToString(),
                GameTexts.FindText("str_cancel").ToString(),
                delegate (string name)
                {
                    customTroop.SetName(new TextObject("{=!}" + name));
                    RefreshValues();
                    ShowEditingOptions();
                },
                null,
                false,
                new Func<string, Tuple<bool, string>>(CampaignUIHelper.IsStringApplicableForHeroName)));
        }

        private void ShowEquipmentOptions(bool levy = true)
        {
            var list = new List<InquiryElement>();
            list.Add(new InquiryElement(new EqupmentEditOption(EquipmentIndex.Weapon0, ItemTypeEnum.Invalid),
                new TextObject("{=!}Weapon 1").ToString(),
                null));

            list.Add(new InquiryElement(new EqupmentEditOption(EquipmentIndex.Weapon1, ItemTypeEnum.Invalid),
                new TextObject("{=!}Weapon 2").ToString(),
                null));

            list.Add(new InquiryElement(new EqupmentEditOption(EquipmentIndex.Weapon2, ItemTypeEnum.Invalid),
                new TextObject("{=!}Weapon 3").ToString(),
                null));

            list.Add(new InquiryElement(new EqupmentEditOption(EquipmentIndex.Weapon3, ItemTypeEnum.Invalid),
                new TextObject("{=!}Weapon 4").ToString(),
                null));

            list.Add(new InquiryElement(new EqupmentEditOption(EquipmentIndex.Head, ItemTypeEnum.HeadArmor),
                GameTexts.FindText("str_inventory_type_12").ToString(),
                null));

            list.Add(new InquiryElement(new EqupmentEditOption(EquipmentIndex.Body, ItemTypeEnum.BodyArmor),
               GameTexts.FindText("str_inventory_type_13").ToString(),
               null));

            list.Add(new InquiryElement(new EqupmentEditOption(EquipmentIndex.Leg, ItemTypeEnum.LegArmor),
               GameTexts.FindText("str_inventory_type_14").ToString(),
               null));

            list.Add(new InquiryElement(new EqupmentEditOption(EquipmentIndex.Gloves, ItemTypeEnum.HandArmor),
               GameTexts.FindText("str_inventory_type_15").ToString(),
               null));

            list.Add(new InquiryElement(new EqupmentEditOption(EquipmentIndex.Cape, ItemTypeEnum.Cape),
               GameTexts.FindText("str_inventory_type_22").ToString(),
               null));

            list.Add(new InquiryElement(new EqupmentEditOption(EquipmentIndex.Horse, ItemTypeEnum.Horse),
               GameTexts.FindText("str_inventory_type_1").ToString(),
               null));

            list.Add(new InquiryElement(new EqupmentEditOption(EquipmentIndex.HorseHarness, ItemTypeEnum.HorseHarness),
               GameTexts.FindText("str_inventory_type_23").ToString(),
               null));

            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                new TextObject("{=!}Equipment Editing").ToString(),
                new TextObject("{=!}Select the inventory slot you would like to edit. Weapons may be assigned to 4 different slots, each with a weapon of a different type, such as slot 1, one-handed weapons, slot 2 bows, and slot 3 arrows.")
                .ToString(),
                list,
                true,
                1, 
                GameTexts.FindText("str_accept").ToString(),
                GameTexts.FindText("str_cancel").ToString(),
                delegate (List<InquiryElement> list)
                {
                    EqupmentEditOption option = (EqupmentEditOption)list[0].Identifier;
                    if (option.ItemType == ItemTypeEnum.Invalid)
                    {
                        ShowWeapons(option.EquipmentIndex, levy);
                    }
                    else
                    {
                        ShowEquipments(option, levy);
                    }
                },
                delegate (List<InquiryElement> list)
                {
                    ShowEditingOptions(levy);
                }));
        }

        private void ShowWeapons(EquipmentIndex index, bool levy = true)
        {
            var list = new List<InquiryElement>();
            list.Add(new InquiryElement(ItemTypeEnum.OneHandedWeapon,
                GameTexts.FindText("str_inventory_type_2").ToString(),
                null));

            list.Add(new InquiryElement(ItemTypeEnum.TwoHandedWeapon,
                GameTexts.FindText("str_inventory_type_3").ToString(),
                null));

            list.Add(new InquiryElement(ItemTypeEnum.Polearm,
                GameTexts.FindText("str_inventory_type_4").ToString(),
                null));

            list.Add(new InquiryElement(ItemTypeEnum.Thrown,
                GameTexts.FindText("str_inventory_type_10").ToString(),
                null));

            list.Add(new InquiryElement(ItemTypeEnum.Shield,
                GameTexts.FindText("str_inventory_type_7").ToString(),
                null));

            list.Add(new InquiryElement(ItemTypeEnum.Bow,
                GameTexts.FindText("str_inventory_type_8").ToString(),
                null));

            list.Add(new InquiryElement(ItemTypeEnum.Crossbow,
                GameTexts.FindText("str_inventory_type_9").ToString(),
                null));

            list.Add(new InquiryElement(ItemTypeEnum.Arrows,
                GameTexts.FindText("str_inventory_type_5").ToString(),
                null));

            list.Add(new InquiryElement(ItemTypeEnum.Bolts,
                GameTexts.FindText("str_inventory_type_6").ToString(),
                null));

            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                new TextObject("{=!}Weapon Editing").ToString(),
                new TextObject("{=!}Select what type of weapon you want to equip in this slot.")
                .ToString(),
                list,
                true,
                1,
                GameTexts.FindText("str_accept").ToString(),
                GameTexts.FindText("str_cancel").ToString(),
                delegate (List<InquiryElement> list)
                {
                    ItemTypeEnum type = (ItemTypeEnum)list[0].Identifier;
                    ShowEquipments(new EqupmentEditOption(index, type), levy);
                },
                delegate (List<InquiryElement> list)
                {
                    ShowEquipmentOptions(levy);
                }));
        }

        private void ShowEquipments(EqupmentEditOption option, bool levy = true)
        {
            var customTroop = Career.GetTroop(Career.Kingdom, levy);
            var list = new List<InquiryElement>();
            var items = Campaign.Current.ObjectManager.GetObjectTypeList<ItemObject>();
            foreach (var item in items)
            {
                if (item.Culture != null && item.Culture != customTroop.Character.Culture)
                {
                    continue;
                }

                if (item.ItemType == option.ItemType)
                {
                    list.Add(new InquiryElement(item,
                        item.Name.ToString(),
                        new ImageIdentifier(item),
                        true,
                        new TextObject("{=!}Tier: {TIER}\nValue: {VALUE}\nType: {TYPE}")
                        .SetTextVariable("TIER", item.Tierf)
                        .SetTextVariable("VALUE", item.Value)
                        .SetTextVariable("TYPE", GameTexts.FindText("str_inventory_type_" + item.ItemType)).ToString()));
                }
            }

            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                new TextObject("{=!}Equipment Editing").ToString(),
                new TextObject("{=!}Choose the item selection for this equipment slot. You may choose from 1 to 5 items.")
                .ToString(),
                list,
                true,
                5,
                GameTexts.FindText("str_accept").ToString(),
                GameTexts.FindText("str_cancel").ToString(),
                delegate (List<InquiryElement> list)
                {
                    List<ItemObject> items = new List<ItemObject>();
                    foreach (var element in list)
                    {
                        items.Add((ItemObject)element.Identifier);
                    }

                    List<Equipment> equipments = customTroop.Equipments;
                    for (int i = 0; i < equipments.Count; i++)
                    {
                        var equipment = equipments[i];
                        ItemObject item = null;
                        if (items.Count > i)
                        {
                            item = items[i];
                        }
                        else
                        {
                            item = items.GetRandomElement();
                        }

                        EquipmentElement equipmentElement = new EquipmentElement(item);
                        equipment[option.EquipmentIndex] = equipmentElement;
                    }

                    customTroop.Equipments = equipments;
                    ShowEditingOptions();
                    RefreshValues();
                },
                delegate (List<InquiryElement> list) 
                {
                    ShowEquipmentOptions(levy);
                }
                ));
        }

        private int CalculateTroopAmount(CharacterObject character)
        {
            int result = 0;
            foreach (var party in Clan.PlayerClan.WarPartyComponents)
            {
                foreach (var element in party.MobileParty.MemberRoster.GetTroopRoster())
                {
                    if (element.Character == character)
                    {
                        result += element.Number;
                    }
                }
            }

            return result;
        }

        [DataSourceProperty]
        public MBBindingList<MercenaryPrivilegeVM> Privileges
        {
            get => privileges;
            set
            {
                if (value != privileges)
                {
                    privileges = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public CharacterViewModel LevyCharacter
        {
            get => levyCharacter;
            set
            {
                if (value != levyCharacter)
                {
                    levyCharacter = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public CharacterViewModel ProfessionalCharacter
        {
            get => professionalCharacter;
            set
            {
                if (value != professionalCharacter)
                {
                    professionalCharacter = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public bool CanEditLevy
        {
            get => canEditLevy;
            set
            {
                if (value != canEditLevy)
                {
                    canEditLevy = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public bool CanEditProfessional
        {
            get => canEditProfessional;
            set
            {
                if (value != canEditProfessional)
                {
                    canEditProfessional = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public bool LevyVisible
        {
            get => levyVisible;
            set
            {
                if (value != levyVisible)
                {
                    levyVisible = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public bool ShowNoPrivilegesText
        {
            get => showNoPrivilegesText;
            set
            {
                if (value != showNoPrivilegesText)
                {
                    showNoPrivilegesText = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public bool ProfessionalVisible
        {
            get => professionalVisible;
            set
            {
                if (value != professionalVisible)
                {
                    professionalVisible = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public bool CanAskForPrivilege
        {
            get => canAskPrivilege;
            set
            {
                if (value != canAskPrivilege)
                {
                    canAskPrivilege = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public string PrivilegeAvailableText
        {
            get => privilegeAvailableText;
            set
            {
                if (value != privilegeAvailableText)
                {
                    privilegeAvailableText = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public string EditLevyText
        {
            get => editLevyText;
            set
            {
                if (value != editLevyText)
                {
                    editLevyText = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public string EditProfessionalText
        {
            get => editProfessionalText;
            set
            {
                if (value != editProfessionalText)
                {
                    editProfessionalText = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public string LevyCharacterName
        {
            get => levyCharacterName;
            set
            {
                if (value != levyCharacterName)
                {
                    levyCharacterName = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public string ProfessionalCharacterName
        {
            get => professionalCharacterName;
            set
            {
                if (value != professionalCharacterName)
                {
                    professionalCharacterName = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public string PointsText
        {
            get => pointsText;
            set
            {
                if (value != pointsText)
                {
                    pointsText = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public string ReputationText
        {
            get => reputationText;
            set
            {
                if (value != reputationText)
                {
                    reputationText = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public string TimeText
        {
            get => timeText;
            set
            {
                if (value != timeText)
                {
                    timeText = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public string DailyPointsGainText
        {
            get => dailyPointsGainText;
            set
            {
                if (value != dailyPointsGainText)
                {
                    dailyPointsGainText = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public HintViewModel DailyPointsGainHint
        {
            get => dailyPointsGainHint;
            set
            {
                if (value != dailyPointsGainHint)
                {
                    dailyPointsGainHint = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        private class EqupmentEditOption
        {
            internal EqupmentEditOption(EquipmentIndex index, ItemTypeEnum type)
            {
                EquipmentIndex = index;
                ItemType = type;
            }

            public EquipmentIndex EquipmentIndex { get; private set; }
            public ItemTypeEnum ItemType { get; private set; }
        }
    }
}
