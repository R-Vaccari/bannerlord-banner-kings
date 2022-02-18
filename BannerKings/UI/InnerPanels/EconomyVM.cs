using BannerKings.Managers.Decisions;
using BannerKings.Managers.Policies;
using BannerKings.Populations;
using BannerKings.UI.Items;
using System;
using System.Collections.Generic;
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
        private PopulationOptionVM exportToogle, tariffToogle;
        private BKTaxPolicy taxItem;
        private BKCriminalPolicy criminalItem;
        private Settlement settlement;
        private ItemRoster roster;

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
            RevenueInfo.Add(new InformationElement("Caravan Attractiveness:", FormatValue(data.EconomicData.CaravanAttraction.ResultNumber),
                  "How attractive this town is for caravans. Likelihood of caravan visits are dictated mainly by prices, and attractiveness is a factor added on top of that"));
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

            criminalItem = (BKCriminalPolicy)BannerKingsConfig.Instance.PolicyManager.GetPolicy(settlement, "criminal");
            CriminalSelector = base.GetSelector(criminalItem, new Action<SelectorVM<BKItemVM>>(this.criminalItem.OnChange));

            //tariffItem = (BKTariffPolicy)BannerKingsConfig.Instance.PolicyManager.GetPolicy(settlement, "tariff"); 
            //TariffSelector = base.GetSelector(tariffItem, new Action<SelectorVM<BKItemVM>>(this.tariffItem.OnChange));

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
                    case "decision_slaves_export":
                        exportToogle = vm;
                        break;
                    case "decision_tariff_exempt":
                        tariffToogle = vm;
                        break;
                }
            }
        }

        private void OnGuildPress()
        {
            UIManager.instance.InitializeGuildWindow();
        }

        [DataSourceProperty]
        public HintViewModel GuildHint => new HintViewModel(new TextObject("{=!}Take actions and check status of local guild, if any is present"));

        [DataSourceProperty]
        public bool GuildAvailable
        {
            get
            {
                if (this.settlement.Town != null)
                    return data.EconomicData.Guild != null;

                return false;
            }
        }

        private void OnTournamentPress()
        {
            TournamentData tournament = new TournamentData(this.settlement.Town);
            data.TournamentData = tournament;
            this.roster = tournament.Roster;
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
                if (this.settlement.Town != null)
                    return !this.settlement.Town.HasTournament && Hero.MainHero.Gold >= 5000;
                
                return false;
            }
        }

        [DataSourceProperty]
        public PopulationOptionVM ExportToogle
        {
            get => exportToogle;
            set
            {
                if (value != exportToogle)
                {
                    exportToogle = value;
                    base.OnPropertyChangedWithValue(value, "ExportToogle");
                }
            }
        }

        [DataSourceProperty]
        public PopulationOptionVM TariffToogle
        {
            get => tariffToogle;
            set
            {
                if (value != tariffToogle)
                {
                    tariffToogle = value;
                    base.OnPropertyChangedWithValue(value, "TariffToogle");
                }
            }
        }

        [DataSourceProperty]
        public SelectorVM<BKItemVM> CriminalSelector
        {
            get
            {
                return this.criminalSelector;
            }
            set
            {
                if (value != this.criminalSelector)
                {
                    this.criminalSelector = value;
                    base.OnPropertyChangedWithValue(value, "CriminalSelector");
                }
            }
        }

       /* [DataSourceProperty]
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
        } */

        [DataSourceProperty]
        public SelectorVM<BKItemVM> TaxSelector
        {
            get
            {
                return this.taxSelector;
            }
            set
            {
                if (value != this.taxSelector)
                {
                    this.taxSelector = value;
                    base.OnPropertyChangedWithValue(value, "TaxSelector");
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

        public override void OnFinalize()
        {
            base.OnFinalize();
            if (this.roster != null && !this.roster.IsEmpty())
            {
                ITournamentManager tournamentManager = Campaign.Current.TournamentManager;
                tournamentManager.AddTournament(Campaign.Current.Models.TournamentModel.CreateTournament(this.settlement.Town));
                Hero.MainHero.ChangeHeroGold(-5000);
                InformationManager.DisplayMessage(new InformationMessage(String
                    .Format("Tournament started with prize: {0}", this.data.TournamentData.Prize.Name), "event:/ui/notification/coins_negative"));
            }     
        }
    }
}
