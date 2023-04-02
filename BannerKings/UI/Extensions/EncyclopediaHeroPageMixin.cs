using System;
using System.Linq;
using BannerKings.Managers.Titles;
using BannerKings.Utils.Extensions;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;
using TaleWorlds.CampaignSystem;
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
        private MBBindingList<StringPairItemVM> marriage;

        public EncyclopediaHeroPageMixin(EncyclopediaHeroPageVM vm) : base(vm)
        {
            heroPageVM = vm;
            Marriage = new MBBindingList<StringPairItemVM>();
        }

        [DataSourceProperty] public string CultureText => GameTexts.FindText("str_culture").ToString();
        [DataSourceProperty] public string MarriageText => new TextObject("{=mxVj1euY}Marriage").ToString();

        [DataSourceProperty]
        public MBBindingList<StringPairItemVM> Marriage
        {
            get => marriage;
            set
            {
                if (value != marriage)
                {
                    marriage = value;
                    ViewModel!.OnPropertyChangedWithValue(value);
                }
            }
        }

        public override void OnRefresh()
        {
            //heroPageVM.Stats.Clear();
            //Marriage.Clear();
            Hero hero = (Hero)heroPageVM.Obj;

            if (!addedFields)
            {
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

                addedFields = true;
            }
        }
    }
}