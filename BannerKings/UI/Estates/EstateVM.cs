using BannerKings.Extensions;
using BannerKings.Managers.Populations;
using BannerKings.Managers.Populations.Estates;
using BannerKings.Managers.Titles.Laws;
using BannerKings.UI.Items;
using BannerKings.UI.Items.UI;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu.TownManagement;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Core.ViewModelCollection.Selector;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using static BannerKings.Managers.Populations.Estates.Estate;

namespace BannerKings.UI.Estates
{
    internal class EstateVM : BannerKingsViewModel
    {
        private MBBindingList<TownManagementDescriptionItemVM> mainInfo;
        private MBBindingList<MBBindingList<InformationElement>> extraInfos;
        private ImageIdentifierVM imageIdentifier;
        private BannerKingsSelectorVM<BKItemVM> taskSelector, dutySelector;
        private EstateAction grantAction, buyAction, reclaimAction;
        private HintViewModel buyHint, grantHint, reclaimHint;
        private bool playerOwned, dutyEnabled, buyVisible, grantVisible, reclaimVisible;
        private string nameText;

        public EstateVM(Estate estate, PopulationData data) : base(data, true)
        {
            Estate = estate;
            LandInfo = new MBBindingList<InformationElement>();
            WorkforceInfo = new MBBindingList<InformationElement>();
            StatsInfo = new MBBindingList<InformationElement>();
            MainInfo = new MBBindingList<TownManagementDescriptionItemVM>();
            ExtraInfos = new MBBindingList<MBBindingList<InformationElement>>();
            
            DutyEnabled = false;
            PlayerOwned = false;

            RefreshValues();
        }

        [DataSourceProperty]
        public bool IsDisabled => Estate.IsDisabled;

        [DataSourceProperty]
        public bool IsEnabled => !Estate.IsDisabled;


        public Estate Estate { get; private set; }

        public override void RefreshValues()
        {
            base.RefreshValues();
            LandInfo.Clear();
            WorkforceInfo.Clear();
            StatsInfo.Clear();
            MainInfo.Clear();
            ExtraInfos.Clear();

            NameText = IsDisabled ? new TextObject("{=!}Vacant Estate").ToString() : Estate.Name.ToString();
            if (!IsDisabled)
            {
                ImageIdentifier = new ImageIdentifierVM(new ImageIdentifier(CampaignUIHelper.GetCharacterCode(Estate.Owner.CharacterObject)));
            }

            MainInfo.Add(new TownManagementDescriptionItemVM(new TextObject("{=!}Population:"), 
                Estate.Population, 
                0,
                TownManagementDescriptionItemVM.DescriptionType.Loyalty));

            var value = Estate.EstateValue;
            MainInfo.Add(new TownManagementDescriptionItemVM(new TextObject("{=!}Accumulated Tax:"),
               (int)value.ResultNumber,
               0,
               TownManagementDescriptionItemVM.DescriptionType.Gold,
               new BasicTooltipViewModel(() => value.GetExplanations())));

            var serfManpower = Estate.GetManpower(Managers.PopulationManager.PopType.Serfs);
            var slavesManpower = Estate.GetManpower(Managers.PopulationManager.PopType.Serfs);
            MainInfo.Add(new TownManagementDescriptionItemVM(new TextObject("{=!}Manpower:"),
               serfManpower + slavesManpower,
               0,
               TownManagementDescriptionItemVM.DescriptionType.Militia,
               new BasicTooltipViewModel(() => new TextObject("{=!}This estate's manpower, drawn from it's population. This manpower is used to fuel the estate owner's volunteers. When volunteers are recruited from an estate-owner, the manpower is drawn from the estate rather than the overall settlement.")
               .ToString())));


            var acreage = Estate.AcreageGrowth;
            MainInfo.Add(new TownManagementDescriptionItemVM(new TextObject("{=!}Acreage:"),
               (int)Estate.Acreage,
               (int)acreage.ResultNumber,
               TownManagementDescriptionItemVM.DescriptionType.Prosperity,
               new BasicTooltipViewModel(() => acreage.GetExplanations())));



            PlayerOwned = Estate.Owner == Hero.MainHero && !IsDisabled;

            TaskSelector = new BannerKingsSelectorVM<BKItemVM>(PlayerOwned, 0, OnTaskChange);
            TaskSelector.AddItem(new BKItemVM(EstateTask.Prodution, true, "",
                GameTexts.FindText("str_bk_estate_task", EstateTask.Prodution.ToString())));

            TaskSelector.AddItem(new BKItemVM(EstateTask.Land_Expansion, true, "",
                GameTexts.FindText("str_bk_estate_task", EstateTask.Land_Expansion.ToString())));

            TaskSelector.SelectedIndex = (int)Estate.Task;
            TaskSelector.SetOnChangeAction(OnTaskChange);

            var settlement = Estate.EstatesData.Settlement;
            var title = BannerKingsConfig.Instance.TitleManager.GetTitle(settlement);
            if (title != null)
            {
                var owner = settlement.IsVillage ? settlement.Village.GetActualOwner() : settlement.Owner;
                DutyEnabled = title.contract.IsLawEnacted(DefaultDemesneLaws.Instance.EstateTenureFeeTail) && owner == Hero.MainHero;
            }
            else
            {
                DutyEnabled = false;
            }
            

            DutySelector = new BannerKingsSelectorVM<BKItemVM>(DutyEnabled, 0, OnDutyChange);
            DutySelector.AddItem(new BKItemVM(EstateDuty.Taxation, true, "",
                GameTexts.FindText("str_bk_estate_duty", EstateDuty.Taxation.ToString())));

            DutySelector.AddItem(new BKItemVM(EstateDuty.Military, true, "",
                GameTexts.FindText("str_bk_estate_duty", EstateDuty.Military.ToString())));

            DutySelector.SelectedIndex = (int)Estate.Duty;
            DutySelector.SetOnChangeAction(OnDutyChange);


            if (IsEnabled)
            {
                
                LandInfo.Add(new InformationElement(new TextObject("{=!}Farmland:").ToString(),
                   new TextObject("{=!}{ACRES} acres").SetTextVariable("ACRES", Estate.Farmland.ToString("0.00")).ToString(),
                   new TextObject("{=ABrCGWep}Acres in this region used as farmland, the main source of food in most places")
                    .ToString()));

                LandInfo.Add(new InformationElement(new TextObject("{=RsRkc9dF}Pastureland:").ToString(),
                   new TextObject("{=!}{ACRES} acres").SetTextVariable("ACRES", Estate.Pastureland.ToString("0.00")).ToString(),
                   new TextObject("{=864UHkZw}Acres in this region used as pastureland, to raise cattle and other animals. These output meat and animal products such as butter and cheese")
                    .ToString()));

                LandInfo.Add(new InformationElement(new TextObject("{=bwEtOiYF}Woodland:").ToString(),
                   new TextObject("{=!}{ACRES} acres").SetTextVariable("ACRES", Estate.Woodland.ToString("0.00")).ToString(), 
                   new TextObject("{=MJYam3iu}Acres in this region used as woodland, kept for hunting, foraging of berries and materials like wood")
                    .ToString()));

                ExtraInfos.Add(LandInfo);

                var production = Estate.Production;
                WorkforceInfo.Add(new InformationElement(new TextObject("{=!}Goods Production:").ToString(),
                    new TextObject("{=!}{POINTS} (Daily)")
                    .SetTextVariable("POINTS", production.ResultNumber.ToString("0.00"))
                    .ToString(),
                    new TextObject("{=ez3NzFgO}{TEXT}\n{EXPLANATIONS}")
                        .SetTextVariable("TEXT",
                            new TextObject("{=!}Sum of goods produced on a daily basis, including all the types produced here."))
                        .SetTextVariable("EXPLANATIONS", production.GetExplanations())
                        .ToString()));

                WorkforceInfo.Add(new InformationElement(new TextObject("{=p7yrSOcC}Available Workforce:").ToString(),
                    Estate.AvailableWorkForce.ToString(),
                    new TextObject("{=1mJgkKHB}The amount of productive workers in this region, able to work the land").ToString()));

                WorkforceInfo.Add(new InformationElement(new TextObject("{=vaT0rnKq}Workforce Saturation:").ToString(),
                    FormatValue(Estate.WorkforceSaturation),
                    new TextObject("{=1KB6Hbpm}Represents how many workers there are in correlation to the amount needed to fully utilize the acreage. Saturation over 100% indicates more workers than the land needs, while under 100% means not all acres are producing output")
                    .ToString()));

                ExtraInfos.Add(WorkforceInfo);


                StatsInfo.Add(new InformationElement(GameTexts.FindText("str_total_influence").ToString(),
                    FormatFloatGain(Estate.Influence),
                    new TextObject("{=!}Influence from local nobles.").ToString()));

                var tax = Estate.TaxRatio;
                StatsInfo.Add(new InformationElement(new TextObject("{=!}Tax Rate:").ToString(),
                    FormatValue(tax.ResultNumber),
                    tax.GetExplanations()));

                
                StatsInfo.Add(new InformationElement(new TextObject("{=!}Tax Accumulated:").ToString(),
                    Estate.TaxAccumulated.ToString("0"),
                    new TextObject("{=!}The accumulated profits since villagers last brought production revenues back. This is zeroed once the estate owner collects their income.").ToString()));


                ExtraInfos.Add(StatsInfo);
            }

            RefreshActions();
        }

        private void RefreshActions()
        {
            buyAction = BannerKingsConfig.Instance.EstatesModel.GetBuy(Estate, Hero.MainHero);
            BuyVisible = !PlayerOwned;
            BuyHint = new HintViewModel(new TextObject("{=!}Acquire this property as your own.\n\n{REASON}")
                .SetTextVariable("REASON", buyAction.Reason));

            grantAction = BannerKingsConfig.Instance.EstatesModel.GetGrant(Estate, Hero.MainHero, null);
            GrantVisible = PlayerOwned;
            GrantHint = new HintViewModel(new TextObject("{=!}Grant this property to someone. To grant it, you must be it's legal and actual owner. Estates may be used to knight companions by talking to them, or gifted to other noble houses."));

            reclaimAction = BannerKingsConfig.Instance.EstatesModel.GetReclaim(Estate, Hero.MainHero);
            var settlement = Estate.EstatesData.Settlement;
            var title = BannerKingsConfig.Instance.TitleManager.GetTitle(settlement);
            ReclaimVisible = Estate.Owner != null && Hero.MainHero == title.deJure && settlement.MapFaction == Hero.MainHero.MapFaction &&
                Estate.Owner.MapFaction != Hero.MainHero.MapFaction;

        }

        private void ExecuteBuy()
        {
            if (buyAction.Possible)
            {
                var seller = Estate.Owner;
                if (seller == null)
                {
                    seller = BannerKingsConfig.Instance.TitleManager.GetTitle(Estate.EstatesData.Settlement).deJure;
                }

                GiveGoldAction.ApplyBetweenCharacters(Hero.MainHero, seller, (int)Estate.EstateValue.ResultNumber);
                Estate.SetOwner(Hero.MainHero);
                RefreshValues();
            } 
        }

        private void ExecuteGrant()
        {
            var kingdom = Clan.PlayerClan.Kingdom;
            if (kingdom != null)
            {
                var list = new List<InquiryElement>();
                foreach (var hero in BannerKingsConfig.Instance.EstatesModel.GetGrantCandidates(grantAction))
                {
                    var action = BannerKingsConfig.Instance.EstatesModel.GetGrant(Estate, Hero.MainHero, hero);
                    list.Add(new InquiryElement(hero,
                        hero.Name.ToString(),
                        new ImageIdentifier(CampaignUIHelper.GetCharacterCode(hero.CharacterObject, true)),
                        action.Possible,
                        action.Reason.ToString()));
                }

                MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                    new TextObject("{=!}Grant Estate").ToString(),
                    new TextObject("{=!}Grant this estate to another person. By granting them ownership, they will owe the estate's income and access to manpower. Taxes may still be applied.").ToString(),
                    list,
                    true,
                    1,
                    GameTexts.FindText("str_accept").ToString(),
                    string.Empty,
                    delegate (List<InquiryElement> list)
                    {
                        var hero = (Hero)list[0].Identifier;
                        Estate.SetOwner(hero);
                        ChangeRelationAction.ApplyPlayerRelation(hero,
                            BannerKingsConfig.Instance.EstatesModel.CalculateEstateGrantRelation(Estate, Hero.MainHero));
                        RefreshValues();
                    },
                    null));
            } 
        }

        private void ExecuteReclaim()
        {
            if (reclaimAction.Possible)
            {
                ChangeRelationAction.ApplyPlayerRelation(Estate.Owner,
                            -BannerKingsConfig.Instance.EstatesModel.CalculateEstateGrantRelation(Estate, Hero.MainHero));
                Estate.SetOwner(Hero.MainHero);
                RefreshValues();
            }
        }

        private void OnTaskChange(SelectorVM<BKItemVM> obj)
        {
            if (obj.SelectedItem != null)
            {
                var vm = obj.GetCurrentItem();
                Estate.ChangeTask((EstateTask)vm.Value);
            }
        }

        private void OnDutyChange(SelectorVM<BKItemVM> obj)
        {
            if (obj.SelectedItem != null)
            {
                var vm = obj.GetCurrentItem();
                Estate.ChangeDuty((EstateDuty)vm.Value);
            }
        }

        public MBBindingList<InformationElement> LandInfo { get; set; }
        public MBBindingList<InformationElement> WorkforceInfo { get; set; }
        public MBBindingList<InformationElement> StatsInfo { get; set; }

        [DataSourceProperty]
        public string NameText 
        {
            get => nameText;
            set
            {
                if (value != nameText)
                {
                    nameText = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }
     

        [DataSourceProperty]
        public string BuyText => new TextObject("{=!}Buy").ToString();

        [DataSourceProperty]
        public string GrantText => new TextObject("{=dugq4xHo}Grant").ToString();


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

        [DataSourceProperty]
        public BannerKingsSelectorVM<BKItemVM> DutySelector
        {
            get => dutySelector;
            set
            {
                if (value != dutySelector)
                {
                    dutySelector = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public bool PlayerOwned
        {
            get => playerOwned;
            set
            {
                if (value != playerOwned)
                {
                    playerOwned = value;
                    OnPropertyChanged("PlayerOwned");
                }
            }
        }


        [DataSourceProperty]
        public bool DutyEnabled
        {
            get => dutyEnabled;
            set
            {
                if (value != dutyEnabled)
                {
                    dutyEnabled = value;
                    OnPropertyChanged("DutyEnabled");
                }
            }
        }

        [DataSourceProperty]
        public bool BuyVisible
        {
            get => buyVisible;
            set
            {
                if (value != buyVisible)
                {
                    buyVisible = value;
                    OnPropertyChanged("BuyVisible");
                }
            }
        }

        [DataSourceProperty]
        public bool GrantVisible
        {
            get => grantVisible;
            set
            {
                if (value != grantVisible)
                {
                    grantVisible = value;
                    OnPropertyChanged("GrantVisible");
                }
            }
        }

        [DataSourceProperty]
        public bool ReclaimVisible
        {
            get => reclaimVisible;
            set
            {
                if (value != reclaimVisible)
                {
                    reclaimVisible = value;
                    OnPropertyChanged("ReclaimVisible");
                }
            }
        }


        [DataSourceProperty]
        public HintViewModel GrantHint
        {
            get => grantHint;
            set
            {
                if (value != grantHint)
                {
                    grantHint = value;
                    OnPropertyChanged("GrantHint");
                }
            }
        }

        [DataSourceProperty]
        public HintViewModel BuyHint
        {
            get => buyHint;
            set
            {
                if (value != buyHint)
                {
                    buyHint = value;
                    OnPropertyChanged("BuyHint");
                }
            }
        }

        [DataSourceProperty]
        public HintViewModel ReclaimHint
        {
            get => reclaimHint;
            set
            {
                if (value != reclaimHint)
                {
                    reclaimHint = value;
                    OnPropertyChanged("ReclaimHint");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<MBBindingList<InformationElement>> ExtraInfos
        {
            get => extraInfos;
            set
            {
                if (value != extraInfos)
                {
                    extraInfos = value;
                    OnPropertyChanged("ExtraInfos");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<TownManagementDescriptionItemVM> MainInfo
        {
            get => mainInfo;
            set
            {
                if (value != mainInfo)
                {
                    mainInfo = value;
                    OnPropertyChanged("MainInfo");
                }
            }
        }

        [DataSourceProperty]
        public ImageIdentifierVM ImageIdentifier
        {
            get => imageIdentifier;
            set
            {
                imageIdentifier = value;
                OnPropertyChanged("ImageIdentifier");
            }
        }
    }
}
