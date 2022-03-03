using BannerKings.Managers.Court;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace BannerKings.Managers
{
    public class CourtManager
    {
        private Dictionary<Hero, Council> COUNCILS { get; set; }

        public CourtManager(Dictionary<Hero, Council> councils)
        {
            this.COUNCILS = councils;
        }

        public void ApplyCouncilEffect(ref ExplainedNumber result, Hero settlementOwner, CouncilPosition position, float maxEffect, bool factor)
        {
            Council council = this.GetCouncil(settlementOwner);
            float competence = council.GetCompetence(position);
            if (competence != 0f)
            {
                if (!factor) result.Add(maxEffect * competence, new TextObject("{=!}Council Effect"));
                else result.AddFactor(maxEffect * competence, new TextObject("{=!}Council Effect"));
            }
        }

        public Council GetCouncil(Hero hero)
        {
            if (this.COUNCILS.ContainsKey(hero))
                return this.COUNCILS[hero];
            else
            {
                Council council = new Council(hero);
                this.COUNCILS.Add(hero, council);
                return council;
            }
        }
    }
}
