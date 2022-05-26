using BannerKings.Managers.Titles;
using BannerKings.Populations;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using TaleWorlds.Localization;

namespace BannerKings.Models
{
    class BKClanFinanceModel : DefaultClanFinanceModel
    {

        public override ExplainedNumber CalculateClanGoldChange(Clan clan, bool includeDescriptions = false, bool applyWithdrawals = false)
        {
			ExplainedNumber baseResult = base.CalculateClanGoldChange(clan, true, applyWithdrawals);
			if (BannerKingsConfig.Instance.TitleManager != null)
			{
				this.CalculateClanExpenseInternal(clan, ref baseResult);
				this.CalculateClanIncomeInternal(clan, ref baseResult, applyWithdrawals);	
			}

			return baseResult;
		}

        public override ExplainedNumber CalculateClanIncome(Clan clan, bool includeDescriptions = false, bool applyWithdrawals = false)
        {
			ExplainedNumber baseResult = base.CalculateClanIncome(clan, includeDescriptions, applyWithdrawals);
			if (BannerKingsConfig.Instance.TitleManager != null) 
				this.CalculateClanIncomeInternal(clan, ref baseResult, applyWithdrawals);

			return baseResult;
        }

		public override ExplainedNumber CalculateClanExpenses(Clan clan, bool includeDescriptions = false, bool applyWithdrawals = false)
		{
			ExplainedNumber baseResult = base.CalculateClanExpenses(clan, includeDescriptions, applyWithdrawals);
			if (BannerKingsConfig.Instance.TitleManager != null)
				this.CalculateClanExpenseInternal(clan, ref baseResult);

			return baseResult;
		}

		public void CalculateClanIncomeInternal(Clan clan, ref ExplainedNumber result, bool applyWithdrawals)
		{
			Kingdom kingdom = clan.Kingdom;
			BKWorkshopModel wkModel = (BKWorkshopModel)Campaign.Current.Models.WorkshopModel;
			foreach (Town town in clan.Fiefs)
            {
				float tax = wkModel.CalculateWorkshopTax(town.Settlement).ResultNumber;
				foreach (Workshop wk in town.Workshops)
					if (wk.IsRunning && wk.Owner != clan.Leader)
						result.Add(base.CalculateOwnerIncomeFromWorkshop(wk) * tax, new TextObject("{=!}Taxes from {WORKSHOP} at {TOWN}")
							.SetTextVariable("WORKSHOP", wk.Name)
							.SetTextVariable("TOWN", town.Name));

				PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(town.Settlement);
				if (data.Stability >= 0.5f && data.NotableSupport >= 0.5f && kingdom != null &&
					FactionManager.GetEnemyFactions(kingdom).Count() > 0 && clan.Influence > 50f * clan.Tier)
					foreach (Hero notable in data.Settlement.Notables)
						if (notable.SupporterOf == clan && notable.Gold > 5000)
                        {
							result.Add(200f, new TextObject("{=!}Aid from {NOTABLE}").SetTextVariable("NOTABLE", notable.Name));
							if (applyWithdrawals)
								notable.Gold -= 200;
                        }
			}
				


			Dictionary<Clan, List<FeudalTitle>> dictionary = BannerKingsConfig.Instance.TitleManager.CalculateVassalClanTitles(clan);
			if (dictionary.Count >= 0)
            {
				foreach (KeyValuePair<Clan, List<FeudalTitle>> pair in dictionary)
				{
					if (clan.Kingdom != pair.Key.Kingdom) continue;
					float amount = 0f;
					foreach (FeudalTitle title in pair.Value)
						amount += title.dueTax;
					result.Add(amount, new TextObject("{=!}Taxes from {CLAN}").SetTextVariable("CLAN", pair.Key.Name));
				}
			}

			if (!clan.IsUnderMercenaryService && clan.Kingdom != null && FactionManager.GetEnemyKingdoms(clan.Kingdom).Count() > 0)
            {
				
				FeudalTitle title = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(kingdom);
				if (title != null && title.contract != null && title.contract.Rights.Contains(FeudalRights.Army_Compensation_Rights))
                {
					DefaultClanFinanceModel model = new DefaultClanFinanceModel();
					MethodInfo expense = model.GetType().GetMethod("AddExpenseFromLeaderParty", BindingFlags.Instance | BindingFlags.NonPublic);
					object[] array = new object[] { clan, new ExplainedNumber(), applyWithdrawals };
					expense.Invoke(model, array);
					float payment = ((ExplainedNumber)array[1]).ResultNumber * -1f;
					float limit = (float)clan.Tier * 1000f;
					if (payment > limit)
						payment = limit;
					if (kingdom.KingdomBudgetWallet >= payment)
					{
						if (applyWithdrawals) kingdom.KingdomBudgetWallet -= (int)payment;
						result.Add((float)payment, new TextObject("{=!}Army compensation rights"), null);
					}
				}
			}
		}

		public void CalculateClanExpenseInternal(Clan clan, ref ExplainedNumber result)
        {
			BKWorkshopModel wkModel = (BKWorkshopModel)Campaign.Current.Models.WorkshopModel;
			foreach (Workshop wk in clan.Leader.OwnedWorkshops)
				if (wk.IsRunning && wk.Settlement.OwnerClan != clan)
                {
					float tax = wkModel.CalculateWorkshopTax(wk.Settlement).ResultNumber;
					result.Add(base.CalculateOwnerIncomeFromWorkshop(wk) * -tax, new TextObject("{=!}{WORKSHOP} taxes to {CLAN}")
						.SetTextVariable("WORKSHOP", wk.Name)
						.SetTextVariable("CLAN", wk.Settlement.OwnerClan.Name));
				}
					

			List <FeudalTitle> titles = BannerKingsConfig.Instance.TitleManager.GetAllDeJure(clan);
			if (titles.Count == 0) return;

			FeudalTitle suzerain = BannerKingsConfig.Instance.TitleManager.CalculateHeroSuzerain(clan.Leader);
			if (suzerain == null) return;

			Kingdom deJureKingdom = null;
			if (suzerain.deJure.Clan != null)
				deJureKingdom = suzerain.deJure.Clan.Kingdom;

			if (deJureKingdom == null || deJureKingdom != clan.Kingdom) return;
			float amount = 0f;
			foreach (FeudalTitle title in titles)
				amount += title.dueTax;
			result.Add(-amount, new TextObject("{=!}Taxes to {SUZERAIN}").SetTextVariable("SUZERAIN", suzerain.deJure.Name));
		}
    }
}
