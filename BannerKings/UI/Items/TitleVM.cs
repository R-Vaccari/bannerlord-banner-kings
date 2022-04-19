using TaleWorlds.CampaignSystem;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using System.Collections.Generic;
using BannerKings.Managers.Titles;
using BannerKings.Models.BKModels;

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
			this.decisions = new MBBindingList<DecisionElement>();
			this.RefreshValues();
		}

		public override void RefreshValues()
		{
			base.RefreshValues();
			this.Decisions.Clear();
			

			if (title != null)
			{
				BKTitleModel model = (BannerKingsConfig.Instance.Models.First(x => x is BKTitleModel) as BKTitleModel);
				TitleAction usurpData = model.GetAction(ActionType.Usurp, title, Hero.MainHero);
				CharacterCode characterCode = CharacterCode.CreateFrom(title.deJure.CharacterObject);
				this.ImageIdentifier = new ImageIdentifierVM(characterCode);

				List<TitleAction> actions = new List<TitleAction>();
				actions.Add(usurpData);
				if (title.GetHeroClaim(Hero.MainHero) != ClaimType.None)
				{
					DecisionElement usurpButton = new DecisionElement().SetAsButtonOption(new TextObject("{=!}Usurp").ToString(),
						() => UIHelper.ShowTitleActionPopup(usurpData, this));
					usurpButton.Enabled = usurpData.Possible;
					this.Decisions.Add(usurpButton);
				}

				TitleAction claimAction = model.GetAction(ActionType.Claim, title, Hero.MainHero);
				actions.Add(claimAction);
				if (claimAction.Possible)
                {
					DecisionElement claimButton = new DecisionElement().SetAsButtonOption(new TextObject("{=!}Claim").ToString(),
						() => UIHelper.ShowTitleActionPopup(claimAction, this));
					claimButton.Enabled = claimAction.Possible;
					this.Decisions.Add(claimButton);
				}

				TitleAction grantData = model.GetAction(ActionType.Grant, title, Hero.MainHero);
				actions.Add(grantData);
				if (grantData.Possible)
                {
					DecisionElement grantButton = new DecisionElement().SetAsButtonOption(new TextObject("{=!}Grant").ToString(),
						() => UIHelper.ShowTitleActionPopup(grantData, this));
					grantButton.Enabled = grantData.Possible;
					this.Decisions.Add(grantButton);
				}

				TitleAction revokeData = model.GetAction(ActionType.Revoke, title, Hero.MainHero);
				actions.Add(revokeData);
				if (revokeData.Possible)
				{
					DecisionElement revokeButton = new DecisionElement().SetAsButtonOption(new TextObject("{=!}Revoke").ToString(),
						() => UIHelper.ShowTitleActionPopup(revokeData, this));
					revokeButton.Enabled = revokeData.Possible;
					this.Decisions.Add(revokeButton);
				}
				

				this.Hint = new BasicTooltipViewModel(() => UIHelper.GetTitleTooltip(title, actions));
			}
		}

		public void ExecuteLink()
		{
			if (this.title.deJure != null)
				Campaign.Current.EncyclopediaManager.GoToLink(this.title.deJure.EncyclopediaLink);
			
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
		public BasicTooltipViewModel Hint
		{
			get => this.hint;
			set
			{
				if (value != this.hint)
				{
					this.hint = value;
					base.OnPropertyChangedWithValue(value, "Hint");
				}
			}
		}

		[DataSourceProperty]
		public ImageIdentifierVM ImageIdentifier
		{
			get => this.imageIdentifier;
			set
			{
				if (value != this.imageIdentifier)
				{
					this.imageIdentifier = value;
					base.OnPropertyChangedWithValue(value, "ImageIdentifier");
				}
			}
		}

		[DataSourceProperty]
		public string NameText => this.title.FullName.ToString();
	}
}
