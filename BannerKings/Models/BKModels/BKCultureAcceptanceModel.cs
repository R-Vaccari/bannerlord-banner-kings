using System.Linq;
using BannerKings.Populations;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;

namespace BannerKings.Models;

public class BKCultureAcceptanceModel : ICultureModel
{
    public ExplainedNumber CalculateEffect(Settlement settlement)
    {
        var result = new ExplainedNumber(0f);

        return result;
    }

    public ExplainedNumber CalculateEffect(Settlement settlement, CultureDataClass data)
    {
        var result = new ExplainedNumber(0f);
        var dataCulture = data.Culture;
        if (dataCulture == settlement.Owner.Culture)
        {
            if (data.Acceptance == 1f)
            {
                return result;
            }

            if (dataCulture == settlement.Owner.Culture)
            {
                var stability = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement).Stability;
                result.Add((stability - 0.5f) * 0.1f, new TextObject("Stability"));

                var clan = settlement.OwnerClan;
                var peace = true;
                if (clan.Kingdom != null)
                {
                    peace = FactionManager.GetEnemyFactions(clan.Kingdom).Count() > 0;
                }

                result.Add(peace ? 0.02f : -0.02f, new TextObject(peace ? "Peace" : "War"));
            }

            if (dataCulture == settlement.Culture)
            {
                result.Add(0.02f, new TextObject("Dominant culture"));
            }
        }
        else
        {
            if (data.Acceptance > data.Assimilation)
            {
                result.Add(-0.02f);
            }
            else if (data.Acceptance < data.Assimilation)
            {
                result.Add(0.02f);
            }
        }

        return result;
    }
}