using BannerKings.Managers.Decisions;
using BannerKings.Managers.Policies;
using BannerKings.Populations;
using BannerKings.UI.Items;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using static BannerKings.Managers.PopulationManager;

namespace BannerKings.UI
{
    public class EconomyVM : BannerKingsViewModel
    {
        private MBBindingList<InformationElement> productionInfo, revenueInfo, satisfactionInfo;
        private SelectorVM<BKItemVM> militiaSelector, garrisonSelector, draftSelector;
        private BKTariffPolicy tariffItem;
        private BKTaxPolicy taxItem;
        private BKDraftPolicy draftItem;
        private Settlement settlement;

        public EconomyVM(PopulationData data, Settlement _settlement, bool selected) : base(data, selected)
        {
            productionInfo = new MBBindingList<InformationElement>();
            revenueInfo = new MBBindingList<InformationElement>();
            satisfactionInfo = new MBBindingList<InformationElement>();
            this.settlement = _settlement;
            this.RefreshValues();
        }

        public override void RefreshValues()
        {
            base.RefreshValues();

            ProductionInfo.Clear();
            RevenueInfo.Clear();
            SatisfactionInfo.Clear();

            ProductionInfo.Add(new InformationElement("Merchants' Revenue:", data.EconomicData.MerchantRevenue.ToString(),
               "Daily revenue of local merchants, based on slave workforce and production efficiency"));
            ProductionInfo.Add(new InformationElement("State Slaves:", FormatValue(data.EconomicData.StateSlaves),
               "Percentage of slaves in this settlement that are state-owned and therefore used for state purposes such as building projects"));
            ProductionInfo.Add(new InformationElement("Production Quality:", FormatValue(data.EconomicData.ProductionQuality.ResultNumber),
               "How many raw goods are required for manufacturing. Higher quality means that output is produced with less input"));
            ProductionInfo.Add(new InformationElement("Production Efficiency:", FormatValue(data.EconomicData.ProductionEfficiency.ResultNumber),
               "The speed at which workshops produce goods, affected by kingdom policies and craftsmen"));
            
            RevenueInfo.Add(new InformationElement("Tariff:", FormatValue(data.EconomicData.Tariff),
                  "Percentage of an item's value charged as tax when sold"));
            RevenueInfo.Add(new InformationElement("Mercantilism:", FormatValue(data.EconomicData.Tariff),
                  "Represents how economicaly free craftsmen, tradesmen and guilds are. Increased mercantilism reduces the tax revenue of these, but allows them to" +
                  " accumulate wealth or contribute more to overall prosperity"));
            RevenueInfo.Add(new InformationElement("Corruption:", FormatValue(data.EconomicData.Corruption),
                  "Tax being diverted for private purposes as opposed to being paid to you"));
            RevenueInfo.Add(new InformationElement("Administrative Cost:", FormatValue(data.EconomicData.AdministrativeCost.ResultNumber),
                    "Costs associated with the settlement administration, including those of active policies and decisions, deducted on tax revenue"));

            for (int i = 0; i < 4; i++)
            {
                float value = data.EconomicData.Satisfactions[i];
                ConsumptionType type = (ConsumptionType)i;
                string desc = type.ToString() + " Goods:";
                SatisfactionInfo.Add(new InformationElement(desc, FormatValue(value), Helpers.Helpers.GetConsumptionHint(type)));
            }

            taxItem = (BKTaxPolicy)BannerKingsConfig.Instance.PolicyManager.GetPolicy(settlement, "tax");
            TaxSelector = base.GetSelector(taxItem, new Action<SelectorVM<BKItemVM>>(this.taxItem.OnChange));

            tariffItem = (BKTariffPolicy)BannerKingsConfig.Instance.PolicyManager.GetPolicy(settlement, "tariff"); 
            TariffSelector = base.GetSelector(tariffItem, new Action<SelectorVM<BKItemVM>>(this.tariffItem.OnChange));

            draftItem = (BKDraftPolicy)BannerKingsConfig.Instance.PolicyManager.GetPolicy(settlement, "draft");
            DraftSelector = base.GetSelector(draftItem, new Action<SelectorVM<BKItemVM>>(this.draftItem.OnChange));

            /*
            HashSet<BannerKingsDecision> decisions = BannerKingsConfig.Instance.PolicyManager.GetDefaultDecisions(settlement);
            foreach (BannerKingsDecision decision in decisions)
            {
                PopulationOptionVM vm = new PopulationOptionVM()
                .SetAsBooleanOption(decision.GetName(), decision.Enabled, delegate (bool value)
                {
                    decision.OnChange(value);
                    this.RefreshValues();

                }, new TextObject(decision.GetHint()));
                switch (decision.GetIdentifier())
                {
                    case "":
                        break;
                }
            }*/
        }

        [DataSourceProperty]
        public SelectorVM<BKItemVM> DraftSelector
        {
            get
            {
                return this.draftSelector;
            }
            set
            {
                if (value != this.draftSelector)
                {
                    this.draftSelector = value;
                    base.OnPropertyChangedWithValue(value, "DraftSelector");
                }
            }
        }

        [DataSourceProperty]
        public SelectorVM<BKItemVM> TariffSelector
        {
            get
            {
                return this.garrisonSelector;
            }
            set
            {
                if (value != this.garrisonSelector)
                {
                    this.garrisonSelector = value;
                    base.OnPropertyChangedWithValue(value, "GarrisonSelector");
                }
            }
        }

        [DataSourceProperty]
        public SelectorVM<BKItemVM> TaxSelector
        {
            get
            {
                return this.militiaSelector;
            }
            set
            {
                if (value != this.militiaSelector)
                {
                    this.militiaSelector = value;
                    base.OnPropertyChangedWithValue(value, "MilitiaSelector");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<InformationElement> ProductionInfo
        {
            get => productionInfo;
            set
            {
                if (value != productionInfo)
                {
                    productionInfo = value;
                    base.OnPropertyChangedWithValue(value, "DefenseInfo");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<InformationElement> RevenueInfo
        {
            get => revenueInfo;
            set
            {
                if (value != revenueInfo)
                {
                    revenueInfo = value;
                    base.OnPropertyChangedWithValue(value, "ManpowerInfo");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<InformationElement> SatisfactionInfo
        {
            get => satisfactionInfo;
            set
            {
                if (value != satisfactionInfo)
                {
                    satisfactionInfo = value;
                    base.OnPropertyChangedWithValue(value, "SiegeInfo");
                }
            }
        }
    }
}
