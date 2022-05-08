using BannerKings.Managers.Court;
using BannerKings.Managers.Titles;
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
			ExplainedNumber baseResult = base.CalculateClanGoldChange(clan, includeDescriptions, applyWithdrawals);
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
			Dictionary<Clan, List<FeudalTitle>> dictionary = BannerKingsConfig.Instance.TitleManager.CalculateVassalClanTitles(clan);
			if (dictionary.Count >= 0)
            {
				foreach (KeyValuePair<Clan, List<FeudalTitle>> pair in dictionary)
				{
					float amount = 0f;
					foreach (FeudalTitle title in pair.Value)
						amount += title.dueTax;
					result.Add(amount, new TextObject("{=!}Taxes from {CLAN}").SetTextVariable("CLAN", pair.Key.Name));
				}
			}

			if (!clan.IsUnderMercenaryService && clan.Kingdom != null && FactionManager.GetEnemyKingdoms(clan.Kingdom).Count() > 0)
            {
				Kingdom kingdom = clan.Kingdom;
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

			if (BannerKingsConfig.Instance.CourtManager != null)
			{
				CouncilMember position = BannerKingsConfig.Instance.CourtManager.GetHeroPosition(clan.Leader);
				if (position != null)
					result.Add(position.DueWage, new TextObject("{=!}Councillor role"));
			}
		}

		public void CalculateClanExpenseInternal(Clan clan, ref ExplainedNumber result, bool applyWithdrawals)
		{

			CouncilData data = BannerKingsConfig.Instance.CourtManager.GetCouncil(clan);
			if (data != null)
				foreach (CouncilMember position in data.GetOccupiedPositions())
				{
					result.Add(-position.DueWage, new TextObject("{=!}Council wage to {NAME}").SetTextVariable("NAME", position.Member.Name));
					if (applyWithdrawals && !position.Member.IsNoble)
						position.Member.Gold += position.DueWage;
				}

			List<FeudalTitle> titles = BannerKingsConfig.Instance.TitleManager.GetAllDeJure(clan);
			if (titles.Count == 0) return;

			FeudalTitle suzerain = BannerKingsConfig.Instance.TitleManager.CalculateHeroSuzerain(clan.Leader);
			if (suzerain == null) return;

			float amount = 0f;
			foreach (FeudalTitle title in titles)
				amount += title.dueTax;
			result.Add(-amount, new TextObject("{=!}Taxes to {SUZERAIN}").SetTextVariable("SUZERAIN", suzerain.deJure.Name));
		}
	}
}
