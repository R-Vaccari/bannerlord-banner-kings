using BannerKings.Extensions;
using BannerKings.Managers.Populations.Estates;
using BannerKings.UI.Items;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.ClanManagement;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Core.ViewModelCollection.Selector;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using static BannerKings.Managers.Populations.Estates.Estate;

namespace BannerKings.UI.VanillaTabs.Clans
{
    internal class ClanIncomeEstateVM : ClanFinanceIncomeItemBaseVM
    {
        private readonly Action<ClanIncomeEstateVM> _onSelectionT;
        private BannerKingsSelectorVM<BKItemVM> taskSelector;
        private void tempOnSelection(ClanFinanceIncomeItemBaseVM item) => _onSelectionT(this);

        internal ClanIncomeEstateVM(Estate estate, Action<ClanIncomeEstateVM> onSelection, Action onRefresh) : base(null, onRefresh)
        {
            Estate = estate;
            _onSelection = new Action<ClanFinanceIncomeItemBaseVM>(tempOnSelection);
            _onSelectionT = onSelection;
            RefreshValues();
        }

        public Estate Estate { get; private set; }

        public override void RefreshValues()
        {
            Name = Estate.Name.ToString();
            Income = Estate.Income;
            IncomeValueText = GameTexts.FindText("str_plus_with_number", null).SetTextVariable("NUMBER", Income).ToString();

            SettlementComponent settlementComponent = Estate.EstatesData.Settlement.SettlementComponent;
            Location = settlementComponent.Settlement.Name.ToString();
           
            ImageName = ((settlementComponent != null) ? settlementComponent.WaitMeshName : "");

            TaskSelector = new BannerKingsSelectorVM<BKItemVM>(true, 0, OnTaskChange);
            TaskSelector.AddItem(new BKItemVM(0, 
                true, 
                new TextObject("{=!}Production task allows your workforce to produce goods wtihout hinderances. This task yields the most productivity and thus income by the estate."),
                GameTexts.FindText("str_bk_estate_task", EstateTask.Prodution.ToString())));

            TaskSelector.AddItem(new BKItemVM(1, 
                true,
                 new TextObject("{=!}Land expansion task sets part of your workforce to expand the estate's useful acreage. They wont be productive and thus the income is reduced. Increased acreage allows more population capacity."),
                GameTexts.FindText("str_bk_estate_task", EstateTask.Land_Expansion.ToString())));

            TaskSelector.AddItem(new BKItemVM(2,
               true,
                new TextObject("{=!}Drafting increases the military participation of your estate population, but reduces its general output."),
               GameTexts.FindText("str_bk_estate_task", EstateTask.Military.ToString())));

            TaskSelector.SelectedIndex = (int)Estate.Task;
            TaskSelector.SetOnChangeAction(OnTaskChange);
        }

        private void OnTaskChange(SelectorVM<BKItemVM> obj)
        {
            if (obj.SelectedItem != null)
            {
                var vm = obj.GetCurrentItem();
                Estate.ChangeTask((EstateTask)vm.Value);
                ItemProperties.Clear();
                PopulateStatsList();
            }
        }

        protected override void PopulateStatsList()
        {
            ItemProperties.Add(new SelectableItemPropertyVM(new TextObject("{=!}Acreage", null).ToString(),
               Estate.Acreage.ToString("0.00"),
               false,
               null));

            ExplainedNumber price = Estate.AcrePriceExplained;
            TextObject priceT = new TextObject("{=!}Acre Value");
            ItemProperties.Add(new SelectableItemPropertyVM(priceT.ToString(),
               price.ResultNumber.ToString("0"),
               false,
               new BasicTooltipViewModel(() => UIHelper.GetAccumulatingWithDescription(priceT,
                new TextObject("{=!}Acre Value represents the monetary value of each acre, variable according to the local economy."),
                price.ResultNumber,
                false,
                ref price))
               ));
            

            ItemProperties.Add(new SelectableItemPropertyVM(new TextObject("{=!}Workforce", null).ToString(),
               (Estate.Population + Estate.Slaves).ToString(),
               false,
               new BasicTooltipViewModel(() => UIHelper.GetEstateWorkforceTooltip(Estate))
               ));

            ExplainedNumber tax = Estate.TaxRatio;
            TextObject taxT = new TextObject("{=!}Tax Rate");
            ItemProperties.Add(new SelectableItemPropertyVM(taxT.ToString(),
               UIHelper.FormatValue(tax.ResultNumber),
               false,
                new BasicTooltipViewModel(() => UIHelper.GetAccumulatingWithDescription(taxT,
                new TextObject("{=!}Represents the % that is deducted from this estate's income, and taken by the village lord instead. If the village lord and estate owner are the same, it makes no difference in the final income."),
                tax.ResultNumber,
                false,
                ref tax))));

            ExplainedNumber capacity = Estate.PopulationCapacityExplained;
            TextObject capacityT = new TextObject("{=!}Population Capacity");
            ItemProperties.Add(new SelectableItemPropertyVM(capacityT.ToString(),
               capacity.ResultNumber.ToString("0"),
               false,
               new BasicTooltipViewModel(() => UIHelper.GetAccumulatingWithDescription(capacityT,
                new TextObject("{=!}The maximum capacity of people this estate can support. Population grows naturally, or can be grown directly by adding slaves to the estate."),
                capacity.ResultNumber,
                false,
                ref capacity))
               ));

            ExplainedNumber manpower = Estate.MaxManpowerExplained;
            TextObject manpowerT = new TextObject("{=!}Manpower Capacity");
            ItemProperties.Add(new SelectableItemPropertyVM(manpowerT.ToString(),
               manpower.ResultNumber.ToString("0"),
               false,
               new BasicTooltipViewModel(() => UIHelper.GetAccumulatingWithDescription(manpowerT,
                new TextObject("{=!}The maximum capacity of people this estate's retinue can be. The retinue is determined by the estate's population, local task and militarism."),
                manpower.ResultNumber,
                false,
                ref manpower))
               ));

            ExplainedNumber production = Estate.Production;
            TextObject productionT = new TextObject("{=!}Production");
            ItemProperties.Add(new SelectableItemPropertyVM(productionT.ToString(),
               production.ResultNumber.ToString("0.00") + '%',
               false,
                new BasicTooltipViewModel(() => UIHelper.GetAccumulatingWithDescription(productionT,
                new TextObject("{=!}Represents the share of the village's production that is owned by this estate. The estate's income equals the total village production * estate's production share * (1 - tax rate)."),
                production.ResultNumber,
                false,
                ref production))
               ));

            ExplainedNumber value = Estate.EstateValue;
            TextObject valueT = new TextObject("{=!}Estate Value");
            ItemProperties.Add(new SelectableItemPropertyVM(valueT.ToString(),
               value.ResultNumber.ToString("0"),
               false,
               new BasicTooltipViewModel(() => UIHelper.GetAccumulatingWithDescription(valueT,
                new TextObject("{=!}Estate Value is the estate's monetary price, according to its acreage, income and local economy."),
                value.ResultNumber,
                false,
                ref value))
               ));

            ItemProperties.Add(new SelectableItemPropertyVM(new TextObject("{=!}Last Income", null).ToString(),
               Estate.LastIncome.ToString(),
               false,
               null));
        }

        [DataSourceProperty]
        public BannerKingsSelectorVM<BKItemVM> TaskSelector
        {
            get => taskSelector;
            set
            {
                if (value != taskSelector)
                {
                    taskSelector = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }
    }
}
