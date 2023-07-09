using BannerKings.UI.Court;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Library;
using static BannerKings.Managers.Court.Grace.CourtExpense;

namespace BannerKings.Managers.Court.Grace
{
    public class CourtGrace
    {
        private CouncilData data;
        private float grace;
        private List<CourtExpense> expenses;

        public CourtGrace(CouncilData data)
        {
            this.data = data;
            expenses = new List<CourtExpense>(5);
            expenses.Add(DefaultCourtExpenses.Instance.MinimalExtravagance);
            expenses.Add(DefaultCourtExpenses.Instance.MinimalLodgings);
            expenses.Add(DefaultCourtExpenses.Instance.MinimalSecurity);
            expenses.Add(DefaultCourtExpenses.Instance.MinimalServants);
            expenses.Add(DefaultCourtExpenses.Instance.MinimalSupplies);
        }

        public void PostInitialize()
        {
            expenses.ForEach(expense => expense.PostInitialize());
        }

        public float Grace => grace;
        public List<CourtExpense> Expenses => expenses;
        public ExplainedNumber GraceTarget => BannerKingsConfig.Instance.CouncilModel.CalculateGraceTarget(data);
        public ExplainedNumber ExpectedGrace => BannerKingsConfig.Instance.CouncilModel.CalculateExpectedGrace(data);

        public float GraceChange
        {
            get
            {
                var target = GraceTarget.ResultNumber;
                float change = target * 0.01f;
                float diff = target - grace;
                if (grace < target) return MathF.Clamp(change, 0f, diff);
                else if (grace > target) return MathF.Clamp(-change, diff, 0f);
                return 0f;
            }
        }
        public void Update()
        {
            grace += GraceChange;
        }

        public void AddExpense(Clan clan, CourtExpense expense)
        {
            var current = expenses.First(x => x.Type == expense.Type);
            if (current != null)
            {
                expenses.Remove(current);
                clan.Leader.ChangeHeroGold(-GetExpenseChangeCost(clan, expense, current));
            }
        
            expenses.Add(expense);
        }

        public CourtExpense GetExpense(ExpenseType type) => expenses.FirstOrDefault(x => x.Type == type);

        public int GetExpenseChangeCost(Clan clan, CourtExpense expense, CourtExpense current)
        {
            float diff = expense.AdministrativeCost - current.AdministrativeCost;
            if (diff > 0f)
            {
                float income = BannerKingsConfig.Instance.ClanFinanceModel.CalculateClanIncome(clan).ResultNumber;
                return MBRandom.RoundRandomized(income * diff * 10f);
            }

            return 0;
        }
    }
}
