
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using TaleWorlds.Localization;

namespace Populations.Models
{
    class ClanIncomeModel : DefaultClanFinanceModel
    {

        public override ExplainedNumber CalculateClanGoldChange(Clan clan, bool includeDescriptions = false, bool applyWithdrawals = false)
        {
            ExplainedNumber baseResult = base.CalculateClanGoldChange(clan, includeDescriptions, applyWithdrawals);
            if (PopulationConfig.Instance.PopulationManager != null)
            {
                List<MobileParty> militias = PopulationConfig.Instance.PopulationManager.GetClanMilitias(clan);
                if (militias.Count > 0)
                    foreach (MobileParty party in militias)
                        baseResult.Add(-party.TotalWage, new TextObject("Raised militia party"));
            }
            
            return baseResult;
        }

        public override ExplainedNumber CalculateClanExpenses(Clan clan, bool includeDescriptions = false, bool applyWithdrawals = false)
        {
            ExplainedNumber baseResult = base.CalculateClanExpenses(clan, includeDescriptions, applyWithdrawals);
            if (PopulationConfig.Instance.PopulationManager != null)
            {
                List<MobileParty> militias = PopulationConfig.Instance.PopulationManager.GetClanMilitias(clan);
                if (militias.Count > 0)
                    foreach (MobileParty party in militias)
                        baseResult.Add(-party.TotalWage, new TextObject("Raised militia party"));
            }

            return baseResult;
        }
    }
}
