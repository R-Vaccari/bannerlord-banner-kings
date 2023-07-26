using System;
using System.Linq;
using BannerKings.Managers.Titles;
using BannerKings.Managers.Traits;
using BannerKings.Utils.Extensions;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.Pages;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Generic;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.Extensions
{
    [ViewModelMixin("Refresh")]
    internal class EncyclopediaHeroPageMixin : BaseViewModelMixin<EncyclopediaHeroPageVM>
    {
        private bool addedFields;
        private readonly EncyclopediaHeroPageVM heroPageVM;
        private MBBindingList<TraitGroupVM> traitGroups;

        public EncyclopediaHeroPageMixin(EncyclopediaHeroPageVM vm) : base(vm)
        {
            heroPageVM = vm;
            TraitGroups = new MBBindingList<TraitGroupVM>();
        }

        [DataSourceProperty] public string CultureText => GameTexts.FindText("str_culture").ToString();
        [DataSourceProperty] public string MarriageText => new TextObject("{=mxVj1euY}Marriage").ToString();

        [DataSourceProperty]
        public MBBindingList<TraitGroupVM> TraitGroups
        {
            get => traitGroups;
            set
            {
                if (value != traitGroups)
                {
                    traitGroups = value;
                    ViewModel!.OnPropertyChangedWithValue(value);
                }
            }
        }

        public override void OnRefresh()
        {
            Hero hero = (Hero)heroPageVM.Obj;

            if (!addedFields)
            {
                TraitGroups.Clear();
                var education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(hero);
                var languages = education.Languages.Keys;
                heroPageVM.Stats.Add(new StringPairItemVM(new TextObject("{=yCaxpVGh}Languages:").ToString(),
                    education.Languages.Count.ToString(),
                    new BasicTooltipViewModel(() => languages.Aggregate(string.Empty, (current, reason) => current + Environment.NewLine + reason))));

                if (education.Lifestyle != null)
                {
                    heroPageVM.Stats.Add(new StringPairItemVM(new TextObject("{=tYO5xwVe}Lifestyle").ToString(),
                        education.Lifestyle.Name.ToString(),
                        new BasicTooltipViewModel(() => education.Lifestyle.Description.ToString())));
                }

                FeudalTitle title = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(hero);
                if (title != null && title.TitleType == TitleType.Lordship && hero.Clan != null && !hero.IsClanLeader() && !hero.IsPlayer() &&
                    !Utils.Helpers.IsCloseFamily(hero, hero.Clan.Leader))
                {
                    float progress = BannerKingsConfig.Instance.TitleManager.GetKnightInfluence(hero) / 350f;
                    heroPageVM.Stats.Add(new StringPairItemVM(new TextObject("{=MuZ2tL8E}Clan Creation:").ToString(), 
                        (progress * 100f).ToString("0.00") + '%', 
                        new BasicTooltipViewModel(() =>
                        new TextObject("{=2GvHUPV9}As a knight or knightess, this person will eventually attemp to lead their own household. Their progress is determined by the influence of the lordships they hold. This does not apply to direct relatives of the family leader.")
                        .ToString())));
                }

                var personality = new TraitGroupVM(new TextObject("{=cBqiYPdT}Personality"));
                TraitGroups.Add(personality);
                foreach (TraitObject trait in BKTraits.Instance.PersonalityTraits)
                {
                    int level = hero.GetTraitLevel(trait);
                    if (level == 0)
                    {
                        continue;
                    }

                    string value = GameTexts.FindText("str_trait_name_" + trait.StringId.ToLower(), (level + MathF.Abs(trait.MinValue)).ToString()).ToString();
                    personality.Traits.Add(new StringPairItemVM(new TextObject("{=B5Bx8p70}{TRAIT}:")
                        .SetTextVariable("TRAIT", trait.Name.ToString()).ToString(),
                        value,
                        new BasicTooltipViewModel(() => trait.Description.ToString())));
                }

                var aptitudes = new TraitGroupVM(new TextObject("{=p2qS5hym}Aptitudes"));
                TraitGroups.Add(aptitudes);
                foreach (TraitObject trait in BKTraits.Instance.AptitudeTraits)
                {
                    int level = hero.GetTraitLevel(trait);
                    string value = GameTexts.FindText("str_trait_name_" + trait.StringId.ToLower(), (level + MathF.Abs(trait.MinValue)).ToString()).ToString();
                    if (level == 0)
                    {
                        value = new TextObject("{=m45gzwyL}Neutral").ToString();
                    }

                    aptitudes.Traits.Add(new StringPairItemVM(new TextObject("{=B5Bx8p70}{TRAIT}:")
                        .SetTextVariable("TRAIT", trait.Name.ToString()).ToString(),
                        value,
                        new BasicTooltipViewModel(() => trait.Description.ToString())));
                }

                var political = new TraitGroupVM(new TextObject("{=HOeiJpH0}Political"));
                TraitGroups.Add(political);
                foreach (TraitObject trait in BKTraits.Instance.PoliticalTraits)
                {
                    float level = hero.GetTraitLevel(trait);
                    float result = level / trait.MaxValue;

                    political.Traits.Add(new StringPairItemVM(new TextObject("{=B5Bx8p70}{TRAIT}:")
                        .SetTextVariable("TRAIT", trait.Name.ToString()).ToString(),
                        (result * 100f).ToString("0.0") + '%',
                        new BasicTooltipViewModel(() => trait.Description.ToString())));
                }

                addedFields = true;
            }
        }

        internal class TraitGroupVM : ViewModel
        {
            private string title;
            private MBBindingList<StringPairItemVM> traits;

            internal TraitGroupVM(TextObject title)
            {
                this.title = title.ToString();
                traits = new MBBindingList<StringPairItemVM>();
            }

            [DataSourceProperty]
            public string Title
            {
                get => title;
                set
                {
                    if (value != title)
                    {
                        title = value;
                        OnPropertyChangedWithValue(value);
                    }
                }
            }

            [DataSourceProperty]
            public MBBindingList<StringPairItemVM> Traits
            {
                get => traits;
                set
                {
                    if (value != traits)
                    {
                        traits = value;
                        OnPropertyChangedWithValue(value);
                    }
                }
            }
        }
    }
}