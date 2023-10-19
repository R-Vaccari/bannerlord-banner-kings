using BannerKings.Managers.Titles;
using BannerKings.UI.Items;
using BannerKings.UI.Items.UI;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Selector;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.Titles
{
    public class DemesneHierarchyVM : BannerKingsViewModel
    {
        private ImageIdentifierVM banner;
        private DecisionElement contract;
        private DecisionElement foundKingdom;
        private MBBindingList<DecisionElement> decisions;
        private MBBindingList<InformationElement> titleInfo;
        private Kingdom kingdom;
        private string name, demesneText;
        private FeudalTitle title;
        private TitleElementVM tree;
        private BannerKingsSelectorVM<KingdomSelectorItem> selector;

        public DemesneHierarchyVM(FeudalTitle title, Kingdom kingdom) : base(null, false)
        {
            this.title = title;
            decisions = new MBBindingList<DecisionElement>();
            TitleInfo = new MBBindingList<InformationElement>();
            DemesneText = new TextObject("{=t8gCwGPJ}Demesne Information").ToString();

            Selector = new BannerKingsSelectorVM<KingdomSelectorItem>(true, 0, null);

            int selected = 0;
            int index = 0;
            foreach (Kingdom k in Kingdom.All)
            {
                var kingdomTitle = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(k);
                if (kingdomTitle == null)
                {
                    continue;
                }

                Selector.AddItem(new KingdomSelectorItem(k));
                if (k == Hero.MainHero.CurrentSettlement.MapFaction)
                {
                    selected = index;
                }

                index++;
            }

            Selector.SelectedIndex = selected;
            Selector.SetOnChangeAction(OnChange);
        }

        private void OnChange(SelectorVM<KingdomSelectorItem> obj)
        {
            if (obj.SelectedItem != null)
            {
                title = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(obj.SelectedItem.Kingdom);
                RefreshValues();
            }
        }

        public int Population { get; set; } = 0;

        [DataSourceProperty]
        public string DemesneText { get => demesneText; set => demesneText = value; } 

        [DataSourceProperty]
        public MBBindingList<InformationElement> TitleInfo { get => titleInfo; set => titleInfo = value; }

        public override void RefreshValues()
        {
            base.RefreshValues();
            Decisions.Clear();
            TitleInfo.Clear();

            Population = 0;
            if (title != null)
            {
                kingdom = BannerKingsConfig.Instance.TitleManager.GetTitleFaction(title);
                Tree = new TitleElementVM(title, this);
                if (kingdom != null)
                {
                    Banner = new ImageIdentifierVM(BannerCode.CreateFrom(kingdom.Banner), true);
                }
                else
                {
                    Banner = new ImageIdentifierVM(BannerCode.CreateFrom(title.deJure.Clan.Banner), true);
                }
                Name = title.FullName.ToString();
            }

            if (title?.Contract == null)
            {
                return;
            }

            var allSetup = kingdom != null && kingdom == BannerKingsConfig.Instance.TitleManager.GetTitleFaction(title);
            var contractButton = new DecisionElement().SetAsButtonOption(new TextObject("{=tpH62HBy}Contract").ToString(),
                () => BannerKingsConfig.Instance.TitleManager.ShowContract(kingdom.Leader,
                    GameTexts.FindText("str_done").ToString()),
                new TextObject("{=yRn9AcwU}Review this kingdom's contract, signed by lords that join it."));
            contractButton.Enabled = allSetup;

            Contract = contractButton;

            TitleInfo.Add(new InformationElement(new TextObject("Total Population:").ToString(),
                Population.ToString(), 
                new TextObject("{=g4pjb4j4}The total population within the fiefs in this hierarchy regardless of who controls them.").ToString()));

            var peerResult = BannerKingsConfig.Instance.InfluenceModel.GetMinimumPeersQuantity(kingdom, true);
            int peers = (int)peerResult.ResultNumber;
            TitleInfo.Add(new InformationElement(new TextObject("{=OD6eU7dQ}Minimum Peers:").ToString(),
                peers.ToString(), 
                new TextObject("{=H1pvLYrA}The minimum amount of full Peerage noble houses this realm requires. A minimum amount of Peers is required to maintain the power equilibrium in the realm, so that the ruler does not monopolize voting and fief rights.\n\n{EXPLANATION}")
                .SetTextVariable("EXPLANATION", peerResult.GetExplanations()).ToString()));

            TitleInfo.Add(new InformationElement(new TextObject("{=aoZYxUYV}Government Type:").ToString(),
                title.Contract.Government.Name.ToString(),
                new TextObject("{=!}{TEXT}\n\n{DESCRIPTION}")
                .SetTextVariable("TEXT", new TextObject("{=PUat0QQf}Government laws describe how different realms are organized. Different governments accept different types of policies and laws. Ie, republics do not accept policies such as Royal Guard, which highly favor a ruling dynasty. Moreover, government types define what kind of Succession a realm can practice."))
                .SetTextVariable("DESCRIPTION", title.Contract.Government.Description)
                .ToString()));

            TitleInfo.Add(new InformationElement(new TextObject("{=HJcuXO5J}Succession Type:").ToString(),
                title.Contract.Succession.Name.ToString().Replace("_", " "),
                new TextObject("{=!}{TEXT}\n\n{DESCRIPTION}")
                .SetTextVariable("TEXT", new TextObject("{=qMmbExKv}The clan succession form associated with this title. Successions only apply to factions."))
                .SetTextVariable("DESCRIPTION", title.Contract.Succession.Description)
                .ToString()));

            TitleInfo.Add(new InformationElement(new TextObject("{=OTuRSNZ5}Inheritance Type:").ToString(),
                title.Contract.Inheritance.Name.ToString(),
                new TextObject("{=!}{TEXT}\n\n{DESCRIPTION}")
                .SetTextVariable("TEXT", new TextObject("{=Y3mAnDLj}The inheritance form associated with this settlement's title. Inheritance dictates who leads the clan after the leader's death."))
                .SetTextVariable("DESCRIPTION", title.Contract.Inheritance.Description)
                .ToString()));

            TitleInfo.Add(new InformationElement(new TextObject("{=vCryQjBB}Gender Law:").ToString(),
                title.Contract.GenderLaw.Name.ToString(),
                new TextObject("{=!}{TEXT}\n\n{DESCRIPTION}")
                .SetTextVariable("TEXT", new TextObject("{=ArvZcS5p}The gender law associated with this settlement's title. Gender law affects how inheritance and other aspects of rule work."))
                .SetTextVariable("DESCRIPTION", title.Contract.GenderLaw.Description)
                .ToString()));
        }

        [DataSourceProperty]
        public BannerKingsSelectorVM<KingdomSelectorItem> Selector
        {
            get => selector;
            set
            {
                if (value != selector)
                {
                    selector = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public DecisionElement FoundKingdom
        {
            get => foundKingdom;
            set
            {
                if (value != foundKingdom)
                {
                    foundKingdom = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public DecisionElement Contract
        {
            get => contract;
            set
            {
                if (value != contract)
                {
                    contract = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<DecisionElement> Decisions
        {
            get => decisions;
            set
            {
                if (value != decisions)
                {
                    decisions = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public string Name
        {
            get => name;
            set
            {
                if (value != name)
                {
                    name = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public ImageIdentifierVM Banner
        {
            get => banner;
            set
            {
                if (value != banner)
                {
                    banner = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }

        [DataSourceProperty]
        public TitleElementVM Tree
        {
            get => tree;
            set
            {
                if (value != tree)
                {
                    tree = value;
                    OnPropertyChangedWithValue(value);
                }
            }
        }
    }
}