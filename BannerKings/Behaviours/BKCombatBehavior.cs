using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace BannerKings.Behaviours
{
    public class BKCombatBehavior : CampaignBehaviorBase
    {
        private bool howlPlayed;

        public override void RegisterEvents()
        {
            CampaignEvents.MissionTickEvent.AddNonSerializedListener(this, OnMissionTick);
        }

        public override void SyncData(IDataStore dataStore)
        {
        }

        private void OnMissionTick(float dt)
        {
            if (!howlPlayed)
            {
                MBInformationManager.AddQuickInformation(new TextObject("{=s5bNOfWZQ}{HERO} is inspired by the spirits!")
                    .SetTextVariable("HERO", Hero.MainHero.Name), 0, Hero.MainHero.CharacterObject, "religions/wolfhowl");

                var index = SoundEvent.GetEventIdFromString("religions/wolfhowl");
                var eventRef =
                    SoundEvent.CreateEvent(index,
                        Mission.Current.Scene); //get a reference to sound and update parameters later.
                eventRef.SetPosition(Mission.Current.MainAgent.Position);
                eventRef.PlayInPosition(Mission.Current.MainAgent.Position);
                howlPlayed = true;
            }
        }
    }
}