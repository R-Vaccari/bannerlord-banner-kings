using BannerKings.Components;
using BannerKings.Models;
using BannerKings.Populations;
using BannerKings.UI.Items;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using static BannerKings.Managers.PolicyManager;
using static BannerKings.Managers.PopulationManager;

namespace BannerKings
{
    namespace UI
    {
        public class PopulationVillageVM : ViewModel
        {
            private MBBindingList<PopulationInfoVM> _popInfo;
            private DecisionElement _popAccelerateToogle;
            private DecisionElement _selfInvestToogle;
            private DecisionElement _subsidizeMilitiaToogle;
            private DecisionElement _raiseMilitiaButton;
            private Settlement settlement;
            private PopulationData data;

            public PopulationVillageVM(Settlement settlement)
            {
                this.settlement = settlement;
                _popInfo = new MBBindingList<PopulationInfoVM>();
            }

            public override void RefreshValues()
            {
                base.RefreshValues();
                PopInfo.Clear();
                PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(Settlement.CurrentSettlement);
                this.data = data;
                if (data != null && data.Classes != null)
                {
                    data.Classes.ForEach(popClass => PopInfo.Add(new PopulationInfoVM(
                        Helpers.Helpers.GetClassName(popClass.type, settlement.Culture).ToString(), popClass.count, 
                        Helpers.Helpers.GetClassHint(popClass.type, settlement.Culture))
                        ));

                    RaiseMilitiaButton = new DecisionElement().SetAsButtonOption("Raise militia", delegate
                    {
                        int serfs = data.GetTypeCount(PopType.Serfs);
                        MobileParty party = settlement.MilitiaPartyComponent.MobileParty;
                        Hero lord = settlement.OwnerClan.Leader;
                        if (serfs >= party.MemberRoster.TotalManCount)
                        {
                            int cost = InfluenceCost;
                            if (cost > -1 && lord.Clan.Influence >= cost)
                            {
                                MobileParty existingParty = Campaign.Current.CampaignObjectManager.Find<MobileParty>(x => x.StringId == "raisedmilitia_" + settlement);
                                if (existingParty == null)
                                {
                                    if (party.CurrentSettlement != null && party.CurrentSettlement == settlement)
                                    {
                                        int menCount = party.MemberRoster.TotalManCount;
                                        MilitiaComponent.CreateMilitiaEscort("raisedmilitia_", settlement, settlement, "Raised Militia from {0}", Hero.MainHero.PartyBelongedTo, party);
                                        if (lord == Hero.MainHero)
                                            InformationManager.DisplayMessage(new InformationMessage(string.Format("{0} men raised as militia at {1}!", menCount, settlement.Name)));
                                        lord.Clan.Influence -= cost;
                                    }
                                } else if (lord == Hero.MainHero)
                                    InformationManager.DisplayMessage(new InformationMessage(string.Format("Militia already raised from {0}", settlement.Name)));
                            }
                            else if (lord == Hero.MainHero)
                                InformationManager.DisplayMessage(new InformationMessage(string.Format("Not enough influence to raise militia at {0}", settlement.Name)));
                        } else if (lord == Hero.MainHero)
                            InformationManager.DisplayMessage(new InformationMessage(string.Format("Not enough men available to raise militia at {0}", settlement.Name)));

                    }, new TextObject("Raise the current militia of this village."));

   
                }
            }

  

            public int InfluenceCost
            {
                get
                {
                    MobileParty party = settlement.MilitiaPartyComponent.MobileParty;
                    Hero lord = settlement.OwnerClan.Leader;
                    if (party != null && lord != null && lord.PartyBelongedTo != null)
                        return new BKInfluenceModel().GetMilitiaInfluenceCost(party, settlement, lord);
                    else return -1;
                }
            }

            [DataSourceProperty]
            public string InfluenceCostText
            {
                get => string.Format("Cost: {0} influence", InfluenceCost);
            }

            [DataSourceProperty]
            public string PopGrowth
            {
                get
                {
                    int growth = (int)new BKGrowthModel().CalculateEffect(settlement, data).ResultNumber;
                    return growth.ToString() + " (Daily)";
                }
            }


            [DataSourceProperty]
            public string Assimilation
            {
                get
                {
                    float result = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement).CultureData.GetAssimilation(Hero.MainHero.Culture);
                    return (result * 100f).ToString() + '%';
                }
            }

    

            [DataSourceProperty]
            public string AdministrativeCost
            {
                get
                {
                    float cost = new AdministrativeModel().CalculateAdministrativeCost(settlement);
                    return (cost * 100f).ToString() + '%' ;
                }
            }



            [DataSourceProperty]
            public MBBindingList<PopulationInfoVM> PopInfo
            {
                get => _popInfo;
                set
                {
                    if (value != _popInfo)
                    {
                        _popInfo = value;
                        base.OnPropertyChangedWithValue(value, "PopInfo");
                    }
                }
            }
            

            [DataSourceProperty]
            public DecisionElement RaiseMilitiaButton
            {
                get => _raiseMilitiaButton;
                set
                {
                    if (value != _raiseMilitiaButton)
                    {
                        _raiseMilitiaButton = value;
                        base.OnPropertyChangedWithValue(value, "RaiseMilitiaButton");
                    }
                }
            }

            [DataSourceProperty]
            public DecisionElement AccelerateToogle
            {
                get => _popAccelerateToogle;
                set
                {
                    if (value != _popAccelerateToogle)
                    {
                        _popAccelerateToogle = value;
                        base.OnPropertyChangedWithValue(value, "AccelerateToogle");
                    }
                }
            }

            [DataSourceProperty]
            public DecisionElement InvestToogle
            {
                get => _selfInvestToogle;
                set
                {
                    if (value != _selfInvestToogle)
                    {
                        _selfInvestToogle = value;
                        base.OnPropertyChangedWithValue(value, "InvestToogle");
                    }
                }
            }
            [DataSourceProperty]
            public DecisionElement SubsidizeToogle
            {
                get => _subsidizeMilitiaToogle;
                set
                {
                    if (value != _subsidizeMilitiaToogle)
                    {
                        _subsidizeMilitiaToogle = value;
                        base.OnPropertyChangedWithValue(value, "SubsidizeToogle");
                    }
                }
            }

            public void ExecuteClose()
            {
                InformationManager.DisplayMessage(new InformationMessage(String
                    .Format("Policies updated for {0}", settlement.Name.ToString())));
                UIManager.Instance.CloseUI();
            }
        }
    } 
}
