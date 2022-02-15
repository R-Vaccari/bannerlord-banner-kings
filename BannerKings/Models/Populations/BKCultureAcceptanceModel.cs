using BannerKings.Populations;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace BannerKings.Models
{
    class BKCultureAcceptanceModel : ICultureModel
    {
        public ExplainedNumber CalculateEffect(Settlement settlement)
        {
            ExplainedNumber result = new ExplainedNumber(0f);

            return result;
        }

        public ExplainedNumber CalculateEffect(Settlement settlement, CultureDataClass data)
        {
            PopulationData popData = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
            ExplainedNumber result = new ExplainedNumber(0f);
            CultureObject dataCulture = data.Culture;
            bool foreigner = dataCulture != settlement.Culture && dataCulture != settlement.Owner.Culture;
            if (!foreigner)
            {
                if (data.Acceptance == 1f)
                    return result;

                if (dataCulture == settlement.Owner.Culture)
                {
                    float stability = popData.Stability;
                    result.Add((stability - 0.5f) * 0.1f, new TextObject("Stability"));

                    Clan clan = settlement.OwnerClan;
                    bool peace = true;
                    if (clan.Kingdom != null)
                        foreach (IFaction faction in Kingdom.All)
                            if (faction != clan.Kingdom && clan.Kingdom.IsAtWarWith(faction)) 
                            {
                                peace = false;
                                break;
                            }
                    result.Add(peace ? 0.02f : -0.02f, new TextObject(peace ? "Peace" : "War"));
                }

                if (dataCulture == settlement.Culture)
                    result.Add(0.02f, new TextObject("Dominant culture"));
            }


            return result;
        }
    }
}
