using System.Linq;
using System.Reflection;
using BannerKings.Managers.Education.Lifestyles;
using BannerKings.Managers.Titles;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Models.Vanilla
{
    public class BKClanFinanceModel : DefaultClanFinanceModel
    {
        public override ExplainedNumber CalculateClanGoldChange(Clan clan, bool includeDescriptions = false,
            bool applyWithdrawals = false)
        {
            var baseResult = base.CalculateClanGoldChange(clan, true, applyWithdrawals);
            if (BannerKingsConfig.Instance.TitleManager != null)
            {
                CalculateClanExpenseInternal(clan, ref baseResult, applyWithdrawals);
                CalculateClanIncomeInternal(clan, ref baseResult, applyWithdrawals);
            }

            return baseResult;
        }

        public override ExplainedNumber CalculateClanIncome(Clan clan, bool includeDescriptions = false,
            bool applyWithdrawals = false)
        {
            var baseResult = base.CalculateClanIncome(clan, includeDescriptions, applyWithdrawals);
            if (BannerKingsConfig.Instance.TitleManager != null)
            {
                CalculateClanIncomeInternal(clan, ref baseResult, applyWithdrawals);
            }

            return baseResult;
        }

        public override ExplainedNumber CalculateClanExpenses(Clan clan, bool includeDescriptions = false,
            bool applyWithdrawals = false)
        {
            var baseResult = base.CalculateClanExpenses(clan, includeDescriptions, applyWithdrawals);
            if (BannerKingsConfig.Instance.TitleManager != null)
            {
                CalculateClanExpenseInternal(clan, ref baseResult, applyWithdrawals);
            }

            return baseResult;
        }

        public void CalculateClanIncomeInternal(Clan clan, ref ExplainedNumber result, bool applyWithdrawals)
        {
            var kingdom = clan.Kingdom;
            var wkModel = (BKWorkshopModel) Campaign.Current.Models.WorkshopModel;
            foreach (var town in clan.Fiefs)
            {
                foreach (var wk in town.Workshops)
                {
                    if (wk.IsRunning && wk.Owner != clan.Leader && wk.WorkshopType.StringId != "artisans")
                    {
                        result.Add(
                            base.CalculateOwnerIncomeFromWorkshop(wk) *
                            wkModel.CalculateWorkshopTax(town.Settlement, wk.Owner).ResultNumber,
                            new TextObject("{=!}Taxes from {WORKSHOP} at {TOWN}")
                                .SetTextVariable("WORKSHOP", wk.Name)
                                .SetTextVariable("TOWN", town.Name));
                    }
                }

                if (BannerKingsConfig.Instance.AI.AcceptNotableAid(clan,
                        BannerKingsConfig.Instance.PopulationManager.GetPopData(town.Settlement)))
                {
                    foreach (var notable in town.Settlement.Notables)
                    {
                        if (notable.SupporterOf == clan && notable.Gold > 5000)
                        {
                            result.Add(200f,
                                new TextObject("{=!}Aid from {NOTABLE}").SetTextVariable("NOTABLE", notable.Name));
                            if (applyWithdrawals)
                            {
                                notable.Gold -= 200;
                            }
                        }
                    }
                }
            }


            var dictionary = BannerKingsConfig.Instance.TitleManager.CalculateVassals(clan);
            if (dictionary.Count >= 0)
            {
                foreach (var pair in dictionary)
                {
                    if (clan.Kingdom != pair.Key.Kingdom)
                    {
                        continue;
                    }

                    var amount = 0f;
                    foreach (var title in pair.Value)
                    {
                        amount += (int) title.dueTax;
                    }

                    result.Add(amount, new TextObject("{=!}Taxes from {CLAN}").SetTextVariable("CLAN", pair.Key.Name));
                }
            }

            if (!clan.IsUnderMercenaryService && clan.Kingdom != null &&
                FactionManager.GetEnemyKingdoms(clan.Kingdom).Any())
            {
                var title = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(kingdom);
                if (title != null && title.contract != null &&
                    title.contract.Rights.Contains(FeudalRights.Army_Compensation_Rights))
                {
                    var model = new DefaultClanFinanceModel();
                    var expense = model.GetType().GetMethod("AddExpenseFromLeaderParty",
                        BindingFlags.Instance | BindingFlags.NonPublic);
                    object[] array = {clan, new ExplainedNumber(), applyWithdrawals};
                    expense.Invoke(model, array);
                    var payment = ((ExplainedNumber) array[1]).ResultNumber * -1f;
                    var limit = clan.Tier * 1000f;
                    if (payment > limit)
                    {
                        payment = limit;
                    }

                    if (kingdom.KingdomBudgetWallet >= payment)
                    {
                        if (applyWithdrawals)
                        {
                            kingdom.KingdomBudgetWallet -= (int) payment;
                        }

                        result.Add(payment, new TextObject("{=!}Army compensation rights"));
                    }
                }
            }

            var position = BannerKingsConfig.Instance.CourtManager.GetHeroPosition(clan.Leader);
            if (position != null)
            {
                result.Add(position.DueWage, new TextObject("{=!}Councillor role"));
            }
        }

        private void AddMercenaryIncome(Clan clan, ref ExplainedNumber goldChange, bool applyWithdrawals)
        {
            if (clan.IsUnderMercenaryService && clan.Leader != null && clan.Kingdom != null)
            {
                var num = MathF.Ceiling(clan.Influence *
                                        (1f / Campaign.Current.Models.ClanFinanceModel.RevenueSmoothenFraction())) *
                          clan.MercenaryAwardMultiplier;
                var education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(clan.Leader);
                if (education.Lifestyle == DefaultLifestyles.Instance.Mercenary)
                {
                    goldChange.Add((int) (num * 0.15f), new TextObject("{=!}Lifestyle"));
                }
            }
        }

        public void CalculateClanExpenseInternal(Clan clan, ref ExplainedNumber result, bool applyWithdrawals)
        {
            var wkModel = (BKWorkshopModel) Campaign.Current.Models.WorkshopModel;
            foreach (var wk in clan.Leader.OwnedWorkshops)
            {
                if (wk.IsRunning && wk.Settlement.OwnerClan != clan)
                {
                    var tax = wkModel.CalculateWorkshopTax(wk.Settlement, clan.Leader).ResultNumber;
                    result.Add(base.CalculateOwnerIncomeFromWorkshop(wk) * -tax,
                        new TextObject("{=!}{WORKSHOP} taxes to {CLAN}")
                            .SetTextVariable("WORKSHOP", wk.Name)
                            .SetTextVariable("CLAN", wk.Settlement.OwnerClan.Name));
                }
            }

            var data = BannerKingsConfig.Instance.CourtManager.GetCouncil(clan);
            if (data != null)
            {
                foreach (var position in data.GetOccupiedPositions())
                {
                    result.Add(-position.DueWage,
                        new TextObject("{=!}Council wage to {NAME}").SetTextVariable("NAME", position.Member.Name));
                    if (applyWithdrawals && !position.Member.IsLord)
                    {
                        position.Member.Gold += position.DueWage;
                    }
                }
            }


            var suzerain = BannerKingsConfig.Instance.TitleManager.CalculateHeroSuzerain(clan.Leader);
            if (suzerain == null)
            {
                return;
            }

            Kingdom deJureKingdom = null;
            if (suzerain.deJure.Clan != null)
            {
                deJureKingdom = suzerain.deJure.Clan.Kingdom;
            }

            if (deJureKingdom == null || deJureKingdom != clan.Kingdom)
            {
                return;
            }

            var amount = 0f;
            var dictionary = BannerKingsConfig.Instance.TitleManager.CalculateVassals(suzerain.deJure.Clan, clan);
            foreach (var title in dictionary[clan])
            {
                amount += (int) title.dueTax;
            }

            result.Add(-amount,
                new TextObject("{=!}Taxes to {SUZERAIN}").SetTextVariable("SUZERAIN", suzerain.deJure.Name));
        }
    }
}