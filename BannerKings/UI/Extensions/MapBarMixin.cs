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
		private int piety;
		private string pietyAbbr;
		public MapBarMixin(MapInfoVM vm) : base(vm)
        {
			Piety = 0;
        }

        public override void OnRefresh()
        {
			if (BannerKingsConfig.Instance.ReligionsManager == null) return;

			Religion rel = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(Hero.MainHero);
			Piety = (int)BannerKingsConfig.Instance.ReligionsManager.GetPiety(rel, Hero.MainHero);
			PietyHint = new BasicTooltipViewModel(() => UIHelper.GetPietyTooltip(rel, Hero.MainHero, Piety));
			PietyWithAbbrText = CampaignUIHelper.GetAbbreviatedValueTextFromValue(Piety);
			//if (rel == null) return;
		}

		[DataSourceProperty]
		public int Piety
		{
			get => piety;
			set
			{
				if (value != this.piety)
				{
					this.piety = value;
					ViewModel!.OnPropertyChangedWithValue(value, "Piety");
				}
			}
		}


		[DataSourceProperty]
		public string PietyWithAbbrText
		{
			get => pietyAbbr;
			set
			{
				if (value != this.pietyAbbr)
				{
					this.pietyAbbr = value;
					ViewModel!.OnPropertyChangedWithValue(value, "PietyWithAbbrText");
				}
			}
		}

		[DataSourceProperty]
		public BasicTooltipViewModel PietyHint
		{
			get => pietyHint;
			set
			{
				if (value != this.pietyHint)
				{
					pietyHint = value;
					ViewModel!.OnPropertyChangedWithValue(value, "PietyHint");
				}
			}
		}
	}
}
