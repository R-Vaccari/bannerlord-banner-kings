using System.Collections.Generic;
using System;
using TaleWorlds.Core;
using System.Linq;
using TaleWorlds.CampaignSystem.Extensions;

namespace BannerKings.Extensions
{
    public static class ItemModifierGroupExtension
    {
        public static ItemModifier GetRandomModifierWithTarget(this ItemModifierGroup instance, float target, float variation = 0f)
        {
            List<ValueTuple<ItemModifier, float>> list = new List<ValueTuple<ItemModifier, float>>();
            foreach (var modifier in instance.ItemModifiers)
            {
                if (modifier.PriceMultiplier >= target - variation && modifier.PriceMultiplier <= target + variation)
                {
                    list.Add(new (modifier, 1f));
                }
            }
        
            return MBRandom.ChooseWeighted<ItemModifier>(list);
        }
    }
}
