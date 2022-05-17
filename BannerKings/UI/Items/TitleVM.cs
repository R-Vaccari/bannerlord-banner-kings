using BannerKings.Managers.Titles;
using BannerKings.Models.BKModels;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI.Items
{
    public class TitleVM : ViewModel
    {
		private FeudalTitle title;
		private ImageIdentifierVM imageIdentifier;
		private MBBindingList<DecisionElement> decisions;
		private BasicTooltipViewModel hint;

		public TitleVM(FeudalTitle title)
		{
			this.title = title;
			decisions = new MBBindingList<DecisionElement>();
			RefreshValues();
		}

		public override void RefreshValues()
		{
			base.RefreshValues();
			Decisions.Clear();
			

			if (title != null)
			{
				BKTitleModel model = (BannerKingsConfig.Instance.Models.First(x => x is BKTitleModel) as BKTitleModel);
				CharacterCode characterCode = CharacterCode.CreateFrom(title.deJure.CharacterObject);
				ImageIdentifier = new ImageIdentifierVM(characterCode);

				List<TitleAction> actions = new List<TitleAction>();
				TitleAction usurpData = model.GetAction(ActionType.Usurp, title, Hero.MainHero);
				if (title.GetHeroClaim(Hero.MainHero) != ClaimType.None)
				{
					DecisionElement usurpButton = new DecisionElement().SetAsButtonOption(new TextObject("{=!}Usurp").ToString(),
						() => UIHelper.ShowActionPopup(usurpData, this));
					usurpButton.Enabled = usurpData.Possible;
					Decisions.Add(usurpButton);
				}

				TitleAction claimAction = model.GetAction(ActionType.Claim, title, Hero.MainHero);
				if (claimAction.Possible)
                {
					DecisionElement claimButton = new DecisionElement().SetAsButtonOption(new TextObject("{=!}Claim").ToString(),
						() => UIHelper.ShowActionPopup(claimAction, this));
					claimButton.Enabled = claimAction.Possible;
					Decisions.Add(claimButton);
				}

				TitleAction grantData = model.GetAction(ActionType.Grant, title, Hero.MainHero);
				if (grantData.Possible)
                {
					DecisionElement grantButton = new DecisionElement().SetAsButtonOption(new TextObject("{=!}Grant").ToString(),
						() => UIHelper.ShowActionPopup(grantData, this));
					grantButton.Enabled = grantData.Possible;
					Decisions.Add(grantButton);
				}

				TitleAction revokeData = model.GetAction(ActionType.Revoke, title, Hero.MainHero);
				if (revokeData.Possible)
				{
					DecisionElement revokeButton = new DecisionElement().SetAsButtonOption(new TextObject("{=!}Revoke").ToString(),
						() => UIHelper.ShowActionPopup(revokeData, this));
					revokeButton.Enabled = revokeData.Possible;
					Decisions.Add(revokeButton);
				}

				if (title.deJure != Hero.MainHero)
				{
					actions.Add(usurpData);
					actions.Add(claimAction);
					actions.Add(revokeData);
				}
				else actions.Add(grantData);

				Hint = new BasicTooltipViewModel(() => UIHelper.GetTitleTooltip(title, actions));
			}
		}

		public void ExecuteLink()
		{
			if (title.deJure != null)
				Campaign.Current.EncyclopediaManager.GoToLink(title.deJure.EncyclopediaLink);
			
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
		public BasicTooltipViewModel Hint
		{
			get => hint;
			set
			{
				if (value != hint)
				{
					hint = value;
					OnPropertyChangedWithValue(value);
				}
			}
		}

		[DataSourceProperty]
		public ImageIdentifierVM ImageIdentifier
		{
			get => imageIdentifier;
			set
			{
				if (value != imageIdentifier)
				{
					imageIdentifier = value;
					OnPropertyChangedWithValue(value);
				}
			}
		}

		[DataSourceProperty]
		public string NameText => title.FullName.ToString();
	}
}
