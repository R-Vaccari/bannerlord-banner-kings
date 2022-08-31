using BannerKings.UI.Titles;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.Pages;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.Extensions
{
    [ViewModelMixin("Refresh")]
    internal class EncyclopediaFactionPageMixin : BaseViewModelMixin<EncyclopediaFactionPageVM>
    {
        private EncyclopediaFactionPageVM vm;
        private MBBindingList<DemesneHierarchyVM> titles;

        public EncyclopediaFactionPageMixin(EncyclopediaFactionPageVM vm) : base(vm)
        {
            this.vm = vm;
            titles = new MBBindingList<DemesneHierarchyVM>();
        }

        [DataSourceProperty] public string CultureText => GameTexts.FindText("str_culture").ToString();

        [DataSourceProperty] public string DemesneText => new TextObject("{=!}Demesne").ToString();

        [DataSourceProperty] public string CompanionsText => new TextObject("{=a3G31iZ0}Companions").ToString();

        [DataSourceProperty] public string CouncilText => new TextObject("{=mUaJDjqO}Council").ToString();

       

        [DataSourceProperty]
        public MBBindingList<DemesneHierarchyVM> Titles
        {
            get => titles;
            set
            {
                if (value != titles)
                {
                    titles = value;
                    ViewModel!.OnPropertyChangedWithValue(value);
                }
            }
        }


        public override void OnRefresh()
        {
            //Titles.Clear();
            //Kingdom kingdom = (Kingdom)vm.Obj;
            //Titles.Add(new DemesneHierarchyVM(BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(kingdom), kingdom));
        }
    }
}