using System.Linq;
using BannerKings.Managers.Populations;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;

namespace BannerKings.Models.BKModels
{
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
                        peace = FactionManager.GetEnemyFactions(clan.Kingdom).Any();
                    }

                    result.Add(peace ? 0.02f : -0.02f, new TextObject(peace ? "{=zgK5zVkQ}Peace" : "{=Ypfy9D3P}War"));
                }

                if (dataCulture == settlement.Culture)
                {
                    result.Add(0.02f, new TextObject("{=djkEJ9wB}Dominant culture"));
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
}