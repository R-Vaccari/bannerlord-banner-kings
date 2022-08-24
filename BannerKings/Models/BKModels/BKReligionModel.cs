using BannerKings.Managers.Institutions.Religions;
using BannerKings.Managers.Populations;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;

namespace BannerKings.Models.BKModels
{
    public class BKReligionModel
    {
        public ExplainedNumber CalculateFervor(Religion religion)
        {
            ExplainedNumber result = new ExplainedNumber(0f, true);

            float villages = 0f;
            float castles = 0f;
            float towns = 0f;
            foreach (Settlement settlement in Settlement.All)
            {
                PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
                if (data != null && data.ReligionData != null)
                {
                    Religion rel = data.ReligionData.DominantReligion;
                    if (rel == religion)
                    {
                        float value = GetSettlementFervorWeight(settlement);
                        if (settlement.IsVillage)
                        {
                            villages += value;
                        }

                        if (settlement.IsCastle)
                        {
                            castles += value;
                        }

                        if (settlement.IsTown)
                        {
                            towns += value;
                        }
                    }
                }
            }

            result.Add(towns, GameTexts.FindText("str_towns"));
            result.Add(castles, GameTexts.FindText("str_castles"));
            result.Add(villages, GameTexts.FindText("str_villages"));

            float clans = 0f;
            foreach (Clan clan in Clan.All)
            {
                Religion rel = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(clan.Leader);
                if (rel == religion)
                {
                    clans += 0.01f;
                }
            }

            result.Add(clans, GameTexts.FindText("str_encyclopedia_clans"));

            return result;
        }

        public float GetSettlementFervorWeight(Settlement settlement)
        {
            if (settlement.IsVillage)
            {
                return 0.005f;
            }

            if (settlement.IsCastle)
            {
                return 0.015f;
            }

            if (settlement.IsTown)
            {
                return 0.03f;
            }

            return 0f;
        }

        public ExplainedNumber CalculateReligionConversion(Religion religion, Settlement settlement)
        {
            ExplainedNumber result = new ExplainedNumber(0f, true);


            return result;
        }
    }
}
