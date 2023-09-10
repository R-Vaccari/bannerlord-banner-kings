using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.ObjectSystem;

namespace BannerKings.Behaviors.Invasions
{
    public class Invasion : BannerKingsObject
    {
        public Invasion(string stringId) : base(stringId)
        {
        }

        public void Initialize(string leaderTemplate,
            Clan rulingClan,
            Settlement spawnSettlement,
            Kingdom kingdom,
            Dictionary<Clan, CharacterObject> clans)
        {
            LeaderTemplate = MBObjectManager.Instance.GetObject<CharacterObject>(leaderTemplate);
            SpawnSettlement = spawnSettlement;
            Kingdom = kingdom;
            RulingClan = rulingClan;
            Clans = clans;
        }

        public CharacterObject LeaderTemplate { get; private set; }
        public Settlement SpawnSettlement { get; private set; }
        public Kingdom Kingdom { get; private set; }
        public Clan RulingClan { get; private set; }
        private Dictionary<Clan, CharacterObject> Clans { get; set; }

        public void StartInvasion()
        {

        }
    }
}
