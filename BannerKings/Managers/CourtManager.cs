using BannerKings.Managers.Court;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;
using System.Linq;
using TaleWorlds.CampaignSystem.Actions;
using BannerKings.Managers.Institutions.Religions;
using BannerKings.Managers.Institutions.Religions.Leaderships;

namespace BannerKings.Managers
{
    public class CourtManager
    {
        [SaveableProperty(1)]
        private Dictionary<Clan, CouncilData> Councils { get; set; }

        public static readonly int ON_FIRED_RELATION = -12;
        public static readonly int ON_REJECTED_RELATION = -6;
        public static readonly int ON_HIRED_RELATION = 6;

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

        private void CheckReligionRankChange(CouncilAction action)
        {
            Religion rel = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(action.Council.Owner);
            if (rel != null && rel.Leadership.GetType() == typeof(KinshipLeadership) && action.TargetPosition.Position == CouncilPosition.Spiritual)
            {
                Hero currentClergyman = action.TargetPosition.Member;
                if (currentClergyman != null)
                    rel.Leadership.ChangeClergymanRank(rel, BannerKingsConfig.Instance.ReligionsManager.GetClergymanFromHeroHero(currentClergyman),
                        rel.Faith.GetIdealRank(currentClergyman.CurrentSettlement != null ? currentClergyman.CurrentSettlement : currentClergyman.BornSettlement,
                        false));

                Hero newClergyman = action.ActionTaker;
                if (newClergyman != null)
                    rel.Leadership.ChangeClergymanRank(rel, BannerKingsConfig.Instance.ReligionsManager.GetClergymanFromHeroHero(newClergyman),
                            rel.Faith.GetMaxClergyRank());
            }
        }

        public void AddHeroToCouncil(CouncilAction action)
        {
            if (action.TargetPosition == null || action.ActionTaker == null || !action.Possible) return;

            if (action.TargetPosition.Member != null)
                ChangeRelationAction.ApplyRelationChangeBetweenHeroes(action.Council.Owner, action.TargetPosition.Member, ON_FIRED_RELATION);

            CheckReligionRankChange(action);
            if (action.ActionTaker == null) return;
            action.TargetPosition.Member = action.ActionTaker;
            if (action.ActionTaker.Clan != null) GainKingdomInfluenceAction.ApplyForDefault(action.ActionTaker, -action.Influence);
            else if (action.ActionTaker.IsNotable) action.ActionTaker.AddPower(-action.Influence);
            ChangeRelationAction.ApplyRelationChangeBetweenHeroes(action.Council.Owner, action.ActionTaker, ON_HIRED_RELATION);
        }

        public void SwapCouncilPositions(CouncilAction action)
        {
            if (action.TargetPosition == null || action.ActionTaker == null || !action.Possible || action.CurrentPosition == null) return;

            Hero currentCouncilman = action.TargetPosition.Member;
            action.CurrentPosition.Member = currentCouncilman;
            action.TargetPosition.Member = action.ActionTaker;
            if (action.ActionTaker.Clan != null) GainKingdomInfluenceAction.ApplyForDefault(action.ActionTaker, -action.Influence);
            else if (action.ActionTaker.IsNotable) action.ActionTaker.AddPower(-action.Influence);
            ChangeRelationAction.ApplyRelationChangeBetweenHeroes(action.TargetPosition.Clan.Leader, action.ActionTaker, ON_HIRED_RELATION);
        }

        public void RelinquishCouncilPosition(CouncilAction action)
        {
            if (action.TargetPosition == null || !action.Possible) return;

            CheckReligionRankChange(action);
            ChangeRelationAction.ApplyRelationChangeBetweenHeroes(action.Council.Owner, action.TargetPosition.Member, ON_FIRED_RELATION);
            action.TargetPosition.Member = null;
        }
    }
}
