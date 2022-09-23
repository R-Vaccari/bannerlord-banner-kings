using BannerKings.Managers.Goals;
using BannerKings.UI.Education;
using BannerKings.UI.Religion;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem.ViewModelCollection.CharacterDeveloper;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.Extensions
{
    [ViewModelMixin("RefreshValues")]
    internal class CharacterDeveloperMixin : BaseViewModelMixin<CharacterDeveloperVM>
    {
        //private BasicTooltipViewModel pietyHint;
        private readonly CharacterDeveloperVM characterDeveloper;
        private EducationVM educationVM;
        private ReligionVM religionVM;
        private bool educationVisible, religionVisible;

        public CharacterDeveloperMixin(CharacterDeveloperVM vm) : base(vm)
        {
            EducationVisible = false;
            ReligionVisible = false;
            characterDeveloper = vm;
        }

        [DataSourceProperty]
        public EducationVM Education
        {
            get => educationVM;
            set
            {
                if (value != educationVM)
                {
                    educationVM = value;
                    ViewModel!.OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public ReligionVM Religion
        {
            get => religionVM;
            set
            {
                if (value != religionVM)
                {
                    religionVM = value;
                    ViewModel!.OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public bool EducationVisible
        {
            get => educationVisible;
            set
            {
                if (value != educationVisible)
                {
                    educationVisible = value;
                    ViewModel!.OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public bool ReligionVisible
        {
            get => religionVisible;
            set
            {
                if (value != religionVisible)
                {
                    religionVisible = value;
                    ViewModel!.OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public string EducationText => new TextObject("{=hntWNxZy}Education").ToString();

        [DataSourceProperty]
        public string DecisionsText => new TextObject("{=GMNhGSUb}Decisions").ToString();

        [DataSourceProperty]
        public string FaithText => new TextObject("{=OKw2P9m1}Faith").ToString();

        public override void OnRefresh()
        {
            Util.TryCatch(() =>
            {
                Education = new EducationVM(characterDeveloper.CurrentCharacter.Hero, characterDeveloper);
                Education.RefreshValues();
                Religion = new ReligionVM(BannerKingsConfig.Instance.ReligionsManager
                    .GetHeroReligion(characterDeveloper.CurrentCharacter.Hero), characterDeveloper.CurrentCharacter.Hero);
                Religion.RefreshValues();
            });

        }

        [DataSourceMethod]
        public void OpenEducation()
        {
            EducationVisible = true;
            ReligionVisible = false;
            OnRefresh();
        }

        [DataSourceMethod]
        public void OpenFaith()
        {
            ReligionVisible = true;
            EducationVisible = false;
            OnRefresh();
        }

        [DataSourceMethod]
        public void OpenDecisions()
        {
            Util.TryCatch(() =>
            {
                var options = new List<InquiryElement>();

                foreach (var goal in DefaultGoals.Instance.All)
                {
                    var enabled = goal.IsFulfilled(out var reasons);
                    var hint = $"{goal.Description}";

                    if (goal.IsAvailable())
                    {
                        hint = reasons.Aggregate(hint, (current, reason) => current + Environment.NewLine + reason);

                        options.Add(new InquiryElement(goal,
                            goal.Name.ToString(),
                            null,
                            enabled,
                            hint));
                    }
                }

                MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                                new TextObject("{=GMNhGSUb}Decisions").ToString(),
                                new TextObject("{=bn2Ohzow}Choose a personal decision to take.").ToString(),
                                options, true, 1, GameTexts.FindText("str_done").ToString(), string.Empty,
                                delegate (List<InquiryElement> x)
                                {
                                    var result = (Goal)x[0].Identifier;
                                    result.ShowInquiry();
                                }, null, string.Empty));
            });
            
            OnRefresh();
        }

        [DataSourceMethod]
        public void CloseEducation()
        {
            EducationVisible = false;
            OnRefresh();
        }

        [DataSourceMethod]
        public void CloseFaith()
        {
            ReligionVisible = false;
            OnRefresh();
        }
    }
}