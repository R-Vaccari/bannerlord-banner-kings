using BannerKings.Models;
using BannerKings.UI.Items;
using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using static BannerKings.Managers.TitleManager;

namespace BannerKings.UI
{
    public class DemesneVM : ViewModel
    {
		private HeroVM _deJure;
		private bool _isSelected;
		private MBBindingList<VassalTitleVM> _vassals;
		private MBBindingList<InformationElement> _demesneInfo;
		private FeudalTitle _title;
		private FeudalTitle _duchy;
		private (bool, string) _titleUsurpable;
		private (bool, string) _duchyUsurpable;
		private UsurpCosts costs;
		private UsurpCosts duchyCosts;
		private UsurpationModel model;
		private bool _contractEnabled;
		private bool _usurpEnabled;
		private bool _usurpDuchyEnabled;

		public DemesneVM(FeudalTitle title, bool isSelected)
        {
			this._title = title;
			this._deJure = new HeroVM(title.deJure, false);
			this._isSelected = isSelected;
			this._vassals = new MBBindingList<VassalTitleVM>();
			this._demesneInfo = new MBBindingList<InformationElement>();
			this.model = new UsurpationModel();
			this.costs = model.GetUsurpationCosts(_title, Hero.MainHero);
			this._duchy = BannerKingsConfig.Instance.TitleManager.GetDuchy(_title);
			this.duchyCosts = model.GetUsurpationCosts(_duchy, Hero.MainHero);
			this._contractEnabled = BannerKingsConfig.Instance.TitleManager.IsHeroTitleHolder(Hero.MainHero) && Clan.PlayerClan.Kingdom != null;
			this._usurpEnabled = _title.deJure != Hero.MainHero;
			this._usurpDuchyEnabled = this._duchy.deJure != Hero.MainHero;
			if (title.vassals != null)
				foreach (FeudalTitle vassal in title.vassals)
					if (vassal.fief != null) _vassals.Add(new VassalTitleVM(vassal));
		}

		public override void RefreshValues()
        {
			base.RefreshValues();
			this._contractEnabled = BannerKingsConfig.Instance.TitleManager.IsHeroTitleHolder(Hero.MainHero) && Clan.PlayerClan.Kingdom != null;
			this._usurpEnabled = _title.deJure != Hero.MainHero;
			this._usurpDuchyEnabled = this._duchy.deJure != Hero.MainHero;
			DemesneInfo.Clear();
			DemesneInfo.Add(new InformationElement("Legitimacy:", new LegitimacyModel().GetRuleType(_title).ToString().Replace('_', ' '), 
				"Your legitimacy to this title and it's vassals. You are lawful when you own this title, and considered a foreigner if your culture differs from it."));
			DemesneInfo.Add(new InformationElement("Sovereign:", _title.sovereign.name.ToString(),
				"The master suzerain of this title, be they a king or emperor type suzerain."));
			DemesneInfo.Add(new InformationElement("Dukedom:", _duchy.name.ToString(),
				"The dukedom this settlement is associated with."));
			_titleUsurpable = model.IsUsurpable(_title, Hero.MainHero);
			_duchyUsurpable = model.IsUsurpable(_duchy, Hero.MainHero);
			costs = model.GetUsurpationCosts(_title, Hero.MainHero);
			duchyCosts = model.GetUsurpationCosts(_duchy, Hero.MainHero);
			DeJure = new HeroVM(_title.deJure, false);
		}

		private void OnUsurpPress()
		{
			bool usurpable = _titleUsurpable.Item1;
			if (usurpable)
				BannerKingsConfig.Instance.TitleManager.UsurpTitle(_title.deJure, Hero.MainHero, _title, costs);
			else InformationManager.DisplayMessage(new InformationMessage(_titleUsurpable.Item2));
			RefreshValues();
		}

		private void OnDuchyUsurpPress()
		{
			bool usurpable = _duchyUsurpable.Item1;
			if (usurpable)
				BannerKingsConfig.Instance.TitleManager.UsurpTitle(_duchy.deJure, Hero.MainHero, _duchy, duchyCosts);
			else InformationManager.DisplayMessage(new InformationMessage(_duchyUsurpable.Item2));
			RefreshValues();
		}

		private void OnSuzerainPress()
        {
			FeudalTitle suzerain = BannerKingsConfig.Instance.TitleManager.CalculateHeroSuzerain(Hero.MainHero);
			if (suzerain != null)
				Campaign.Current.EncyclopediaManager.GoToLink(suzerain.deJure.EncyclopediaLink);
			else InformationManager.DisplayMessage(new InformationMessage("You currently have no suzerain."));
		}

		private void OnVassalsPress()
        {
			List<InquiryElement> list = new List<InquiryElement>();
			HashSet<FeudalTitle> titles = BannerKingsConfig.Instance.TitleManager.GetTitles(Hero.MainHero);
			foreach (FeudalTitle title in titles)
            {
				if (title.vassals != null )
					foreach (FeudalTitle vassal in title.vassals)
					{
						Hero deJure = vassal.deJure;
						if (deJure != Hero.MainHero && (deJure.Clan.Kingdom == Clan.PlayerClan.Kingdom || deJure.Clan == Clan.PlayerClan))
						{
							FeudalTitle deJureSuzerain = BannerKingsConfig.Instance.TitleManager.CalculateHeroSuzerain(deJure);
							if (deJureSuzerain != null && deJureSuzerain.deFacto == Hero.MainHero)
								list.Add(new InquiryElement(deJure, deJure.Name.ToString(),
									new ImageIdentifier(CampaignUIHelper.GetCharacterCode(deJure.CharacterObject, false))
								));
						}
					}	
			}

			if (list.Count > 0)
				InformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData(
					"Your direct vassals", "You are the legal suzerain of these lords. They must fulfill their duties towards you and you uphold their rights.", list, true, 1,
					GameTexts.FindText("str_done", null).ToString(), "", null, null, ""), false);
			else InformationManager.DisplayMessage(new InformationMessage("You currently have no vassals."));
		}

		private void OnContractPress()
        {
			Kingdom kingdom = _title.fief.OwnerClan.Kingdom;
			if (kingdom != null)
				BannerKingsConfig.Instance.TitleManager.ShowContract(kingdom.Leader, "Close");
			else InformationManager.DisplayMessage(new InformationMessage("Unable to open contract: no kingdom associated with this title."));
		}

		[DataSourceProperty]
		public bool IsSelected
		{
			get => this._isSelected;
			set
			{
				if (value != this._isSelected)
				{
					this._isSelected = value;
					if (value) this.RefreshValues();
					base.OnPropertyChangedWithValue(value, "IsSelected");
				}
			}
		}

		[DataSourceProperty]
		public bool UsurpEnabled
		{
			get =>this._usurpEnabled;
			set
			{
				if (value != this._usurpEnabled)
				{
					this._usurpEnabled = value;
					if (value) this.RefreshValues();
					base.OnPropertyChangedWithValue(value, "UsurpEnabled");
				}
			}
		}

		[DataSourceProperty]
		public bool UsurpDuchyEnabled
		{
			get => this._usurpDuchyEnabled;
			set
			{
				if (value != this._usurpDuchyEnabled)
				{
					this._usurpDuchyEnabled = value;
					if (value) this.RefreshValues();
					base.OnPropertyChangedWithValue(value, "UsurpDuchyEnabled");
				}
			}
		}

		[DataSourceProperty]
		public bool ContractEnabled
		{
			get => this._contractEnabled;
			set
			{
				if (value != this._contractEnabled)
				{
					this._contractEnabled = value;
					if (value) this.RefreshValues();
					base.OnPropertyChangedWithValue(value, "ContractEnabled");
				}
			}
		}

		[DataSourceProperty]
		public MBBindingList<InformationElement> DemesneInfo
		{
			get => _demesneInfo;
			set
			{
				if (value != _demesneInfo)
				{
					_demesneInfo = value;
					base.OnPropertyChangedWithValue(value, "DemesneInfo");
				}
			}
		}


		[DataSourceProperty]
		public HeroVM DeJure
		{
			get => this._deJure;
			set
			{
				if (value != this._deJure)
				{
					this._deJure = value;
					base.OnPropertyChangedWithValue(value, "DeJure");
				}
			}
		}

		[DataSourceProperty]
		public MBBindingList<VassalTitleVM> Vassals
		{
			get => this._vassals;
			set
			{
				if (value != this._vassals)
				{
					this._vassals = value;
					base.OnPropertyChanged("Vassals");
				}
			}
		}

		[DataSourceProperty]
		public HintViewModel UsurpHint
		{
			get
			{
				UsurpCosts costs = model.GetUsurpationCosts(_title, Hero.MainHero);
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
		}

		[DataSourceProperty]
		public HintViewModel UsurpDuchyHint
		{
			get
			{
				StringBuilder sb = new StringBuilder("Usurp the duchy associated with this settlement, making you the lawful suzerain of any other lords within the duchy that are not dukes or higher themselves. Usurping a duchy requires the ownership of at least one of the counties within it.");
				sb.Append(Environment.NewLine);
				sb.Append("Costs:");
				sb.Append(Environment.NewLine);
				sb.Append(string.Format("{0} gold.", (int)duchyCosts.gold));
				sb.Append(Environment.NewLine);
				sb.Append(string.Format("{0} influence.", (int)duchyCosts.influence));
				sb.Append(Environment.NewLine);
				sb.Append(string.Format("{0} renown.", (int)duchyCosts.renown));
				return new HintViewModel(new TextObject(sb.ToString()));
			}
		}
	}
}
