using BannerKings.Behaviours.Diplomacy;
using BannerKings.Behaviours.Diplomacy.Groups;
using BannerKings.Managers.Titles;
using BannerKings.Utils.Extensions;
using BannerKings.Utils.Models;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Localization;

namespace BannerKings.Models.BKModels
{
    public class BKInterestGroupsModel
    {
        public BKExplainedNumber CalculateGroupInfluence(InterestGroup group, KingdomDiplomacy diplomacy, bool explanations = false)
        {
            var result = new BKExplainedNumber(0f, explanations);
            result.LimitMin(0f);
            result.LimitMax(1f);

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

            float totalClanInfluence = 0f;
            foreach (var clan in diplomacy.Kingdom.Clans)
            {
                totalClanInfluence += CalculateClanInfluence(clan, diplomacy).ResultNumber;
            }

            foreach (var member in group.Members)
            {
                if (member.IsNotable)
                {
                    result.Add(0.25f * (member.Power / totalPower), member.Name);
                }

                if (member.Clan != null && member.IsClanLeader())
                {
                    float strength = Campaign.Current.Models.DiplomacyModel.GetClanStrength(member.Clan);
                    result.Add(0.75f * (CalculateClanInfluence(member.Clan, diplomacy).ResultNumber / totalClanInfluence), member.Clan.Name);
                }
            }

            if (group.Equals(DefaultInterestGroup.Instance.Commoners))
            {
                foreach (var fief in diplomacy.Kingdom.Fiefs)
                {
                    if (fief.Loyalty < 30f)
                    {
                        result.Add(CalculateTownInfluence(fief).ResultNumber);
                    }
                }
            }

            return result;
        }

        public BKExplainedNumber CalculateClanInfluence(Clan clan, KingdomDiplomacy diplomacy, bool explanations = false)
        {
            var result = new BKExplainedNumber(0f, explanations);
            result.LimitMin(0f);
            result.Add(Campaign.Current.Models.DiplomacyModel.GetClanStrength(clan));
            if (clan.Influence > 0)
            {
                result.Add(clan.Influence * 3f);
            }
           
            if (clan.Gold > 0)
            {
                result.Add(clan.Gold / 10f);
            }

            foreach (var town in clan.Fiefs)
            {
                if (town.Loyalty > 70f)
                {
                    result.Add(CalculateTownInfluence(town).ResultNumber);
                }
            }

            FeudalTitle sovereignTitle = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(diplomacy.Kingdom);
            foreach (var title in BannerKingsConfig.Instance.TitleManager.GetAllDeJure(clan))
            {
                if (!title.IsSovereignLevel && title.sovereign == sovereignTitle)
                {
                    result.Add(5000f / (float)title.type);
                }
            }

            return result;
        }

        public BKExplainedNumber CalculateTownInfluence(Town town, bool explanations = false)
        {
            var result = new BKExplainedNumber(0f, explanations);
            result.LimitMin(0f);
            result.Add(town.Prosperity);

            return result;
        }

        public BKExplainedNumber CalculateGroupSupport(InterestGroup group, KingdomDiplomacy diplomacy, bool explanations = false)
        {
            var result = new BKExplainedNumber(0f, explanations);
            result.LimitMin(0f);
            result.LimitMax(1f);
            Hero sovereign = diplomacy.Kingdom.Leader;
            if (group.Leader != null)
            {
                result.Add(0.25f * group.Leader.GetRelation(sovereign) * 0.01f, new TextObject("{=uYDaqbt6}Approval by {HERO}")
                    .SetTextVariable("HERO", group.Leader.Name));
            }

            float approval = 0f;
            foreach (var member in group.Members)
            {
                if (member != group.Leader)
                {
                    approval += (0.25f / group.Members.Count) * member.GetRelation(sovereign) * 0.01f;
                }
            }
            result.Add(approval, new TextObject("{=!}Approval by group members"));

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
            result.Add(supportedPolicies, new TextObject("{=!}Endorsed policies active (x{COUNT})")
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
            result.Add(-shunnedPolicies, new TextObject("{=!}Shunned policies active (x{COUNT})")
                .SetTextVariable("COUNT", shunnedPoliciesCount));

            FeudalTitle title = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(diplomacy.Kingdom);
            if (title != null)
            {
                float supportedLaws = 0f;
                int supportedLawsCount = 0;
                foreach (var law in group.SupportedLaws)
                {
                    if (title.contract.IsLawEnacted(law))
                    {
                        supportedLaws += 0.25f / group.SupportedLaws.Count;
                        supportedLawsCount++;
                    }
                }
                result.Add(supportedLaws, new TextObject("{=!}Endorsed laws active (x{COUNT})")
                    .SetTextVariable("COUNT", supportedLawsCount));

                float shunnedLaws = 0f;
                int shunnedLawsCount = 0;
                foreach (var law in group.ShunnedLaws)
                {
                    if (title.contract.IsLawEnacted(law))
                    {
                        shunnedLaws += 0.25f / group.ShunnedLaws.Count;
                        shunnedLawsCount++;
                    }
                }
                result.Add(-shunnedLaws, new TextObject("{=!}Shunned laws active (x{COUNT})")
                    .SetTextVariable("COUNT", shunnedLawsCount));
            }

            return result;
        }

        public BKExplainedNumber CalculateHeroInfluence(InterestGroup group, KingdomDiplomacy diplomacy,
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
                result.Add((hero.Power / totalPower));
            }

            if (hero.Clan != null)
            {
                result.Add((hero.IsClanLeader() ? 1f : 0.1f) * CalculateClanInfluence(hero.Clan, diplomacy).ResultNumber);
            }

            return result;
        }

        public BKExplainedNumber CalculateHeroJoinChance(Hero hero, InterestGroup group)
        {
            var result = new BKExplainedNumber(0f, false);
            result.LimitMin(-1f);
            result.LimitMax(1f);
            if (hero.IsLord && !group.AllowsNobles)
            {
                return result;
            }

            if (!hero.IsLord && !group.AllowsCommoners)
            {
                return result;
            }

            if (hero.IsLord && hero.MapFaction.Leader == hero)
            {
                return result;
            }

            if (hero.IsChild || hero.IsDead)
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
                    result.Add((5f - (int)title.type) * 0.25f);
                }
            }

            if (group.PreferredOccupations.Contains(hero.Occupation))
            {
                result.Add(0.2f);
            }
            
            result.Add(hero.GetTraitLevel(group.MainTrait) * 0.15f);
            return result;
        }

        public bool IsGroupAdequateForKingdom(KingdomDiplomacy diplomacy, InterestGroup group)
        {
            var title = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(diplomacy.Kingdom);
            if (title != null)
            {
                if (group.Equals(DefaultInterestGroup.Instance.Royalists) && (title.contract.Government == GovernmentType.Feudal 
                    || title.contract.Government == GovernmentType.Imperial))
                {
                    return true;
                }

                if (group.Equals(DefaultInterestGroup.Instance.Traditionalists) && title.contract.Government == GovernmentType.Tribal)
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
