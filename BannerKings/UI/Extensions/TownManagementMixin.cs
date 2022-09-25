using BannerKings.Managers.Items;
using BannerKings.UI.TownManagement;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu.TownManagement;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.Extensions
{
    [ViewModelMixin("RefreshTownManagementStats")]
    public class TownManagementMixin : BaseViewModelMixin<TownManagementVM>
    {
        private readonly TownManagementVM townManagement;
        private MBBindingList<MaterialItemVM> materials;

        public TownManagementMixin(TownManagementVM vm) : base(vm)
        {
            townManagement = vm;
            materials = new MBBindingList<MaterialItemVM>();
        }

        [DataSourceProperty] public string ArmorText => new TextObject("{=h40bm0cG}Craft").ToString();

        [DataSourceProperty]
        public MBBindingList<MaterialItemVM> Materials
        {
            get => materials;
            set
            {
                if (value != materials)
                {
                    materials = value;
                    ViewModel!.OnPropertyChangedWithValue(value);
                }
            }
        }


        public override void OnRefresh()
        {
            Materials.Clear();
            var settlement = Settlement.CurrentSettlement;

            Materials.Add(new MaterialItemVM(DefaultItems.HardWood, settlement));
            Materials.Add(new MaterialItemVM(Campaign.Current.ObjectManager.GetObject<ItemObject>("clay"), settlement));
            Materials.Add(new MaterialItemVM(BKItems.Instance.Limestone, settlement));
            Materials.Add(new MaterialItemVM(BKItems.Instance.Marble, settlement));
        }
    }
}