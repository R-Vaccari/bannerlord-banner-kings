using BannerKings.Behaviours;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Behaviors.Invasions
{
    public class InvasionBehavior : BannerKingsBehavior
    {
        private List<Invasion> invasions;
        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, OnDailyTick);
        }

        public override void SyncData(IDataStore dataStore)
        {
            if (invasions == null) invasions = new List<Invasion>(1);
        }

        private void OnDailyTick()
        {
            if (MBRandom.RandomFloat > 1f / CampaignTime.DaysInYear * 10f) return;
            
            Invasion invasion = DefaultInvasions.Instance.All.ToList().GetRandomElementWithPredicate(x => !invasions.Contains(x));
            if (invasion != null)
            {

                InformationManager.DisplayMessage(new InformationMessage(
                new TextObject("{=DqVwd13t}{HERO} has invaded the continent alongside their army... Reports say they arrived near {TOWN}...")
                .SetTextVariable("TOWN", invasion.SpawnSettlement.Name)
                .ToString()));
            }
        }
    }
}
