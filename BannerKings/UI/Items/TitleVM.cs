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

namespace BannerKings.UI.Items
{
    public class TitleVM : ViewModel
    {
		private FeudalTitle title;
		private ImageIdentifierVM imageIdentifier;
		private BasicTooltipViewModel hint;
		private UsurpCosts costs;
		private (bool, string) canUsurp;
		private bool showUsurp;

		public TitleVM(FeudalTitle title)
		{
			if (title != null)
			{
				CharacterCode characterCode = CharacterCode.CreateFrom(title.deJure.CharacterObject);
				this.ImageIdentifier = new ImageIdentifierVM(characterCode);
				this.title = title;
			}
			this.Hint = new BasicTooltipViewModel(() => UIHelper.GetHeroCourtTooltip(title.deJure));
			this.RefreshValues();
		}

		public override void RefreshValues()
		{
			base.RefreshValues();
			this.ShowUsurp = EvaluateShowUsurp();
			BKUsurpationModel model = (BannerKingsConfig.Instance.Models.First(x => x is BKUsurpationModel) as BKUsurpationModel);
			this.canUsurp = model.IsUsurpable(title, Hero.MainHero);
			this.costs = model.GetUsurpationCosts(title, Hero.MainHero);
		}

		private bool EvaluateShowUsurp()
        {
			if (title.deJure == Hero.MainHero) return false;
			else if (title.deFacto == Hero.MainHero) return true;
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

		private void OnUsurpPress()
		{
			if (title != null)
			{
				bool usurpable = canUsurp.Item1;
				if (usurpable)
					BannerKingsConfig.Instance.TitleManager.UsurpTitle(title.deJure, Hero.MainHero, title, costs);
				else InformationManager.DisplayMessage(new InformationMessage(canUsurp.Item2));
				RefreshValues();
			}
		}

		[DataSourceProperty]
		public bool ShowUsurp
		{
			get => this.showUsurp;
			set
			{
				if (value != this.showUsurp)
				{
					this.showUsurp = value;
					base.OnPropertyChangedWithValue(value, "ShowUsurp");
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
		public HintViewModel UsurpHint
		{
			get
			{
				if (title != null)
				{
					UsurpCosts costs = this.costs;
					StringBuilder sb = new StringBuilder("Usurp this title from it's owner, making you the lawful ruler of this settlement. Usurping from lords within your kingdom degrades your clan's reputation.");
					sb.Append(Environment.NewLine);
					sb.Append("Costs:");
					sb.Append(Environment.NewLine);
					sb.Append(string.Format("{0} gold.", (int)costs.gold));
					sb.Append(Environment.NewLine);
					sb.Append(string.Format("{0} influence.", (int)costs.influence));
					sb.Append(Environment.NewLine);
					sb.Append(string.Format("{0} renown.", (int)costs.renown));
					return new HintViewModel(new TextObject(sb.ToString()));
				}
				return new HintViewModel(new TextObject("{=!}No title identified for this settlement."));
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
