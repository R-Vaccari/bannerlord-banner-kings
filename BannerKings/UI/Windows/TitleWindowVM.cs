using BannerKings.Populations;
using BannerKings.UI.Items;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using static BannerKings.Managers.TitleManager;

namespace BannerKings.UI.Windows
{
    public class TitleWindowVM : BannerKingsViewModel
    {
		private TitleElementVM tree;
		private MBBindingList<DecisionElement> decisions;
		private ImageIdentifierVM banner;
		private Kingdom kingdom;
		private string name;

		public TitleWindowVM(PopulationData data) : base(data, true)
        {
			FeudalTitle title = BannerKingsConfig.Instance.TitleManager.GetTitle(data.Settlement);
			this.decisions = new MBBindingList<DecisionElement>();
			if (title != null)
            {
				if (title.sovereign != null)
				{
					this.Tree = new TitleElementVM(title.sovereign);
					kingdom = BannerKingsConfig.Instance.TitleManager.GetTitleFaction(title.sovereign);
					if (kingdom != null)
                    {
						Banner = new ImageIdentifierVM(BannerCode.CreateFrom(kingdom.Banner), true);
					}
						
					Name = title.sovereign.name.ToString();
				}
				else
				{
					this.Tree = new TitleElementVM(title);
					Banner = new ImageIdentifierVM(BannerCode.CreateFrom(data.Settlement.OwnerClan.Banner), true);
					Name = title.name.ToString();
				}
			}
			
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
			this.Decisions.Clear();

			DecisionElement usurpButton = new DecisionElement().SetAsButtonOption(new TextObject("{=!}Contract").ToString(),
				() => BannerKingsConfig.Instance.TitleManager.ShowContract(kingdom.Leader, GameTexts.FindText("str_done").ToString()),
				new TextObject("{=!}Review this kingdom's contract, signed by lords that join it."));
			usurpButton.Enabled = this.kingdom != null;
			this.Decisions.Add(usurpButton);

		}

	

		[DataSourceProperty]
		public MBBindingList<DecisionElement> Decisions
		{
			get => this.decisions;
			set
			{
				if (value != this.decisions)
				{
					this.decisions = value;
					base.OnPropertyChangedWithValue(value, "Decisions");
				}
			}
		}

		[DataSourceProperty]
		public string Name
		{
			get => this.name;
			set
			{
				if (value != this.name)
				{
					this.name = value;
					base.OnPropertyChangedWithValue(value, "Name");
				}
			}
		}

		[DataSourceProperty]
		public ImageIdentifierVM Banner
		{
			get => this.banner;	
			set
			{
				if (value != this.banner)
				{
					this.banner = value;
					base.OnPropertyChangedWithValue(value, "Banner");
				}
			}
		}

		[DataSourceProperty]
		public TitleElementVM Tree
		{
			get => this.tree;
			set
			{
				if (value != this.tree)
				{
					this.tree = value;
					base.OnPropertyChangedWithValue(value, "Tree");
				}
			}
		}
	}
}
