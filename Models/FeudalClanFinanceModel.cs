using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment.Managers;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using static Populations.Managers.TitleManager;

namespace Populations.Models
{
    class FeudalClanFinanceModel : DefaultClanFinanceModel
    {

        public override ExplainedNumber CalculateClanExpenses(Clan clan, bool includeDescriptions = false, bool applyWithdrawals = false)
        {
            return base.CalculateClanExpenses(clan, includeDescriptions, applyWithdrawals);
        }

        public override ExplainedNumber CalculateClanGoldChange(Clan clan, bool includeDescriptions = false, bool applyWithdrawals = false)
        {
			if (PopulationConfig.Instance.TitleManager != null)
			{
				ExplainedNumber result = new ExplainedNumber(0f, includeDescriptions, null);
				this.CalculateClanIncomeInternal(clan, ref result, applyWithdrawals);
				return result;
			} else 
				return base.CalculateClanGoldChange(clan, includeDescriptions, applyWithdrawals);
        }

        public override ExplainedNumber CalculateClanIncome(Clan clan, bool includeDescriptions = false, bool applyWithdrawals = false)
        {
			if (PopulationConfig.Instance.TitleManager != null) {

				ExplainedNumber result = new ExplainedNumber(0f, includeDescriptions, null);
				this.CalculateClanIncomeInternal(clan, ref result, applyWithdrawals);
				return result;
			} else return base.CalculateClanIncome(clan, includeDescriptions, applyWithdrawals);
        }

		private void CalculateClanIncomeInternal(Clan clan, ref ExplainedNumber goldChange, bool applyWithdrawals = false)
        {
			if (clan.IsEliminated)
			{
				return;
			}
			if (clan != Clan.PlayerClan && (!clan.MapFaction.IsKingdomFaction || clan.IsUnderMercenaryService) && clan.Fiefs.Count<Town>() == 0)
			{
				int num = clan.Tier * (80 + (clan.IsUnderMercenaryService ? 40 : 0));
				goldChange.Add((float)num, null, null);
			}
			this.AddVillagesIncome(clan, ref goldChange, applyWithdrawals);
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

		private void AddVillagesIncome(Clan clan, ref ExplainedNumber goldChange, bool applyWithdrawals)
		{
			foreach (Village village3 in clan.Villages)
			{
				bool leaderOwned = true;
				Hero owner = null;
				FeudalTitle title = PopulationConfig.Instance.TitleManager.GetTitle(village3.Settlement);
				if (title != null)
                {
					owner = title.deJure;
					if (title.deJure != clan.Leader)
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
					FeudalTitle title = PopulationConfig.Instance.TitleManager.GetTitle(village2.Settlement);
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

		private static readonly TextObject _villageIncomeStr = new TextObject("{=!}{A0}", null);
	}
}
