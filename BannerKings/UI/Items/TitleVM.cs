using BannerKings.Utils;
using TaleWorlds.CampaignSystem;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using static BannerKings.Managers.TitleManager;
using BannerKings.Models;
using System.Text;
using System;
using TaleWorlds.Localization;
using System.Collections.Generic;

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
				UsurpData usurpData = model.IsUsurpable(title, Hero.MainHero);
				List<Hero> claimants = model.GetClaimants(title);

				CharacterCode characterCode = CharacterCode.CreateFrom(title.deJure.CharacterObject);
				this.ImageIdentifier = new ImageIdentifierVM(characterCode);
				this.Hint = new BasicTooltipViewModel(() => UIHelper.GetHeroCourtTooltip(title.deJure, usurpData, claimants));
				
				if (claimants.Contains(Hero.MainHero))
				{

					TextObject sb = new TextObject("{=!}Usurp this title from it's owner, making you the lawful ruler of this settlement. Usurping from lords within your kingdom degrades your clan's reputation.");
					DecisionElement usurpButton = new DecisionElement().SetAsButtonOption(new TextObject("{=!}Usurp").ToString(),
						delegate 
						{
							if (usurpData.Usurpable)
							{
								BannerKingsConfig.Instance.TitleManager.UsurpTitle(title.deJure, Hero.MainHero, title, usurpData);
								RefreshValues();
							}	
						},
						new TextObject(sb.ToString()));
					usurpButton.Enabled = usurpData.Usurpable;
					this.Decisions.Add(usurpButton);
				}
			}
		}

		private bool EvaluateShowUsurp()
        {
			if (title.deJure == Hero.MainHero) return false;
			else if (title.DeFacto == Hero.MainHero) return true;
			else
            {
				bool result = false;
				if (title.vassals != null && title.vassals.Count > 0)
					foreach (FeudalTitle vassal in title.vassals)
						if (vassal.deJure == Hero.MainHero)
                        {
							result = true;
							break;
                        }
				return result;
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
		public string NameText => this.title.name.ToString();
	}
}
