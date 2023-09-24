using BannerKings.Managers.Court.Members.Tasks;
using BannerKings.Managers.Institutions.Religions;
using BannerKings.Managers.Institutions.Religions.Doctrines;
using BannerKings.Managers.Institutions.Religions.Faiths;
using BannerKings.Managers.Skills;
using BannerKings.Managers.Traits;
using BannerKings.Utils;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Models.BKModels
{
    public class BKReligionModel : IReligionModel
    {
        public ExplainedNumber CalculatePietyChange(Hero hero, bool descriptions = false)
        {
            var result = new ExplainedNumber(0f, descriptions);
            var rel = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(hero);
            if (rel != null)
            {
                ExceptionUtils.TryCatch(() =>
                {
                    foreach (var tuple in rel.Faith.Traits)
                    {
                        TraitObject trait = tuple.Key;
                        int traitLevel = hero.GetTraitLevel(trait);
                        if (traitLevel != 0)
                        {
                            result.Add(traitLevel * 0.2f * (tuple.Value ? 1f : -1f), trait.Name);
                        }
                    }

                    result.Add(hero.GetTraitLevel(BKTraits.Instance.Zealous) * 0.2f, BKTraits.Instance.Zealous.Name);

                    if (rel.FavoredCultures.Contains(hero.Culture))
                    {
                        result.Add(0.1f, GameTexts.FindText("str_culture"));
                    }

                    if (hero.GetPerkValue(BKPerks.Instance.TheologyFaithful))
                    {
                        result.Add(0.2f, BKPerks.Instance.TheologyFaithful.Name);
                    }

                    if (hero.Clan != null && rel.HasDoctrine(DefaultDoctrines.Instance.Animism))
                    {
                        var acres = 0f;
                        foreach (var settlement in hero.Clan.Settlements)
                        {
                            var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
                            if (data != null && data.LandData != null)
                            {
                                acres += data.LandData.Woodland;
                            }
                        }

                        if (acres != 0f)
                        {
                            result.Add(acres / 10000f, DefaultDoctrines.Instance.Animism.Name);
                        }
                    }

                    if (hero.Clan != null && rel.HasDoctrine(DefaultDoctrines.Instance.AncestorWorship))
                    {
                        result.Add(hero.Clan.Tier * 0.05f, DefaultDoctrines.Instance.AncestorWorship.Name);
                    }

                    if (rel.HasDoctrine(DefaultDoctrines.Instance.Literalism))
                    {
                        var skill = hero.GetSkillValue(BKSkills.Instance.Scholarship);
                        if (!hero.GetPerkValue(BKPerks.Instance.ScholarshipLiterate))
                        {
                            result.Add(-0.2f, DefaultDoctrines.Instance.Literalism.Name);
                        }
                        else if (skill > 100)
                        {
                            result.Add(skill * 0.01f, DefaultDoctrines.Instance.Literalism.Name);
                        }
                    }

                    if (rel.HasDoctrine(DefaultDoctrines.Instance.Esotericism))
                    {
                        result.Add(hero.GetAttributeValue(BKAttributes.Instance.Wisdom) * 0.1f, 
                            DefaultDoctrines.Instance.Esotericism.Name);
                    }

                    if (hero.Clan != null)
                    {
                        if (BannerKingsConfig.Instance.CourtManager.HasCurrentTask(hero.Clan,
                                              DefaultCouncilTasks.Instance.CultivatePiety,
                                              out float pietyCompetence))
                        {
                            result.Add(1f * pietyCompetence, DefaultCouncilTasks.Instance.CultivatePiety.Name);
                        }
                    }
                },
                GetType().Name,
                false);
            }

            return result;
        }

        public ExplainedNumber GetAppointInfluence(Hero appointer, ReligionData data, bool descriptions = false)
        {
            var result = new ExplainedNumber(50f, descriptions);

            result.AddFactor(data.Tension.ResultNumber * 0.5f, new TextObject("{=T88BUMMU}Religious tensions"));
            result.Add(BannerKingsConfig.Instance.InfluenceModel.CalculateInfluenceCap(appointer.Clan).ResultNumber * 0.1f,
                new TextObject("{=!}Clan Influence Limit"));
            return result;
        }

        public ExplainedNumber GetAppointCost(Hero appointer, ReligionData data, bool descriptions = false)
        {
            var result = new ExplainedNumber(300f, descriptions);


            result.AddFactor(data.Tension.ResultNumber, new TextObject("{=T88BUMMU}Religious tensions"));
            return result;
        }

        public ExplainedNumber GetRemoveInfluence(Hero appointer, Hero removed, ReligionData data, bool descriptions = false)
        {
            var result = new ExplainedNumber(50f, descriptions);

            result.AddFactor(GetNotableFactor(removed, data.Settlement),
                new TextObject("{=!}Power of {HERO}")
                .SetTextVariable("HERO", removed.Name));

            result.Add(BannerKingsConfig.Instance.InfluenceModel.CalculateInfluenceCap(appointer.Clan).ResultNumber * 0.2f,
                new TextObject("{=!}Clan Influence Limit"));
            return result;
        }

        public ExplainedNumber GetRemoveCost(Hero appointer, Hero removed, ReligionData data, bool descriptions = false)
        {
            var result = new ExplainedNumber(200f, descriptions);
            result.AddFactor(GetNotableFactor(removed, data.Settlement), 
                new TextObject("{=!}Power of {HERO}")
                .SetTextVariable("HERO", removed.Name));

            return result;
        }

        public ExplainedNumber GetRemoveLoyaltyCost(Hero appointer, Hero removed, ReligionData data, bool descriptions = false)
        {
            var result = new ExplainedNumber(0f, descriptions);
            result.Add(GetNotableFactor(removed, data.Settlement) * 50f,
                new TextObject("{=!}Power of {HERO}")
                .SetTextVariable("HERO", removed.Name));

            result.AddFactor(data.Tension.ResultNumber, new TextObject("{=T88BUMMU}Religious tensions"));

            return result;
        }

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
                    FaithStance stance = convertedRel.GetStance(rel.Faith);
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

            foreach (var tuple in data.Religions)
            {
                var rel = tuple.Key;
                if (rel == dominant)
                {
                    continue;
                }

                var tensionFactor = GetStanceTensionFactor(dominant.GetStance(rel.Faith));
                if (tensionFactor != 0f)
                {
                    result.AddFactor(tuple.Value * tensionFactor, rel.Faith.GetFaithName());
                }
            }

            if (BannerKingsConfig.Instance.ReligionsManager.HasBlessing(data.Settlement.OwnerClan.Leader, 
                DefaultDivinities.Instance.WindHeaven))
            {
                result.AddFactor(-0.2f, DefaultDivinities.Instance.WindHeaven.Name);
            }

            return result;
        }

        private float GetStanceTensionFactor(FaithStance stance)
        {
            switch (stance)
            {
                case FaithStance.Tolerated:
                    return 0.1f;
                case FaithStance.Hostile:
                    return 1f;
                default:
                    return 0.5f;
            }
        }

        public ExplainedNumber CalculateFervor(Religion religion)
        {
            ExplainedNumber result = new ExplainedNumber(0.3f, true);
            result.LimitMin(0f);
            result.LimitMax(1f);

            float villages = 0f;
            float castles = 0f;
            float towns = 0f;

            List<Settlement> holySites = new List<Settlement>(3);
            var mainDivinity = religion.Faith.GetMainDivinity();
            if (mainDivinity.Shrine != null)
            {
                holySites.Add(mainDivinity.Shrine);
            }

            foreach (Divinity divinity in religion.Faith.GetSecondaryDivinities())
            {
                if (divinity.Shrine != null)
                {
                    holySites.Add(divinity.Shrine);
                }
            }

            foreach (Settlement settlement in Settlement.All)
            {
                var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
                if (data?.ReligionData == null)
                {
                    continue;
                }

                var rel = data.ReligionData.DominantReligion;
                if (rel != religion)
                {
                    if (holySites.Contains(settlement))
                    {
                        result.Add(-0.05f, new TextObject("{=!}Missing holy site ({FIEF})")
                            .SetTextVariable("FIEF", settlement.Name));
                    }

                    if (settlement == religion.Faith.FaithSeat)
                    {
                        result.Add(-0.15f, new TextObject("{=!}Missing faith seat ({FIEF})")
                           .SetTextVariable("FIEF", settlement.Name));
                    }
                    continue;
                }

                float value = GetSettlementFervorWeight(settlement) * data.ReligionData.GetReligionPercentage(rel);
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

                if (holySites.Contains(settlement))
                {
                    result.Add(0.05f, new TextObject("{=!}Holy site ({FIEF})")
                        .SetTextVariable("FIEF", settlement.Name));
                }

                if (settlement == religion.Faith.FaithSeat)
                {
                    result.Add(0.15f, new TextObject("{=!}Faith seat ({FIEF})")
                       .SetTextVariable("FIEF", settlement.Name));
                }
            }

            result.Add(towns, GameTexts.FindText("str_towns"));
            result.Add(castles, GameTexts.FindText("str_castles"));
            result.Add(villages, GameTexts.FindText("str_villages"));

            float clans = 0f;
            float kingdomsCount = Kingdom.All.Count;
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

            result.Add(clans / kingdomsCount, GameTexts.FindText("str_encyclopedia_clans"));

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
