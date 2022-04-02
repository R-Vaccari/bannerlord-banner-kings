using BannerKings.Managers.Kingdoms.Contract;
using BannerKings.Managers.Policies;
using BannerKings.Models;
using BannerKings.Populations;
using BannerKings.UI.Items;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using static BannerKings.Managers.TitleManager;

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
				this.deJure = new HeroVM(title.deJure, false);
				this.duchy = BannerKingsConfig.Instance.TitleManager.GetDuchy(this.title);
			}

			this.demesneInfo = new MBBindingList<InformationElement>();
			this.governmentInfo = new MBBindingList<InformationElement>();
			this.landInfo = new MBBindingList<InformationElement>();
			this.terrainInfo = new MBBindingList<InformationElement>();
			this.workforceInfo = new MBBindingList<InformationElement>();
			
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
			LandData landData = base.data.LandData;
			DemesneInfo.Clear();
			LandInfo.Clear();
			TerrainInfo.Clear();
			WorkforceInfo.Clear();
			GovernmentInfo.Clear();

			if (title != null)
            {
				float legitimacyType = new BKLegitimacyModel().CalculateEffect(base.data.Settlement).ResultNumber;
				if (legitimacyType > 0f)
				{
					LegitimacyType legitimacy = (LegitimacyType)legitimacyType;
					DemesneInfo.Add(new InformationElement("Legitimacy:", legitimacy.ToString().Replace('_', ' '),
						"Your legitimacy to this title and it's vassals. You are lawful when you own this title, and considered a foreigner if your culture differs from it."));
				}

				if (title.sovereign != null) DemesneInfo.Add(new InformationElement("Sovereign:", title.sovereign.FullName.ToString(),
					"The master suzerain of this title, be they a king or emperor type suzerain."));
				if (duchy != null) DemesneInfo.Add(new InformationElement("Dukedom:", duchy.FullName.ToString(),
					"The dukedom this settlement is associated with."));

				GovernmentInfo.Add(new InformationElement("Government Type:", title.contract.Government.ToString(),
					"The dukedom this settlement is associated with."));
				GovernmentInfo.Add(new InformationElement("Succession Type:", title.contract.Succession.ToString().Replace("_", " "),
					"The dukedom this settlement is associated with."));
				GovernmentInfo.Add(new InformationElement("Inheritance Type:", title.contract.Inheritance.ToString(),
					"The dukedom this settlement is associated with."));
				GovernmentInfo.Add(new InformationElement("Gender Law:", title.contract.GenderLaw.ToString(),
					"The dukedom this settlement is associated with."));

				DeJure = new HeroVM(title.deJure, false);
			}
			

			LandInfo.Add(new InformationElement("Acreage:", landData.Acreage.ToString() + " acres",
				"Current quantity of usable acres in this region"));
			LandInfo.Add(new InformationElement("Farmland:", landData.Farmland.ToString() + " acres",
				"Acres in this region used as farmland, the main source of food in most places"));
			LandInfo.Add(new InformationElement("Pastureland:", landData.Pastureland.ToString() + " acres",
				"Acres in this region used as pastureland, to raise cattle and other animals. These output meat and animal products such as butter and cheese"));
			LandInfo.Add(new InformationElement("Woodland:", landData.Woodland.ToString() + " acres",
				"Acres in this region used as woodland, kept for hunting, foraging of berries and materials like wood"));

			TerrainInfo.Add(new InformationElement("Type:", landData.Terrain.ToString(),
				"The local terrain type. Dictates fertility and terrain difficulty."));
			TerrainInfo.Add(new InformationElement("Fertility:", base.FormatValue(landData.Fertility),
				"How fertile the region is. This depends solely on the local terrain type - harsher environments like deserts are less fertile than plains and grassy hills"));
			TerrainInfo.Add(new InformationElement("Terrain Difficulty:", base.FormatValue(landData.Difficulty),
				"Represents how difficult it is to create new usable acres. Like fertility, depends on terrain, but is not strictly correlated to it"));

			WorkforceInfo.Add(new InformationElement("Available Workforce:", landData.AvailableWorkForce.ToString(),
				"The amount of productive workers in this region, able to work the land"));
			WorkforceInfo.Add(new InformationElement("Workforce Saturation:", base.FormatValue(landData.WorkforceSaturation),
				"Represents how many workers there are in correlation to the amount needed to fully utilize the acreage. Saturation over 100% indicates more workers than the land needs, while under 100% means not all acres are producing output"));

			if (base.HasTown)
			{
				workforceItem = (BKWorkforcePolicy)BannerKingsConfig.Instance.PolicyManager.GetPolicy(data.Settlement, "workforce");
				WorkforceSelector = base.GetSelector(workforceItem, new Action<SelectorVM<BKItemVM>>(this.workforceItem.OnChange));
				WorkforceSelector.SelectedIndex = workforceItem.Selected;
				WorkforceSelector.SetOnChangeAction(this.workforceItem.OnChange);
			}
		}

		[DataSourceProperty]
		public SelectorVM<BKItemVM> WorkforceSelector
		{
			get => this.workforceVM;
			set
			{
				if (value != this.workforceVM)
				{
					this.workforceVM = value;
					base.OnPropertyChangedWithValue(value, "WorkforceSelector");
				}
			}
		}

		
		private void OnUsurpPress()
		{
			if (title != null)
            {
				Kingdom kingdom = this.data.Settlement.OwnerClan.Kingdom;
				if (kingdom != null)
					kingdom.AddDecision(new BKGovernmentDecision(this.data.Settlement.OwnerClan, GovernmentType.Imperial, this.title));
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
					base.OnPropertyChangedWithValue(value, "LandInfo");
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
					base.OnPropertyChangedWithValue(value, "TerrainInfo");
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
					base.OnPropertyChangedWithValue(value, "WorkforceInfo");
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
					base.OnPropertyChangedWithValue(value, "GovernmentInfo");
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
					base.OnPropertyChangedWithValue(value, "DemesneInfo");
				}
			}
		}


		[DataSourceProperty]
		public HeroVM DeJure
		{
			get => this.deJure;
			set
			{
				if (value != this.deJure)
				{
					this.deJure = value;
					base.OnPropertyChangedWithValue(value, "DeJure");
				}
			}
		}
	}
}
