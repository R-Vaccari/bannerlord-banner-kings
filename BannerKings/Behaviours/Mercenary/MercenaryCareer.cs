using System.Collections.Generic;
using TaleWorlds.CampaignSystem;

namespace BannerKings.Behaviours.Mercenary
{
    public class MercenaryCareer
    {

        public MercenaryCareer(Clan clan, Kingdom kingdom)
        {
            Clan = clan;
            Kingdom = kingdom;
            Reputation = 0f;
            kingdomPrivileges = new Dictionary<Kingdom, List<MercenaryPrivilege>>();
            kingdomProgress = new Dictionary<Kingdom, float>();
            AddKingdom(kingdom);
        }


        public Clan Clan { get; private set;  }
        public Kingdom Kingdom { get; private set;  }
        public float Reputation { get; private set; }
        

        private Dictionary<Kingdom, List<MercenaryPrivilege>> kingdomPrivileges { get; set; }
        private Dictionary<Kingdom, float> kingdomProgress { get; set; }

        public void AddKingdom(Kingdom kingdom)
        {
            if (!kingdomPrivileges.ContainsKey(kingdom))
            {
                kingdomPrivileges.Add(kingdom, new List<MercenaryPrivilege>());
            }

            if (!kingdomProgress.ContainsKey(kingdom))
            {
                kingdomProgress.Add(kingdom, 0f);
            }
        }
    }
}
