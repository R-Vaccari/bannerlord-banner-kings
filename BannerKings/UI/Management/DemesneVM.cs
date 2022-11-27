using BannerKings.Managers.Kingdoms.Contract;
using BannerKings.Managers.Policies;
using BannerKings.Managers.Populations;
using BannerKings.Managers.Titles;
using BannerKings.Models.BKModels;
using BannerKings.UI.Items;
using BannerKings.UI.Items.UI;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Selector;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.Management
{
    public class DemesneVM : BannerKingsViewModel
    {
        private HeroVM deJure;
        private MBBindingList<InformationElement> demesneInfo, landInfo, mineralInfo, terrainInfo, workforceInfo, governmentInfo;
        private readonly FeudalTitle duchy;
        private readonly FeudalTitle title;
        private BKWorkforcePolicy workforceItem;
        private SelectorVM<BKItemVM> workforceVM;

        public DemesneVM(PopulationData data, FeudalTitle title, bool isSelected) : base(data, isSelected)
        {
            this.title = title;
            if (title != null)
            {
                deJure = new HeroVM(title.deJure);
                duchy = BannerKingsConfig.Instance.TitleManager.GetDuchy(this.title);
            }

            mineralInfo = new MBBindingList<InformationElement>();
            demesneInfo = new MBBindingList<InformationElement>();
            governmentInfo = new MBBindingList<InformationElement>();
            landInfo = new MBBindingList<InformationElement>();
            terrainInfo = new MBBindingList<InformationElement>();
            workforceInfo = new MBBindingList<InformationElement>();

            /*
            
            this.duchyCosts = model.GetUsurpationCosts(_duchy, Hero.MainHero);
            
            this._usurpDuchyEnabled = this._duchy.deJure != Hero.MainHero;
            if (title.vassals != null)
                foreach (FeudalTitle vassal in title.vassals)
                    if (vassal.fief != null) _vassals.Add(new VassalTitleVM(vassal)); */
        }

        [DataSourceProperty]
        public string WorkforcePolicyText => new TextObject("Workforce policy").ToString();

        [DataSourceProperty]
        public string TerrainText => new TextObject("{=BZacZ2Cj}Terrain").ToString();

        [DataSourceProperty]
        public string MiningText => new TextObject("{=LObjpLQY}Mining").ToString();

        [DataSourceProperty]
        public SelectorVM<BKItemVM> WorkforceSelector
        {
            get => workforceVM;
            set
            {
                if (value != workforceVM)
                {
                    workforceVM = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<InformationElement> LandInfo
        {
            get => landInfo;
            set
            {
                if (value != landInfo)
                {
                    landInfo = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<InformationElement> TerrainInfo
        {
            get => terrainInfo;
            set
            {
                if (value != terrainInfo)
                {
                    terrainInfo = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<InformationElement> WorkforceInfo
        {
            get => workforceInfo;
            set
            {
                if (value != workforceInfo)
                {
                    workforceInfo = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<InformationElement> MineralInfo
        {
            get => mineralInfo;
            set
            {
                if (value != mineralInfo)
                {
                    mineralInfo = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<InformationElement> GovernmentInfo
        {
            get => governmentInfo;
            set
            {
                if (value != governmentInfo)
                {
                    governmentInfo = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<InformationElement> DemesneInfo
        {
            get => demesneInfo;
            set
            {
                if (value != demesneInfo)
                {
                    demesneInfo = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }


        [DataSourceProperty]
        public HeroVM DeJure
        {
            get => deJure;
            set
            {
                if (value != deJure)
                {
                    deJure = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            var landData = data.LandData;
            DemesneInfo.Clear();
            LandInfo.Clear();
            TerrainInfo.Clear();
            WorkforceInfo.Clear();
            GovernmentInfo.Clear();
            MineralInfo.Clear();

            if (title != null)
            {
                var legitimacyType = new BKLegitimacyModel().CalculateEffect(data.Settlement).ResultNumber;
                if (legitimacyType > 0f)
                {
                    var legitimacy = (LegitimacyType) legitimacyType;
                    DemesneInfo.Add(new InformationElement(new TextObject("{=UqLsS4GV}Legitimacy:").ToString(),
                        legitimacy.ToString().Replace('_', ' '),
                        new TextObject("{=2GV3HnQ4}Your legitimacy to this title and it's vassals. You are lawful when you own this title, and considered a foreigner if your culture differs from it.")
                            .ToString()));
                }

                if (title.sovereign != null)
                {
                    DemesneInfo.Add(new InformationElement(new TextObject("{=qXe7jFLM}Sovereign:").ToString(),
                        title.sovereign.FullName.ToString(),
                        new TextObject("{=phHrT2JN}The master suzerain of this title, be they a king or emperor type suzerain.")
                            .ToString()));
                }

                if (duchy != null)
                {
                    DemesneInfo.Add(new InformationElement(new TextObject("{=9EWirk3d}Dukedom:").ToString(), duchy.FullName.ToString(),
                        new TextObject("{=BvJb2QSM}The dukedom this settlement is associated with.").ToString()));
                }

                var currentDemesne = BannerKingsConfig.Instance.StabilityModel.CalculateCurrentDemesne(Hero.MainHero.Clan);
                var demesneCap = BannerKingsConfig.Instance.StabilityModel.CalculateDemesneLimit(Hero.MainHero);

                GovernmentInfo.Add(new InformationElement(new TextObject("{=02REd9mG}Demesne limit:").ToString(),
                    $"{currentDemesne.ResultNumber:n2}/{demesneCap.ResultNumber:n2}",
                    new TextObject("{=dhr9NJoA}{TEXT}\nCurrent demesne:\n{CURRENT}\n \nLimit:\n{LIMIT}")
                        .SetTextVariable("TEXT",
                            new TextObject("{=oHJ6Y66V}Demesne limit describes how many settlements you may own without negative implications. Different settlement types have different weights, villages being the lowest, towns being the highest. Being over the limit reduces stability across all your settlements. Owning a settlement's title will reduce it's weight."))
                        .SetTextVariable("CURRENT", currentDemesne.GetExplanations())
                        .SetTextVariable("LIMIT", demesneCap.GetExplanations())
                        .ToString()));

                var currentUnlandedDemesne = BannerKingsConfig.Instance.StabilityModel.CalculateCurrentUnlandedDemesne(Hero.MainHero.Clan);
                var unlandedDemesneCap = BannerKingsConfig.Instance.StabilityModel.CalculateUnlandedDemesneLimit(Hero.MainHero);

                GovernmentInfo.Add(new InformationElement(new TextObject("{=8J3DQsNE}Unlanded Demesne limit:").ToString(),
                    $"{currentUnlandedDemesne.ResultNumber:n2}/{unlandedDemesneCap.ResultNumber:n2}",
                    new TextObject("{=dhr9NJoA}{TEXT}\nCurrent demesne:\n{CURRENT}\n \nLimit:\n{LIMIT}")
                        .SetTextVariable("TEXT",
                            new TextObject(
                                "{=XAvCOvv4}Unlanded demesne limit describes how many unlanded titles you may own. Unlanded titles are titles such as dukedoms and kingdoms - titles not directly associated with a settlement. Dukedoms have the lowest weight while empires have the biggest. Being over the limit progressively reduces relations with your vassals."))
                        .SetTextVariable("CURRENT", currentUnlandedDemesne.GetExplanations())
                        .SetTextVariable("LIMIT", unlandedDemesneCap.GetExplanations())
                        .ToString()));

                var currentVassals = BannerKingsConfig.Instance.StabilityModel.CalculateCurrentVassals(Hero.MainHero.Clan);
                var vassalsCap = BannerKingsConfig.Instance.StabilityModel.CalculateVassalLimit(Hero.MainHero);

                GovernmentInfo.Add(new InformationElement(new TextObject("{=dB5y6tTY}Vassal limit:").ToString(),
                    $"{currentVassals.ResultNumber:n2}/{vassalsCap.ResultNumber:n2}",
                    new TextObject("{=Q50amDu9}{TEXT}\nCurrent vassals:\n{CURRENT}\n \nLimit:\n{LIMIT}")
                        .SetTextVariable("TEXT",
                            new TextObject("{=nhBf1JY5}Vassal limit is how many vassals you may have without negative consequences. Vassals are clans whose highest title are under your own (ie, a barony title under your county title, or knight clans with a single lordship) or knights in your clan. Knights only weight 0.5 towards the limit, while clan leaders weight 1. Companions and family members do not count. Being over the limit progressively reduces your influence gain."))
                        .SetTextVariable("CURRENT", currentVassals.GetExplanations())
                        .SetTextVariable("LIMIT", vassalsCap.GetExplanations())
                        .ToString()));

                GovernmentInfo.Add(new InformationElement(new TextObject("Government Type:").ToString(),
                    title.contract.Government.ToString(),
                    new TextObject("{=BvJb2QSM}The dukedom this settlement is associated with.").ToString()));
                GovernmentInfo.Add(new InformationElement(new TextObject("{=HJcuXO5J}Succession Type:").ToString(),
                    title.contract.Succession.ToString().Replace("_", " "),
                    new TextObject("{=qMmbExKv}The clan succession form associated with this title. Successions only apply to factions.")
                        .ToString()));
                GovernmentInfo.Add(new InformationElement(new TextObject("{=OTuRSNZ5}Inheritance Type:").ToString(),
                    title.contract.Inheritance.ToString(),
                    new TextObject("{=Y3mAnDLj}The inheritance form associated with this settlement's title. Inheritance dictates who leads the clan after the leader's death.")
                        .ToString()));
                GovernmentInfo.Add(new InformationElement(new TextObject("{=vCryQjBB}Gender Law:").ToString(),
                    title.contract.GenderLaw.ToString(),
                    new TextObject("{=ArvZcS5p}The gender law associated with this settlement's title. Gender law affects how inheritance and other aspects of rule work.")
                        .ToString()));

                DeJure = new HeroVM(title.deJure);
            }


            LandInfo.Add(new InformationElement(new TextObject("{=FT5kL9k5}Acreage:").ToString(), landData.Acreage + " acres",
                new TextObject("{=thVdn5fm}Current quantity of usable acres in this region").ToString()));
            LandInfo.Add(new InformationElement(new TextObject("{=56YOTTBC}Farmland:").ToString(), landData.Farmland + " acres",
                new TextObject("{=ABrCGWep}Acres in this region used as farmland, the main source of food in most places")
                    .ToString()));
            LandInfo.Add(new InformationElement(new TextObject("{=RsRkc9dF}Pastureland:").ToString(), landData.Pastureland + " acres",
                new TextObject("{=864UHkZw}Acres in this region used as pastureland, to raise cattle and other animals. These output meat and animal products such as butter and cheese")
                    .ToString()));
            LandInfo.Add(new InformationElement(new TextObject("{=bwEtOiYF}Woodland:").ToString(), landData.Woodland + " acres",
                new TextObject("{=MJYam3iu}Acres in this region used as woodland, kept for hunting, foraging of berries and materials like wood")
                    .ToString()));

            TerrainInfo.Add(new InformationElement(new TextObject("{=zRUcs9ct}Type:").ToString(), landData.Terrain.ToString(),
                new TextObject("{=EPerAMda}The local terrain type. Dictates fertility and terrain difficulty.").ToString()));
            TerrainInfo.Add(new InformationElement(new TextObject("{=n5kVRwat}Fertility:").ToString(), FormatValue(landData.Fertility),
                new TextObject("{=UMZTmCeE}How fertile the region is. This depends solely on the local terrain type - harsher environments like deserts are less fertile than plains and grassy hills")
                    .ToString()));
            TerrainInfo.Add(new InformationElement(new TextObject("{=XKe1Q6Db}Terrain Difficulty:").ToString(),
                FormatValue(landData.Difficulty),
                new TextObject("{=TVp8DsE9}Represents how difficult it is to create new usable acres. Like fertility, depends on terrain, but is not strictly correlated to it")
                    .ToString()));


            if (data.MineralData != null)
            {
                MineralInfo.Add(new InformationElement(new TextObject("{=iEGG5vQ9}Mineral Richness:").ToString(),
                    GameTexts.FindText("str_bk_mineral_richness", data.MineralData.Richness.ToString().ToLower()).ToString(),
                    new TextObject("{=Koax8hLJ}How rich and accessible the land is for mineral extraction. Adequate land will yield returns but at reduced rate. Poor land is hardly worth the exploration effort. Rich lands may be quite profitable.").ToString()));

                foreach (var item in data.MineralData.Compositions)
                {
                    MineralInfo.Add(new InformationElement(GameTexts.FindText("str_bk_mineral", item.Key.ToString().ToLower()).ToString(), 
                        FormatValue(item.Value), 
                        ""));
                }
            }


            WorkforceInfo.Add(new InformationElement(new TextObject("{=p7yrSOcC}Available Workforce:").ToString(),
                landData.AvailableWorkForce.ToString(),
                new TextObject("{=1mJgkKHB}The amount of productive workers in this region, able to work the land").ToString()));
            WorkforceInfo.Add(new InformationElement(new TextObject("{=vaT0rnKq}Workforce Saturation:").ToString(),
                FormatValue(landData.WorkforceSaturation),
                new TextObject("{=1KB6Hbpm}Represents how many workers there are in correlation to the amount needed to fully utilize the acreage. Saturation over 100% indicates more workers than the land needs, while under 100% means not all acres are producing output")
                    .ToString()));

            if (HasTown)
            {
                workforceItem = (BKWorkforcePolicy) BannerKingsConfig.Instance.PolicyManager.GetPolicy(data.Settlement, "workforce");
                WorkforceSelector = GetSelector(workforceItem, workforceItem.OnChange);
                WorkforceSelector.SelectedIndex = workforceItem.Selected;
                WorkforceSelector.SetOnChangeAction(workforceItem.OnChange);
            }
        }


        private void OnUsurpPress()
        {
            if (title != null)
            {
                var kingdom = data.Settlement.OwnerClan.Kingdom;
                kingdom?.AddDecision(new BKGovernmentDecision(data.Settlement.OwnerClan, GovernmentType.Imperial, title));
            }
        }
    }
}