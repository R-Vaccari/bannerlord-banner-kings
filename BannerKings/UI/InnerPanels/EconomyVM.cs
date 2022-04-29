﻿using BannerKings.Managers.Decisions;
using BannerKings.Managers.Policies;
using BannerKings.Models;
using BannerKings.Populations;
using BannerKings.UI.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using static BannerKings.Managers.PopulationManager;

namespace BannerKings.UI
{
    public class EconomyVM : BannerKingsViewModel
    {
        private MBBindingList<InformationElement> productionInfo, revenueInfo, satisfactionInfo;
        private SelectorVM<BKItemVM> taxSelector, criminalSelector;
        private DecisionElement exportToogle, tariffToogle, slaveTaxToogle, mercantilismToogle;
        private BKTaxPolicy taxItem;
        private BKCriminalPolicy criminalItem;
        private Settlement settlement;
        private ItemRoster roster;

        public EconomyVM(PopulationData data, Settlement _settlement, bool selected) : base(data, selected)
        {
            productionInfo = new MBBindingList<InformationElement>();
            revenueInfo = new MBBindingList<InformationElement>();
            satisfactionInfo = new MBBindingList<InformationElement>();
            settlement = _settlement;
            RefreshValues();
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            ProductionInfo.Clear();
            RevenueInfo.Clear();
            SatisfactionInfo.Clear();

            ProductionInfo.Add(new InformationElement("Merchants' Revenue:", data.EconomicData.MerchantRevenue.ToString(),
               "Daily revenue of local merchants, based on slave workforce and production efficiency."));
            ProductionInfo.Add(new InformationElement("State Slaves:", FormatValue(data.EconomicData.StateSlaves),
               "Percentage of slaves in this settlement that are state-owned and therefore used for state purposes such as building projects."));
            ProductionInfo.Add(new InformationElement("Production Quality:", FormatValue(data.EconomicData.ProductionQuality.ResultNumber),
               "How much workshops expend to produce output. Higher quality yields more profit."));
            ProductionInfo.Add(new InformationElement("Production Efficiency:", FormatValue(data.EconomicData.ProductionEfficiency.ResultNumber),
               "The speed at which workshops produce goods, affected by kingdom policies and craftsmen."));

            if (IsVillage)
            {
                VillageData villageData = data.VillageData;
                BKVillageProductionModel model = new BKVillageProductionModel();
                float productionQuantity = 0f;
                StringBuilder sb = new StringBuilder();
                foreach ((ItemObject, float) production in BannerKingsConfig.Instance.PopulationManager.GetProductions(villageData))
                {
                    sb.Append(production.Item1.Name + ", ");
                    productionQuantity += model.CalculateDailyProductionAmount(villageData.Village, production.Item1);
                }
                sb.Remove(sb.Length - 2, 1);
                string productionString = sb.ToString();
                ProductionInfo.Add(new InformationElement("Goods Production:", productionQuantity + " (Daily)",
                   "How much the local population can progress with construction projects, on a daily basis."));
                ProductionInfo.Add(new InformationElement("Items Produced:", productionString,
                   "Goods locally produced by the population."));
            } 
            else
            {
                RevenueInfo.Add(new InformationElement("Tariff:", FormatValue(data.EconomicData.Tariff),
                      "Percentage of an item's value charged as tax when sold."));
                RevenueInfo.Add(new InformationElement("Mercantilism:", FormatValue(data.EconomicData.Mercantilism.ResultNumber),
                      "Represents how economicaly free craftsmen, tradesmen and guilds are. Increased mercantilism reduces the tax revenue of these, but allows them to" +
                      " accumulate wealth or contribute more to overall prosperity."));
                RevenueInfo.Add(new InformationElement("Caravan Attractiveness:", FormatValue(data.EconomicData.CaravanAttraction.ResultNumber),
                      "How attractive this town is for caravans. Likelihood of caravan visits are dictated mainly by prices, and attractiveness is a factor added on top of that."));

                for (int i = 0; i < 4; i++)
                {
                    float value = data.EconomicData.Satisfactions[i];
                    ConsumptionType type = (ConsumptionType)i;
                    string desc = type + " Goods:";
                    SatisfactionInfo.Add(new InformationElement(desc, FormatValue(value), Helpers.Helpers.GetConsumptionHint(type)));
                }

                criminalItem = (BKCriminalPolicy)BannerKingsConfig.Instance.PolicyManager.GetPolicy(settlement, "criminal");
                CriminalSelector = GetSelector(criminalItem, criminalItem.OnChange);
                CriminalSelector.SelectedIndex = criminalItem.Selected;
                CriminalSelector.SetOnChangeAction(criminalItem.OnChange);

                List<BannerKingsDecision> decisions = BannerKingsConfig.Instance.PolicyManager.GetDefaultDecisions(settlement);
                BannerKingsDecision slaveDecision = decisions.FirstOrDefault(x => x.GetIdentifier() == "decision_slaves_export");
                BannerKingsDecision tariffDecision = decisions.FirstOrDefault(x => x.GetIdentifier() == "decision_tariff_exempt");
                BannerKingsDecision slaveTaxDecision = decisions.FirstOrDefault(x => x.GetIdentifier() == "decision_slaves_tax");
                BannerKingsDecision mercantilismDecision = decisions.FirstOrDefault(x => x.GetIdentifier() == "decision_mercantilism");
                exportToogle = new DecisionElement()
                    .SetAsBooleanOption(slaveDecision.GetName(), slaveDecision.Enabled, delegate (bool value)
                    {
                        slaveDecision.OnChange(value);
                        RefreshValues();

                    }, new TextObject(slaveDecision.GetHint()));

                tariffToogle = new DecisionElement()
                    .SetAsBooleanOption(tariffDecision.GetName(), tariffDecision.Enabled, delegate (bool value)
                    {
                        tariffDecision.OnChange(value);
                        RefreshValues();

                    }, new TextObject(slaveDecision.GetHint()));

                slaveTaxToogle = new DecisionElement()
                    .SetAsBooleanOption(slaveTaxDecision.GetName(), slaveTaxDecision.Enabled, delegate (bool value)
                    {
                        slaveTaxDecision.OnChange(value);
                        RefreshValues();

                    }, new TextObject(slaveTaxDecision.GetHint()));

                mercantilismToogle = new DecisionElement()
                    .SetAsBooleanOption(mercantilismDecision.GetName(), mercantilismDecision.Enabled, delegate (bool value)
                    {
                        mercantilismDecision.OnChange(value);
                        RefreshValues();

                    }, new TextObject(mercantilismDecision.GetHint()));
            }

            RevenueInfo.Add(new InformationElement("Administrative Cost:", FormatValue(data.EconomicData.AdministrativeCost.ResultNumber),
                    "Costs associated with the settlement administration, including those of active policies and decisions, deducted on tax revenue."));

            taxItem = (BKTaxPolicy)BannerKingsConfig.Instance.PolicyManager.GetPolicy(settlement, "tax");
            TaxSelector = GetSelector(taxItem, taxItem.OnChange);
            TaxSelector.SelectedIndex = taxItem.Selected;
            TaxSelector.SetOnChangeAction(taxItem.OnChange);

            
        }

        private void OnTournamentPress()
        {
            TournamentData tournament = new TournamentData(settlement.Town);
            data.TournamentData = tournament;
            roster = tournament.Roster;
            InventoryManager.OpenScreenAsStash(tournament.Roster);
            RefreshValues();
        }

        [DataSourceProperty]
        public HintViewModel TournamentHint => new HintViewModel(new TextObject("{=!}Sponsor a tournament in this town. As the main sponsor, you have to pay 5000 coins for the tournament costs, as well as " +
            "provide an adequate prize. Sponsoring games improves population loyalty towards you, and valuable prizes provide renown to your name."));

        [DataSourceProperty]
        public bool TournamentAvailable 
        {
            get 
            {
                if (settlement.IsTown)
                    return !settlement.Town.HasTournament && Hero.MainHero.Gold >= 5000;
                
                return false;
            }
        }

        [DataSourceProperty]
        public DecisionElement ExportToogle
        {
            get => exportToogle;
            set
            {
                if (value != exportToogle)
                {
                    exportToogle = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public DecisionElement TariffToogle
        {
            get => tariffToogle;
            set
            {
                if (value != tariffToogle)
                {
                    tariffToogle = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public DecisionElement SlaveTaxToogle
        {
            get => slaveTaxToogle;
            set
            {
                if (value != slaveTaxToogle)
                {
                    slaveTaxToogle = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public DecisionElement MercantilismToogle
        {
            get => mercantilismToogle;
            set
            {
                if (value != mercantilismToogle)
                {
                    mercantilismToogle = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public SelectorVM<BKItemVM> CriminalSelector
        {
            get => criminalSelector;
            set
            {
                if (value != criminalSelector)
                {
                    criminalSelector = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public SelectorVM<BKItemVM> TaxSelector
        {
            get => taxSelector;
            set
            {
                if (value != taxSelector)
                {
                    taxSelector = value;
                    OnPropertyChangedWithValue(value);
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
                    OnPropertyChangedWithValue(value);
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
                    OnPropertyChangedWithValue(value);
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
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        public override void OnFinalize()
        {
            base.OnFinalize();
            if (roster != null && !roster.IsEmpty())
            {
                ITournamentManager tournamentManager = Campaign.Current.TournamentManager;
                tournamentManager.AddTournament(Campaign.Current.Models.TournamentModel.CreateTournament(settlement.Town));
                Hero.MainHero.ChangeHeroGold(-5000);
                InformationManager.DisplayMessage(new InformationMessage(String
                    .Format("Tournament started with prize: {0}", data.TournamentData.Prize.Name), "event:/ui/notification/coins_negative"));
            }     
        }
    }
}
