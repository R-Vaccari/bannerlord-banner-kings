using System.Collections.Generic;
using TaleWorlds.CampaignSystem;

namespace BannerKings.Managers.Religions
{
    
    public abstract class Religion
    {
        protected List<Settlement> HolySites { get; set; } = new();
    }
}