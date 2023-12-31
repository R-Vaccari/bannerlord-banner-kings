using BannerKings.Managers.Goals;
using BannerKings.Managers.Institutions.Religions;
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

namespace BannerKings.UI.VanillaTabs.Character
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
            Education = new EducationVM(characterDeveloper.CurrentCharacter.Hero, characterDeveloper);
            Education.RefreshValues();
            Religion = new ReligionVM(BannerKingsConfig.Instance.ReligionsManager
                .GetHeroReligion(characterDeveloper.CurrentCharacter.Hero), characterDeveloper.CurrentCharacter.Hero);
            Religion.RefreshValues();
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
            if (DefaultReligions.Instance.All.Count() > 0)
            {
                ReligionVisible = true;
                EducationVisible = false;
            }

            OnRefresh();
        }

        [DataSourceMethod]
        public void OpenDecisions()
        {
            var options = new List<InquiryElement>();
            var personalDecisions = new List<InquiryElement>();
            var kingdomDecisions = new List<InquiryElement>();
            var religiousDecisions = new List<InquiryElement>();
            var criminalDecisions = new List<InquiryElement>();
            var uniqueDecisions = new List<InquiryElement>();

            foreach (var goal in DefaultGoals.Instance.All)
            {
                if (goal.IsAvailable())
                {
                    var enabled = goal.IsFulfilled(out var reasons);
                    var hint = $"{goal.Description}";

                    if (!enabled)
                    {
                        hint = reasons.Aggregate(hint, (current, reason) => current + Environment.NewLine + reason);
                    }

                    var element = new InquiryElement(goal,
                        goal.Name.ToString(),
                        null,
                        enabled,
                        hint);

                    if (goal.goalType == GoalCategory.Personal)
                    {
                        personalDecisions.Add(element);
                    }
                    else if (goal.goalType == GoalCategory.Kingdom)
                    {
                        kingdomDecisions.Add(element);
                    }
                    else if (goal.goalType == GoalCategory.Religious)
                    {
                        religiousDecisions.Add(element);
                    }
                    else if (goal.goalType == GoalCategory.Criminal)
                    {
                        criminalDecisions.Add(element);
                    }
                    else
                    {
                        uniqueDecisions.Add(element);
                    }
                }
            }

            options.Add(new InquiryElement(
                new DecisionCategoryOption(
                new TextObject("{=AaS8WTzj}Personal"),
                new TextObject("{=AaS8WTzj}Personal decisions affect your character and sometimes your close family."),
                personalDecisions),
            new TextObject("{=AaS8WTzj}Personal").ToString(),
            null,
            personalDecisions.Count > 0,
            new TextObject("{=AaS8WTzj}Personal decisions affect your character and sometimes your close family.").ToString()));

            options.Add(new InquiryElement(
            new DecisionCategoryOption(
                GameTexts.FindText("str_kingdom"),
                new TextObject("{=Tcir35E5}Kingdom decisions affect your realm as a whole or your family's position in the realm."),
                kingdomDecisions),
            GameTexts.FindText("str_kingdom").ToString(),
            null,
            kingdomDecisions.Count > 0,
            new TextObject("{=Tcir35E5}Kingdom decisions affect your realm as a whole or your family's position in the realm.").ToString()));

            /*options.Add(new InquiryElement(
            new DecisionCategoryOption(
                new TextObject("{=8uURr54W}Religious"),
                new TextObject("{=8uURr54W}Religious decisions are ways for you interact with the world through your faith."),
                religiousDecisions),
            new TextObject("{=8uURr54W}Religious").ToString(),
            null,
            religiousDecisions.Count > 0,
            new TextObject("{=8uURr54W}Religious decisions are ways for you interact with the world through your faith.").ToString()));*/

            options.Add(new InquiryElement(
            new DecisionCategoryOption(
                new TextObject("{=!}Criminal"),
                new TextObject("{=!}Criminal decisions relate to carrying out sentences or crimes. Captured criminals can be sentenced with a criminal decision."),
                criminalDecisions),
            new TextObject("{=!}Criminal").ToString(),
            null,
            criminalDecisions.Count > 0,
            new TextObject("{=!}Criminal decisions relate to carrying out sentences or crimes. Captured criminals can be sentenced with a criminal decision.").ToString()));


            options.Add(new InquiryElement(
            new DecisionCategoryOption(
                new TextObject("{=8MLr4eSD}Unique"),
                new TextObject("{=8MLr4eSD}Unique decisions are difficult and special decisions that often can only be taken once, such as reviving a historical empire."),
                uniqueDecisions),
            new TextObject("{=8MLr4eSD}Unique").ToString(),
            null,
            uniqueDecisions.Count > 0,
            new TextObject("{=8MLr4eSD}Unique decisions are special decisions that often can only be taken once, such as reviving a historical empire.").ToString()));

            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                            new TextObject("{=GMNhGSUb}Decisions").ToString(),
                            new TextObject("{=VbM15ztE}Choose a category of decisions to take. Decisions are always taken on behalf of the family head, regardless of what hero is chosen in the Character tab.").ToString(),
                            options,
                            true,
                            1,
                            1,
                            GameTexts.FindText("str_done").ToString(),
                            string.Empty,
                            delegate (List<InquiryElement> x)
                            {
                                DecisionCategoryOption categoryOption = (DecisionCategoryOption)x[0].Identifier;
                                MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                                        categoryOption.Title.ToString(),
                                        categoryOption.Description.ToString(),
                                        categoryOption.Options,
                                        true,
                                        1,
                                        1,
                                        GameTexts.FindText("str_done").ToString(),
                                        GameTexts.FindText("str_cancel").ToString(),
                                        delegate (List<InquiryElement> x)
                                        {
                                            var result = (Goal)x[0].Identifier;
                                            result.ShowInquiry();
                                        },
                                        delegate (List<InquiryElement> x)
                                        {
                                            OpenDecisions();
                                        },
                                        string.Empty));
                            },
                            null,
                            string.Empty));
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

        private class DecisionCategoryOption
        {
            public DecisionCategoryOption(TextObject title, TextObject description, List<InquiryElement> options)
            {
                Title = title;
                Description = description;
                Options = options;
            }

            public TextObject Title { get; private set; }
            public TextObject Description { get; private set; }
            public List<InquiryElement> Options { get; private set; }
        }
    }
}