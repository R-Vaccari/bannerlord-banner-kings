using System;
using System.Linq;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
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
            heroPageVM.Stats.Clear();
            Marriage.Clear();
            Hero hero = (Hero)heroPageVM.Obj;

            if (!addedFields)
            {

                if (hero.Culture != null)
                {
                    string definition = GameTexts.FindText("str_enc_sf_culture", null).ToString();
                    heroPageVM.Stats.Add(new StringPairItemVM(definition, hero.Culture.Name.ToString(), null));
                }
                string definition2 = GameTexts.FindText("str_enc_sf_age", null).ToString();
                heroPageVM.Stats.Add(new StringPairItemVM(definition2, ((int)hero.Age).ToString(), null));
                string heroOccupationName = CampaignUIHelper.GetHeroOccupationName(hero);
                if (!string.IsNullOrEmpty(heroOccupationName))
                {
                    string definition3 = GameTexts.FindText("str_enc_sf_occupation", null).ToString();
                    heroPageVM.Stats.Add(new StringPairItemVM(definition3, heroOccupationName, null));
                }


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


                if (hero != Hero.MainHero)
                {

                    string definition4 = GameTexts.FindText("str_enc_sf_relation", null).ToString();
                    heroPageVM.Stats.Add(new StringPairItemVM(definition4, hero.GetRelationWithPlayer().ToString(), null));

                }

                addedFields = true;
            }
        }
    }
}