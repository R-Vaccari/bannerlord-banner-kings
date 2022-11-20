using BannerKings.Behaviours.Mercenary;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.Mercenary
{
    internal class MercenaryCareerVM : BannerKingsViewModel
    {
        private string reputationText, pointsText, timeText,
            levyCharacterName, professionalCharacterName,
            editLevyText, editProfessionalText;
        private CharacterViewModel levyCharacter, professionalCharacter;
        private MBBindingList<MercenaryPrivilegeVM> privileges;
        private bool canEditLevy, canEditProfessional;
        public MercenaryCareerVM() : base(null, false)
        {
            Career = Campaign.Current.GetCampaignBehavior<BKMercenaryCareerBehavior>().GetCareer(Clan.PlayerClan);
            Privileges = new MBBindingList<MercenaryPrivilegeVM>();
        }

        public MercenaryCareer Career { get; private set; }

        [DataSourceProperty] public string CareerText => new TextObject("{=!}{CLAN} Career in {KINGDOM}")
            .SetTextVariable("CLAN", Clan.PlayerClan.Name)
            .SetTextVariable("KINGDOM", Career.Kingdom.Name)
            .ToString();
        [DataSourceProperty] public string RequestPrivilegeText => new TextObject("{=ZYyxmOv9}Request").ToString();
        [DataSourceProperty] public string PrivilegesText => new TextObject("{=!}Privileges").ToString();
        [DataSourceProperty] public string NoPrivilegesText => new TextObject("{=!}No privileges yet! Acquire Career Points through service time and merit. Request privileges by spending these points.").ToString();
        [DataSourceProperty] public string PointsHeaderText => new TextObject("{=!}Career Points").ToString();
        [DataSourceProperty] public string ReputationHeaderText => new TextObject("{=!}Reputation").ToString();
        [DataSourceProperty] public string TimeHeaderText => new TextObject("{=!}Service Time").ToString();
        [DataSourceProperty] public string LevyCharacterText => new TextObject("{=!}Levy Character").ToString();
        [DataSourceProperty] public string ProfessionalCharacterText => new TextObject("{=!}Professional Character").ToString();

        [DataSourceProperty] public HintViewModel TimeHint => new HintViewModel(
            new TextObject("{=!}Your time of consecutive service for this kingdom."));

        [DataSourceProperty]
        public HintViewModel ReputationHint => new HintViewModel(
           new TextObject("{=!}Your time of consecutive service for this kingdom."));

        [DataSourceProperty]
        public HintViewModel PointsHint => new HintViewModel(
           new TextObject("{=!}Your Career Points within this kingdom. Career Points are used for requesting Privileges."));

        public override void RefreshValues()
        {
            base.RefreshValues();

            if (Career != null)
            {
                Privileges.Clear();
                PointsText = Career.GetPoints(Career.Kingdom).ToString();
                ReputationText = FormatValue(Career.Reputation);

                LevyCharacter = new CharacterViewModel(CharacterViewModel.StanceTypes.OnMount);
                ProfessionalCharacter = new CharacterViewModel(CharacterViewModel.StanceTypes.OnMount);

                var privilegesList = Career.GetPrivileges(Career.Kingdom);
                foreach (var privilege in privilegesList)
                {
                    Privileges.Add(new MercenaryPrivilegeVM(privilege));
                }

                CanEditLevy = privilegesList.Contains(DefaultMercenaryPrivileges.Instance.CustomTroop3);
                CanEditProfessional = privilegesList.Contains(DefaultMercenaryPrivileges.Instance.CustomTroop5);

                EditLevyText = new TextObject("{=!}Create").ToString();
                EditProfessionalText = new TextObject("{=!}Create").ToString();


                var levy = Career.GetTroop(Career.Kingdom);
                if (levy != null)
                {
                    EditLevyText = new TextObject("{=!}Edit").ToString();
                    LevyCharacterName = levy.Name.ToString();
                    LevyCharacter.FillFrom(levy);
                    LevyCharacter.SetEquipment(levy.BattleEquipments.First());
                }

                var professional = Career.GetTroop(Career.Kingdom, false);
                if (professional != null)
                {
                    EditProfessionalText = new TextObject("{=!}Edit").ToString();
                    ProfessionalCharacterName = professional.Name.ToString();
                    ProfessionalCharacter.FillFrom(professional);
                    ProfessionalCharacter.SetEquipment(professional.BattleEquipments.First());
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
                },
                null));
        }

        private void EditLevy()
        {
            var levy = Career.GetTroop(Career.Kingdom);
            if (levy == null)
            {
                var preset = DefaultCustomTroopPresets.Instance.SargeantLevy;
                var character = CharacterObject.CreateFrom(Career.Kingdom.Culture.BasicTroop);
                

                
                


               // AccessTools.Property(character.GetType(), "BodyPropertyRange").SetValue(character, reference.BodyPropertyRange);
            }
        }

        private void SetSkills(CharacterObject character, CustomTroopPreset preset)
        {
            MBCharacterSkills skills = (MBCharacterSkills)AccessTools.Property(character.GetType(), "CharacterSkills")
                    .GetValue(character);

            skills.Skills.SetPropertyValue(DefaultSkills.OneHanded, preset.OneHanded);
            skills.Skills.SetPropertyValue(DefaultSkills.TwoHanded, preset.TwoHanded);
            skills.Skills.SetPropertyValue(DefaultSkills.Polearm, preset.Polearm);
            skills.Skills.SetPropertyValue(DefaultSkills.Riding, preset.Riding);
            skills.Skills.SetPropertyValue(DefaultSkills.Athletics, preset.Athletics);
            skills.Skills.SetPropertyValue(DefaultSkills.Bow, preset.Bow);
            skills.Skills.SetPropertyValue(DefaultSkills.Crossbow, preset.Crossbow);
            skills.Skills.SetPropertyValue(DefaultSkills.Throwing, preset.Throwing);
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

        private class TroopEditOption
        {

        }

        private enum OptionType
        {
            Equipment,
            Name,
            Skills
        }
    }
}
