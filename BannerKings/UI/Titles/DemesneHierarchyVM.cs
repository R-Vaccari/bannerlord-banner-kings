using BannerKings.Managers.Titles;
using BannerKings.UI.Items;
using BannerKings.UI.Items.UI;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
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
        private readonly Kingdom kingdom;
        private string name, demesneText;
        private readonly FeudalTitle title;
        private TitleElementVM tree;

        public DemesneHierarchyVM(FeudalTitle title, Kingdom kingdom) : base(null, false)
        {
            this.title = title;
            if (kingdom == null)
            {
                kingdom = BannerKingsConfig.Instance.TitleManager.GetTitleFaction(title);
            }
            this.kingdom = kingdom;
            decisions = new MBBindingList<DecisionElement>();
            if (title != null)
            {
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

            TitleInfo = new MBBindingList<InformationElement>();
            DemesneText = new TextObject("{=t8gCwGPJ}Demesne Information").ToString();
        }

        public int Population { get; set; } = 0;

        [DataSourceProperty]
        public string DemesneText { get => demesneText; set => demesneText = value; } 

        [DataSourceProperty]
        public MBBindingList<InformationElement> TitleInfo { get => titleInfo; set => titleInfo = value; }

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

        public override void RefreshValues()
        {
            base.RefreshValues();
            Decisions.Clear();
            TitleInfo.Clear();

            if (title?.contract == null)
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

            TitleInfo.Add(new InformationElement(new TextObject("{=bLbvfBnb}Total Population").ToString(),
                Population.ToString(), string.Empty));

            TitleInfo.Add(new InformationElement(new TextObject("{=!}Government Type:").ToString(),
             title.contract.Government.ToString(),
             new TextObject("{=BvJb2QSM}The dukedom this settlement is associated with.").ToString()));
            TitleInfo.Add(new InformationElement(new TextObject("{=HJcuXO5J}Succession Type:").ToString(),
                title.contract.Succession.ToString().Replace("_", " "),
                new TextObject("{=qMmbExKv}The clan succession form associated with this title. Successions only apply to factions.")
                    .ToString()));
            TitleInfo.Add(new InformationElement(new TextObject("{=OTuRSNZ5}Inheritance Type:").ToString(),
                title.contract.Inheritance.ToString(),
                new TextObject("{=Y3mAnDLj}The inheritance form associated with this settlement's title. Inheritance dictates who leads the clan after the leader's death.")
                    .ToString()));
            TitleInfo.Add(new InformationElement(new TextObject("{=vCryQjBB}Gender Law:").ToString(),
                title.contract.GenderLaw.ToString(),
                new TextObject("{=ArvZcS5p}The gender law associated with this settlement's title. Gender law affects how inheritance and other aspects of rule work.")
                    .ToString()));
        }
    }
}