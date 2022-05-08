using BannerKings.Managers.Court;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;
using System.Linq;

namespace BannerKings.Managers
{
    public class CourtManager
    {
        [SaveableProperty(1)]
        private Dictionary<Clan, CouncilData> Councils { get; set; }

        public CourtManager(Dictionary<Clan, CouncilData> councils)
        {
            Councils = councils;
        }

        public void ApplyCouncilEffect(ref ExplainedNumber result, Hero settlementOwner, CouncilPosition position, float maxEffect, bool factor)
        {
            CouncilData council = GetCouncil(settlementOwner);
            float competence = council.GetCompetence(position);
            if (competence != 0f)
            {
                if (!factor) result.Add(maxEffect * competence, new TextObject("{=!}Council Effect"));
                else result.AddFactor(maxEffect * competence, new TextObject("{=!}Council Effect"));
            }
        }

        public int GetCouncilEffectInteger(Hero settlementOwner, CouncilPosition position, float maxEffect)
        {
            CouncilData council = GetCouncil(settlementOwner);
            float competence = council.GetCompetence(position);
            return (int)(maxEffect * competence);
        }

        public CouncilData GetCouncil(Hero hero)
        {
            Clan clan = hero.Clan;
            if (Councils.ContainsKey(clan))
                return Councils[clan];
            CouncilData council = new CouncilData(clan);
            Councils.Add(clan, council);
            return council;
        }

        public CouncilData GetCouncil(Clan clan)
        {
            if (Councils.ContainsKey(clan))
                return Councils[clan];
            CouncilData council = new CouncilData(clan);
            Councils.Add(clan, council);
            return council;
        }

        public CouncilMember GetHeroPosition(Hero hero)
        {
            if (hero.IsNoble && (hero.Clan == null || hero.Clan.Kingdom == null) || hero.IsChild ||
                hero.IsDead) return null;
            Kingdom kingdom = null;
            if ((hero.IsNoble || hero.IsWanderer) && hero.Clan != null) kingdom = hero.Clan.Kingdom;
            else if (hero.CurrentSettlement != null && hero.CurrentSettlement.OwnerClan != null) 
                kingdom = hero.CurrentSettlement.OwnerClan.Kingdom;

            Clan targetClan = null;
            if (kingdom != null)
            {
                List<Clan> clans = Councils.Keys.ToList();
                foreach (Clan clan in clans)
                    if (Councils[clan].GetMembers().Contains(hero))
                    {
                        targetClan = clan;
                        break;
                    }
            }

            if (targetClan != null)
                return Councils[targetClan].GetHeroPosition(hero);

            return null;
        }

        public void UpdateCouncil(Clan clan)
        {
            CouncilData data = GetCouncil(clan.Leader);
            data.Update(null);
        }
    }
}
