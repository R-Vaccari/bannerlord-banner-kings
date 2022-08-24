using BannerKings.Managers.Institutions.Religions;
using BannerKings.Managers.Skills;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Models.BKModels
{
    public class BKReligionModel
    {
        public ExplainedNumber CalculateFervor(Religion religion)
        {
            var result = new ExplainedNumber(5f, true);

            var villages = 0f;
            var castles = 0f;
            var towns = 0f;
            foreach (var settlement in Settlement.All)
            {
                var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
                if (data?.ReligionData == null)
                {
                    continue;
                }

                var rel = data.ReligionData.DominantReligion;
                if (rel != religion)
                {
                    continue;
                }

                var value = GetSettlementFervorWeight(settlement);
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

            result.Add(towns, GameTexts.FindText("str_towns"));
            result.Add(castles, GameTexts.FindText("str_castles"));
            result.Add(villages, GameTexts.FindText("str_villages"));

            var clans = 0f;
            foreach (var clan in Clan.All)
            {
                var rel = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(clan.Leader);
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

        public ExplainedNumber CalculateReligionWeight(Religion religion, Settlement settlement)
        {
            var result = new ExplainedNumber(0f, true);
            result.Add(religion.Fervor.ResultNumber * 100f, new TextObject("{=AfsRi9wL}Fervor"));

            Hero owner = null;
            if (settlement.OwnerClan != null)
            {
                owner = settlement.OwnerClan.Leader;
            }

            if (owner != null)
            {
                var rel = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(owner);
                if (rel != null && rel == religion)
                {
                    result.AddFactor(30f, new TextObject("{=tKhBP7mF}Owner's faith"));
                }

                if (owner.GetPerkValue(BKPerks.Instance.TheologyPreacher))
                {
                    result.AddFactor(0.05f, BKPerks.Instance.TheologyPreacher.Name);
                }
            }

            return result;
        }

        public ExplainedNumber CalculateReligionConversion(Religion religion, Settlement settlement, float diff)
        {
            var result = new ExplainedNumber(0f, true);
            if (diff > 0f)
            {
                result.LimitMax(diff);
            }
            else
            {
                result.LimitMin(diff);
                result.AddFactor(-1f);
            }

            result.Add(religion.Fervor.ResultNumber, new TextObject("{=AfsRi9wL}Fervor"));

            Hero owner = null;
            if (settlement.OwnerClan != null)
            {
                owner = settlement.OwnerClan.Leader;
            }

            if (owner != null)
            {
                var rel = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(owner);
                if (rel != null && rel == religion)
                {
                    result.AddFactor(0.1f, new TextObject("{=tKhBP7mF}Owner's faith"));
                }

                if (owner.GetPerkValue(BKPerks.Instance.TheologyPreacher))
                {
                    result.AddFactor(0.05f, BKPerks.Instance.TheologyPreacher.Name);
                }
            }

            return result;
        }
    }
}
