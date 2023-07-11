using BannerKings.Actions;
using BannerKings.UI.Court;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
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
            if (CampaignTime.Now.GetDayOfSeason != 1 || data.Clan.IsUnderMercenaryService) return;

            Dictionary<ItemCategory, int> toBuy = new Dictionary<ItemCategory, int>();
            Dictionary<ItemCategory, int> toConsume = new Dictionary<ItemCategory, int>();
            foreach (CourtExpense expense in Expenses)
            {
                foreach (var pair in expense.ItemCategories)
                {
                    if (toBuy.ContainsKey(pair.Key))
                    {
                        toBuy[pair.Key] += pair.Value;
                    }
                    else
                    {
                        toBuy.Add(pair.Key, pair.Value);
                    }

                    if (expense.ConsumeItems)
                    {
                        if (toConsume.ContainsKey(pair.Key))
                        {
                            toConsume[pair.Key] += pair.Value;
                        }
                        else
                        {
                            toConsume.Add(pair.Key, pair.Value);
                        }
                    }
                }
            }

            if (data.Location != null)
            {
                TextObject reason = null;
                if (data.Clan == Clan.PlayerClan) 
                    reason = new TextObject("{=!}Your court has spent {GOLD}{GOLD_ICON} buying {ITEMS} items for its good requirements.");
                BuyGoodsAction.BuyBestToWorst(data.Location.Settlement.Stash,
                    data.Location,
                    data.Clan.Leader,
                    toBuy,
                    data.Clan.Leader.Gold,
                    reason);
            }

            float graceChange = 0f;
            int totalItems = 0;
            int totalConsumed = 0;
            foreach (var pair in toConsume)
            {
                int consumed = 0;
                totalItems += pair.Value;
                graceChange -= 2f * pair.Value;
                if (data.Location != null)
                {
                    ItemRoster roster = data.Location.Settlement.Stash;
                    foreach (ItemRosterElement element in roster)
                    {
                        if (consumed >= pair.Value) break;

                        if (element.EquipmentElement.Item.ItemCategory == pair.Key)
                        {
                            int max = MathF.Min(pair.Value - consumed, element.Amount);
                            roster.AddToCounts(element.EquipmentElement, -max);
                            consumed += max;
                            float priceModifier = element.EquipmentElement.ItemModifier != null ?
                                element.EquipmentElement.ItemModifier.PriceMultiplier : 1f;
                            graceChange += max * 2f * priceModifier;
                            totalConsumed += max;
                        }
                    }
                }
            }

            if (data.Clan == Clan.PlayerClan)
            {
                InformationManager.DisplayMessage(new InformationMessage(
                    new TextObject("{=!}Your court has consumed {COUNT} out of {DESIRED} required goods, changing your grace by {GRACE}.")
                    .SetTextVariable("GRACE", graceChange.ToString("0.0"))
                    .SetTextVariable("COUNT", totalConsumed)
                    .SetTextVariable("DESIRED", totalItems)
                    .ToString(),
                    Color.FromUint(graceChange >= 0f ? Utils.TextHelper.COLOR_LIGHT_BLUE : Utils.TextHelper.COLOR_LIGHT_RED)));
            }
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
                return MBRandom.RoundRandomized(income * diff * 15f);
            }

            return 0;
        }
    }
}
