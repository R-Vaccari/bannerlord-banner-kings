using BannerKings.Managers.Court;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace BannerKings.Managers
{
    public class CourtManager
    {
        private Dictionary<Clan, CouncilData> COUNCILS { get; set; }

        public CourtManager(Dictionary<Clan, CouncilData> councils)
        {
            this.COUNCILS = councils;
        }

        public void ApplyCouncilEffect(ref ExplainedNumber result, Hero settlementOwner, CouncilPosition position, float maxEffect, bool factor)
        {
            CouncilData council = this.GetCouncil(settlementOwner);
            float competence = council.GetCompetence(position);
            if (competence != 0f)
            {
                if (!factor) result.Add(maxEffect * competence, new TextObject("{=!}Council Effect"));
                else result.AddFactor(maxEffect * competence, new TextObject("{=!}Council Effect"));
            }
        }

        public CouncilData GetCouncil(Hero hero)
        {
            Clan clan = hero.Clan;
            if (this.COUNCILS.ContainsKey(clan))
                return this.COUNCILS[clan];
            else
            {
                CouncilData council = new CouncilData(clan);
                this.COUNCILS.Add(clan, council);
                return council;
            }
        }

        public CouncilData GetCouncil(Clan clan)
        {
            if (this.COUNCILS.ContainsKey(clan))
                return this.COUNCILS[clan];
            else
            {
                CouncilData council = new CouncilData(clan);
                this.COUNCILS.Add(clan, council);
                return council;
            }
        }

        public void UpdateCouncil(Clan clan)
        {
            CouncilData data = this.GetCouncil(clan.Leader);
            data.Update(null);
        }
    }
}
