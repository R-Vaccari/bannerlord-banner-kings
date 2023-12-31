using BannerKings.Extensions;
using BannerKings.Managers.Populations;
using BannerKings.Managers.Populations.Estates;
using BannerKings.Managers.Titles.Laws;
using BannerKings.UI.Items;
using BannerKings.UI.Items.UI;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
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
        private BannerKingsSelectorVM<BKItemVM> taskSelector;
        private EstateAction grantAction, buyAction, reclaimAction;
        private HintViewModel buyHint, grantHint, reclaimHint;
        private bool playerOwned, buyVisible, grantVisible, reclaimVisible;
        private string nameText;

        public EstateVM(Estate estate, PopulationData data) : base(data, true)
        {
            Estate = estate;
            LandInfo = new MBBindingList<InformationElement>();
            WorkforceInfo = new MBBindingList<InformationElement>();
            StatsInfo = new MBBindingList<InformationElement>();
            MainInfo = new MBBindingList<TownManagementDescriptionItemVM>();
            ExtraInfos = new MBBindingList<MBBindingList<InformationElement>>();
            
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

            NameText = IsDisabled ? new TextObject("{=P8w8FYfp}Vacant Estate").ToString() : Estate.Name.ToString();
            if (!IsDisabled)
            {
                ImageIdentifier = new ImageIdentifierVM(new ImageIdentifier(CampaignUIHelper.GetCharacterCode(Estate.Owner.CharacterObject)));
            }

            MainInfo.Add(new TownManagementDescriptionItemVM(new TextObject("{=VRbXbsPE}Population:"), 
                Estate.Population, 
                0,
                TownManagementDescriptionItemVM.DescriptionType.Loyalty));

            var value = Estate.EstateValue;
            MainInfo.Add(new TownManagementDescriptionItemVM(new TextObject("{=mLtr8h47}Estate Value:"),
               (int)value.ResultNumber,
               0,
               TownManagementDescriptionItemVM.DescriptionType.Gold,
               new BasicTooltipViewModel(() => value.GetExplanations())));

            var acreage = Estate.AcreageGrowth;
            MainInfo.Add(new TownManagementDescriptionItemVM(new TextObject("{=FT5kL9k5}Acreage:"),
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


            if (IsEnabled)
            {
                LandInfo.Add(new InformationElement(new TextObject("{=56YOTTBC}Farmland:").ToString(),
                   new TextObject("{=xqot659p}{ACRES} acres").SetTextVariable("ACRES", Estate.Farmland.ToString("0.00")).ToString(),
                   new TextObject("{=ABrCGWep}Acres in this region used as farmland, the main source of food in most places")
                    .ToString()));

                LandInfo.Add(new InformationElement(new TextObject("{=RsRkc9dF}Pastureland:").ToString(),
                   new TextObject("{=xqot659p}{ACRES} acres").SetTextVariable("ACRES", Estate.Pastureland.ToString("0.00")).ToString(),
                   new TextObject("{=864UHkZw}Acres in this region used as pastureland, to raise cattle and other animals. These output meat and animal products such as butter and cheese")
                    .ToString()));

                LandInfo.Add(new InformationElement(new TextObject("{=bwEtOiYF}Woodland:").ToString(),
                   new TextObject("{=xqot659p}{ACRES} acres").SetTextVariable("ACRES", Estate.Woodland.ToString("0.00")).ToString(), 
                   new TextObject("{=MJYam3iu}Acres in this region used as woodland, kept for hunting, foraging of berries and materials like wood")
                    .ToString()));

                ExtraInfos.Add(LandInfo);

                WorkforceInfo.Add(new InformationElement(new TextObject("{=p7yrSOcC}Available Workforce:").ToString(),
                    Estate.AvailableWorkForce.ToString(),
                    new TextObject("{=1mJgkKHB}The amount of productive workers in this region, able to work the land").ToString()));

                WorkforceInfo.Add(new InformationElement(new TextObject("{=vaT0rnKq}Workforce Saturation:").ToString(),
                    FormatValue(Estate.WorkforceSaturation),
                    new TextObject("{=1KB6Hbpm}Represents how many workers there are in correlation to the amount needed to fully utilize the acreage. Saturation over 100% indicates more workers than the land needs, while under 100% means not all acres are producing output")
                    .ToString()));

                ExtraInfos.Add(WorkforceInfo);

                var tax = Estate.TaxRatio;
                StatsInfo.Add(new InformationElement(new TextObject("{=Kq3T4MBV}Tax Rate:").ToString(),
                    FormatValue(tax.ResultNumber),
                    tax.GetExplanations()));

                ExtraInfos.Add(StatsInfo);
            }

            RefreshActions();
        }

        private void RefreshActions()
        {
            buyAction = BannerKingsConfig.Instance.EstatesModel.GetBuy(Estate, Hero.MainHero);
            BuyVisible = !PlayerOwned;
            BuyHint = new HintViewModel(new TextObject("{=1kX621pV}Acquire this property as your own.\n\n{REASON}")
                .SetTextVariable("REASON", buyAction.Reason));

            grantAction = BannerKingsConfig.Instance.EstatesModel.GetGrant(Estate, Hero.MainHero, null);
            GrantVisible = PlayerOwned;
            GrantHint = new HintViewModel(new TextObject("{=FAn8ahnU}Grant this property to someone. To grant it, you must be it's legal and actual owner. Estates may be used to knight companions by talking to them, or gifted to other noble houses."));

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
                buyAction.TakeAction();
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
                    list.Add(new InquiryElement(action,
                        hero.Name.ToString(),
                        new ImageIdentifier(CampaignUIHelper.GetCharacterCode(hero.CharacterObject, true)),
                        action.Possible,
                        new TextObject("{=!}{POSSIBLE}{newline}Grant this property to {HERO}. They serve the {CLAN} clan ({OWNER}) and have {OPINION} opinion towards you.")
                        .SetTextVariable("POSSIBLE", action.Reason)
                        .SetTextVariable("HERO", hero.Name)
                        .SetTextVariable("CLAN", hero.Clan.Name)
                        .SetTextVariable("OWNER", hero.Clan == Clan.PlayerClan ? new TextObject("{=!}your clan") : hero.Clan.Leader.Name)
                        .SetTextVariable("OPINION", (int)hero.GetRelationWithPlayer())
                        .ToString()));
                }

                MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
                    new TextObject("{=3nTOToLe}Grant Estate").ToString(),
                    new TextObject("{=1bBJj789}Grant this estate to another person. By granting them ownership, they will owe the estate's income and access to manpower. Taxes may still be applied.").ToString(),
                    list,
                    true,
                    1,
                    1,
                    GameTexts.FindText("str_accept").ToString(),
                    string.Empty,
                    delegate (List<InquiryElement> list)
                    {
                        var action = (EstateAction)list[0].Identifier;
                        action.TakeAction();
                        RefreshValues();
                    },
                    null));
            } 
        }

        private void ExecuteReclaim()
        {
            if (reclaimAction.Possible)
            {
                reclaimAction.TakeAction();
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
        public string BuyText => new TextObject("{=WabTyEdr}Buy").ToString();

        [DataSourceProperty]
        public string GrantText => new TextObject("{=dugq4xHo}Grant").ToString();

        [DataSourceProperty]
        public string ReclaimText => new TextObject("{=RmEtkH3A}Reclaim").ToString();


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
