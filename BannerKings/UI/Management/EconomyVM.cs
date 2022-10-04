using System.Linq;
using System.Text;
using BannerKings.Managers.Policies;
using BannerKings.Managers.Populations;
using BannerKings.Models.Vanilla;
using BannerKings.UI.Items;
using BannerKings.UI.Items.UI;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Inventory;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Core.ViewModelCollection.Selector;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using static BannerKings.Managers.PopulationManager;

namespace BannerKings.UI.Management
{
    public class EconomyVM : BannerKingsViewModel
    {
        private BKCriminalPolicy criminalItem;
        private DecisionElement exportToogle, tariffToogle, slaveTaxToogle, mercantilismToogle;
        private MBBindingList<InformationElement> productionInfo, revenueInfo, satisfactionInfo;
        private ItemRoster roster;
        private readonly Settlement settlement;
        private BKTaxPolicy taxItem;
        private SelectorVM<BKItemVM> taxSelector, criminalSelector;

        public EconomyVM(PopulationData data, Settlement _settlement, bool selected) : base(data, selected)
        {
            productionInfo = new MBBindingList<InformationElement>();
            revenueInfo = new MBBindingList<InformationElement>();
            satisfactionInfo = new MBBindingList<InformationElement>();
            settlement = _settlement;
            RefreshValues();
        }

        [DataSourceProperty]
        public HintViewModel TournamentHint => new(new TextObject("{=VeMSE94s}Sponsor a tournament in this town. As the main sponsor, you have to pay 5000 coins for the tournament costs, as well as " +
                                                                  "provide an adequate prize. Sponsoring games improves population loyalty towards you, and valuable prizes provide renown to your name."));

        [DataSourceProperty]
        public bool TournamentAvailable
        {
            get
            {
                if (settlement.IsTown)
                {
                    return !settlement.Town.HasTournament && Hero.MainHero.Gold >= 5000;
                }

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

        public override void RefreshValues()
        {
            base.RefreshValues();
            ProductionInfo.Clear();
            RevenueInfo.Clear();
            SatisfactionInfo.Clear();

           

            ProductionInfo.Add(new InformationElement(new TextObject("{=NnOoYOTC}State Slaves:").ToString(),
                $"{data.EconomicData.StateSlaves:P}",
                new TextObject("{=yJzJMg5Z}Percentage of slaves in this settlement that are state-owned and therefore used for state purposes such as building projects.")
                    .ToString()));

            var quality = data.EconomicData.ProductionQuality;
            ProductionInfo.Add(new InformationElement(new TextObject("{=6gaLfex6}Production Quality:").ToString(),
                $"{quality.ResultNumber:P}",
                new TextObject("{=ez3NzFgO}{TEXT}\n{EXPLANATIONS}")
                    .SetTextVariable("TEXT",
                        new TextObject(
                            "{=rHXu32s5}Describes the quality of products produced in this settlement. Higher quality means workshops are more likely to produce goods with positive modifiers, therefore yielding a higher income. Because better products are more expensive, more money is extracted from caravans into market gold and the owner's tariff. This is also a factor in workshop prices."))
                    .SetTextVariable("EXPLANATIONS", quality.GetExplanations())
                    .ToString()));

            var efficiency = data.EconomicData.ProductionEfficiency;
            ProductionInfo.Add(new InformationElement(new TextObject("{=oJKPne1U}Production Efficiency:").ToString(),
                $"{efficiency.ResultNumber:P}",
                new TextObject("{=ez3NzFgO}{TEXT}\n{EXPLANATIONS}")
                    .SetTextVariable("TEXT",
                        new TextObject("{=0z7FL2oe}The speed at which workshops produce goods, affected by kingdom policies and craftsmen"))
                    .SetTextVariable("EXPLANATIONS", efficiency.GetExplanations())
                    .ToString()));


            if (IsVillage)
            {
                var villageData = data.VillageData;

                ProductionInfo.Add(new InformationElement(new TextObject("{=!}Construction:").ToString(),
                    new TextObject("{=!}{POINTS} (Daily)")
                    .SetTextVariable("POINTS", villageData.Construction).ToString(),
                    new TextObject("{=Gm0F8o7L}How much the local population can progress with construction projects, on a daily basis.")
                        .ToString()));


                var model = new BKVillageProductionModel();
                var productionQuantity = 0f;
                var sb = new StringBuilder();
                foreach (var production in BannerKingsConfig.Instance.PopulationManager.GetProductions(data))
                {
                    sb.Append(production.Item1.Name + ", ");
                    productionQuantity += model.CalculateDailyProductionAmount(villageData.Village, production.Item1);
                }

                sb.Remove(sb.Length - 2, 1);
                var productionString = sb.ToString();
                ProductionInfo.Add(new InformationElement(new TextObject("{=!}Goods Production:").ToString(),
                    new TextObject("{=!}{POINTS} (Daily)")
                    .SetTextVariable("POINTS", productionQuantity)
                    .ToString(),
                    new TextObject("{=!}Sum of goods produced on a daily basis, including all the types produced here").ToString()));

                ProductionInfo.Add(new InformationElement(new TextObject("{=hmtRGrpt}Items Produced:").ToString(), productionString,
                    new TextObject("{=0RAPEDaT}Goods locally produced by the population.").ToString()));
            }
            else
            {
                ProductionInfo.Add(new InformationElement(new TextObject("{=GZooOyxK}Merchants' Revenue:").ToString(),
                   $"{data.EconomicData.MerchantRevenue:n0}",
                   new TextObject("{=rcApyg1K}Daily revenue of local merchants, based on slave workforce and production efficiency.")
                       .ToString()));

                RevenueInfo.Add(new InformationElement(new TextObject("{=Re0UyaL5}Tariff:").ToString(),
                    $"{data.EconomicData.Tariff:P}",
                    new TextObject("{=UgD3or79}Percentage of an item's value charged as tax when sold.").ToString()));

                var mercantilism = data.EconomicData.Mercantilism;
                RevenueInfo.Add(new InformationElement(new TextObject("Mercantilism:").ToString(),
                    $"{mercantilism.ResultNumber:P}",
                    new TextObject("{=ez3NzFgO}{TEXT}\n{EXPLANATIONS}")
                        .SetTextVariable("TEXT",
                            new TextObject("{=tOk3vpRY}Represents how economicaly free craftsmen, tradesmen and guilds are. Increased mercantilism reduces the tax revenue of these, but allows them to accumulate wealth or contribute more to overall prosperity."))
                        .SetTextVariable("EXPLANATIONS", mercantilism.GetExplanations())
                        .ToString()));

                var caravanAttractiveness = data.EconomicData.CaravanAttraction;
                RevenueInfo.Add(new InformationElement(new TextObject("{=O9p6A7yD}Caravan Attractiveness:").ToString(),
                    $"{caravanAttractiveness.ResultNumber:P}",
                    new TextObject("{=ez3NzFgO}{TEXT}\n{EXPLANATIONS}")
                        .SetTextVariable("TEXT",
                            new TextObject("{=GDiY2iFh}How attractive this town is for caravans. Likelihood of caravan visits are dictated mainly by prices, and attractiveness is a factor added on top of that."))
                        .SetTextVariable("EXPLANATIONS", caravanAttractiveness.GetExplanations())
                        .ToString()));


                for (var i = 0; i < 4; i++)
                {
                    var value = data.EconomicData.Satisfactions[i];
                    var type = (ConsumptionType) i;
                    var desc = type + " Goods:";
                    SatisfactionInfo.Add(new InformationElement(desc, FormatValue(value),
                        Utils.Helpers.GetConsumptionHint(type)));
                }

                criminalItem =
                    (BKCriminalPolicy) BannerKingsConfig.Instance.PolicyManager.GetPolicy(settlement, "criminal");
                CriminalSelector = GetSelector(criminalItem, criminalItem.OnChange);
                CriminalSelector.SelectedIndex = criminalItem.Selected;
                CriminalSelector.SetOnChangeAction(criminalItem.OnChange);

                var decisions = BannerKingsConfig.Instance.PolicyManager.GetDefaultDecisions(settlement);
                var slaveDecision = decisions.FirstOrDefault(x => x.GetIdentifier() == "decision_slaves_export");
                var tariffDecision = decisions.FirstOrDefault(x => x.GetIdentifier() == "decision_tariff_exempt");
                var slaveTaxDecision = decisions.FirstOrDefault(x => x.GetIdentifier() == "decision_slaves_tax");
                var mercantilismDecision = decisions.FirstOrDefault(x => x.GetIdentifier() == "decision_mercantilism");
                exportToogle = new DecisionElement()
                    .SetAsBooleanOption(slaveDecision.GetName(), slaveDecision.Enabled, delegate(bool value)
                    {
                        slaveDecision.OnChange(value);
                        RefreshValues();
                    }, new TextObject(slaveDecision.GetHint()));

                tariffToogle = new DecisionElement()
                    .SetAsBooleanOption(tariffDecision.GetName(), tariffDecision.Enabled, delegate(bool value)
                    {
                        tariffDecision.OnChange(value);
                        RefreshValues();
                    }, new TextObject(slaveDecision.GetHint()));

                slaveTaxToogle = new DecisionElement()
                    .SetAsBooleanOption(slaveTaxDecision.GetName(), slaveTaxDecision.Enabled, delegate(bool value)
                    {
                        slaveTaxDecision.OnChange(value);
                        RefreshValues();
                    }, new TextObject(slaveTaxDecision.GetHint()));

                mercantilismToogle = new DecisionElement()
                    .SetAsBooleanOption(mercantilismDecision.GetName(), mercantilismDecision.Enabled, delegate(bool value)
                    {
                        mercantilismDecision.OnChange(value);
                        RefreshValues();
                    }, new TextObject(mercantilismDecision.GetHint()));
            }

            var admCost = data.EconomicData.AdministrativeCost;
            RevenueInfo.Add(new InformationElement(new TextObject("{=MhzdyoWG}Administrative Cost:").ToString(), 
                $"{admCost.ResultNumber:P}",
                new TextObject("{=ez3NzFgO}{TEXT}\n{EXPLANATIONS}")
                    .SetTextVariable("TEXT",
                        new TextObject("{=ay7jnvEJ}Costs associated with the settlement administration, including those of active policies and decisions, deducted on tax revenue."))
                    .SetTextVariable("EXPLANATIONS", admCost.GetExplanations())
                    .ToString()));

            taxItem = (BKTaxPolicy) BannerKingsConfig.Instance.PolicyManager.GetPolicy(settlement, "tax");
            TaxSelector = GetSelector(taxItem, taxItem.OnChange);
            TaxSelector.SelectedIndex = taxItem.Selected;
            TaxSelector.SetOnChangeAction(taxItem.OnChange);
        }

        private void OnTournamentPress()
        {
            var tournament = new TournamentData(settlement.Town);
            data.TournamentData = tournament;
            roster = tournament.Roster;
            InventoryManager.OpenScreenAsStash(tournament.Roster);
            RefreshValues();
        }

        public override void OnFinalize()
        {
            base.OnFinalize();
            if (roster != null && !roster.IsEmpty())
            {
                var tournamentManager = Campaign.Current.TournamentManager;
                tournamentManager.AddTournament(Campaign.Current.Models.TournamentModel.CreateTournament(settlement.Town));
                Hero.MainHero.ChangeHeroGold(-5000);
                InformationManager.DisplayMessage(new InformationMessage(
                    $"Tournament started with prize: {data.TournamentData.Prize.Name}",
                    "event:/ui/notification/coins_negative"));
            }
        }
    }
}