using BannerKings.Utils;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using static BannerKings.Managers.TitleManager;

namespace BannerKings.UI.Items
{
    public class TitleVM : ViewModel
    {
		private FeudalTitle title;
		private ImageIdentifierVM imageIdentifier;
		private BasicTooltipViewModel hint;

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
		}

		public void ExecuteLink()
		{
			if (this.title.deJure != null)
				Campaign.Current.EncyclopediaManager.GoToLink(this.title.deJure.EncyclopediaLink);
			
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
