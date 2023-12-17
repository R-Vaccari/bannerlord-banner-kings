using System.Collections.Generic;
using System.Linq;
using System.Text;
using BannerKings.Managers.Policies;
using BannerKings.Managers.Populations;
using BannerKings.Managers.Shipping;
using BannerKings.UI.Items;
using BannerKings.UI.Items.UI;
using TaleWorlds.CampaignSystem;

using TaleWorlds.CampaignSystem.Party;
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
        private MBBindingList<InformationElement> productionInfo, revenueInfo, satisfactionInfo,
            slaveryInfo, lanesList;
        private TournamentData tournamentData;
        private bool hasLanes;
      
        private BKTaxPolicy taxItem;
        private SelectorVM<BKItemVM> taxSelector, criminalSelector;

        public EconomyVM(PopulationData data, Settlement _settlement, bool selected) : base(data, selected)
        {
            lanesList = new MBBindingList<InformationElement>();
            productionInfo = new MBBindingList<InformationElement>();
            revenueInfo = new MBBindingList<InformationElement>();
            satisfactionInfo = new MBBindingList<InformationElement>();
            slaveryInfo = new MBBindingList<InformationElement>();
            RefreshValues();
        }

        private Settlement Settlement => data.Settlement;

        [DataSourceProperty]
        public string ProductionText => new TextObject("{=bk_production}Production").ToString();

        [DataSourceProperty]
        public string SatisfactionsText => new TextObject("{=E7ZOdG5n}Satisfactions").ToString();

        [DataSourceProperty]
        public string SlaveryText => new TextObject("{=bk_slavery}Slavery").ToString();

        [DataSourceProperty]
        public string LanesText => new TextObject("{=!}Shipping Lanes").ToString();

        [DataSourceProperty]
        public string TaxPolicyText => new TextObject("{=L7QhNa6a}Tax policy").ToString();

        [DataSourceProperty]
        public string CriminalPolicyText => new TextObject("{=qyjqPWxJ}Criminal policy").ToString();

        [DataSourceProperty]
        public HintViewModel TournamentHint => new(new TextObject("{=GqmH24N4}Sponsor a tournament in this town. As the main sponsor, you have to pay 5000 coins for the tournament costs, as well as provide an adequate prize. Sponsoring games improves population loyalty towards you, and valuable prizes provide renown to your name."));

        public override void RefreshValues()
        {
            base.RefreshValues();
            ProductionInfo.Clear();
            RevenueInfo.Clear();
            SatisfactionInfo.Clear();
            SlaveryInfo.Clear();
            LanesList.Clear();

            foreach (var lane in DefaultShippingLanes.Instance.GetSettlementLanes(Settlement))
            {
                LanesList.Add(new InformationElement(lane.Name,
                    new TextObject("{=!}{COUNT} ports")
                    .SetTextVariable("COUNT", lane.Ports.Count),
                    lane.Description));
                HasLanes = true;
            }

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
                var villageRevenue = BannerKingsConfig.Instance.TaxModel.CalculateVillageTaxFromIncome(villageData.Village, true);
                RevenueInfo.Add(new InformationElement(new TextObject("{=BXFLXR6B}Village Revenue:").ToString(),
                    FormatFloatGain(villageRevenue.ResultNumber),
                    new TextObject("{=ez3NzFgO}{TEXT}\n{EXPLANATIONS}")
                        .SetTextVariable("TEXT",
                            new TextObject("{=L3KACGcQ}The village's revenue output. Most of the revenue in villages is generated through production and selling of products by serfs and slaves. They are taxed through their labor rather than in coin. Nobles and craftsmen however may be taxed in coins through construction of tax offices."))
                        .SetTextVariable("EXPLANATIONS", villageRevenue.GetExplanations())
                        .ToString()));

                RevenueInfo.Add(new InformationElement(new TextObject("{=krVcSaH5}Last Payment:").ToString(),
                    FormatFloatGain(villageData.LastPayment),
                    new TextObject("{=oGNtP5yt}The last payment this it's owner village has done.").ToString()));

                ProductionInfo.Add(new InformationElement(new TextObject("{=KbTvcQko}Construction:").ToString(),
                    new TextObject("{=mbUwoU0h}{POINTS} (Daily)")
                    .SetTextVariable("POINTS", villageData.Construction.ToString("0.00")).ToString(),
                    new TextObject("{=Gm0F8o7L}How much the local population can progress with construction projects, on a daily basis.")
                        .ToString()));

                var sb = new StringBuilder();
                foreach (var production in BannerKingsConfig.Instance.PopulationManager.GetProductions(data))
                {
                    sb.Append(production.Item1.Name + ", ");
                }

                sb.Remove(sb.Length - 2, 1);
                var productionString = sb.ToString();
                var productionExplained = villageData.ProductionsExplained;
                ProductionInfo.Add(new InformationElement(new TextObject("{=Fin3KXMP}Goods Production:").ToString(),
                    new TextObject("{=mbUwoU0h}{POINTS} (Daily)")
                    .SetTextVariable("POINTS", productionExplained.ResultNumber.ToString("0.00"))
                    .ToString(),
                    new TextObject("{=ez3NzFgO}{TEXT}\n{EXPLANATIONS}")
                    .SetTextVariable("TEXT",
                        new TextObject("{=g480uUyC}Sum of goods produced on a daily basis, including all the types produced here."))
                    .SetTextVariable("EXPLANATIONS", productionExplained.GetExplanations())
                    .ToString()));

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
                RevenueInfo.Add(new InformationElement(new TextObject("{=5E2NZBtK}Mercantilism:").ToString(),
                    $"{mercantilism.ResultNumber:P}",
                    new TextObject("{=ez3NzFgO}{TEXT}\n{EXPLANATIONS}")
                        .SetTextVariable("TEXT",
                            new TextObject("{=tOk3vpRY}Represents how economicaly free craftsmen, tradesmen and guilds are. Increased mercantilism reduces the tax revenue of these, but allows them to accumulate wealth or contribute more to overall prosperity."))
                        .SetTextVariable("EXPLANATIONS", mercantilism.GetExplanations())
                        .ToString()));

                var caravanAttractiveness = BannerKingsConfig.Instance.EconomyModel.CalculateCaravanAttraction(Settlement, true);
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
                    var type = (ConsumptionType)i;
                    var desc = type + " Goods:";
                    SatisfactionInfo.Add(new InformationElement(Utils.TextHelper.GetConsumptionSatisfactionText((ConsumptionType)i)
                        .ToString(),
                        FormatValue(value),
                        Utils.Helpers.GetConsumptionHint(type)));
                }

                criminalItem =
                    (BKCriminalPolicy)BannerKingsConfig.Instance.PolicyManager.GetPolicy(Settlement, "criminal");
                CriminalSelector = GetSelector(criminalItem, criminalItem.OnChange);
                CriminalSelector.SelectedIndex = criminalItem.Selected;
                CriminalSelector.SetOnChangeAction(criminalItem.OnChange);

                var decisions = BannerKingsConfig.Instance.PolicyManager.GetDefaultDecisions(Settlement);
                var slaveDecision = decisions.FirstOrDefault(x => x.GetIdentifier() == "decision_slaves_export");
                var tariffDecision = decisions.FirstOrDefault(x => x.GetIdentifier() == "decision_tariff_exempt");
                var slaveTaxDecision = decisions.FirstOrDefault(x => x.GetIdentifier() == "decision_slaves_tax");
                var mercantilismDecision = decisions.FirstOrDefault(x => x.GetIdentifier() == "decision_mercantilism");
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
                    }, new TextObject(tariffDecision.GetHint()));

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

            var slavePrice = BannerKingsConfig.Instance.GrowthModel.CalculateSlavePrice(Settlement, true);
            SlaveryInfo.Add(new InformationElement(new TextObject("{=CTerwC4b}Slave Price:").ToString(),
                ((int)slavePrice.ResultNumber).ToString(),
                slavePrice.GetExplanations()));

            var slaveDemand = BannerKingsConfig.Instance.GrowthModel.CalculatePopulationClassDemand(Settlement, PopType.Slaves, true);
            SlaveryInfo.Add(new InformationElement(new TextObject("{=KdEiqFm2}Slave Demand:").ToString(),
                FormatValue(slaveDemand.ResultNumber),
                slaveDemand.GetExplanations()));

            var admCost = data.EconomicData.AdministrativeCost;
            RevenueInfo.Add(new InformationElement(new TextObject("{=MhzdyoWG}Administrative Cost:").ToString(),
                $"{admCost.ResultNumber:P}",
                new TextObject("{=ez3NzFgO}{TEXT}\n{EXPLANATIONS}")
                    .SetTextVariable("TEXT",
                        new TextObject("{=ay7jnvEJ}Costs associated with the settlement administration, including those of active policies and decisions, deducted on tax revenue."))
                    .SetTextVariable("EXPLANATIONS", admCost.GetExplanations())
                    .ToString()));

            if (Settlement.Town != null)
            {
                var taxes = BannerKingsConfig.Instance.TaxModel.CalculateTownTax(Settlement.Town, true);
                RevenueInfo.Add(new InformationElement(new TextObject("{=E61zQNSt}Tax Revenues:").ToString(),
                    MBRandom.RoundRandomized(taxes.ResultNumber).ToString(),
                    new TextObject("{=ez3NzFgO}{TEXT}\n{EXPLANATIONS}")
                        .SetTextVariable("TEXT",
                            new TextObject("{=yZhrtORs}Taxes levied on local population, and other local expenses and revenues. To maximize your revenues, increase local stability and reduce administrative costs."))
                        .SetTextVariable("EXPLANATIONS", taxes.GetExplanations())
                        .ToString()));
            }

            taxItem = (BKTaxPolicy)BannerKingsConfig.Instance.PolicyManager.GetPolicy(Settlement, "tax");
            TaxSelector = GetSelector(taxItem, taxItem.OnChange);
            TaxSelector.SelectedIndex = taxItem.Selected;
            TaxSelector.SetOnChangeAction(taxItem.OnChange);
        }

        private void OnTournamentPress()
        {
            var tournament = new TournamentData(Settlement.Town);
            data.TournamentData = tournament;

            var list = new List<InquiryElement>();
            foreach (var element in MobileParty.MainParty.ItemRoster)
            {
                var item = element.EquipmentElement.Item;
                if (item.HasWeaponComponent || item.HasArmorComponent || (item.HasHorseComponent && !item.HorseComponent.IsLiveStock))
                {
                    if (item.Value > 100)
                    {
                        list.Add(new InquiryElement(item,
                                                element.EquipmentElement.GetModifiedItemName().ToString(),
                                                new ImageIdentifier(item),
                                                true,
                                                ""));
                    }
                }
            }

            MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                GameTexts.FindText("str_tournament")
                .SetTextVariable("SETTLEMENT_NAME", Settlement.Name)
                .ToString(),
                new TextObject("{=JdH9ubwj}Select a prize for your tournament. The bigger is it's value, the more renown will be awarded to once the tournament is finished.").ToString(),
                list,
                true,
                1,
                1,
                GameTexts.FindText("str_accept").ToString(),
                GameTexts.FindText("str_reject").ToString(),
                delegate (List<InquiryElement> list)
                {
                    ItemObject item = (ItemObject)list[0].Identifier;
                    tournament.SetPrize(item);
                    tournamentData = tournament;
                    RefreshValues();
                },
                null));
        }

        public override void OnFinalize()
        {
            base.OnFinalize();
            if (tournamentData != null)
            {
                tournamentData.Start(Settlement.Town);
            }
        }

        [DataSourceProperty]
        public bool HasLanes
        {
            get => hasLanes;
            set
            {
                if (value != hasLanes)
                {
                    hasLanes = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public bool TournamentAvailable
        {
            get
            {
                if (Settlement.IsTown)
                {
                    return !Settlement.Town.HasTournament && Hero.MainHero.Gold >= 5000;
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
        public MBBindingList<InformationElement> LanesList
        {
            get => lanesList;
            set
            {
                if (value != lanesList)
                {
                    lanesList = value;
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
        public MBBindingList<InformationElement> SlaveryInfo
        {
            get => slaveryInfo;
            set
            {
                if (value != slaveryInfo)
                {
                    slaveryInfo = value;
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
    }
}