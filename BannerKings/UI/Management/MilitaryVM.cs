using System.Linq;
using System.Text;
using BannerKings.Components;
using BannerKings.Managers.Policies;
using BannerKings.Managers.Populations;
using BannerKings.Models.Vanilla;
using BannerKings.UI.Items;
using BannerKings.UI.Items.UI;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core.ViewModelCollection.Selector;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using static BannerKings.Managers.PopulationManager;

namespace BannerKings.UI.Management
{
    public class MilitaryVM : BannerKingsViewModel
    {
        private DecisionElement conscriptionToogle, subsidizeToogle, rationToogle;
        private MBBindingList<InformationElement> defenseInfo;
        private BKDraftPolicy draftItem;
        private BKGarrisonPolicy garrisonItem;
        private MBBindingList<InformationElement> manpowerInfo;
        private BKMilitiaPolicy militiaItem;
        private SelectorVM<BKItemVM> militiaSelector, garrisonSelector, draftSelector;
        private DecisionElement raiseMilitiaButton;
        private readonly Settlement settlement;
        private MBBindingList<InformationElement> siegeInfo;

        public MilitaryVM(PopulationData data, Settlement _settlement, bool selected) : base(data, selected)
        {
            defenseInfo = new MBBindingList<InformationElement>();
            manpowerInfo = new MBBindingList<InformationElement>();
            siegeInfo = new MBBindingList<InformationElement>();
            settlement = _settlement;
            RefreshValues();
        }

        [DataSourceProperty]
        public DecisionElement RaiseMilitiaButton
        {
            get => raiseMilitiaButton;
            set
            {
                if (value != raiseMilitiaButton)
                {
                    raiseMilitiaButton = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public SelectorVM<BKItemVM> DraftSelector
        {
            get => draftSelector;
            set
            {
                if (value != draftSelector)
                {
                    draftSelector = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public SelectorVM<BKItemVM> GarrisonSelector
        {
            get => garrisonSelector;
            set
            {
                if (value != garrisonSelector)
                {
                    garrisonSelector = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public SelectorVM<BKItemVM> MilitiaSelector
        {
            get => militiaSelector;
            set
            {
                if (value != militiaSelector)
                {
                    militiaSelector = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public DecisionElement SubsidizeToogle
        {
            get => subsidizeToogle;
            set
            {
                if (value != subsidizeToogle)
                {
                    subsidizeToogle = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public DecisionElement RationToogle
        {
            get => rationToogle;
            set
            {
                if (value != rationToogle)
                {
                    rationToogle = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public DecisionElement ConscriptionToogle
        {
            get => conscriptionToogle;
            set
            {
                if (value != conscriptionToogle)
                {
                    conscriptionToogle = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }


        [DataSourceProperty]
        public MBBindingList<InformationElement> DefenseInfo
        {
            get => defenseInfo;
            set
            {
                if (value != defenseInfo)
                {
                    defenseInfo = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<InformationElement> ManpowerInfo
        {
            get => manpowerInfo;
            set
            {
                if (value != manpowerInfo)
                {
                    manpowerInfo = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<InformationElement> SiegeInfo
        {
            get => siegeInfo;
            set
            {
                if (value != siegeInfo)
                {
                    siegeInfo = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        public override void RefreshValues()
        {
            base.RefreshValues();

            DefenseInfo.Clear();
            ManpowerInfo.Clear();
            SiegeInfo.Clear();

            var militiaCap = new BKMilitiaModel().GetMilitiaLimit(data, settlement);
            DefenseInfo.Add(new InformationElement("Militia Cap:", militiaCap.ResultNumber.ToString(),
                new TextObject("{=ez3NzFgO}{TEXT}\n{EXPLANATIONS}")
                    .SetTextVariable("TEXT",
                        new TextObject(
                            "{=AyyuwpBd}The maximum number of militiamen this settlement can support, based on it's population."))
                    .SetTextVariable("EXPLANATIONS", militiaCap.GetExplanations())
                    .ToString()));

            var militiaQuality = new BKMilitiaModel().MilitiaSpawnChanceExplained(settlement);
            DefenseInfo.Add(new InformationElement("Militia Quality:", FormatValue(militiaQuality.ResultNumber),
                new TextObject("{=ez3NzFgO}{TEXT}\n{EXPLANATIONS}")
                    .SetTextVariable("TEXT",
                        new TextObject("{=xQbPBzgn}Chance of militiamen being spawned as veterans instead of recruits."))
                    .SetTextVariable("EXPLANATIONS", militiaQuality.GetExplanations())
                    .ToString()));

            ManpowerInfo.Add(new InformationElement("Manpower:", data.MilitaryData.Manpower.ToString(),
                new TextObject("{=MYdkfodC}The total manpower of nobles plus peasants.").ToString()));
            ManpowerInfo.Add(new InformationElement("Noble Manpower:", data.MilitaryData.NobleManpower.ToString(),
                new TextObject(
                        "{=08n0UTDS}Manpower from noble population. Noble militarism is higher, but nobles often are less numerous. These are drafted as noble recruits.")
                    .ToString()));
            ManpowerInfo.Add(new InformationElement("Peasant Manpower:", data.MilitaryData.PeasantManpower.ToString(),
                new TextObject(
                        "{=uaEXD3tE}Manpower from serf and craftsmen classes. These are drafted as cultural non-noble recruits.")
                    .ToString()));
            ManpowerInfo.Add(new InformationElement("Militarism:", FormatValue(data.MilitaryData.Militarism.ResultNumber),
                new TextObject("{=ez3NzFgO}{TEXT}\n{EXPLANATIONS}")
                    .SetTextVariable("TEXT",
                        new TextObject(
                            "{=MHFeNBXS}How much the population is willing or able to militarily serve. Militarism increases the manpower caps."))
                    .SetTextVariable("EXPLANATIONS", data.MilitaryData.Militarism.GetExplanations())
                    .ToString()));
            ManpowerInfo.Add(new InformationElement("Draft Efficiency:",
                FormatValue(data.MilitaryData.DraftEfficiency.ResultNumber),
                new TextObject("{=ez3NzFgO}{TEXT}\n{EXPLANATIONS}")
                    .SetTextVariable("TEXT",
                        new TextObject("{=g5NdEeBX}How quickly volunteer availability in notables replenishes."))
                    .SetTextVariable("EXPLANATIONS", data.MilitaryData.DraftEfficiency.GetExplanations())
                    .ToString()));

            var decisions = BannerKingsConfig.Instance.PolicyManager.GetDefaultDecisions(settlement);
            if (HasTown)
            {
                SiegeInfo.Add(new InformationElement("Storage Limit:", settlement.Town.FoodStocksUpperLimit().ToString(),
                    "The amount of food this settlement is capable of storing."));
                SiegeInfo.Add(new InformationElement("Estimated Holdout:",
                    $"{data.MilitaryData.Holdout} Days",
                    "How long this settlement will take to start starving in case of a siege."));

                var sb = new StringBuilder();
                sb.Append(data.MilitaryData.Ballistae);
                sb.Append(", ");
                sb.Append(data.MilitaryData.Catapultae);
                sb.Append(", ");
                sb.Append(data.MilitaryData.Trebuchets);
                sb.Append(" (Ballis., Catap., Treb.)");
                SiegeInfo.Add(new InformationElement("Engines:", sb.ToString(),
                    "Pre-built siege engines to defend the walls, in case of siege."));

                garrisonItem =
                    (BKGarrisonPolicy) BannerKingsConfig.Instance.PolicyManager.GetPolicy(settlement, "garrison");
                GarrisonSelector = GetSelector(garrisonItem, garrisonItem.OnChange);
                GarrisonSelector.SelectedIndex = garrisonItem.Selected;
                GarrisonSelector.SetOnChangeAction(garrisonItem.OnChange);


                var rationDecision = decisions.FirstOrDefault(x => x.GetIdentifier() == "decision_ration");

                rationToogle = new DecisionElement()
                    .SetAsBooleanOption(rationDecision.GetName(), rationDecision.Enabled, delegate(bool value)
                    {
                        rationDecision.OnChange(value);
                        RefreshValues();
                    }, new TextObject(rationDecision.GetHint()));
            }
            else
            {
                RaiseMilitiaButton = new DecisionElement().SetAsButtonOption("Raise militia", delegate
                {
                    var serfs = data.GetTypeCount(PopType.Serfs);
                    var party = settlement.MilitiaPartyComponent.MobileParty;
                    var lord = settlement.OwnerClan.Leader;
                    if (serfs >= party.MemberRoster.TotalManCount)
                    {
                        var existingParty =
                            Campaign.Current.CampaignObjectManager.Find<MobileParty>(x =>
                                x.StringId == "raisedmilitia_" + settlement);
                        if (existingParty == null)
                        {
                            if (party.CurrentSettlement != null && party.CurrentSettlement == settlement)
                            {
                                var menCount = party.MemberRoster.TotalManCount;
                                MilitiaComponent.CreateMilitiaEscort(settlement, Hero.MainHero.PartyBelongedTo, party);
                                if (lord == Hero.MainHero)
                                {
                                    InformationManager.DisplayMessage(new InformationMessage(
                                        $"{menCount} men raised as militia at {settlement.Name}!"));
                                }
                            }
                        }
                        else if (lord == Hero.MainHero)
                        {
                            InformationManager.DisplayMessage(
                                new InformationMessage($"Militia already raised from {settlement.Name}"));
                        }
                    }
                    else if (lord == Hero.MainHero)
                    {
                        InformationManager.DisplayMessage(new InformationMessage(
                            $"Not enough men available to raise militia at {settlement.Name}"));
                    }
                }, new TextObject("Raise the current militia of this village."));
            }

            militiaItem = (BKMilitiaPolicy) BannerKingsConfig.Instance.PolicyManager.GetPolicy(settlement, "militia");
            MilitiaSelector = GetSelector(militiaItem, militiaItem.OnChange);
            MilitiaSelector.SelectedIndex = militiaItem.Selected;
            MilitiaSelector.SetOnChangeAction(militiaItem.OnChange);

            draftItem = (BKDraftPolicy) BannerKingsConfig.Instance.PolicyManager.GetPolicy(settlement, "draft");
            DraftSelector = GetSelector(draftItem, draftItem.OnChange);
            DraftSelector.SelectedIndex = draftItem.Selected;
            DraftSelector.SetOnChangeAction(delegate(SelectorVM<BKItemVM> obj)
            {
                draftItem.OnChange(obj);
                RefreshValues();
            });

            var conscriptionDecision = decisions.FirstOrDefault(x => x.GetIdentifier() == "decision_militia_encourage");
            var subsidizeDecision = decisions.FirstOrDefault(x => x.GetIdentifier() == "decision_militia_subsidize");

            conscriptionToogle = new DecisionElement()
                .SetAsBooleanOption(conscriptionDecision.GetName(), conscriptionDecision.Enabled, delegate(bool value)
                {
                    conscriptionDecision.OnChange(value);
                    RefreshValues();
                }, new TextObject(conscriptionDecision.GetHint()));

            subsidizeToogle = new DecisionElement()
                .SetAsBooleanOption(subsidizeDecision.GetName(), subsidizeDecision.Enabled, delegate(bool value)
                {
                    subsidizeDecision.OnChange(value);
                    RefreshValues();
                }, new TextObject(subsidizeDecision.GetHint()));
        }
    }
}