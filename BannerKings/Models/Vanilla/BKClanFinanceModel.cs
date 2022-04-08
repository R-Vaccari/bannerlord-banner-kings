using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using TaleWorlds.Localization;
using static BannerKings.Managers.TitleManager;

namespace BannerKings.Models
{
    class BKClanFinanceModel : DefaultClanFinanceModel
    {

        public override ExplainedNumber CalculateClanGoldChange(Clan clan, bool includeDescriptions = false, bool applyWithdrawals = false)
        {
			ExplainedNumber baseResult = base.CalculateClanGoldChange(clan, includeDescriptions, applyWithdrawals);
			if (BannerKingsConfig.Instance.TitleManager != null)
			{
				this.CalculateClanExpenseInternal(clan, ref baseResult);
				this.CalculateClanIncomeInternal(clan, ref baseResult);	
			}

			return baseResult;
		}

        public override ExplainedNumber CalculateClanIncome(Clan clan, bool includeDescriptions = false, bool applyWithdrawals = false)
        {
			ExplainedNumber baseResult = base.CalculateClanIncome(clan, includeDescriptions, applyWithdrawals);
			if (BannerKingsConfig.Instance.TitleManager != null) 
				this.CalculateClanIncomeInternal(clan, ref baseResult);

			return baseResult;
        }

		public override ExplainedNumber CalculateClanExpenses(Clan clan, bool includeDescriptions = false, bool applyWithdrawals = false)
		{
			ExplainedNumber baseResult = base.CalculateClanExpenses(clan, includeDescriptions, applyWithdrawals);
			if (BannerKingsConfig.Instance.TitleManager != null)
				this.CalculateClanExpenseInternal(clan, ref baseResult);

			return baseResult;
		}

		public void CalculateClanIncomeInternal(Clan clan, ref ExplainedNumber result)
		{
			Dictionary<Clan, List<FeudalTitle>> dictionary = BannerKingsConfig.Instance.TitleManager.CalculateVassalClanTitles(clan);
			if (dictionary.Count == 0) return;

			foreach (KeyValuePair<Clan, List<FeudalTitle>> pair in dictionary)
            {
				float amount = 0f;
				foreach (FeudalTitle title in pair.Value)
					amount += title.dueTax;
				result.Add(amount, new TextObject("{=!}Taxes from {CLAN}").SetTextVariable("CLAN", pair.Key.Name));
            }
		}

		public void CalculateClanExpenseInternal(Clan clan, ref ExplainedNumber result)
        {
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
