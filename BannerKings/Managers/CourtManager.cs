using BannerKings.Managers.Court;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers
{
    public class CourtManager
    {
        [SaveableProperty(1)]
        private Dictionary<Clan, CouncilData> Councils { get; set; }

        public CourtManager(Dictionary<Clan, CouncilData> councils)
        {
            this.Councils = councils;
        }

        public void ApplyCouncilEffect(ref ExplainedNumber result, Hero settlementOwner, CouncilPosition position, float maxEffect, bool factor)
        {
            CouncilData council = this.GetCouncil(settlementOwner);
            float competence = council.GetCompetence(position);
            if (competence != 0f)
            {
                if (!factor) result.Add(maxEffect * competence, new TextObject("{=!}Council effect"));
                else result.AddFactor(maxEffect * competence, new TextObject("{=!}Council effect"));
            }
        }

        public int GetCouncilEffectInteger(Hero settlementOwner, CouncilPosition position, float maxEffect)
        {
            CouncilData council = this.GetCouncil(settlementOwner);
            float competence = council.GetCompetence(position);
            return (int)(maxEffect * competence);
        }

        public CouncilData GetCouncil(Hero hero)
        {
            Clan clan = hero.Clan;
            if (this.Councils.ContainsKey(clan))
                return this.Councils[clan];
            else
            {
                CouncilData council = new CouncilData(clan);
                this.Councils.Add(clan, council);
                return council;
            }
        }

        public CouncilData GetCouncil(Clan clan)
        {
            if (this.Councils.ContainsKey(clan))
                return this.Councils[clan];
            else
            {
                CouncilData council = new CouncilData(clan);
                this.Councils.Add(clan, council);
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
