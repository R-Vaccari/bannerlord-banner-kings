using BannerKings.Utils;
using TaleWorlds.CampaignSystem;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using BannerKings.Models;
using TaleWorlds.Localization;
using System.Collections.Generic;
using BannerKings.Managers.Titles;

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
				List<Hero> claimants = model.GetClaimants(title);

				CharacterCode characterCode = CharacterCode.CreateFrom(title.deJure.CharacterObject);
				this.ImageIdentifier = new ImageIdentifierVM(characterCode);
				this.Hint = new BasicTooltipViewModel(() => UIHelper.GetHeroCourtTooltip(title.deJure, usurpData, claimants));
				
				if (claimants.Contains(Hero.MainHero))
				{
					DecisionElement usurpButton = new DecisionElement().SetAsButtonOption(new TextObject("{=!}Usurp").ToString(),
						delegate 
						{
							if (usurpData.Possible)
							{
								UIHelper.ShowTitleActionPopup(usurpData);
								RefreshValues();
							}	
						}, null);
					usurpButton.Enabled = usurpData.Possible;
					this.Decisions.Add(usurpButton);
				} else
                {
					TitleAction grantData = model.GetAction(ActionType.Grant, title, Hero.MainHero);
					if (grantData.Possible)
                    {
						DecisionElement grantButton = new DecisionElement().SetAsButtonOption(new TextObject("{=!}Grant").ToString(),
							delegate
							{
								UIHelper.ShowTitleActionPopup(grantData);
								RefreshValues();
							}, null);
						grantButton.Enabled = grantData.Possible;
						this.Decisions.Add(grantButton);
					}
				}
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
