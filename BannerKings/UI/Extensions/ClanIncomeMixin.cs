using BannerKings.Behaviours.Workshops;
using BannerKings.UI.Items.UI;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.ClanManagement.Categories;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.Extensions
{
    [ViewModelMixin("OnIncomeSelection")]
    internal class ClanIncomeMixin : BaseViewModelMixin<ClanIncomeVM>
    {
        private MBBindingList<InformationElement> workshopInfo;
        private bool canUpgrade, isSelected;

        private ClanIncomeVM viewModel;
        public ClanIncomeMixin(ClanIncomeVM vm) : base(vm)
        {
            viewModel = vm;
            WorkshopInfo = new MBBindingList<InformationElement>();
        }

        [DataSourceProperty] public string UpgradeText => new TextObject("{=1rE1OLMj}Upgrade").ToString();

        public override void OnRefresh()
        {
            base.OnRefresh();
            WorkshopInfo.Clear();

            if (viewModel.CurrentSelectedIncome != null)
            {
                viewModel.CurrentSelectedIncome.RefreshValues();
                var workshop = viewModel.CurrentSelectedIncome.Workshop;
                var time = CampaignTime.Now - CampaignTime.Days(workshop.NotRunnedDays);
                //WorkshopInfo.Add(new InformationElement(new TextObject("{=OXes1FKN}Last run:").ToString(),
                //    time.ToString(),
                //    new TextObject("{=a2FbzfvH}The last day this workshop has successfully run without issues.").ToString()));

                CanUpgrade = workshop.CanBeUpgraded;
                TextObject state = new TextObject("{=Bom9FTfz}Running normally.");
                if (!workshop.IsRunning && workshop.ConstructionTimeRemained > 0)
                {
                    state = new TextObject("{=KnWnjURr}Workshop under expansion! {DAYS} construction day(s) left.")
                        .SetTextVariable("DAYS", workshop.ConstructionTimeRemained);
                    CanUpgrade = false;
                }
                else
                {
                    bool inventory = false;
                    WorkshopData data = Campaign.Current.GetCampaignBehavior<BKWorkshopBehavior>().GetInventory(workshop);
                    if (data != null)
                    {
                        if (data.IsRunningOnInventory)
                        {
                            inventory = true;
                            state = new TextObject("{=VX9LJJpS}Workshop is running on inventory!");
                        }

                        WorkshopInfo.Add(new InformationElement(GameTexts.FindText("str_inventory").ToString() + ':',
                            new TextObject("{=8YCJrv0F}{NUMBER} / {CAPACITY}")
                            .SetTextVariable("NUMBER", data.GetInventoryCount())
                            .SetTextVariable("CAPACITY", data.GetInventoryCapacity())
                            .ToString(),
                            ""));
                    }

                    if (!inventory)
                    {
                        var behavior = Campaign.Current.GetCampaignBehavior<WorkshopsCampaignBehavior>();
                        var method = AccessTools.Method(behavior.GetType(), "DetermineTownHasSufficientInputs");
                        for (int i = 0; i < workshop.WorkshopType.Productions.Count; i++)
                        {
                            bool insufficient = (bool)method.Invoke(behavior, new object[] { workshop.WorkshopType.Productions[i],
                    workshop.Settlement.Town, 0 });
                            if (!insufficient)
                            {
                                CampaignUIHelper.ProductInputOutputEqualityComparer comparer = new CampaignUIHelper.ProductInputOutputEqualityComparer();
                                IEnumerable<TextObject> texts = from x in workshop.WorkshopType.Productions.SelectMany((WorkshopType.Production p) => p.Outputs).Distinct(comparer)
                                                                select x.Item1.GetName();
                                state = new TextObject("{=Pc4NcRHR}Insufficient inputs! {TOWN} is missing inputs for {OUTPUT}.")
                                    .SetTextVariable("TOWN", workshop.Settlement.Name)
                                    .SetTextVariable("OUTPUT", texts.ElementAt(0));
                                break;
                            }
                        }
                    }
  
                }

                WorkshopInfo.Add(new InformationElement(new TextObject("{=QP368C1V}Running state:"),
                    state,
                    new TextObject("{=66zjiMe4}The workshop running conditions. It may not be producing due to lack of necessary inputs.")));

                var tax = BannerKingsConfig.Instance.WorkshopModel.CalculateWorkshopTax(workshop.Settlement, workshop.Owner);
                WorkshopInfo.Add(new InformationElement(new TextObject("{=1qiPuhoF}Tax ratio:").ToString(),
                    FormatValue(tax.ResultNumber),
                    tax.GetExplanations()));

                var quality = BannerKingsConfig.Instance.WorkshopModel.GetProductionQuality(workshop, true);
                WorkshopInfo.Add(new InformationElement(new TextObject("{=6gaLfex6}Production Quality:").ToString(),
                    FormatValue(quality.ResultNumber),
                    quality.GetExplanations()));
            }
        }

        protected string FormatValue(float value)
        {
            return (value * 100f).ToString("0.00") + '%';
        }

        [DataSourceMethod]
        public void ExecuteUpgrade()
        {
            int cost = BannerKingsConfig.Instance.WorkshopModel.GetUpgradeCost(viewModel.CurrentSelectedIncome.Workshop);
            InformationManager.ShowInquiry(new InquiryData(
                new TextObject("{=kZMXmhpv}Workshop Upgrading").ToString(),
                new TextObject("{=gVt0aj0G}Would you like to upgrade this workshop property? By upgrading, the workshop's expenses will increase by 12%, while it's production quality increases by 8%. The process will take 3 construction days and cost {COST}{GOLD_ICON}")
                .SetTextVariable("COST", cost)
                .ToString(),
                Hero.MainHero.Gold >= cost,
                true,
                GameTexts.FindText("str_accept").ToString(),
                GameTexts.FindText("str_cancel").ToString(),
                () =>
                {
                    var workshop = viewModel.CurrentSelectedIncome.Workshop;
                    AccessTools.Property(workshop.GetType(), "ConstructionTimeRemained").SetValue(workshop, 3);
                    AccessTools.Property(workshop.GetType(), "Level").SetValue(workshop, workshop.Level + 1);
                    Hero.MainHero.ChangeHeroGold(-cost);
                    OnRefresh();
                },
                null));
        }

        [DataSourceProperty]
        public bool WorkshopSelected
        {
            get => isSelected;
            set
            {
                if (value != isSelected)
                {
                    isSelected = value;
                    ViewModel!.OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public bool CanUpgrade
        {
            get => canUpgrade;
            set
            {
                if (value != canUpgrade)
                {
                    canUpgrade = value;
                    ViewModel!.OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<InformationElement> WorkshopInfo
        {
            get => workshopInfo;
            set
            {
                if (value != workshopInfo)
                {
                    workshopInfo = value;
                    ViewModel!.OnPropertyChangedWithValue(value);
                }
            }
        }
    }
}
