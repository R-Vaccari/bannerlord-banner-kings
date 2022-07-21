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

				ExplainedNumber currentDemesne = BannerKingsConfig.Instance.StabilityModel.CalculateCurrentDemesne(Hero.MainHero.Clan);
				ExplainedNumber demesneCap = BannerKingsConfig.Instance.StabilityModel.CalculateDemesneLimit(Hero.MainHero);

				GovernmentInfo.Add(new InformationElement(new TextObject("Demesne limit:").ToString(), currentDemesne.ResultNumber + " / " + demesneCap.ResultNumber,
					new TextObject("{TEXT}\nCurrent demesne: {CURRENT}\nLimit:{LIMIT}.")
					.SetTextVariable("TEXT", new TextObject("{=!}Demesne limit describes how many settlements you may own without negative implications. Different settlement types have different weights, villages being the lowest, towns being the highest. Being over the limit reduces stability across all your settlements."))
					.SetTextVariable("CURRENT", currentDemesne.GetExplanations())
					.SetTextVariable("LIMIT", demesneCap.GetExplanations())
					.ToString()));

				ExplainedNumber currentUnlandedDemesne = BannerKingsConfig.Instance.StabilityModel.CalculateCurrentUnlandedDemesne(Hero.MainHero.Clan);
				ExplainedNumber unlandedDemesneCap = BannerKingsConfig.Instance.StabilityModel.CalculateUnlandedDemesneLimit(Hero.MainHero);

				GovernmentInfo.Add(new InformationElement(new TextObject("Demesne limit:").ToString(), currentUnlandedDemesne.ResultNumber + " / " + unlandedDemesneCap.ResultNumber,
					new TextObject("{TEXT}\nCurrent demesne: {CURRENT}\nLimit:{LIMIT}.")
					.SetTextVariable("TEXT", new TextObject("{=!}Unlanded demesne limit describes how many unlanded titles you may own. Unlanded titles are titles such as dukedoms and kingdoms - titles not directly associated with a settlement. Dukedoms have the lowest weight while empires have the biggest. Being over the limit progressively reduces relations with your vassals."))
					.SetTextVariable("CURRENT", currentUnlandedDemesne.GetExplanations())
					.SetTextVariable("LIMIT", unlandedDemesneCap.GetExplanations())
					.ToString()));

				ExplainedNumber currentVassals = BannerKingsConfig.Instance.StabilityModel.CalculateCurrentVassals(Hero.MainHero.Clan);
				ExplainedNumber vassalsCap = BannerKingsConfig.Instance.StabilityModel.CalculateVassalLimit(Hero.MainHero);

				GovernmentInfo.Add(new InformationElement(new TextObject("Vassal limit:").ToString(), currentVassals.ResultNumber + " / " + vassalsCap.ResultNumber,
					new TextObject("{TEXT}\nCurrent vassals:\n{CURRENT}\n \nLimit:\n{LIMIT}.")
					.SetTextVariable("TEXT", new TextObject("{=!}Vassal limit is how many vassals you may have without negative consequences. Vassals are clans whose highest title are under your own (ie, a barony title under your county title, or knight clans with a single lordship) or knights in your clan. Companions and family members do not count. Being over the limit progressively reduces your influence gain."))
					.SetTextVariable("CURRENT", currentVassals.GetExplanations())
					.SetTextVariable("LIMIT", vassalsCap.GetExplanations())
					.ToString()));

				GovernmentInfo.Add(new InformationElement(new TextObject("Government Type:").ToString(), title.contract.Government.ToString(),
					new TextObject("The dukedom this settlement is associated with.").ToString()));
				GovernmentInfo.Add(new InformationElement(new TextObject("Succession Type:").ToString(), title.contract.Succession.ToString().Replace("_", " "),
					new TextObject("The clan succession form associated with this title. Successions only apply to factions.").ToString()));
				GovernmentInfo.Add(new InformationElement(new TextObject("Inheritance Type:").ToString(), title.contract.Inheritance.ToString(),
					new TextObject("The inheritance form associated with this settlement's title. Inheritance dictates who leads the clan after the leader's death.").ToString()));
				GovernmentInfo.Add(new InformationElement(new TextObject("Gender Law:").ToString(), title.contract.GenderLaw.ToString(),
					new TextObject("The gender law associated with this settlement's title. Gender law affects how inheritance and other aspects of rule work.").ToString()));

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
