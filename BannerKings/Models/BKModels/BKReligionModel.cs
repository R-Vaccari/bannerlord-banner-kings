using BannerKings.Managers.Institutions.Religions;
using BannerKings.Managers.Institutions.Religions.Doctrines;
using BannerKings.Managers.Institutions.Religions.Faiths;
using BannerKings.Managers.Skills;
using BannerKings.Managers.Traits;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Models.BKModels
{
    public class BKReligionModel
    {
        public ExplainedNumber GetConversionLikelihood(Hero converter, Hero converted)
        {
            var result = new ExplainedNumber(15f, false);
            result.LimitMin(-1f);
            result.LimitMax(1f);

            Religion rel = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(converter);
            if (rel != null)
            {
                foreach (var virtue in rel.Faith.Traits)
                {
                    int level = converted.GetTraitLevel(virtue.Key);
                    result.Add(0.2f * level * (virtue.Value ? 1f : -1f), virtue.Key.Name);
                }

                result.Add(converted.GetTraitLevel(BKTraits.Instance.Zealous) * -0.33f, BKTraits.Instance.Zealous.Name);
                result.AddFactor(converted.GetRelation(converter) * 0.005f, new TextObject("{=BlidMNGT}Relation"));

                Religion convertedRel = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(converted);
                if (convertedRel != null)
                {
                    FaithStance stance = convertedRel.Faith.GetStance(rel.Faith);
                    if (stance == FaithStance.Untolerated)
                    {
                        result.Add(-0.3f, new TextObject("{=gyHK87NL}Faith differences"));
                    }
                    else if (stance == FaithStance.Hostile)
                    {
                        result.Add(-0.9f, new TextObject("{=gyHK87NL}Faith differences"));
                    }
                }

                if (rel.FavoredCultures.Contains(converted.Culture))
                {
                    result.AddFactor(0.15f, new TextObject("{=PUjDWe5j}Culture"));
                }
            } 

            return result;
        }

        public ExplainedNumber GetConversionInfluenceCost(Hero notable, Hero converter)
        {
            var result = new ExplainedNumber(15f, false);
            result.LimitMin(15f);
            result.LimitMax(150f);

            if (!notable.IsNotable || notable.CurrentSettlement == null)
            {
                return new ExplainedNumber(0f);
            }

            result.Add(notable.GetRelation(converter) * -0.1f);
            result.Add(GetNotableFactor(notable, notable.CurrentSettlement) / 2f);
            var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(notable.CurrentSettlement);

            if (data != null && data.ReligionData != null)
            {
                var tension = data.ReligionData.Tension;
                result.AddFactor(tension.ResultNumber);
            }

            if (BannerKingsConfig.Instance.ReligionsManager.HasBlessing(converter,
                DefaultDivinities.Instance.DarusosianMain))
            {
                result.AddFactor(-0.2f, DefaultDivinities.Instance.DarusosianMain.Name);
            }

            return result;
        }

        public ExplainedNumber GetConversionPietyCost(Hero converted, Hero converter)
        {
            var result = new ExplainedNumber(40f, false);
            result.LimitMin(40f);
            result.LimitMax(150f);

            result.Add(converted.GetRelation(converter) * -0.1f);

            if (converted.IsNotable)
            {
                result.Add(GetNotableFactor(converted, converted.CurrentSettlement));
                var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(converted.CurrentSettlement);

                if (data != null && data.ReligionData != null)
                {
                    var tension = data.ReligionData.Tension;
                    result.AddFactor(tension.ResultNumber);
                }
            }

            result.Add(MathF.Clamp(40f * -GetConversionLikelihood(converter, converted).ResultNumber, -10f, 40f),
                new TextObject("{=bYHRQmAW}Willingness to convert"));

            if (BannerKingsConfig.Instance.ReligionsManager.HasBlessing(converter,
                DefaultDivinities.Instance.DarusosianMain))
            {
                result.AddFactor(-0.2f, DefaultDivinities.Instance.DarusosianMain.Name);
            }

            return result;
        }

        public ExplainedNumber CalculateTensionTarget(ReligionData data)
        {
            var result = new ExplainedNumber(0f, true);
            result.LimitMin(0f);
            result.LimitMax(1f);

            var dominant = data.DominantReligion;
            if (dominant == null)
            {
                return result;
            }

            var dominantShare = data.Religions[dominant];
            result.Add(1f - dominantShare, new TextObject("{=SFRmmVms}Dominant faith's share"));

            foreach (var tuple in data.Religions)
            {
                var rel = tuple.Key;
                if (rel == dominant)
                {
                    continue;
                }

                var tensionFactor = GetStanceTensionFactor(dominant.Faith.GetStance(rel.Faith));
                if (tensionFactor != 0f)
                {
                    result.AddFactor(tuple.Value * tensionFactor, rel.Faith.GetFaithName());
                }
            }

            return result;
        }

        private float GetStanceTensionFactor(FaithStance stance)
        {
            switch (stance)
            {
                case FaithStance.Tolerated:
                    return 0f;
                case FaithStance.Hostile:
                    return 1f;
                default:
                    return 0.5f;
            }
        }

        public ExplainedNumber CalculateFervor(Religion religion)
        {
            var result = new ExplainedNumber(0.05f, true);
            result.LimitMin(0f);
            result.LimitMax(1f);

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
                if (clan.IsBanditFaction || clan.IsEliminated || clan.Leader == null)
                {
                    continue;
                }

                var rel = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(clan.Leader);
                if (rel != null && rel == religion)
                {
                    clans += 0.01f;
                }
            }

            result.Add(clans, GameTexts.FindText("str_encyclopedia_clans"));

            if (religion.HasDoctrine(DefaultDoctrines.Instance.Animism))
            {
                result.Add(-0.05f, DefaultDoctrines.Instance.Animism.Name);
            }

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
            if (settlement == null)
            {
                return result;
            }

            if (settlement.Notables != null)
            {
                foreach (var notable in settlement.Notables)
                {
                    if (notable.IsPreacher)
                    {
                        continue;
                    }

                    var rel = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(notable);
                    if (rel != null && rel == religion)
                    {
                        result.Add(GetNotableFactor(notable, settlement), notable.Name);
                    }
                }
            }

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
                    result.Add(5f, new TextObject("{=tKhBP7mF}Owner's faith"));
                }

                if (owner.GetPerkValue(BKPerks.Instance.TheologyPreacher))
                {
                    result.AddFactor(0.05f, BKPerks.Instance.TheologyPreacher.Name);
                }

                if (BannerKingsConfig.Instance.ReligionsManager.HasBlessing(owner, DefaultDivinities.Instance.VlandiaSecondary1))
                {
                    result.AddFactor(0.1f, DefaultDivinities.Instance.VlandiaSecondary1.Name);
                }

                if (owner.Culture != settlement.Culture && BannerKingsConfig.Instance.ReligionsManager.HasBlessing(owner,
                    DefaultDivinities.Instance.AseraSecondary3))
                {
                    result.AddFactor(0.15f, DefaultDivinities.Instance.AseraSecondary1.Name);
                }
            }

            result.AddFactor(religion.Fervor.ResultNumber, new TextObject("{=AfsRi9wL}Fervor"));
            return result;
        }

        public float GetNotableFactor(Hero notable, Settlement settlement)
        {
            var totalPower = 0f;
            foreach (var hero in settlement.Notables)
            {
                totalPower += hero.Power;
            }

            return (settlement.Notables.Count * 25f) * (notable.Power / totalPower);
        }

        public ExplainedNumber CalculateReligionConversion(Religion religion, Settlement settlement, float diff)
        {
            var result = new ExplainedNumber(0f, true);
            result.LimitMin(0f);
            result.LimitMax(1f);
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
