using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using static BannerKings.Managers.TitleManager;

namespace BannerKings.Models
{
    class FeudalClanFinanceModel : DefaultClanFinanceModel
    {

        public override ExplainedNumber CalculateClanGoldChange(Clan clan, bool includeDescriptions = false, bool applyWithdrawals = false)
        {
			if (BannerKingsConfig.Instance.TitleManager != null)
			{
				ExplainedNumber result = new ExplainedNumber(0f, true, null);
				this.CalculateClanIncomeInternal(clan, ref result, applyWithdrawals);
				this.CalculateClanExpensesInternal(clan, ref result, applyWithdrawals);
				return result;
			} else 
				return base.CalculateClanGoldChange(clan, includeDescriptions, applyWithdrawals);
        }

        public override ExplainedNumber CalculateClanIncome(Clan clan, bool includeDescriptions = false, bool applyWithdrawals = false)
        {
			if (BannerKingsConfig.Instance.TitleManager != null) {

				ExplainedNumber result = new ExplainedNumber(0f, includeDescriptions, null);
				this.CalculateClanIncomeInternal(clan, ref result, applyWithdrawals);
				return result;
			} else return base.CalculateClanIncome(clan, includeDescriptions, applyWithdrawals);
        }

		private void CalculateClanIncomeInternal(Clan clan, ref ExplainedNumber goldChange, bool applyWithdrawals = false)
        {
			if (clan.IsEliminated)
				return;

			Kingdom kingdom = clan.Kingdom;
			if (((kingdom != null) ? kingdom.RulingClan : null) == clan)
			{
				this.AddRulingClanIncome(clan, ref goldChange, applyWithdrawals);
			}
			if (clan != Clan.PlayerClan && (!clan.MapFaction.IsKingdomFaction || clan.IsUnderMercenaryService) && clan.Fiefs.Count<Town>() == 0)
			{
				int num = clan.Tier * (80 + (clan.IsUnderMercenaryService ? 40 : 0));
				goldChange.Add((float)num, null, null);
			}
			this.AddMercenaryIncome(clan, ref goldChange, applyWithdrawals);
			this.AddVillagesIncome(clan, ref goldChange, applyWithdrawals);
			this.AddTownTaxes(clan, ref goldChange, applyWithdrawals);
			this.CalculateHeroIncomeFromWorkshops(clan.Leader, ref goldChange, applyWithdrawals);
			this.AddIncomeFromParties(clan, ref goldChange, applyWithdrawals);
			this.AddIncomeFromTownProjects(clan, ref goldChange, applyWithdrawals);
			if (!clan.IsUnderMercenaryService)
				this.AddIncomeFromTribute(clan, ref goldChange, applyWithdrawals);
			
			if (clan.Gold < 30000 && clan.Kingdom != null && clan.Leader != Hero.MainHero && !clan.IsUnderMercenaryService)
			{
				this.AddIncomeFromKingdomBudget(clan, ref goldChange, applyWithdrawals);
			}
			Hero leader = clan.Leader;
			if (leader != null && leader.GetPerkValue(DefaultPerks.Trade.SpringOfGold))
			{
				int num2 = MathF.Min(1000, MathF.Round((float)clan.Leader.Gold * DefaultPerks.Trade.SpringOfGold.PrimaryBonus));
				goldChange.Add((float)num2, DefaultPerks.Trade.SpringOfGold.Name, null);
			}
		}


		public override int CalculateNotableDailyGoldChange(Hero hero, bool applyWithdrawals)
        {
            return base.CalculateNotableDailyGoldChange(hero, applyWithdrawals);
        }

        public override int CalculateOwnerExpenseFromWorkshop(Workshop workshop)
        {
            return base.CalculateOwnerExpenseFromWorkshop(workshop);
        }

        public override int CalculateOwnerIncomeFromWorkshop(Workshop workshop)
        {
            return base.CalculateOwnerIncomeFromWorkshop(workshop);
        }

        public override int CalculateOwnerIncomeFromCaravan(MobileParty caravan)
        {
            return base.CalculateOwnerIncomeFromCaravan(caravan);
        }

		private void AddTownTaxes(Clan clan, ref ExplainedNumber goldChange, bool applyWithdrawals)
        {
			object[] array = new object[] { clan, goldChange, applyWithdrawals };
			DefaultClanFinanceModel model = new DefaultClanFinanceModel();
			MethodInfo baseMethod = model.GetType().GetMethod("AddTownTaxes", BindingFlags.NonPublic | BindingFlags.Instance);
			baseMethod.Invoke(model, array);
			goldChange = (ExplainedNumber)array[1];
		}

		private void AddIncomeFromKingdomBudget(Clan clan, ref ExplainedNumber goldChange, bool applyWithdrawals)
        {
			object[] array = new object[] { clan, goldChange, applyWithdrawals };
			DefaultClanFinanceModel model = new DefaultClanFinanceModel();
			MethodInfo baseMethod = model.GetType().GetMethod("AddIncomeFromKingdomBudget", BindingFlags.NonPublic | BindingFlags.Instance);
			baseMethod.Invoke(model, array);
			goldChange = (ExplainedNumber)array[1];
		}

		private void AddIncomeFromTribute(Clan clan, ref ExplainedNumber goldChange, bool applyWithdrawals)
        {
			object[] array = new object[] { clan, goldChange, applyWithdrawals };
			DefaultClanFinanceModel model = new DefaultClanFinanceModel();
			MethodInfo baseMethod = model.GetType().GetMethod("AddIncomeFromTribute", BindingFlags.NonPublic | BindingFlags.Instance);
			baseMethod.Invoke(model, array);
			goldChange = (ExplainedNumber)array[1];
		}

		private void AddIncomeFromTownProjects(Clan clan, ref ExplainedNumber goldChange, bool applyWithdrawals)
        {
			object[] array = new object[] { clan, goldChange, applyWithdrawals };
			DefaultClanFinanceModel model = new DefaultClanFinanceModel();
			MethodInfo baseMethod = model.GetType().GetMethod("AddIncomeFromTownProjects", BindingFlags.NonPublic | BindingFlags.Instance);
			baseMethod.Invoke(model, array);
			goldChange = (ExplainedNumber)array[1];
		}

		private void AddIncomeFromParties(Clan clan, ref ExplainedNumber goldChange, bool applyWithdrawals = false)
        {
			object[] array = new object[] { clan, goldChange, applyWithdrawals };
			DefaultClanFinanceModel model = new DefaultClanFinanceModel();
			MethodInfo baseMethod = model.GetType().GetMethod("AddIncomeFromParties", BindingFlags.NonPublic | BindingFlags.Instance);
			baseMethod.Invoke(model, array);
			goldChange = (ExplainedNumber)array[1];
		}

		private void CalculateHeroIncomeFromWorkshops(Hero hero, ref ExplainedNumber goldChange, bool applyWithdrawals)
        {
			object[] array = new object[] { hero, goldChange, applyWithdrawals };
			DefaultClanFinanceModel model = new DefaultClanFinanceModel();
			MethodInfo baseMethod = model.GetType().GetMethod("CalculateHeroIncomeFromWorkshops", BindingFlags.NonPublic | BindingFlags.Instance);
			baseMethod.Invoke(model, array);
			goldChange = (ExplainedNumber)array[1];
		}

		private void AddMercenaryIncome(Clan clan, ref ExplainedNumber goldChange, bool applyWithdrawals)
        {
			object[] array = new object[] { clan, goldChange, applyWithdrawals };
			DefaultClanFinanceModel model = new DefaultClanFinanceModel();
			MethodInfo baseMethod = model.GetType().GetMethod("AddMercenaryIncome", BindingFlags.NonPublic | BindingFlags.Instance);
			baseMethod.Invoke(model, array);
			goldChange = (ExplainedNumber)array[1];
		}

		private void AddRulingClanIncome(Clan clan, ref ExplainedNumber goldChange, bool applyWithdrawals)
		{
			object[] array = new object[] { clan, goldChange, applyWithdrawals };
			DefaultClanFinanceModel model = new DefaultClanFinanceModel();
			MethodInfo baseMethod = model.GetType().GetMethod("AddRulingClanIncome", BindingFlags.NonPublic | BindingFlags.Instance);
			baseMethod.Invoke(model, array);
			goldChange = (ExplainedNumber)array[1];
		}

		private void AddVillagesIncome(Clan clan, ref ExplainedNumber goldChange, bool applyWithdrawals)
		{
			foreach (Village village3 in clan.Villages)
			{
				bool leaderOwned = true;
				Hero owner = null;
				FeudalTitle title = BannerKingsConfig.Instance.TitleManager.GetTitle(village3.Settlement);
				if (title != null)
                {
					owner = title.deFacto;
					if (title.deFacto != clan.Leader)
						leaderOwned = false;
				}

				int num = (village3.VillageState == Village.VillageStates.Looted || village3.VillageState == Village.VillageStates.BeingRaided) ? 0 : ((int)((float)village3.TradeTaxAccumulated / this.RevenueSmoothenFraction()));
				int num2 = num;
				if (clan.Kingdom != null && clan.Kingdom.RulingClan != clan && clan.Kingdom.ActivePolicies.Contains(DefaultPolicies.LandTax))
                {
					if (leaderOwned) goldChange.Add((float)(-(float)num) * 0.05f, DefaultPolicies.LandTax.Name, null);
					else owner.Gold += (int)((-(float)num) * 0.05f);
				}
					
				
				if (village3.Bound.Town != null && village3.Bound.Town.Governor != null && village3.Bound.Town.Governor.GetPerkValue(DefaultPerks.Scouting.ForestKin))
					num += MathF.Round((float)num * DefaultPerks.Scouting.ForestKin.SecondaryBonus * 0.01f);
				
				Settlement bound = village3.Bound;
				bool flag;
				if (bound == null)
					flag = (null != null);
				
				else
				{
					Town town = bound.Town;
					flag = (((town != null) ? town.Governor : null) != null);
				}
				if (flag && village3.Bound.Town.Governor.GetPerkValue(DefaultPerks.Steward.Logistician))
					num += MathF.Round((float)num * DefaultPerks.Steward.Logistician.SecondaryBonus * 0.01f);
				
				if (applyWithdrawals)
					village3.TradeTaxAccumulated -= num2;

				if (leaderOwned) goldChange.Add((float)num, _villageIncomeStr, village3.Name);
				else owner.Gold += num;
			}
			if (clan.Kingdom != null && clan.Kingdom.RulingClan == clan && clan.Kingdom.ActivePolicies.Contains(DefaultPolicies.LandTax))
			{
				float num3 = 0f;
				IEnumerable<Village> villages = clan.Kingdom.Villages;
				foreach (Village village2 in villages.Where((Village village) => !village.IsOwnerUnassigned && village.Settlement.OwnerClan != clan))
				{
					bool leaderOwned = true;
					Hero owner = null;
					FeudalTitle title = BannerKingsConfig.Instance.TitleManager.GetTitle(village2.Settlement);
					if (title != null)
					{
						owner = title.deJure;
						if (title.deJure != clan.Leader)
							leaderOwned = false;
					}

					int num4 = (village2.VillageState == Village.VillageStates.Looted || village2.VillageState == Village.VillageStates.BeingRaided) ? 0 : ((int)((float)village2.TradeTaxAccumulated / this.RevenueSmoothenFraction()));
					if (leaderOwned) num3 += (float)num4 * 0.05f;
					else owner.Gold += (int)((float)num4 * 0.05f);
				}
				goldChange.Add(num3, DefaultPolicies.LandTax.Name, null);
			}
		}


		public override ExplainedNumber CalculateClanExpenses(Clan clan, bool includeDescriptions = false, bool applyWithdrawals = false)
		{
			if (BannerKingsConfig.Instance.TitleManager != null)
			{
				ExplainedNumber result = new ExplainedNumber(0f, includeDescriptions, null);
				this.CalculateClanExpensesInternal(clan, ref result, applyWithdrawals);
				return result;
			}
			else return base.CalculateClanExpenses(clan, includeDescriptions, applyWithdrawals);
		}


		public void CalculateClanExpensesInternal(Clan clan, ref ExplainedNumber goldChange, bool applyWithdrawals = false)
		{
			this.AddExpenseFromLeaderParty(clan, ref goldChange, applyWithdrawals);
			this.AddExpensesFromParties(clan, ref goldChange, applyWithdrawals);
			this.AddExpensesFromGarrisons(clan, ref goldChange, applyWithdrawals);
			if (!clan.IsUnderMercenaryService)
			{
				this.AddExpensesForHiredMercenaries(clan, ref goldChange, applyWithdrawals);
				this.AddExpensesForTributes(clan, ref goldChange, applyWithdrawals);
			}
			this.AddExpensesForAutoRecruitment(clan, ref goldChange, applyWithdrawals);
			if (clan.Gold > 100000 && clan.Kingdom != null && clan.Leader != Hero.MainHero && !clan.IsUnderMercenaryService)
			{
				int num = (int)(((float)clan.Gold - 100000f) * 0.01f);
				if (applyWithdrawals)
					clan.Kingdom.KingdomBudgetWallet += num;
				
				goldChange.Add((float)(-(float)num), _kingdomBudgetStr, null);
			}
			if (clan.DebtToKingdom > 0)
				this.AddPaymentForDebts(clan, ref goldChange, applyWithdrawals);

			if (BannerKingsConfig.Instance.TitleManager.IsHeroTitleHolder(clan.Leader))
				this.AddExpenseFromTaxes(clan, ref goldChange, applyWithdrawals);
		}

		private void AddExpenseFromTaxes(Clan clan, ref ExplainedNumber goldChange, bool applyWithdrawals)
		{
			HashSet<FeudalTitle> titles = BannerKingsConfig.Instance.TitleManager.GetTitles(clan.Leader);
			FeudalTitle suzerain = BannerKingsConfig.Instance.TitleManager.CalculateHeroSuzerain(clan.Leader);

			if (titles != null && titles.Count > 0 && suzerain != null)
				foreach (FeudalTitle title in titles)
					if (title.dueTax > 0)
						goldChange.Add(-title.dueTax, new TextObject("Taxes due to suzerain {0}"), new TextObject(title.deJure.Name.ToString()));
		}

		private void AddExpenseFromLeaderParty(Clan clan, ref ExplainedNumber goldChange, bool applyWithdrawals)
		{
			object[] array = new object[] { clan, goldChange, applyWithdrawals };
			DefaultClanFinanceModel model = new DefaultClanFinanceModel();
			MethodInfo baseMethod = model.GetType().GetMethod("AddExpenseFromLeaderParty", BindingFlags.NonPublic | BindingFlags.Instance);
			baseMethod.Invoke(model, array);
			goldChange = (ExplainedNumber)array[1];
		}

		private void AddExpensesFromGarrisons(Clan clan, ref ExplainedNumber goldChange, bool applyWithdrawals)
		{
			object[] array = new object[] { clan, goldChange, applyWithdrawals };
			DefaultClanFinanceModel model = new DefaultClanFinanceModel();
			MethodInfo baseMethod = model.GetType().GetMethod("AddExpensesFromGarrisons", BindingFlags.NonPublic | BindingFlags.Instance);
			baseMethod.Invoke(model, array);
			goldChange = (ExplainedNumber)array[1];
		}

		private void AddExpensesForHiredMercenaries(Clan clan, ref ExplainedNumber goldChange, bool applyWithdrawals)
		{
			object[] array = new object[] { clan, goldChange, applyWithdrawals };
			DefaultClanFinanceModel model = new DefaultClanFinanceModel();
			MethodInfo baseMethod = model.GetType().GetMethod("AddExpensesForHiredMercenaries", BindingFlags.NonPublic | BindingFlags.Instance);
			baseMethod.Invoke(model, array);
			goldChange = (ExplainedNumber)array[1];
		}

		private void AddExpensesForAutoRecruitment(Clan clan, ref ExplainedNumber goldChange, bool applyWithdrawals)
		{
			object[] array = new object[] { clan, goldChange, applyWithdrawals };
			DefaultClanFinanceModel model = new DefaultClanFinanceModel();
			MethodInfo baseMethod = model.GetType().GetMethod("AddExpensesForAutoRecruitment", BindingFlags.NonPublic | BindingFlags.Instance);
			baseMethod.Invoke(model, array);
			goldChange = (ExplainedNumber)array[1];
		}

		private void AddExpensesForTributes(Clan clan, ref ExplainedNumber goldChange, bool applyWithdrawals)
		{
			object[] array = new object[] { clan, goldChange, applyWithdrawals };
			DefaultClanFinanceModel model = new DefaultClanFinanceModel();
			MethodInfo baseMethod = model.GetType().GetMethod("AddExpensesForTributes", BindingFlags.NonPublic | BindingFlags.Instance);
			baseMethod.Invoke(model, array);
			goldChange = (ExplainedNumber)array[1];
		}

		private void AddPaymentForDebts(Clan clan, ref ExplainedNumber goldChange, bool applyWithdrawals)
		{
			object[] array = new object[] { clan, goldChange, applyWithdrawals };
			DefaultClanFinanceModel model = new DefaultClanFinanceModel();
			MethodInfo baseMethod = model.GetType().GetMethod("AddPaymentForDebts", BindingFlags.NonPublic | BindingFlags.Instance);
			baseMethod.Invoke(model, array);
			goldChange = (ExplainedNumber)array[1];
		}

		private int CalculatePartyWage(MobileParty mobileParty, int budget, bool applyWithdrawals)
		{
			object[] array = new object[] { mobileParty, budget, applyWithdrawals };
			DefaultClanFinanceModel model = new DefaultClanFinanceModel();
			MethodInfo baseMethod = model.GetType().GetMethod("CalculatePartyWage", BindingFlags.NonPublic | BindingFlags.Instance);
			return (int)baseMethod.Invoke(model, array);
		}

		private void AddPartyExpense(MobileParty party, Clan clan, ref ExplainedNumber goldChange, bool applyWithdrawals)
		{
			int num = clan.Gold + (int)goldChange.ResultNumber;
			int num2 = num;
			if (num < (party.IsGarrison ? 8000 : 4000) && applyWithdrawals && clan != Clan.PlayerClan)
			{
				num2 = ((party.LeaderHero != null && party.LeaderHero.Gold < 500) ? MathF.Min(num, 250) : 0);
			}
			int num3 = this.CalculatePartyWage(party, num2, applyWithdrawals);
			int num4 = (party.IsLordParty && party.LeaderHero != null) ? party.LeaderHero.Gold : party.PartyTradeGold;
			if (applyWithdrawals)
			{
				if (party.IsLordParty && party.LeaderHero != null)
				{
					party.LeaderHero.Gold -= num3;
				}
				else
				{
					party.PartyTradeGold -= num3;
				}
			}
			num4 -= num3;
			if (num4 < 5000)
			{
				int num5 = 5000 - num4;
				if (applyWithdrawals)
				{
					num5 = MathF.Min(num5, num2);
					if (party.IsLordParty && party.LeaderHero != null)
					{
						party.LeaderHero.Gold += num5;
					}
					else
					{
						party.PartyTradeGold += num5;
					}
				}
				goldChange.Add((float)(-(float)num5), _partyExpensesStr, party.Name);
			}

			/*
			if (PopulationConfig.Instance.TitleManager.IsHeroTitleHolder(clan.Leader))
			{
				FeudalTitle suzerain = PopulationConfig.Instance.TitleManager.CalculateHeroSuzerain(clan.Leader);
				if (suzerain != null)
				{
					FeudalContract contract = suzerain.contract;

				}

			} */
		}


		
		private void AddExpensesFromParties(Clan clan, ref ExplainedNumber goldChange, bool applyWithdrawals = false)
		{
			object[] array = new object[] { clan, goldChange, applyWithdrawals };
			DefaultClanFinanceModel model = new DefaultClanFinanceModel();
			MethodInfo baseMethod = model.GetType().GetMethod("AddExpensesFromParties", BindingFlags.NonPublic | BindingFlags.Instance);
			baseMethod.Invoke(model, array);
			goldChange = (ExplainedNumber)array[1];
		}

		private static readonly TextObject _villageIncomeStr = new TextObject("{=!}{A0}", null);

		private static readonly TextObject _partyExpensesStr = new TextObject("{=iPDOLbi3}Party wages {A0}", null);

		private static readonly TextObject _kingdomBudgetStr = new TextObject("{=7uzvI8e8}Kingdom Budget Expenses", null);
	}
}
