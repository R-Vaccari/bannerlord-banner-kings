using BannerKings.Managers.Skills;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;

namespace BannerKings.Models.BKModels
{
    public class BKPietyModel : IReligionModel
    {
        public ExplainedNumber CalculateEffect(Settlement settlement) => new ExplainedNumber();

        public ExplainedNumber CalculateEffect(Hero hero)
        {
            var result = new ExplainedNumber(0f, true);
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
            }

            return result;
        }
    }
}