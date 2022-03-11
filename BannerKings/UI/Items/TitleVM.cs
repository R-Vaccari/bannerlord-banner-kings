using TaleWorlds.Core;
using TaleWorlds.Library;
using static BannerKings.Managers.TitleManager;

namespace BannerKings.UI.Items
{
    public class TitleVM : ViewModel
    {
		private FeudalTitle title;
		private ImageIdentifierVM _imageIdentifier;

		public TitleVM(FeudalTitle title)
		{
			if (title != null)
			{
				CharacterCode characterCode = CharacterCode.CreateFrom(title.deJure.CharacterObject);
				this.ImageIdentifier = new ImageIdentifierVM(characterCode);
				this.title = title;
			}
			this.RefreshValues();
		}

		public override void RefreshValues()
		{
			base.RefreshValues();
		}


		[DataSourceProperty]
		public ImageIdentifierVM ImageIdentifier
		{
			get => this._imageIdentifier;
			set
			{
				if (value != this._imageIdentifier)
				{
					this._imageIdentifier = value;
					base.OnPropertyChangedWithValue(value, "ImageIdentifier");
				}
			}
		}

		[DataSourceProperty]
		public string NameText => this.title.name.ToString();
	}
}
