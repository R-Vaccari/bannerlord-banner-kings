using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BannerKings.Components;
using BannerKings.Managers.Policies;
using BannerKings.Managers.Populations;
using BannerKings.Managers.Recruits;
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
        public string MilitiaPolicyText => new TextObject("{=sYmEYKF8}Militia policy").ToString();

        [DataSourceProperty]
        public string GarrisonPolicyText => new TextObject("{=DEhtngoL}Garrison policy").ToString();

        [DataSourceProperty]
        public string DraftingPolicyText => new TextObject("{=T614zQR8}Drafting policy").ToString();

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
            DefenseInfo.Add(new InformationElement(new TextObject("{=UADFWZgq}Militia Cap:").ToString(), 
                $"{militiaCap.ResultNumber:n0}",
                new TextObject("{=ez3NzFgO}{TEXT}\n{EXPLANATIONS}")
                    .SetTextVariable("TEXT",
                        new TextObject("{=AyyuwpBd}The maximum number of militiamen this settlement can support, based on it's population."))
                    .SetTextVariable("EXPLANATIONS", militiaCap.GetExplanations())
                    .ToString()));

            var militiaQuality = new BKMilitiaModel().MilitiaSpawnChanceExplained(settlement);
            DefenseInfo.Add(new InformationElement(new TextObject("{=ROFzvP4W}Militia Quality:").ToString(), 
                $"{militiaQuality.ResultNumber:P}",
                new TextObject("{=ez3NzFgO}{TEXT}\n{EXPLANATIONS}")
                    .SetTextVariable("TEXT",
                        new TextObject("{=xQbPBzgn}Chance of militiamen being spawned as veterans instead of recruits."))
                    .SetTextVariable("EXPLANATIONS", militiaQuality.GetExplanations())
                    .ToString()));

            ManpowerInfo.Add(new InformationElement(new TextObject("{=t9sG2dMh}Manpower:").ToString(), 
                $"{data.MilitaryData.Manpower:n0}",
                new TextObject("{=MYdkfodC}The total manpower of nobles plus peasants.").ToString()));

            ManpowerInfo.Add(new InformationElement(new TextObject("{=pQ9cKQoK}Noble Manpower:").ToString(), 
                $"{data.MilitaryData.NobleManpower:n0}",
                new TextObject("{=08n0UTDS}Manpower from noble population. Noble militarism is higher, but nobles often are less numerous. These are drafted as noble recruits.")
                    .ToString()));

            ManpowerInfo.Add(new InformationElement(new TextObject("{=nkk8no8d}Peasant Manpower:").ToString(), 
                $"{data.MilitaryData.PeasantManpower:n0}",
                new TextObject("{=!}Manpower from every non-noble population class. Available classes are affected by kingdom demesne laws. Peasant manpower compromises the majority of military forces.")
                    .ToString()));

            List<RecruitSpawn> recruits = DefaultRecruitSpawns.Instance.GetPossibleSpawns(settlement.Culture, settlement);
            Dictionary<PopType, float> weights = new Dictionary<PopType, float>(5) 
            {
                { PopType.Serfs, 0f },
                { PopType.Tenants, 0f },
                { PopType.Craftsmen, 0f },
                { PopType.Nobles, 0f },
                { PopType.Slaves, 0f }
            };

            foreach (var spawn in recruits)
                foreach (var type in spawn.GetPossibleTypes())
                    weights[type] += spawn.GetChance(type);

            ManpowerInfo.Add(new InformationElement(new TextObject("{=!}Possible Recruits:").ToString(),
                recruits.Count.ToString(),
                recruits.Aggregate(new TextObject("{=!}These are the troops the notables may directly muster, not accounting for further trainning. The chance of each one is correlated to its population class' manpower in relation to the overall manpower.\n\n").ToString(), 
                (current, recruit) =>
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var type in recruit.GetPossibleTypes())
                        sb.Append(new TextObject("{=!}{newline}-- {TYPE}: {CHANCE}")
                            .SetTextVariable("TYPE", Utils.Helpers.GetClassName(type, recruit.Culture))
                            .SetTextVariable("CHANCE", FormatValue(BannerKingsConfig.Instance.VolunteerModel
                                .GetPopTypeSpawnChance(data, type) * (recruit.GetChance(type) / weights[type])))
                            .ToString());

                    return current + Environment.NewLine + Environment.NewLine + new TextObject("{=!}{TROOP}{LIST}")
                    .SetTextVariable("TROOP", recruit.Troop.Name)
                    .SetTextVariable("LIST", sb.ToString())
                    .ToString();
                })));

            ManpowerInfo.Add(new InformationElement(new TextObject("{=4gnA3tsw}Militarism:").ToString(), 
                $"{data.MilitaryData.Militarism.ResultNumber:P}",
                new TextObject("{=ez3NzFgO}{TEXT}\n{EXPLANATIONS}")
                    .SetTextVariable("TEXT",
                        new TextObject("{=MHFeNBXS}How much the population is willing or able to militarily serve. Militarism increases the manpower caps."))
                    .SetTextVariable("EXPLANATIONS", data.MilitaryData.Militarism.GetExplanations())
                    .ToString()));

            ExplainedNumber draftEfficiency = data.MilitaryData.DraftEfficiency;
            ManpowerInfo.Add(new InformationElement(new TextObject("{=AJMjhhVL}Draft Efficiency:").ToString(),
                FormatValue(draftEfficiency.ResultNumber),
                new TextObject("{=ez3NzFgO}{TEXT}\n{EXPLANATIONS}")
                    .SetTextVariable("TEXT",
                        new TextObject("{=g5NdEeBX}How quickly volunteer availability in notables replenishes."))
                    .SetTextVariable("EXPLANATIONS", draftEfficiency.GetExplanations())
                    .ToString()));

            var decisions = BannerKingsConfig.Instance.PolicyManager.GetDefaultDecisions(settlement);
            if (HasTown)
            {
                SiegeInfo.Add(new InformationElement(new TextObject("{=qbHVtQgS}Storage Limit:").ToString(), settlement.Town.FoodStocksUpperLimit().ToString(),
                    new TextObject("{=scsKRhWJ}The amount of food this settlement is capable of storing.").ToString()));

                SiegeInfo.Add(new InformationElement(new TextObject("{=GX46BKVV}Estimated Holdout:").ToString(),
                    $"{data.MilitaryData.Holdout} Days",
                    new TextObject("{=3gN048NJ}How long this settlement will take to start starving in case of a siege.").ToString()));

                var sb = new StringBuilder();
                sb.Append(data.MilitaryData.Ballistae);
                sb.Append(", ");
                sb.Append(data.MilitaryData.Catapultae);
                sb.Append(", ");
                sb.Append(data.MilitaryData.Trebuchets);
                sb.Append(" (Ballis., Catap., Treb.)");
                SiegeInfo.Add(new InformationElement(new TextObject("{=WNz6O64F}Engines:").ToString(), sb.ToString(),
                    new TextObject("{=FqCCSBHu}Pre-built siege engines to defend the walls, in case of siege.").ToString()));

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
                RaiseMilitiaButton = new DecisionElement().SetAsButtonOption(new TextObject("{=mGfT9o4X}Raise militia").ToString(), 
                    delegate
                {
                    var serfs = data.GetTypeCount(PopType.Serfs);
                    var party = settlement.MilitiaPartyComponent.MobileParty;
                    var lord = settlement.OwnerClan.Leader;
                    if (serfs >= party.MemberRoster.TotalManCount)
                    {
                        var existingParty = TaleWorlds.CampaignSystem.Campaign.Current.CampaignObjectManager.Find<MobileParty>(x => x.StringId == "raisedmilitia_" + settlement);
                        if (existingParty == null)
                        {
                            if (party.CurrentSettlement == null || party.CurrentSettlement != settlement)
                            {
                                return;
                            }

                            var menCount = party.MemberRoster.TotalManCount;
                            MilitiaComponent.CreateMilitiaEscort(settlement, Hero.MainHero.PartyBelongedTo, party);
                            if (lord == Hero.MainHero)
                            {
                                InformationManager.DisplayMessage(new InformationMessage(new TextObject("{=b5S7tfOo}{COUNT} men raised at {SETTLEMENT}")
                                    .SetTextVariable("COUNT", menCount)
                                    .SetTextVariable("SETTLEMENT", settlement.Name).ToString()));
                            }
                        }
                        else if (lord == Hero.MainHero)
                        {
                            InformationManager.DisplayMessage(new InformationMessage(new TextObject("{=V8KAUwe9}Militia already raised from {SETTLEMENT}")
                                .SetTextVariable("SETTLEMENT", settlement.Name).ToString()));
                        }
                    }
                    else if (lord == Hero.MainHero)
                    {
                        InformationManager.DisplayMessage(new InformationMessage($"Not enough men available to raise militia at {settlement.Name}"));
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