using BannerKings.Managers.Helpers;
using BannerKings.Managers.Kingdoms.Contract;
using BannerKings.Populations;
using BannerKings.UI.Items;
using System;
using System.Linq;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using static BannerKings.Managers.TitleManager;

namespace BannerKings.UI.Windows
{
    public class TitleWindowVM : BannerKingsViewModel
    {
		private TitleElementVM tree;
		private FeudalTitle title;
		private MBBindingList<DecisionElement> decisions;
		private ImageIdentifierVM banner;
		private Kingdom kingdom;
		private DecisionElement contract;
		private string name;

		public TitleWindowVM(PopulationData data) : base(data, true)
        {
			this.title = BannerKingsConfig.Instance.TitleManager.GetTitle(data.Settlement);
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
						
					Name = title.sovereign.FullName.ToString();
				}
				else
				{
					this.Tree = new TitleElementVM(title);
					Banner = new ImageIdentifierVM(BannerCode.CreateFrom(data.Settlement.OwnerClan.Banner), true);
					Name = title.FullName.ToString();
				}
			}
			
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
			this.Decisions.Clear();

			bool allSetup = this.kingdom != null && this.title != null && this.title.contract != null;
			DecisionElement contractButton = new DecisionElement().SetAsButtonOption(new TextObject("{=!}Contract").ToString(),
				() => BannerKingsConfig.Instance.TitleManager.ShowContract(kingdom.Leader, GameTexts.FindText("str_done").ToString()),
				new TextObject("{=!}Review this kingdom's contract, signed by lords that join it."));
			contractButton.Enabled = allSetup;


			List<InquiryElement> governments = this.GetGovernments();
			DecisionElement governmentButton = this.CreateButton(governments, new BKGovernmentDecision(this.data.Settlement.OwnerClan, (GovernmentType)governments[0].Identifier, this.title?.sovereign),
				new TextObject("{=!}Government").ToString(),
				new TextObject("{=!}Propose a change in government structure, altering the allowed succession forms and aspects of settlement governance. Depending on the government choice, an appropriate succession type will be enforced as well."));
			governmentButton.Enabled = allSetup;

			List<InquiryElement> successions = this.GetSuccessions();
			DecisionElement successionButton = this.CreateButton(successions, new BKSuccessionDecision(this.data.Settlement.OwnerClan, (SuccessionType)successions[0].Identifier, this.title?.sovereign),
				new TextObject("{=!}Succession").ToString(),
				new TextObject("{=!}Propose a change in the realm's succession, altering how the next sovereign is chosen."));
			successionButton.Enabled = allSetup && this.title.contract.Government != GovernmentType.Imperial && this.title.contract.Government != GovernmentType.Republic;

			List<InquiryElement> inheritances = this.GetInheritances();
			DecisionElement inheritanceButton = this.CreateButton(inheritances, new BKInheritanceDecision(this.data.Settlement.OwnerClan, (InheritanceType)inheritances[0].Identifier, this.title?.sovereign),
				new TextObject("{=!}Inheritance").ToString(),
				new TextObject("{=!}Propose a change in clan inheritances, that is, who becomes the clan leader once the leader dies."));
			inheritanceButton.Enabled = allSetup;

			List<InquiryElement> genderLaws = this.GetGenderLaws();
			DecisionElement genderButton = this.CreateButton(genderLaws, new BKGenderDecision(this.data.Settlement.OwnerClan, (GenderLaw)genderLaws[0].Identifier, this.title?.sovereign), 
				new TextObject("{=!}Gender Law").ToString(),
				new TextObject("{=!}Propose a change in gender laws, dictating whether males and females are viewed equally in various aspects."));
			genderButton.Enabled = allSetup;
				

			this.Contract = contractButton;
			this.Decisions.Add(governmentButton);
			this.Decisions.Add(successionButton);
			this.Decisions.Add(inheritanceButton);
			this.Decisions.Add(genderButton);
		}

		private DecisionElement CreateButton(List<InquiryElement> options, BKContractDecision decision, string law, TextObject hint) => new DecisionElement()
			.SetAsButtonOption(law, delegate
				{
					TextObject description = new TextObject("{=!}Select a {LAW} to be voted on. Starting an election costs {INFLUENCE} influence.");
					description.SetTextVariable("LAW", law);
					int cost = decision.GetInfluenceCost(null);
					description.SetTextVariable("INFLUENCE", cost);

					InformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(law, description.ToString(), 
						options, true, 1, GameTexts.FindText("str_done", null).ToString(), string.Empty,
						new Action<List<InquiryElement>>(delegate (List<InquiryElement> x)
						{
							if (Clan.PlayerClan.Influence < cost) 
								InformationManager.DisplayMessage(new InformationMessage("Not enough influence."));
							else if (decision.Kingdom.UnresolvedDecisions.Any(x => x is BKContractDecision))
								InformationManager.DisplayMessage(new InformationMessage("Ongoing contract-altering decision."));
							else
							{
								GainKingdomInfluenceAction.ApplyForDefault(Hero.MainHero, -cost);
								kingdom.AddDecision(decision, true);
							}
						}), null, string.Empty), false);
				}, hint);


		private List<InquiryElement> GetGenderLaws()
        {
			List<InquiryElement> laws = new List<InquiryElement>();
			foreach (GenderLaw type in BannerKingsConfig.Instance.TitleManager.GetGenderLawTypes())
				if (type != this.title.contract.GenderLaw)
                {
					BKGenderDecision decision = new BKGenderDecision(this.data.Settlement.OwnerClan, type, this.title?.sovereign);
					TextObject text = new TextObject("{=!}{LAW} - ({SUPPORT}% support)");
					text.SetTextVariable("LAW", type.ToString());
					text.SetTextVariable("SUPPORT", decision.CalculateKingdomSupport(this.kingdom));
					laws.Add(new InquiryElement(type, text.ToString(), null, true, BannerKings.Helpers.Helpers.GetGenderLawDescription(type)));
				}
					
			return laws;
		}

		private List<InquiryElement> GetInheritances()
		{
			List<InquiryElement> laws = new List<InquiryElement>();
			foreach (InheritanceType type in BannerKingsConfig.Instance.TitleManager.GetInheritanceTypes())
				if (type != this.title.contract.Inheritance)
                {
					BKInheritanceDecision decision = new BKInheritanceDecision(this.data.Settlement.OwnerClan, type, this.title?.sovereign);
					TextObject text = new TextObject("{=!}{LAW} - ({SUPPORT}% support)");
					text.SetTextVariable("LAW", type.ToString());
					text.SetTextVariable("SUPPORT", decision.CalculateKingdomSupport(this.kingdom));
					laws.Add(new InquiryElement(type, text.ToString(), null, true, BannerKings.Helpers.Helpers.GetInheritanceDescription(type)));
				}
					
			return laws;
		}

		private List<InquiryElement> GetSuccessions()
		{
			List<InquiryElement> laws = new List<InquiryElement>();
			foreach (SuccessionType type in SuccessionHelper.GetValidSuccessions(this.title.contract.Government))
				if (type != this.title.contract.Succession)
                {
					BKSuccessionDecision decision = new BKSuccessionDecision(this.data.Settlement.OwnerClan, type, this.title?.sovereign);
					TextObject text = new TextObject("{=!}{LAW} - ({SUPPORT}% support)");
					text.SetTextVariable("LAW", Helpers.Helpers.GetSuccessionTypeName(type));
					text.SetTextVariable("SUPPORT", decision.CalculateKingdomSupport(this.kingdom));
					laws.Add(new InquiryElement(type, text.ToString(), null, true, BannerKings.Helpers.Helpers.GetSuccessionTypeDescription(type)));
				}
					
			return laws;
		}

		private List<InquiryElement> GetGovernments()
		{
			List<InquiryElement> laws = new List<InquiryElement>();
			foreach (GovernmentType type in BannerKingsConfig.Instance.TitleManager.GetGovernmentTypes())
				if (type != this.title.contract.Government)
                {
					BKGovernmentDecision decision = new BKGovernmentDecision(this.data.Settlement.OwnerClan, type, this.title?.sovereign);
					TextObject text = new TextObject("{=!}{LAW} - ({SUPPORT}% support)");
					text.SetTextVariable("LAW", type.ToString());
					text.SetTextVariable("SUPPORT", decision.CalculateKingdomSupport(this.kingdom));
					laws.Add(new InquiryElement(type, text.ToString(), null, true, BannerKings.Helpers.Helpers.GetGovernmentDescription(type)));
				}
					
			return laws;
		}


		[DataSourceProperty]
		public DecisionElement Contract
		{
			get => this.contract;
			set
			{
				if (value != this.contract)
				{
					this.contract = value;
					base.OnPropertyChangedWithValue(value, "Contract");
				}
			}
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
