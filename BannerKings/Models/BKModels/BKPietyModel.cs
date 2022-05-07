using BannerKings.Managers.Institutions.Religions;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Models.BKModels
{
    public class BKPietyModel : IReligionModel
    {
        public ExplainedNumber CalculateEffect(Settlement settlement)
        {
            throw new NotImplementedException();
        }

        public ExplainedNumber CalculateEffect(Hero hero)
        {
            ExplainedNumber result = new ExplainedNumber(0f, true);
            Religion rel = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(hero);
            if (rel != null)
            {
                MBReadOnlyDictionary<TraitObject, bool> traits = rel.Faith.Traits;
                List<TraitObject> toCheck = new List<TraitObject>() { DefaultTraits.Honor , DefaultTraits.Calculating , DefaultTraits.Valor , DefaultTraits.Mercy };
                foreach (TraitObject trait in toCheck)
                {
                    int traitLevel = hero.GetTraitLevel(trait);
                    if (traits.ContainsKey(trait) && traitLevel != 0)
                    {
                        int target = traits[trait] ? 1 : -1;
                        if (target > 0) result.Add(traitLevel * (traitLevel > 0 ? 0.2f : -0.2f), trait.Name);
                        else result.Add(traitLevel * (-traitLevel < 0 ? 0.2f : -0.2f), trait.Name);
                    }
                }

                result.Add(0.1f, new TextObject("{=!}Faithful"));
            }

            return result;
        }
    }
}
