using System.Collections.Generic;
using System.Linq;
using BannerKings.Managers.Court;
using BannerKings.Managers.Court.Members;
using BannerKings.Managers.Institutions.Religions.Leaderships;
using BannerKings.Managers.Skills;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace BannerKings.Managers
{
    public class CourtManager
    {
        public static readonly int ON_FIRED_RELATION = -12;
        public static readonly int ON_REJECTED_RELATION = -6;
        public static readonly int ON_HIRED_RELATION = 6;

        public CourtManager(Dictionary<Clan, CouncilData> councils)
        {
            Councils = councils;
        }

        [SaveableProperty(1)] private Dictionary<Clan, CouncilData> Councils { get; set; }

        public void PostInitialize()
        {
            foreach (var council in Councils)
            {
                council.Value.PostInitialize();
                if (council.Value.Peerage == null)
                {
                    council.Value.SetPeerage(Peerage.GetAdequatePeerage(council.Key));
                }
            }
        }

        public void ApplyCouncilEffect(ref ExplainedNumber result, Hero settlementOwner, CouncilPosition position,
            float maxEffect, bool factor)
        {
            var council = GetCouncil(settlementOwner);
            var competence = council.GetCompetence(position);
            if (competence != 0f)
            {
                if (!factor)
                {
                    result.Add(maxEffect * competence, new TextObject("{=5TbiMahb}Council effect"));
                }
                else
                {
                    result.AddFactor(maxEffect * competence, new TextObject("{=5TbiMahb}Council effect"));
                }
            }
        }

        public int GetCouncilEffectInteger(Hero settlementOwner, CouncilPosition position, float maxEffect)
        {
            var council = GetCouncil(settlementOwner);
            var competence = council.GetCompetence(position);
            return (int) (maxEffect * competence);
        }

        public void CreateCouncil(Clan clan)
        {
            if (Councils.ContainsKey(clan))
            {
                return;
            }

            Councils.Add(clan, new CouncilData(clan));
        }

        public CouncilData GetCouncil(Hero hero)
        {
            var clan = hero.Clan;
            if (Councils.ContainsKey(clan))
            {
                return Councils[clan];
            }

            var council = new CouncilData(clan);
            Councils.Add(clan, council);
            return council;
        }

        public CouncilData GetCouncil(Clan clan)
        {
            if (Councils.ContainsKey(clan))
            {
                return Councils[clan];
            }

            var council = new CouncilData(clan);
            Councils.Add(clan, council);
            return council;
        }

        public CouncilPosition GetHeroPosition(Hero hero)
        {
            if ((hero.IsLord && hero.Clan?.Kingdom == null) || hero.IsChild ||
                hero.IsDead)
            {
                return null;
            }

            Kingdom kingdom = null;
            if ((hero.IsLord || hero.IsWanderer) && hero.Clan != null)
            {
                kingdom = hero.Clan.Kingdom;
            }
            else if (hero.CurrentSettlement is {OwnerClan: { }})
            {
                kingdom = hero.CurrentSettlement.OwnerClan.Kingdom;
            }

            Clan targetClan = null;
            if (kingdom != null)
            {
                var clans = Councils.Keys.ToList();
                foreach (var clan in clans)
                {
                    if (Councils[clan].GetMembers().Contains(hero))
                    {
                        targetClan = clan;
                        break;
                    }
                }
            }

            if (targetClan != null)
            {
                return Councils[targetClan].GetHeroPosition(hero);
            }

            return null;
        }

        public int GetCouncilloursCount(Clan clan)
        {
            return GetCouncil(clan.Leader).GetOccupiedPositions().Count;
        }

        public void UpdateCouncil(Clan clan)
        {
            var data = GetCouncil(clan.Leader);
            data.Update(null);
        }

        private void CheckReligionRankChange(CouncilAction action)
        {
            var rel = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(action.Council.Owner);
            if (rel != null && rel.Leadership.GetType() == typeof(KinshipLeadership) &&
                action.TargetPosition.Equals(DefaultCouncilPositions.Instance.Spiritual))
            {
                var currentClergyman = action.TargetPosition.Member;
                if (currentClergyman != null)
                {
                    rel.Leadership.ChangeClergymanRank(rel,
                        BannerKingsConfig.Instance.ReligionsManager.GetClergymanFromHeroHero(currentClergyman),
                        rel.Faith.GetIdealRank(
                            currentClergyman.CurrentSettlement ?? currentClergyman.BornSettlement,
                            false));
                }

                var newClergyman = action.ActionTaker;
                if (newClergyman != null)
                {
                    rel.Leadership.ChangeClergymanRank(rel,
                        BannerKingsConfig.Instance.ReligionsManager.GetClergymanFromHeroHero(newClergyman),
                        rel.Faith.GetMaxClergyRank());
                }
            }
        }

        public void AddHeroToCouncil(CouncilAction action)
        {
            if (action.TargetPosition == null || action.ActionTaker == null || !action.Possible)
            {
                return;
            }

            if (action.TargetPosition.Member != null)
            {
                ChangeRelationAction.ApplyRelationChangeBetweenHeroes(action.Council.Owner, action.TargetPosition.Member,
                    ON_FIRED_RELATION);
            }

            CheckReligionRankChange(action);
            if (action.ActionTaker == null)
            {
                return;
            }

            if (action.ActionTaker == Hero.MainHero)
            {
                MBInformationManager.AddQuickInformation(
                    new TextObject("{=f2V1XRaf}{OWNER} has appointed you as their {POSITION}.")
                        .SetTextVariable("OWNER", action.Council.Owner.Name)
                        .SetTextVariable("POSITION", action.TargetPosition.Name),
                    0, action.Council.Owner.CharacterObject, "event:/ui/notification/relation");
            }

            action.TargetPosition.SetMember(action.ActionTaker);
            if (action.ActionTaker.Clan != null)
            {
                GainKingdomInfluenceAction.ApplyForDefault(action.ActionTaker, -action.Influence);
            }
            else if (action.ActionTaker.IsNotable)
            {
                action.ActionTaker.AddPower(-action.Influence);
            }

            ChangeRelationAction.ApplyRelationChangeBetweenHeroes(action.Council.Owner, action.ActionTaker,
                ON_HIRED_RELATION);
            action.ActionTaker.AddSkillXp(BKSkills.Instance.Lordship, 15f);
        }

        public void SwapCouncilPositions(CouncilAction action)
        {
            if (action.TargetPosition == null || action.ActionTaker == null || !action.Possible ||
                action.CurrentPosition == null)
            {
                return;
            }

            var currentCouncilman = action.TargetPosition.Member;
            action.CurrentPosition.SetMember(currentCouncilman);
            action.TargetPosition.SetMember(action.ActionTaker);
            if (action.ActionTaker.Clan != null)
            {
                GainKingdomInfluenceAction.ApplyForDefault(action.ActionTaker, -action.Influence);
            }
            else if (action.ActionTaker.IsNotable)
            {
                action.ActionTaker.AddPower(-action.Influence);
            }

            ChangeRelationAction.ApplyRelationChangeBetweenHeroes(action.TargetPosition.Clan.Leader, action.ActionTaker,
                ON_HIRED_RELATION);
            action.ActionTaker.AddSkillXp(BKSkills.Instance.Lordship, 10f);
        }

        public void RelinquishCouncilPosition(CouncilAction action)
        {
            if (action.TargetPosition == null || !action.Possible)
            {
                return;
            }

            CheckReligionRankChange(action);
            ChangeRelationAction.ApplyRelationChangeBetweenHeroes(action.Council.Owner, action.TargetPosition.Member,
                ON_FIRED_RELATION);
            action.TargetPosition.SetMember(null);
        }
    }
}