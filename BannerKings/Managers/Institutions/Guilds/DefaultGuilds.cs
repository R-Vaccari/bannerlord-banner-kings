using System.Collections.Generic;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Guilds
{
    public class DefaultGuilds : DefaultTypeInitializer<DefaultGuilds, Guild>
    {
        public Guild Merchants { get; } = new Guild("Merchants");
        public Guild Masons { get; } = new Guild("Masons");
        public Guild Metalsmiths { get; } = new Guild("Metalsmiths");

        public override IEnumerable<Guild> All
        {
            get
            {
                yield return Merchants;
                yield return Masons;
                yield return Metalsmiths;
            }
        }

        public override void Initialize()
        {
            Merchants.Initialize(new TextObject("{=z69WpCc8}Merchants Guild"),
                new TextObject("{=!}An association of merchants. As a group, merchants seek to have the freest markets possible, avoiding tariffs to noble overlords or other regulations.\nEffects:\nIncreased mercantilism\nIncreased caravan attractiveness"),
                new List<WorkshopType>()
                {
                    WorkshopType.Find("velvet_weavery"), 
                    WorkshopType.Find("bakery")
                },
                new List<VillageType>()
                {
                    DefaultVillageTypes.WheatFarm,
                    DefaultVillageTypes.SaltMine,
                    DefaultVillageTypes.SilkPlant
                });

            Masons.Initialize(new TextObject("{=qLVinLay}Masons Guild"),
                new TextObject("{=!}An association of construction workers and engineers. Masons are responsible for stonework in construction and hence essential for towns and castles.\nEffects:\nImproved construction power"),
                new List<WorkshopType>()
                {
                    WorkshopType.Find("pottery_shop"),
                    WorkshopType.Find("mines")
                },
                new List<VillageType>()
                {
                    DefaultVillageTypes.ClayMine,
                    DefaultVillageTypes.Lumberjack,
                    DefaultVillageTypes.IronMine
                });

            Metalsmiths.Initialize(new TextObject("{=VPDXcJu7}Metalsmiths Guild"),
                new TextObject("{=!}An association of various types of metalworkers, such as tool makers, horseshoe makers, among others."),
                new List<WorkshopType>()
                {
                    WorkshopType.Find("smithy"),
                    WorkshopType.Find("armorsmithy"),
                    WorkshopType.Find("weaponsmithy"), 
                    WorkshopType.Find("silversmithy"),
                    WorkshopType.Find("barding-smithy")
                },
                new List<VillageType>()
                {
                    DefaultVillageTypes.IronMine,
                    DefaultVillageTypes.SilverMine
                });
        }
    }
}
