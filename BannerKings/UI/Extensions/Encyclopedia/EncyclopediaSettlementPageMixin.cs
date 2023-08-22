using BannerKings.UI.Items;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.Pages;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace BannerKings.UI.Extensions.Encyclopedia
{
    [ViewModelMixin("Refresh")]
    internal class EncyclopediaSettlementPageMixin : BaseViewModelMixin<EncyclopediaSettlementPageVM>
    {
        private bool addedFields;
        private readonly EncyclopediaSettlementPageVM settlementPageVM;
        private MBBindingList<EncyclopediaWorkshopVM> workshops;

        public EncyclopediaSettlementPageMixin(EncyclopediaSettlementPageVM vm) : base(vm)
        {
            settlementPageVM = vm;
            Workshops = new MBBindingList<EncyclopediaWorkshopVM>();
        }

        [DataSourceProperty] public string WorkshopsText => GameTexts.FindText("str_clan_workshops").ToString();

        public override void OnRefresh()
        {
            var settlement = settlementPageVM.Obj as Settlement;
            Workshops.Clear();

            if (settlement.Town != null)
            {
                foreach (var workshop in settlement.Town.Workshops)
                {
                    if (workshop.WorkshopType.StringId == "artisans")
                    {
                        continue;
                    }

                    Workshops.Add(new EncyclopediaWorkshopVM(workshop));
                }
            }

            if (!addedFields)
            {
                addedFields = true;
            }
        }

        [DataSourceProperty]
        public MBBindingList<EncyclopediaWorkshopVM> Workshops
        {
            get => workshops;
            set
            {
                if (value != workshops)
                {
                    workshops = value;
                    ViewModel!.OnPropertyChangedWithValue(value, "Workshops");
                }
            }
        }
    }
}