using BannerKings.Utils;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace BannerKings.Behaviours
{
    public abstract class BannerKingsBehavior : CampaignBehaviorBase
    {
        protected void RunWeekly(Action method, string className, bool notifty = true)
        {
            if (MBRandom.RandomFloat < 0.14)
            {
                ExceptionUtils.TryCatch(method, className, notifty);
            }
        }
    }
}
