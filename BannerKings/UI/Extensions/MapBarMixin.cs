using BannerKings.Managers.Institutions.Religions;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.CampaignSystem.ViewModelCollection.Map;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;

namespace BannerKings.UI.Extensions
{
    [ViewModelMixin("UpdatePlayerInfo")]
    internal class MapBarMixin : BaseViewModelMixin<MapInfoVM>
    {
		private BasicTooltipViewModel pietyHint;
		public MapBarMixin(MapInfoVM vm) : base(vm)
        {
			Piety = 0;
        }

        public override void OnRefresh()
        {
			if (BannerKingsConfig.Instance.ReligionsManager == null) return;

			Religion rel = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(Hero.MainHero);
			pietyHint = new BasicTooltipViewModel(() => UIHelper.GetPietyTooltip(rel, Hero.MainHero, Piety));
			PietyWithAbbrText = CampaignUIHelper.GetAbbreviatedValueTextFromValue(Piety);
			//if (rel == null) return;
		}

		[DataSourceProperty]
		public int Piety { get; set; }

		[DataSourceProperty]
		public string PietyWithAbbrText { get; set; }

		[DataSourceProperty]
		public BasicTooltipViewModel PietyHint => pietyHint;
	}
}
