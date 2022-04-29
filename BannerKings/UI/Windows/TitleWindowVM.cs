﻿using BannerKings.Managers.Helpers;
using BannerKings.Managers.Kingdoms.Contract;
using BannerKings.Managers.Titles;
using BannerKings.Populations;
using BannerKings.UI.Items;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

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
			title = BannerKingsConfig.Instance.TitleManager.GetTitle(data.Settlement);
			decisions = new MBBindingList<DecisionElement>();
			if (title != null)
            {
				kingdom = data.Settlement.OwnerClan.Kingdom;
				if (title.sovereign != null)
				{
					Tree = new TitleElementVM(title.sovereign);
					Kingdom deJureKingdom = BannerKingsConfig.Instance.TitleManager.GetTitleFaction(title.sovereign);
					if (deJureKingdom != null)
						Banner = new ImageIdentifierVM(BannerCode.CreateFrom(deJureKingdom.Banner), true);

					Name = title.sovereign.FullName.ToString();
				}
				else Tree = new TitleElementVM(title);

				if (Banner == null)
					Banner = new ImageIdentifierVM(BannerCode.CreateFrom(data.Settlement.OwnerClan.Banner), true);
				if (Name == null)
					Name = title.FullName.ToString();
			}
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
			Decisions.Clear();

			bool allSetup = kingdom != null && title != null && title.contract != null &&
				kingdom == BannerKingsConfig.Instance.TitleManager.GetTitleFaction(title);
			DecisionElement contractButton = new DecisionElement().SetAsButtonOption(new TextObject("{=!}Contract").ToString(),
				() => BannerKingsConfig.Instance.TitleManager.ShowContract(kingdom.Leader, GameTexts.FindText("str_done").ToString()),
				new TextObject("{=!}Review this kingdom's contract, signed by lords that join it."));
			contractButton.Enabled = allSetup;


			List<InquiryElement> governments = GetGovernments();
			DecisionElement governmentButton = CreateButton(governments, governments.Count >= 1 ? new BKGovernmentDecision(data.Settlement.OwnerClan, (GovernmentType)governments[0].Identifier, title?.sovereign) : null,
				new TextObject("{=!}Government").ToString(),
				new TextObject("{=!}Propose a change in government structure, altering the allowed succession forms and aspects of settlement governance. Depending on the government choice, an appropriate succession type will be enforced as well."));
			governmentButton.Enabled = allSetup && governments.Count >= 1;

			List<InquiryElement> successions = GetSuccessions();
			DecisionElement successionButton = CreateButton(successions, successions.Count >= 1 ? new BKSuccessionDecision(data.Settlement.OwnerClan, (SuccessionType)successions[0].Identifier, title?.sovereign) : null,
				new TextObject("{=!}Succession").ToString(),
				new TextObject("{=!}Propose a change in the realm's succession, altering how the next sovereign is chosen."));
			successionButton.Enabled = allSetup && successions.Count >= 1 && title.contract.Government != GovernmentType.Imperial && title.contract.Government != GovernmentType.Republic;

			List<InquiryElement> inheritances = GetInheritances();
			DecisionElement inheritanceButton = CreateButton(inheritances, inheritances.Count >= 1 ? new BKInheritanceDecision(data.Settlement.OwnerClan, (InheritanceType)inheritances[0].Identifier, title?.sovereign) : null,
				new TextObject("{=!}Inheritance").ToString(),
				new TextObject("{=!}Propose a change in clan inheritances, that is, who becomes the clan leader once the leader dies."));
			inheritanceButton.Enabled = allSetup && inheritances.Count >= 1;

			List<InquiryElement> genderLaws = GetGenderLaws();
			DecisionElement genderButton = CreateButton(genderLaws, genderLaws.Count >= 1 ? new BKGenderDecision(data.Settlement.OwnerClan, (GenderLaw)genderLaws[0].Identifier, title?.sovereign) : null, 
				new TextObject("{=!}Gender Law").ToString(),
				new TextObject("{=!}Propose a change in gender laws, dictating whether males and females are viewed equally in various aspects."));
			genderButton.Enabled = allSetup && genderLaws.Count >= 1;

			Contract = contractButton;
			Decisions.Add(governmentButton);
			Decisions.Add(successionButton);
			Decisions.Add(inheritanceButton);
			Decisions.Add(genderButton);
		}

		private DecisionElement CreateButton(List<InquiryElement> options, BKContractDecision decision, string law, TextObject hint) => new DecisionElement()
			.SetAsButtonOption(law, delegate
				{
					TextObject description = new TextObject("{=!}Select a {LAW} to be voted on. Starting an election costs {INFLUENCE} influence.");
					description.SetTextVariable("LAW", law);
					int cost = decision != null ? decision.GetInfluenceCost(null) : 0;
					description.SetTextVariable("INFLUENCE", cost);

					if (kingdom != null && options.Count > 0 && decision != null)
                    {
						InformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(law, description.ToString(),
						options, true, 1, GameTexts.FindText("str_done").ToString(), string.Empty,
						delegate (List<InquiryElement> x)
						{
							if (Clan.PlayerClan.Influence < cost)
								InformationManager.DisplayMessage(new InformationMessage("Not enough influence."));
							else if (decision.Kingdom.UnresolvedDecisions.Any(x => x is BKContractDecision))
								InformationManager.DisplayMessage(new InformationMessage("Ongoing contract-altering decision."));
							else
							{
								GainKingdomInfluenceAction.ApplyForDefault(Hero.MainHero, -cost);
								kingdom.AddDecision((BKContractDecision)x[0].Identifier, true);
							}
						}, null, string.Empty));
					}
				}, hint);

		private List<InquiryElement> GetGenderLaws()
        {
			List<InquiryElement> laws = new List<InquiryElement>();
			foreach (GenderLaw type in BannerKingsConfig.Instance.TitleManager.GetGenderLawTypes())
				if (kingdom != null && type != title.contract.GenderLaw)
                {
					BKGenderDecision decision = new BKGenderDecision(data.Settlement.OwnerClan, type, title?.sovereign);
					TextObject text = new TextObject("{=!}{LAW} - ({SUPPORT}% support)");
					text.SetTextVariable("LAW", type.ToString());
					text.SetTextVariable("SUPPORT", decision.CalculateKingdomSupport(kingdom));
					laws.Add(new InquiryElement(type, text.ToString(), null, true, Helpers.Helpers.GetGenderLawDescription(type)));
				}
					
			return laws;
		}

		private List<InquiryElement> GetInheritances()
		{
			List<InquiryElement> laws = new List<InquiryElement>();
			foreach (InheritanceType type in BannerKingsConfig.Instance.TitleManager.GetInheritanceTypes())
				if (kingdom != null && type != title.contract.Inheritance)
                {
					BKInheritanceDecision decision = new BKInheritanceDecision(data.Settlement.OwnerClan, type, title?.sovereign);
					TextObject text = new TextObject("{=!}{LAW} - ({SUPPORT}% support)");
					text.SetTextVariable("LAW", type.ToString());
					text.SetTextVariable("SUPPORT", decision.CalculateKingdomSupport(kingdom));
					laws.Add(new InquiryElement(type, text.ToString(), null, true, Helpers.Helpers.GetInheritanceDescription(type)));
				}
					
			return laws;
		}

		private List<InquiryElement> GetSuccessions()
		{
			List<InquiryElement> laws = new List<InquiryElement>();
			foreach (SuccessionType type in SuccessionHelper.GetValidSuccessions(title.contract.Government))
				if (kingdom != null && type != title.contract.Succession)
                {
					BKSuccessionDecision decision = new BKSuccessionDecision(data.Settlement.OwnerClan, type, title?.sovereign);
					TextObject text = new TextObject("{=!}{LAW} - ({SUPPORT}% support)");
					text.SetTextVariable("LAW", Helpers.Helpers.GetSuccessionTypeName(type));
					text.SetTextVariable("SUPPORT", decision.CalculateKingdomSupport(kingdom));
					laws.Add(new InquiryElement(type, text.ToString(), null, true, Helpers.Helpers.GetSuccessionTypeDescription(type)));
				}
					
			return laws;
		}

		private List<InquiryElement> GetGovernments()
		{
			List<InquiryElement> laws = new List<InquiryElement>();
			foreach (GovernmentType type in BannerKingsConfig.Instance.TitleManager.GetGovernmentTypes())
				if (kingdom != null && type != title.contract.Government)
                {
					BKGovernmentDecision decision = new BKGovernmentDecision(data.Settlement.OwnerClan, type, title?.sovereign);
					TextObject text = new TextObject("{=!}{LAW} - ({SUPPORT}% support)");
					text.SetTextVariable("LAW", type.ToString());
					text.SetTextVariable("SUPPORT", decision.CalculateKingdomSupport(kingdom));
					laws.Add(new InquiryElement(type, text.ToString(), null, true, Helpers.Helpers.GetGovernmentDescription(type)));
				}
					
			return laws;
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
