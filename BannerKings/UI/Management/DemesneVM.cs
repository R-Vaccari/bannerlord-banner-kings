using BannerKings.Managers.Kingdoms.Contract;
using BannerKings.Managers.Policies;
using BannerKings.Managers.Titles;
using BannerKings.Models;
using BannerKings.Populations;
using BannerKings.UI.Items;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.UI
{
    public class DemesneVM : BannerKingsViewModel
    {
		private HeroVM deJure;
		private MBBindingList<InformationElement> demesneInfo, landInfo, terrainInfo, workforceInfo, governmentInfo;
		private FeudalTitle title;
		private FeudalTitle duchy;
		private SelectorVM<BKItemVM> workforceVM;
		private BKWorkforcePolicy workforceItem;

		public DemesneVM(PopulationData data, FeudalTitle title, bool isSelected) : base(data, isSelected)
        {
			this.title = title;
			if (title != null)
            {
				deJure = new HeroVM(title.deJure);
				duchy = BannerKingsConfig.Instance.TitleManager.GetDuchy(this.title);
			}

			demesneInfo = new MBBindingList<InformationElement>();
			governmentInfo = new MBBindingList<InformationElement>();
			landInfo = new MBBindingList<InformationElement>();
			terrainInfo = new MBBindingList<InformationElement>();
			workforceInfo = new MBBindingList<InformationElement>();
			
			/*
			
			this.duchyCosts = model.GetUsurpationCosts(_duchy, Hero.MainHero);
			
			this._usurpDuchyEnabled = this._duchy.deJure != Hero.MainHero;
			if (title.vassals != null)
				foreach (FeudalTitle vassal in title.vassals)
					if (vassal.fief != null) _vassals.Add(new VassalTitleVM(vassal)); */
		}

		public override void RefreshValues()
        {
			base.RefreshValues();
			LandData landData = data.LandData;
			DemesneInfo.Clear();
			LandInfo.Clear();
			TerrainInfo.Clear();
			WorkforceInfo.Clear();
			GovernmentInfo.Clear();

			if (title != null)
            {
				float legitimacyType = new BKLegitimacyModel().CalculateEffect(data.Settlement).ResultNumber;
				if (legitimacyType > 0f)
				{
					LegitimacyType legitimacy = (LegitimacyType)legitimacyType;
					DemesneInfo.Add(new InformationElement(new TextObject("Legitimacy:").ToString(), legitimacy.ToString().Replace('_', ' '),
						new TextObject("Your legitimacy to this title and it's vassals. You are lawful when you own this title, and considered a foreigner if your culture differs from it.").ToString()));
				}

				if (title.sovereign != null) DemesneInfo.Add(new InformationElement(new TextObject("Sovereign:").ToString(), title.sovereign.FullName.ToString(),
					new TextObject("The master suzerain of this title, be they a king or emperor type suzerain.").ToString()));
				if (duchy != null) DemesneInfo.Add(new InformationElement("Dukedom:", duchy.FullName.ToString(),
					"The dukedom this settlement is associated with."));

				GovernmentInfo.Add(new InformationElement(new TextObject("Government Type:").ToString(), title.contract.Government.ToString(),
					new TextObject("The dukedom this settlement is associated with.").ToString()));
				GovernmentInfo.Add(new InformationElement(new TextObject("Succession Type:").ToString(), title.contract.Succession.ToString().Replace("_", " "),
					new TextObject("The dukedom this settlement is associated with.").ToString()));
				GovernmentInfo.Add(new InformationElement(new TextObject("Inheritance Type:").ToString(), title.contract.Inheritance.ToString(),
					new TextObject("The dukedom this settlement is associated with.").ToString()));
				GovernmentInfo.Add(new InformationElement(new TextObject("Gender Law:").ToString(), title.contract.GenderLaw.ToString(),
					new TextObject("The dukedom this settlement is associated with.").ToString()));

				DeJure = new HeroVM(title.deJure);
			}
			

			LandInfo.Add(new InformationElement(new TextObject("Acreage:").ToString(), landData.Acreage + " acres",
				new TextObject("Current quantity of usable acres in this region").ToString()));
			LandInfo.Add(new InformationElement(new TextObject("Farmland:").ToString(), landData.Farmland + " acres",
				new TextObject("Acres in this region used as farmland, the main source of food in most places").ToString()));
			LandInfo.Add(new InformationElement(new TextObject("Pastureland:").ToString(), landData.Pastureland + " acres",
				new TextObject("Acres in this region used as pastureland, to raise cattle and other animals. These output meat and animal products such as butter and cheese").ToString()));
			LandInfo.Add(new InformationElement(new TextObject("Woodland:").ToString(), landData.Woodland + " acres",
				new TextObject("Acres in this region used as woodland, kept for hunting, foraging of berries and materials like wood").ToString()));

			TerrainInfo.Add(new InformationElement(new TextObject("Type:").ToString(), landData.Terrain.ToString(),
				new TextObject("The local terrain type. Dictates fertility and terrain difficulty.").ToString()));
			TerrainInfo.Add(new InformationElement(new TextObject("Fertility:").ToString(), FormatValue(landData.Fertility),
				new TextObject("How fertile the region is. This depends solely on the local terrain type - harsher environments like deserts are less fertile than plains and grassy hills").ToString()));
			TerrainInfo.Add(new InformationElement(new TextObject("Terrain Difficulty:").ToString(), FormatValue(landData.Difficulty),
				new TextObject("Represents how difficult it is to create new usable acres. Like fertility, depends on terrain, but is not strictly correlated to it").ToString()));

			WorkforceInfo.Add(new InformationElement(new TextObject("Available Workforce:").ToString(), landData.AvailableWorkForce.ToString(),
				new TextObject("The amount of productive workers in this region, able to work the land").ToString()));
			WorkforceInfo.Add(new InformationElement(new TextObject("Workforce Saturation:").ToString(), FormatValue(landData.WorkforceSaturation),
				new TextObject("Represents how many workers there are in correlation to the amount needed to fully utilize the acreage. Saturation over 100% indicates more workers than the land needs, while under 100% means not all acres are producing output").ToString()));

			if (HasTown)
			{
				workforceItem = (BKWorkforcePolicy)BannerKingsConfig.Instance.PolicyManager.GetPolicy(data.Settlement, "workforce");
				WorkforceSelector = GetSelector(workforceItem, workforceItem.OnChange);
				WorkforceSelector.SelectedIndex = workforceItem.Selected;
				WorkforceSelector.SetOnChangeAction(workforceItem.OnChange);
			}
		}

		[DataSourceProperty]
		public SelectorVM<BKItemVM> WorkforceSelector
		{
			get => workforceVM;
			set
			{
				if (value != workforceVM)
				{
					workforceVM = value;
					OnPropertyChangedWithValue(value);
				}
			}
		}

		
		private void OnUsurpPress()
		{
			if (title != null)
            {
				Kingdom kingdom = data.Settlement.OwnerClan.Kingdom;
				if (kingdom != null)
					kingdom.AddDecision(new BKGovernmentDecision(data.Settlement.OwnerClan, GovernmentType.Imperial, title));
			}
		}

		[DataSourceProperty]
		public MBBindingList<InformationElement> LandInfo
		{
			get => landInfo;
			set
			{
				if (value != landInfo)
				{
					landInfo = value;
					OnPropertyChangedWithValue(value);
				}
			}
		}

		[DataSourceProperty]
		public MBBindingList<InformationElement> TerrainInfo
		{
			get => terrainInfo;
			set
			{
				if (value != terrainInfo)
				{
					terrainInfo = value;
					OnPropertyChangedWithValue(value);
				}
			}
		}

		[DataSourceProperty]
		public MBBindingList<InformationElement> WorkforceInfo
		{
			get => workforceInfo;
			set
			{
				if (value != workforceInfo)
				{
					workforceInfo = value;
					OnPropertyChangedWithValue(value);
				}
			}
		}

		[DataSourceProperty]
		public MBBindingList<InformationElement> GovernmentInfo
		{
			get => governmentInfo;
			set
			{
				if (value != governmentInfo)
				{
					governmentInfo = value;
					OnPropertyChangedWithValue(value);
				}
			}
		}

		[DataSourceProperty]
		public MBBindingList<InformationElement> DemesneInfo
		{
			get => demesneInfo;
			set
			{
				if (value != demesneInfo)
				{
					demesneInfo = value;
					OnPropertyChangedWithValue(value);
				}
			}
		}


		[DataSourceProperty]
		public HeroVM DeJure
		{
			get => deJure;
			set
			{
				if (value != deJure)
				{
					deJure = value;
					OnPropertyChangedWithValue(value);
				}
			}
		}
	}
}
