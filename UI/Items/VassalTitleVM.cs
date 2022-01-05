using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.EncyclopediaItems;
using TaleWorlds.Library;
using static Populations.Managers.TitleManager;

namespace Populations.UI.Items
{
    public class VassalTitleVM : EncyclopediaSettlementVM
    {
        FeudalTitle _title;
        public VassalTitleVM(FeudalTitle title): base(title.fief)
        {
            this._title = title;
        }

		[DataSourceProperty]
		public string FullNameText
		{
			get => this._title.name.ToString();
		}
	}
}
