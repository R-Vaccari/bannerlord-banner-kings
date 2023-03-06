using BannerKings.Managers.Court.Members.Tasks;
using BannerKings.Managers.Institutions.Religions.Doctrines;
using BannerKings.Managers.Skills;
using BannerKings.UI.Court;
using BannerKings.Utils;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;

namespace BannerKings.Models.BKModels
{
    public class BKPietyModel : IReligionModel
    {
        public int GetHeroVirtuesCount(Hero hero)
        {
            var result = 0;
            var rel = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(hero);
            if (rel != null)
            {
                var traits = rel.Faith.Traits;
                var toCheck = new List<TraitObject>
                    {DefaultTraits.Honor, DefaultTraits.Calculating, DefaultTraits.Valor, DefaultTraits.Mercy};
                foreach (var trait in toCheck)
                {
                    var traitLevel = hero.GetTraitLevel(trait);
                    if (traits.ContainsKey(trait) && traitLevel != 0)
                    {
                        var target = traits[trait] ? 1 : -1;
                        if (target > 0)
                        {
                            result += traitLevel > 0 ? 1 : 0;
                        }
                        else
                        {
                            result += -traitLevel < 0 ? 1 : 0;
                        }
                    }
                }
            }

            return result;
        }

        public ExplainedNumber CalculateEffect(Hero hero, bool descriptions = false)
        {
            var result = new ExplainedNumber(0f, descriptions);
            var rel = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(hero);
            if (rel != null)
            {
                ExceptionUtils.TryCatch(() =>
                {
                    var traits = rel.Faith.Traits;
                    var toCheck = new List<TraitObject>
                    {DefaultTraits.Honor, DefaultTraits.Calculating, DefaultTraits.Valor, DefaultTraits.Mercy};
                    foreach (var trait in toCheck)
                    {
                        var traitLevel = hero.GetTraitLevel(trait);
                        if (traits.ContainsKey(trait) && traitLevel != 0)
                        {
                            var target = traits[trait] ? 1 : -1;
                            if (target > 0)
                            {
                                result.Add(traitLevel * (traitLevel > 0 ? 0.2f : -0.2f), trait.Name);
                            }
                            else
                            {
                                result.Add(traitLevel * (-traitLevel < 0 ? 0.2f : -0.2f), trait.Name);
                            }
                        }
                    }

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

        public ExplainedNumber CalculateEffect(Settlement settlement) => new ExplainedNumber();
    }
}