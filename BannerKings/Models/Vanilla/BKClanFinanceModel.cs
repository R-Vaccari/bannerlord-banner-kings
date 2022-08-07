using BannerKings.Managers.Court;
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
    public class BKClanFinanceModel : DefaultClanFinanceModel
    {

        public override ExplainedNumber CalculateClanGoldChange(Clan clan, bool includeDescriptions = false, bool applyWithdrawals = false)
        {
			ExplainedNumber baseResult = base.CalculateClanGoldChange(clan, true, applyWithdrawals);
			if (BannerKingsConfig.Instance.TitleManager != null)
			{
				CalculateClanExpenseInternal(clan, ref baseResult, applyWithdrawals);
				CalculateClanIncomeInternal(clan, ref baseResult, applyWithdrawals);	
			}

			return baseResult;
		}

        public override ExplainedNumber CalculateClanIncome(Clan clan, bool includeDescriptions = false, bool applyWithdrawals = false)
        {
			ExplainedNumber baseResult = base.CalculateClanIncome(clan, includeDescriptions, applyWithdrawals);
			if (BannerKingsConfig.Instance.TitleManager != null) 
				CalculateClanIncomeInternal(clan, ref baseResult, applyWithdrawals);

			return baseResult;
        }

		public override ExplainedNumber CalculateClanExpenses(Clan clan, bool includeDescriptions = false, bool applyWithdrawals = false)
		{
			ExplainedNumber baseResult = base.CalculateClanExpenses(clan, includeDescriptions, applyWithdrawals);
			if (BannerKingsConfig.Instance.TitleManager != null)
				CalculateClanExpenseInternal(clan, ref baseResult, applyWithdrawals);

			return baseResult;
		}

		public void CalculateClanIncomeInternal(Clan clan, ref ExplainedNumber result, bool applyWithdrawals)
		{

			Kingdom kingdom = clan.Kingdom;
			BKWorkshopModel wkModel = (BKWorkshopModel)Campaign.Current.Models.WorkshopModel;
			foreach (Town town in clan.Fiefs)
            {
				foreach (Workshop wk in town.Workshops)
					if (wk.IsRunning && wk.Owner != clan.Leader && wk.WorkshopType.StringId != "artisans")
						result.Add(base.CalculateOwnerIncomeFromWorkshop(wk) * wkModel.CalculateWorkshopTax(town.Settlement, wk.Owner).ResultNumber, new TextObject("{=!}Taxes from {WORKSHOP} at {TOWN}")
							.SetTextVariable("WORKSHOP", wk.Name)
							.SetTextVariable("TOWN", town.Name));

				if (BannerKingsConfig.Instance.AI.AcceptNotableAid(clan, BannerKingsConfig.Instance.PopulationManager.GetPopData(town.Settlement)))
					foreach (Hero notable in town.Settlement.Notables)
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
						amount += (int)title.dueTax;
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
					object[] array = { clan, new ExplainedNumber(), applyWithdrawals };
					expense.Invoke(model, array);
					float payment = ((ExplainedNumber)array[1]).ResultNumber * -1f;
					float limit = clan.Tier * 1000f;
					if (payment > limit)
						payment = limit;
					if (kingdom.KingdomBudgetWallet >= payment)
					{
						if (applyWithdrawals) kingdom.KingdomBudgetWallet -= (int)payment;
						result.Add(payment, new TextObject("{=!}Army compensation rights"));
					}
				}
			}

			CouncilMember position = BannerKingsConfig.Instance.CourtManager.GetHeroPosition(clan.Leader);
			if (position != null)
				result.Add(position.DueWage, new TextObject("{=!}Councillor role"));
		}

		public void CalculateClanExpenseInternal(Clan clan, ref ExplainedNumber result, bool applyWithdrawals)
        {
			BKWorkshopModel wkModel = (BKWorkshopModel)Campaign.Current.Models.WorkshopModel;
			foreach (Workshop wk in clan.Leader.OwnedWorkshops)
				if (wk.IsRunning && wk.Settlement.OwnerClan != clan)
                {
					float tax = wkModel.CalculateWorkshopTax(wk.Settlement, clan.Leader).ResultNumber;
					result.Add(base.CalculateOwnerIncomeFromWorkshop(wk) * -tax, new TextObject("{=!}{WORKSHOP} taxes to {CLAN}")
						.SetTextVariable("WORKSHOP", wk.Name)
						.SetTextVariable("CLAN", wk.Settlement.OwnerClan.Name));
				}

			CouncilData data = BannerKingsConfig.Instance.CourtManager.GetCouncil(clan);
			if (data != null)
				foreach (CouncilMember position in data.GetOccupiedPositions())
				{
					result.Add(-position.DueWage, new TextObject("{=!}Council wage to {NAME}").SetTextVariable("NAME", position.Member.Name));
					if (applyWithdrawals && !position.Member.IsNoble)
						position.Member.Gold += position.DueWage;
				}


			FeudalTitle suzerain = BannerKingsConfig.Instance.TitleManager.CalculateHeroSuzerain(clan.Leader);
			if (suzerain == null) return;

			Kingdom deJureKingdom = null;
			if (suzerain.deJure.Clan != null)
				deJureKingdom = suzerain.deJure.Clan.Kingdom;

			if (deJureKingdom == null || deJureKingdom != clan.Kingdom) return;

			float amount = 0f;
			Dictionary<Clan, List<FeudalTitle>> dictionary = BannerKingsConfig.Instance.TitleManager.CalculateVassalClanTitles(suzerain.deJure.Clan, clan);
			foreach (FeudalTitle title in dictionary[clan])
				amount += (int)title.dueTax;

			result.Add(-amount, new TextObject("{=!}Taxes to {SUZERAIN}").SetTextVariable("SUZERAIN", suzerain.deJure.Name));
		}
    }
}
