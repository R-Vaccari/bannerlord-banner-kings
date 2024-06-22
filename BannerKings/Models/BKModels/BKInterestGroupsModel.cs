using BannerKings.Behaviours.Diplomacy;
using BannerKings.Behaviours.Diplomacy.Groups;
using BannerKings.Behaviours.Diplomacy.Groups.Demands;
using BannerKings.Managers.Titles;
using BannerKings.Managers.Titles.Governments;
using BannerKings.Managers.Traits;
using BannerKings.Models.BKModels.Abstract;
using BannerKings.Utils.Extensions;
using BannerKings.Utils.Models;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Models.BKModels
{
    public class BKInterestGroupsModel : GroupsModel
    {
        public override bool WillHeroCreateGroup(DiplomacyGroup group, Hero hero, KingdomDiplomacy diplomacy)
        {
            if (hero == Hero.MainHero || !CanHeroCreateAGroup(hero, diplomacy)) return false;

            if (diplomacy.RadicalGroups.Any(x => x.Equals(group))) return false;

            if (group.CanHeroJoin(hero, diplomacy))
            {
                if (group is RadicalGroup)
                {
                    float chance = CalculateHeroJoinChance(hero, group, diplomacy).ResultNumber;
                    if (chance > 0f && chance < MBRandom.RandomFloat)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public ExplainedNumber CalculateFinancialCompromiseCost(Hero fulfiller, int minimumCost, float factor, bool explanations = false)
        {
            ExplainedNumber result = new ExplainedNumber(minimumCost, explanations);
            ExplainedNumber income = BannerKingsConfig.Instance.ClanFinanceModel.CalculateClanIncome(fulfiller.Clan);

            result.Add(income.ResultNumber * 10f, new TextObject("{=Ssi15mFy}Revenues of {CLAN}")
                .SetTextVariable("CLAN", fulfiller.Clan.Name));
            result.AddFactor(factor - 1f, new TextObject("{=ZQgfSkQ8}Generosity of the group leader"));
            return result;
        }

        public ExplainedNumber CalculateLeverageInfluenceCost(Hero fulfiller, int minimumInfluence, float factor, bool explanations = false)
        {
            ExplainedNumber result = new ExplainedNumber(minimumInfluence, explanations);

            return result;
        }

        public BKExplainedNumber CalculateGroupInfluence(InterestGroup group, bool explanations = false)
        {
            var result = new BKExplainedNumber(0f, explanations);
            result.LimitMin(0f);
            result.LimitMax(1f);

            KingdomDiplomacy diplomacy = group.KingdomDiplomacy;
            float totalPower = 0;
            foreach (var settlement in diplomacy.Kingdom.Settlements)
                if (settlement.Notables != null)
                    foreach (var notable in settlement.Notables)
                        totalPower += notable.Power;

            Dictionary<Clan, float> clanInfluences = new Dictionary<Clan, float>();
            float totalClanInfluence = 0f;
            foreach (var clan in diplomacy.Kingdom.Clans)
            {
                float f = CalculateClanInfluence(clan, diplomacy).ResultNumber;
                totalClanInfluence += f;
                clanInfluences.Add(clan, f);
            }

            int notables = 0;
            float notableInfluence = 0f;
            foreach (var member in group.Members)
            {
                if (member.IsNotable)
                {
                    notableInfluence += 0.25f * (member.Power / totalPower);
                    notables++;
                }

                if (member.Clan != null && member.IsClanLeader())
                {
                    if (!clanInfluences.ContainsKey(member.Clan))
                    {
                        continue;
                    }

                    result.Add(0.75f * (clanInfluences[member.Clan] / totalClanInfluence), member.Clan.Name);
                }
            }

            if (notables > 0) result.Add(notableInfluence, new TextObject("{=Ce2gcy3j}Dignataries (x{MEMBERS})")
                    .SetTextVariable("MEMBERS", notables));

            foreach (var outcome in group.RecentOucomes)
                if (outcome.Success && outcome.Enabled)
                    result.Add(-0.1f, outcome.Explanation);

            if (group.StringId == DefaultInterestGroup.Instance.Commoners.StringId)
                foreach (var fief in diplomacy.Kingdom.Fiefs)
                    if (fief.Loyalty <= 25f) result.Add(CalculateTownInfluence(fief).ResultNumber / diplomacy.Kingdom.Fiefs.Count,
                            new TextObject("{=K0pRPse7}{TOWN}'s loyalty is low"));

            return result;
        }

        public BKExplainedNumber CalculateClanInfluence(Clan clan, KingdomDiplomacy diplomacy, bool explanations = false)
        {
            var result = new BKExplainedNumber(0f, explanations);
            result.LimitMin(0f);
            result.Add(TaleWorlds.CampaignSystem.Campaign.Current.Models.DiplomacyModel.GetClanStrength(clan), GameTexts.FindText("str_notable_power"));
           
            if (clan.Gold > 0)
            {
                result.Add(clan.Gold / 10f, GameTexts.FindText("str_wealth"));
            }

            result.Add(clan.Influence * 5f, 
                new TextObject("{=wwYABLRd}Clan Influence Limit"));

            return result;
        }

        public BKExplainedNumber CalculateTownInfluence(Town town, bool explanations = false)
        {
            var result = new BKExplainedNumber(0f, explanations);
            result.LimitMin(0f);
            result.Add(town.Prosperity);

            return result;
        }

        public BKExplainedNumber CalculateGroupSupport(InterestGroup group, bool explanations = false)
        {
            var result = new BKExplainedNumber(0f, explanations);
            result.LimitMin(0f);
            result.LimitMax(1f);
            KingdomDiplomacy diplomacy = group.KingdomDiplomacy;
            Hero sovereign = diplomacy.Kingdom.Leader;

            result.Add(diplomacy.Legitimacy * group.LegitimacyFactor, new TextObject("Legitimacy"));

            if (group.Leader != null)
            {
                result.Add(0.25f * group.Leader.GetRelation(sovereign) * 0.01f, new TextObject("{=uYDaqbt6}Approval by {HERO}")
                    .SetTextVariable("HERO", group.Leader.Name));
            }

            float approval = 0f;
            float notableApproval = 0f;
            int notables = 0;
            int otherMembers = 0;
            foreach (var member in group.Members)
            {
                if (member != group.Leader)
                {
                    float approvalResult = (0.25f / group.Members.Count) * member.GetRelation(sovereign) * 0.01f;
                    if (member.IsNotable)
                    {
                        notableApproval += approvalResult;
                        notables++;
                    }
                    else
                    {
                        approval += approvalResult;
                        otherMembers++;
                    }
                }
            }

            foreach (var outcome in group.RecentOucomes)
            {
                result.Add(outcome.Success ? 0.15f : -0.15f, outcome.Explanation);
            }

            if (otherMembers > 0)
            {
                result.Add(approval, new TextObject("{=ShSqfhkh}Approval by nobility members (x{MEMBERS})")
                    .SetTextVariable("MEMBERS", otherMembers));
            }

            if (notables > 0)
            {
                result.Add(notableApproval, new TextObject("{=JXfGqamr}Approval by dignataries (x{MEMBERS})")
                    .SetTextVariable("MEMBERS", notables));
            }

            float supportedPolicies = 0f;
            int supportedPoliciesCount = 0;
            foreach (var policy in group.SupportedPolicies)
            {
                if (diplomacy.Kingdom.ActivePolicies.Contains(policy))
                {
                    supportedPolicies += 0.25f / group.SupportedPolicies.Count;
                    supportedPoliciesCount++;
                }
            }
            result.Add(supportedPolicies, new TextObject("{=hxOViTwY}Endorsed policies active (x{COUNT})")
                .SetTextVariable("COUNT", supportedPoliciesCount));

            float shunnedPolicies = 0f;
            int shunnedPoliciesCount = 0;
            foreach (var policy in group.ShunnedPolicies)
            {
                if (diplomacy.Kingdom.ActivePolicies.Contains(policy))
                {
                    shunnedPolicies += -0.25f / group.ShunnedPolicies.Count;
                    shunnedPoliciesCount++;
                }
            }
            result.Add(-shunnedPolicies, new TextObject("{=ETikwXjV}Shunned policies active (x{COUNT})")
                .SetTextVariable("COUNT", shunnedPoliciesCount));

            FeudalTitle title = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(diplomacy.Kingdom);
            if (title != null)
            {
                float supportedLaws = 0f;
                int supportedLawsCount = 0;
                foreach (var law in group.SupportedLaws)
                {
                    if (title.Contract.IsLawEnacted(law))
                    {
                        supportedLaws += 0.25f / group.SupportedLaws.Count;
                        supportedLawsCount++;
                    }
                }
                result.Add(supportedLaws, new TextObject("{=MP5kk91f}Endorsed laws active (x{COUNT})")
                    .SetTextVariable("COUNT", supportedLawsCount));

                float shunnedLaws = 0f;
                int shunnedLawsCount = 0;
                foreach (var law in group.ShunnedLaws)
                {
                    if (title.Contract.IsLawEnacted(law))
                    {
                        shunnedLaws += 0.25f / group.ShunnedLaws.Count;
                        shunnedLawsCount++;
                    }
                }
                result.Add(-shunnedLaws, new TextObject("{=4UXFSw4t}Shunned laws active (x{COUNT})")
                    .SetTextVariable("COUNT", shunnedLawsCount));
            }

            var rel = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(sovereign);
            bool matchingReligion = false;
            if (group.KingdomDiplomacy.Religion != null)
                if (rel != null && rel.Equals(group.KingdomDiplomacy.Religion))
                    matchingReligion = true;

            if (group.StringId == DefaultInterestGroup.Instance.Traditionalists.StringId)
            {
                if (sovereign.Culture == group.KingdomDiplomacy.Kingdom.Culture)
                    result.Add(0.12f, new TextObject("{=O8tcdKZ9}{HERO} is of traditional culture")
                        .SetTextVariable("HERO", sovereign.Name));

                if (matchingReligion) result.Add(0.12f, new TextObject("{=v3kM9Awv}{HERO} is of traditional faith")
                        .SetTextVariable("HERO", sovereign.Name));
            }

            if (group.StringId == DefaultInterestGroup.Instance.Zealots.StringId)
            {
                if (!matchingReligion) result.Add(-0.4f, new TextObject("{=xL5afAGh}{HERO} is not of traditional faith")
                        .SetTextVariable("HERO", sovereign.Name));
                else
                {
                    foreach (var tuple in rel.Faith.Traits)
                    {
                        TraitObject trait = tuple.Key;
                        int traitLevel = sovereign.GetTraitLevel(trait);
                        if (traitLevel != 0)
                        {
                            result.Add(traitLevel * 0.1f * (tuple.Value ? 1f : -1f), trait.Name);
                        }
                    }
                }
            }

            return result;
        }

        public BKExplainedNumber CalculateHeroInfluence(DiplomacyGroup group, KingdomDiplomacy diplomacy,
            Hero hero, bool explanations = false)
        {
            var result = new BKExplainedNumber(0f, explanations);
            float totalPower = 0;
            foreach (var settlement in diplomacy.Kingdom.Settlements)
            {
                if (settlement.Notables != null)
                {
                    foreach (var notable in settlement.Notables)
                    {
                        totalPower += notable.Power;
                    }
                }
            }

            if (hero.IsNotable)
            {
                result.Add((hero.Power / totalPower), GameTexts.FindText("str_notable_power"));
            }

            if (hero.Clan != null)
            {
                result.Add((hero.IsClanLeader() ? 1f : 0.1f) * CalculateClanInfluence(hero.Clan, diplomacy).ResultNumber, hero.Clan.Name);
            }

            return result;
        }

        public bool CanHeroJoinAGroup(Hero hero, KingdomDiplomacy diplomacy)
        {
            if (diplomacy.Kingdom != hero.MapFaction)
            {
                return false;
            }

            if (hero.IsLord && hero.MapFaction.Leader == hero)
            {
                return false;
            }

            if (hero.IsChild || hero.IsDead)
            {
                return false;
            }

            return true;
        }

        public BKExplainedNumber InviteToGroupInfluenceCost(DiplomacyGroup group, Hero invitee, KingdomDiplomacy diplomacy, bool explanations = false)
        {
            BKExplainedNumber result = new BKExplainedNumber(75f, explanations);

            if (invitee.Clan != null)
            {
                Dictionary<Clan, float> clanInfluences = new Dictionary<Clan, float>();
                float totalClanInfluence = 0f;
                foreach (var clan in diplomacy.Kingdom.Clans)
                {
                    float f = CalculateClanInfluence(clan, diplomacy).ResultNumber;
                    totalClanInfluence += f;
                    clanInfluences.Add(clan, f);
                }

                result.Add(200f * (clanInfluences[invitee.Clan] / totalClanInfluence), new TextObject("{=8JtaP3Ak}Political relevance of {CLAN}")
                    .SetTextVariable("CLAN", invitee.Clan.Name));

                float willingness = CalculateHeroJoinChance(invitee, group, diplomacy).ResultNumber;
                result.AddFactor(-willingness * 0.5f, new TextObject("{=JHcib2AV}Willingness to join this group"));
            }

            float leaderCap = BannerKingsConfig.Instance.InfluenceModel.CalculateInfluenceCap(group.Leader.Clan).ResultNumber;
            result.Add(leaderCap * 0.07f, new TextObject("{=1RD1OWYP}Influence limit of {CLAN}")
                .SetTextVariable("CLAN", group.Leader.Clan.Name));      

            return result;
        }

        public bool CanHeroJoinARadicalGroup(Hero hero, KingdomDiplomacy diplomacy) => CanHeroJoinAGroup(hero, diplomacy) && hero.IsClanLeader();
        

        public bool CanHeroCreateAGroup(Hero hero, KingdomDiplomacy diplomacy)
        {
            return CanHeroJoinAGroup(hero, diplomacy) && hero.IsClanLeader() && diplomacy.Kingdom.Leader != hero;
        }

        public override BKExplainedNumber CalculateHeroJoinChance(Hero hero, DiplomacyGroup group, KingdomDiplomacy diplomacy, bool explanations = false)
        {
            var result = new BKExplainedNumber(0f, explanations);
            result.LimitMin(-1f);
            result.LimitMax(1f);
            if (!CanHeroJoinAGroup(hero, diplomacy))
            {
                return result;
            }
            
            return group.IsInterestGroup ? CalculateHeroJoinInterestGroup(hero, (InterestGroup)group, diplomacy, ref result) :
               CalculateHeroJoinRadicalGroup(hero, (RadicalGroup)group, diplomacy, ref result);
        }

        public override BKExplainedNumber CalculateHeroJoinRadicalGroup(Hero hero, RadicalGroup group, KingdomDiplomacy diplomacy, ref BKExplainedNumber result)
        {
            Dictionary<Clan, float> clanInfluences = new Dictionary<Clan, float>();
            float totalClanInfluence = 0f;
            foreach (var clan in diplomacy.Kingdom.Clans)
            {
                float f = CalculateClanInfluence(clan, diplomacy).ResultNumber;
                totalClanInfluence += f;
                clanInfluences.Add(clan, f);
            }

            result.Add(-0.1f + (clanInfluences[hero.Clan] / totalClanInfluence), new TextObject("{=!}Reluctance"));
            Hero ruler = diplomacy.Kingdom.Leader;
            float support = -MBMath.Map(diplomacy.Legitimacy, 0f, 1f, -0.5f, 0.5f);
            result.Add(support, new TextObject("{=KDH6VoKQ}Legitimacy of {HERO}")
                .SetTextVariable("HERO", ruler.Name));

            float relation = -MBMath.Map(hero.GetRelation(ruler), -100f, 100f, -0.5f, 0.5f);
            result.Add(relation, new TextObject("{=nnYfQnWv}{HERO1}`s opinion of {HERO2}")
                    .SetTextVariable("HERO1", hero.Name)
                    .SetTextVariable("HERO2", ruler.Name));

            InterestGroup interestGroup = diplomacy.GetHeroGroup(hero);
            if (interestGroup != null)
            {
                float groupSupport = -MBMath.Map(interestGroup.Support.ResultNumber, 0f, 1f, -0.1f, 0.1f);
                result.Add(groupSupport, new TextObject("{=!}Support from interest group ({GROUP})")
                        .SetTextVariable("GROUP", interestGroup.Name));
            }

            if (group.Leader != null && hero != group.Leader)
            {
                float relationLeader = -MBMath.Map(hero.GetRelation(group.Leader), -100f, 100f, -0.25f, 0.25f);
                result.Add(relationLeader, new TextObject("{=nnYfQnWv}{HERO1}`s opinion of {HERO2}")
                    .SetTextVariable("HERO1", hero.Name)
                    .SetTextVariable("HERO2", group.Leader.Name));
            }

            bool positiveResult = result.ResultNumber > 0f;
            float honor = hero.GetTraitLevel(DefaultTraits.Honor);
            result.AddFactor(honor * 0.2f * (positiveResult ? -1 : 1), DefaultTraits.Honor.Name);

            float ambition = hero.GetTraitLevel(BKTraits.Instance.Ambitious);
            result.AddFactor(ambition * 0.3f * (positiveResult ? 1 : -1), BKTraits.Instance.Ambitious.Name);

            if (group.StringId == DefaultRadicalGroups.Instance.Claimant.StringId)
            {
                ClaimantDemand demand = (ClaimantDemand)group.CurrentDemand;
                if (demand.Claimant != null)
                {
                    if (demand.Claimant != hero)
                    {
                        float relationClaimant = MBMath.Map(hero.GetRelation(demand.Claimant), -100f, 100f, -0.25f, 0.25f);
                        result.Add(relationClaimant, new TextObject("{=nnYfQnWv}{HERO1}`s opinion of {HERO2}")
                            .SetTextVariable("HERO1", hero.Name)
                            .SetTextVariable("HERO2", demand.Claimant.Name));
                    }
                    else
                    {
                        result.Add(0.2f + (ambition * 0.1f), new TextObject("{=s7sxJgWg}{HERO1}`s ambition of ruling")
                            .SetTextVariable("HERO1", hero.Name)
                                                    .SetTextVariable("HERO2", demand.Claimant.Name));
                    }
                }
            }
         
            return result;
        }

        public BKExplainedNumber CalculateHeroJoinInterestGroup(Hero hero, InterestGroup group, KingdomDiplomacy diplomacy, ref BKExplainedNumber result)
        {
            if (hero.IsLord && !group.AllowsNobles)
            {
                return result;
            }

            if (!hero.IsLord && !group.AllowsCommoners)
            {
                return result;
            }

            if (group.Equals(DefaultInterestGroup.Instance.Royalists))
            {
                Hero leader = hero.MapFaction.Leader;
                float relation = hero.GetRelation(leader);
                result.Add(relation * 0.003f);
            }

            if (group.Equals(DefaultInterestGroup.Instance.Traditionalists))
            {
                Hero leader = hero.MapFaction.Leader;
                float relation = hero.GetRelation(leader);
                result.Add(relation * 0.001f);

                if (hero.Clan != null)
                {
                    result.Add(hero.Clan.Tier * 0.02f);
                }

                if (hero.Culture == hero.MapFaction.Culture)
                {
                    result.Add(0.1f);
                }
            }

            if (group.Equals(DefaultInterestGroup.Instance.Oligarchists))
            {
                result.Add(hero.Clan.Tier * 0.05f);
                var title = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(hero);
                if (title != null)
                {
                    result.Add((5f - (int)title.TitleType) * 0.25f);
                }
            }

            if (group.PreferredOccupations.Contains(hero.Occupation))
            {
                result.Add(0.2f);
            }

            var rel = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(hero);
            if (rel != null && rel.Equals(diplomacy.Religion))
            {
                result.Add(0.075f);
            }

            result.Add(hero.GetTraitLevel(group.MainTrait) * 0.15f);
            return result;
        }

        public bool IsGroupAdequateForKingdom(KingdomDiplomacy diplomacy, InterestGroup group)
        {
            var title = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(diplomacy.Kingdom);
            if (title != null)
            {
                if (group.Equals(DefaultInterestGroup.Instance.Royalists) && (title.Contract.Government == DefaultGovernments.Instance.Feudal 
                    || title.Contract.Government == DefaultGovernments.Instance.Imperial))
                {
                    return true;
                }

                if (group.Equals(DefaultInterestGroup.Instance.Traditionalists) && title.Contract.Government == DefaultGovernments.Instance.Tribal)
                {
                    return true;
                }
            }

            if (group.Equals(DefaultInterestGroup.Instance.Commoners) || group.Equals(DefaultInterestGroup.Instance.Oligarchists))
            {
                return true;
            }

            if (group.Equals(DefaultInterestGroup.Instance.Zealots))
            {
                return diplomacy.Religion != null;
            }

            return false;
        }
    }
}
