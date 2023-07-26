using BannerKings.Managers.Court.Grace;
using System;
using System.Linq;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Core.ViewModelCollection.Selector;
using TaleWorlds.Localization;

namespace BannerKings.UI.Court
{
    public class CourtExpenseSelectorItemVM : SelectorItemVM
    {
        public CourtExpenseSelectorItemVM(CourtExpense expense, bool isAvailable) : base("")
        {
            Expense = expense;
            StringItem = expense.Name.ToString();
            CanBeSelected = isAvailable;
            Hint = new HintViewModel(new TextObject("{=G2jQF2Ne}{DESCRIPTION}\n\nGrace: {GRACE}\nAdm. Costs: {COSTS}\nGoods requirements:\n{GOODS}")
                .SetTextVariable("DESCRIPTION", expense.Description)
                .SetTextVariable("GRACE", expense.Grace)
                .SetTextVariable("COSTS", (expense.AdministrativeCost * 100f).ToString("0.00") + '%')
                .SetTextVariable("GOODS", string.Join(Environment.NewLine, expense.ItemCategories.Select(x => $"{x.Key}: {x.Value}")))
                );
        }

        public CourtExpense Expense { get; private set; }
    }
}
