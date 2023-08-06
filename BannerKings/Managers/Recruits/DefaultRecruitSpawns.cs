using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using static BannerKings.Managers.PopulationManager;

namespace BannerKings.Managers.Recruits
{
    public class DefaultRecruitSpawns : DefaultTypeInitializer<DefaultRecruitSpawns, RecruitSpawn>
    {
        public override IEnumerable<RecruitSpawn> All
        {
            get
            {
                foreach (RecruitSpawn item in ModAdditions)
                {
                    yield return item;
                }
            }
        }

        public List<RecruitSpawn> GetPossibleSpawns(CultureObject culture, Settlement settlement = null)
        {
            List<RecruitSpawn> spawns = new List<RecruitSpawn>(2);
            if (culture == null) return spawns;

            foreach (RecruitSpawn spawn in All)
            {
                if (culture != spawn.Culture) continue;

                if (spawn.Fief != null)
                {
                    if (settlement == spawn.Fief.Settlement || (settlement.Village != null && 
                        spawn.Fief.Villages.Contains(settlement.Village)))
                    {
                        spawns.Add(spawn);
                    }

                    continue;
                }

                spawns.Add(spawn);
            }

            return spawns;
        }

        public List<RecruitSpawn> GetPossibleSpawns(CultureObject culture, PopType popType, Settlement settlement = null)
        {
            return GetPossibleSpawns(culture, settlement).Where(x => x.PopType == popType).ToList();
        }  

        public override void Initialize()
        {
        }
    }
}
