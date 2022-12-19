using System;
using System.Collections.Generic;
using System.Linq;
using BannerKings.Managers.Skills;
using BannerKings.Managers.Titles;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Models.BKModels
{
    public class BKTitleModel : IBannerKingsModel
    {
        public ExplainedNumber CalculateEffect(Settlement settlement)
        {
            return new ExplainedNumber();
        }

        public ExplainedNumber GetSuccessionHeirScore(Hero currentLeader, Hero candidate, FeudalContract contract, bool explanations = false)
        {
            var result = new ExplainedNumber(0f, explanations);

            var succession = contract.Succession;
            if (succession == SuccessionType.Imperial)
            {
                result.Add(currentLeader.GetRelation(candidate) / 3f, new TextObject("{=uYDaqbt6}Approval by {HERO}")
                    .SetTextVariable("HERO", currentLeader.Name));

                result.Add(candidate.Age / 2f, new TextObject("Age"));
                result.Add(candidate.GetSkillValue(DefaultSkills.Leadership) * 0.1f, DefaultSkills.Leadership.Name);
                result.Add(candidate.GetSkillValue(DefaultSkills.Tactics) * 0.1f, DefaultSkills.Tactics.Name);
                result.Add(candidate.GetSkillValue(BKSkills.Instance.Lordship) * 0.1f, BKSkills.Instance.Lordship.Name);
                result.Add(candidate.GetSkillValue(DefaultSkills.Charm) * 0.1f, DefaultSkills.Charm.Name);

                if (candidate.Clan == currentLeader.Clan)
                {
                    result.Add(0.15f, candidate.Clan.Name);
                }

                result.AddFactor(candidate.Clan.Tier * 0.05f, GameTexts.FindText("str_clan_tier_bonus"));
            }

            if (succession == SuccessionType.Republic)
            {
                result.Add(Campaign.Current.Models.DiplomacyModel.GetClanStrength(candidate.Clan) / 5, GameTexts.FindText("str_notable_power"));
                result.Add(candidate.Age / 2f, new TextObject("Age"));
                result.Add(candidate.GetSkillValue(DefaultSkills.Leadership) * 0.1f, DefaultSkills.Leadership.Name);
                result.Add(candidate.GetSkillValue(DefaultSkills.Charm) * 0.1f, DefaultSkills.Charm.Name);
                result.Add(candidate.GetSkillValue(DefaultSkills.Steward) * 0.1f, DefaultSkills.Steward.Name);

                result.AddFactor(candidate.Clan.Tier * 0.08f, GameTexts.FindText("str_clan_tier_bonus"));
            }

            if (succession == SuccessionType.Hereditary_Monarchy)
            {
                if (GetInheritanceCandidates(currentLeader).Contains(candidate))
                {
                    result = GetInheritanceHeirScore(currentLeader, candidate, contract, explanations);
                }
            }

            if (succession == SuccessionType.Elective_Monarchy)
            {
                var government = contract.Government;
                if (government == GovernmentType.Tribal)
                {
                    result.Add(Campaign.Current.Models.DiplomacyModel.GetClanStrength(candidate.Clan) / 2, GameTexts.FindText("str_notable_power"));
                }

                if (government == GovernmentType.Feudal)
                {
                    result.Add(candidate.GetSkillValue(BKSkills.Instance.Lordship) * 0.1f, BKSkills.Instance.Lordship.Name);
                }
            }

            if (candidate.Culture != currentLeader.Clan.Kingdom.Culture)
            {
                result.AddFactor(-0.2f, GameTexts.FindText("str_culture"));
            }

            return result;
        }

        public ExplainedNumber GetInheritanceHeirScore(Hero currentLeader, Hero candidate, FeudalContract contract, bool explanations = false)
        {
            var result = new ExplainedNumber(0f, explanations);


            if (contract == null)
            {
                contract = new FeudalContract(null, null,
                    GovernmentType.Feudal, SuccessionType.Elective_Monarchy, InheritanceType.Primogeniture, GenderLaw.Agnatic);
            }

            GenderLaw genderLaw = contract.GenderLaw;
            InheritanceType inheritance = contract.Inheritance;
            if (inheritance == InheritanceType.Seniority)
            {
                result.Add(candidate.Age, new TextObject("Age"));
            }
            else
            {
                if (currentLeader.Children.Contains(candidate))
                {
                    result.Add(300f, GameTexts.FindText(candidate.IsFemale ? "str_daughter" : "str_son"));
                }
                else if (currentLeader.Spouse == candidate)
                {
                    result.Add(150f, GameTexts.FindText("str_spouse"));
                }
                else if (currentLeader.Siblings.Contains(candidate))
                {
                    result.Add(100f, GameTexts.FindText(candidate.IsFemale ? "str_bigsister" : "str_bigbrother"));
                }

                if (inheritance == InheritanceType.Primogeniture)
                {
                    result.Add(candidate.Age, new TextObject("Age"));
                }

                if (inheritance == InheritanceType.Ultimogeniture)
                {
                    result.Add(-candidate.Age, new TextObject("Age"));
                }

                if (candidate.CharacterObject != null && candidate.CharacterObject.OriginalCharacter != null &&
                   candidate.CharacterObject.OriginalCharacter.IsTemplate)
                {
                    result.AddFactor(-0.5f, new TextObject("{=9RG3GwJD}Common born"));
                }
            }

            if (genderLaw == GenderLaw.Agnatic && candidate.IsFemale)
            {
                result.AddFactor(-0.9f, GameTexts.FindText("str_bk_agnatic"));
            }

            return result;
        }

        public IEnumerable<KeyValuePair<Hero, ExplainedNumber>> CalculateSuccessionLine(FeudalContract contract, Clan clan, Hero victim = null, int count = 6)
        {
            var leader = victim != null ? victim : clan.Leader;
            var candidates = BannerKingsConfig.Instance.TitleModel.GetSuccessionCandidates(leader, contract);
            var explanations = new Dictionary<Hero, ExplainedNumber>();

            foreach (Hero hero in candidates)
            {
                var explanation = BannerKingsConfig.Instance.TitleModel.GetSuccessionHeirScore(leader,
                    hero, contract, true);
                explanations.Add(hero, explanation);
            }

            return (from x in explanations
                    orderby x.Value.ResultNumber descending
                    select x).Take(count);
        }

        public IEnumerable<KeyValuePair<Hero, ExplainedNumber>> CalculateInheritanceLine(Clan clan, Hero victim = null, int count = 6)
        {
            var leader = victim != null ? victim : clan.Leader;
            var candidates = BannerKingsConfig.Instance.TitleModel.GetInheritanceCandidates(leader);
            var explanations = new Dictionary<Hero, ExplainedNumber>();
            var clanTitle = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(leader);

            foreach (Hero hero in candidates)
            {
                var contract = clanTitle != null ? clanTitle.contract : null;
                var explanation = BannerKingsConfig.Instance.TitleModel.GetInheritanceHeirScore(leader,
                    hero, contract, true);
                explanations.Add(hero, explanation);
            }

            return (from x in explanations
                    orderby x.Value.ResultNumber descending
                    select x).Take(count);
        }

        public HashSet<Hero> GetSuccessionCandidates(Hero currentLeader, FeudalContract contract)
        {
            var list = new List<Hero>();
            var succession = contract.Succession;

            if (succession == SuccessionType.Hereditary_Monarchy)
            {
                foreach (Hero hero in GetInheritanceCandidates(currentLeader))
                {
                    list.Add(hero);
                }
            }
            else
            {
                var clans = new List<Clan>();
                if (currentLeader.Clan.Kingdom != null)
                {
                    clans = (from t in currentLeader.Clan.Kingdom.Clans
                            where !t.IsEliminated && !t.IsUnderMercenaryService
                            select t).ToList();
                }

                foreach (var clan in clans)
                {
                    if (clan == currentLeader.Clan && succession == SuccessionType.Imperial)
                    {
                        list.AddRange(GetInheritanceCandidates(currentLeader));
                    }
                    else if (clan != currentLeader.Clan)
                    {
                        list.Add(clan.Leader);
                    }
                }
            }

            return list;
        }

        public List<Hero> GetInheritanceCandidates(Hero currentLeader)
        {
            var list = new List<Hero>();
            foreach (var x in currentLeader.Clan.Heroes)
            {
                if (!x.IsChild && x != currentLeader && x.IsAlive && x.Occupation == Occupation.Lord)
                {
                    list.Add(x);
                }
            }

            return list;
        }

        public ExplainedNumber GetGrantKnighthoodCost(Hero grantor)
        {
            var result = new ExplainedNumber(120f, true);
            var highest = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(grantor);
            var extra = 0f;

            if (highest is {type: < TitleType.Barony})
            {
                extra = highest.type switch
                {
                    TitleType.County => 30f,
                    TitleType.Dukedom => 60f,
                    TitleType.Kingdom => 100f,
                    _ => 180f
                };
            }

            if (extra != 0f)
            {
                result.Add(extra, new TextObject("{=Jh6FdJFE}Highest title level"));
            }

            if (grantor.GetPerkValue(BKPerks.Instance.LordshipAccolade))
            {
                result.AddFactor(-0.15f, BKPerks.Instance.LordshipAccolade.Name);
            }

            return result;
        }

        public TitleAction GetFoundKingdom(Kingdom faction, Hero founder)
        {
            var foundAction = new TitleAction(ActionType.Found, null, founder)
            {
                Gold = 500000 + BannerKingsConfig.Instance.ClanFinanceModel.CalculateClanIncome(founder.Clan).ResultNumber * CampaignTime.DaysInYear,
                Influence = 1000 + BannerKingsConfig.Instance.InfluenceModel.CalculateInfluenceChange(founder.Clan).ResultNumber * CampaignTime.DaysInYear * 0.1f,
                Renown = 100
            };

            if (faction == null)
            {
                foundAction.Possible = false;
                foundAction.Reason = new TextObject("{=JDFpx1eN}No kingdom.");
                return foundAction;
            }

            var title = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(faction);
            if (title != null)
            {
                foundAction.Possible = false;
                foundAction.Reason = new TextObject("{=eTMvobFw}Faction sovereign title already exists.");
                return foundAction;
            }

            if (faction.Leader != founder)
            {
                foundAction.Possible = false;
                foundAction.Reason = new TextObject("{=Z2tSyHfC}Not leader of current faction.");
                return foundAction;
            }

            var titles = BannerKingsConfig.Instance.TitleManager.GetAllDeJure(founder);
            if (titles.Any(x => x.type <= TitleType.Kingdom))
            {
                foundAction.Possible = false;
                foundAction.Reason = new TextObject("{=t8HXoFk7}Cannot found a kingdom while already being a de Jure sovereign.");
                return foundAction;
            }

            if (!titles.Any(x => x.type <= TitleType.Dukedom))
            {
                foundAction.Possible = false;
                foundAction.Reason = new TextObject("{=Ac5LJzsc}Cannot found a kingdom without a de Jure duke level title.");
                return foundAction;
            }

            if (faction.Clans.Count < 3)
            {
                foundAction.Possible = false;
                foundAction.Reason = new TextObject("{=ZTj1JVuN}Cannot found a kingdom for a faction with less than 3 clans.");
                return foundAction;
            }

            if (founder.Gold < foundAction.Gold || founder.Clan.Influence < foundAction.Influence)
            {
                foundAction.Possible = false;
                foundAction.Reason = new TextObject("{=oXeDiAaA}You lack the required resources.");
                return foundAction;
            }

            foundAction.Possible = true;
            foundAction.Reason = new TextObject("{=7x3HJ29f}Kingdom can be founded.");
            ApplyDiscounts(foundAction);
            return foundAction;
        }

        public TitleAction GetAction(ActionType type, FeudalTitle title, Hero taker, Hero receiver = null)
        {
            return type switch
            {
                ActionType.Usurp => GetUsurp(title, taker),
                ActionType.Revoke => GetRevoke(title, taker),
                ActionType.Claim => GetClaim(title, taker),
                _ => GetGrant(title, taker)
            };
        }

        private TitleAction GetClaim(FeudalTitle title, Hero claimant)
        {
            var claimAction = new TitleAction(ActionType.Claim, title, claimant)
            {
                Gold = GetGoldUsurpCost(title) * 0.1f,
                Influence = GetInfluenceUsurpCost(title) * 0.2f,
                Renown = GetRenownUsurpCost(title) * 0.2f
            };
            var possibleClaimants = GetClaimants(title);

            if (title.deJure == claimant)
            {
                claimAction.Possible = false;
                claimAction.Reason = new TextObject("{=sPvMf4oj}Already legal owner.");
                return claimAction;
            }

            if (!possibleClaimants.Contains(claimant))
            {
                claimAction.Possible = false;
                claimAction.Reason = new TextObject("{=KR2fio4X}Not a possible claimant.");
                return claimAction;
            }

            var claimType = title.GetHeroClaim(claimant);
            if (claimType == ClaimType.Ongoing)
            {
                claimAction.Possible = false;
                claimAction.Reason = new TextObject("{=O5tRmhZA}Already building a claim.");
                return claimAction;
            }

            if (claimType != ClaimType.None)
            {
                claimAction.Possible = false;
                claimAction.Reason = new TextObject("{=B5JQcObq}Already a claimant.");
                return claimAction;
            }

            if (title.deJure.Clan == claimant.Clan)
            {
                claimAction.Possible = false;
                claimAction.Reason = new TextObject("{=2LK3kist}Owner is in same clan.");
                return claimAction;
            }

            if (title.type == TitleType.Lordship)
            {
                var kingdom = claimant.Clan.Kingdom;
                if (kingdom != null && kingdom == title.deJure.Clan.Kingdom &&
                    BannerKingsConfig.Instance.TitleManager.GetAllDeJure(title.deJure).Count == 1)
                {
                    claimAction.Possible = false;
                    claimAction.Reason =
                        new TextObject("{=u43RH5cY}Not possible to claim a knight's lordship within your faction.");
                    return claimAction;
                }

                var boundTitle = BannerKingsConfig.Instance.TitleManager.GetTitle(title.fief.Village.Bound);
                if (claimant != boundTitle.deJure)
                {
                    claimAction.Possible = false;
                    claimAction.Reason =
                        new TextObject("{=99QF9LyL}Not possible to claim lordships without owning it's suzerain title.");
                    return claimAction;
                }
            }

            if (claimant.Gold < claimAction.Gold || claimant.Clan.Influence < claimAction.Influence)
            {
                claimAction.Possible = false;
                claimAction.Reason = new TextObject("{=zuKjwXH6}Missing required resources.");
                return claimAction;
            }

            claimAction.Possible = true;
            claimAction.Reason = new TextObject("{=zMnXdAxp}You may claim this title.");
            ApplyDiscounts(claimAction);

            return claimAction;
        }

        private TitleAction GetRevoke(FeudalTitle title, Hero revoker)
        {
            var revokeAction = new TitleAction(ActionType.Revoke, title, revoker);
            if (title == null || revoker == null)
            {
                return null;
            }

            revokeAction.Influence = GetInfluenceUsurpCost(title) * 0.8f;
            revokeAction.Renown = GetRenownUsurpCost(title) * 0.6f;

            if (title.deJure == revoker)
            {
                revokeAction.Possible = false;
                revokeAction.Reason = new TextObject("{=sPvMf4oj}Already legal owner.");
                return revokeAction;
            }

            var revokerKingdom = revoker.Clan.Kingdom;
            if (revokerKingdom == null || revokerKingdom != title.deJure.Clan.Kingdom)
            {
                revokeAction.Possible = false;
                revokeAction.Reason = new TextObject("{=OAgroH3G}Can not revoke a title of a lord outside your realm.");
                return revokeAction;
            }

            var governmentType = title.contract.Government;
            switch (governmentType)
            {
                case GovernmentType.Tribal:
                    revokeAction.Possible = false;
                    revokeAction.Reason = new TextObject("{=duRc8Vrs}Tribal government does not allow revoking.");
                    return revokeAction;
                case GovernmentType.Republic when title.type != TitleType.Dukedom:
                    revokeAction.Possible = false;
                    revokeAction.Reason = new TextObject("{=MSaLufNx}Republics can only revoke duke titles.");
                    return revokeAction;
                case GovernmentType.Republic:
                {
                    var sovereign = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(revokerKingdom);
                    if (revoker != sovereign.deJure)
                    {
                        revokeAction.Possible = false;
                        revokeAction.Reason = new TextObject("{=w7b5SE48}Not de Jure faction leader.");
                        return revokeAction;
                    }

                    break;
                }
                case GovernmentType.Imperial:
                {
                    var sovereign = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(revokerKingdom);
                    if (sovereign == null || revoker != sovereign.deJure)
                    {
                        revokeAction.Possible = false;
                        revokeAction.Reason = new TextObject("{=w7b5SE48}Not de Jure faction leader.");
                        return revokeAction;
                    }

                    break;
                }
                default:
                {
                    var titles = BannerKingsConfig.Instance.TitleManager.GetAllDeJure(revoker);
                    var vassal = false;
                    foreach (var revokerTitle in titles)
                    {
                        if (revokerTitle.vassals != null)
                        {
                            foreach (var revokerTitleVassal in revokerTitle.vassals)
                            {
                                if (revokerTitleVassal.deJure == title.deJure)
                                {
                                    vassal = true;
                                    break;
                                }
                            }
                        }
                    }

                    if (!vassal)
                    {
                        revokeAction.Possible = false;
                        revokeAction.Reason = new TextObject("{=Mk29oGgs}Not a direct vassal.");
                        return revokeAction;
                    }

                    break;
                }
            }


            var revokerHighest = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(revoker);
            var targetHighest = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(title.deJure);

            if (revokerHighest != null)
            {
                if (targetHighest.type <= revokerHighest.type)
                {
                    revokeAction.Possible = false;
                    revokeAction.Reason = new TextObject("{=1DGBGp8e}Can not revoke from a lord of superior hierarchy.");
                    return revokeAction;
                }
            }
            

            revokeAction.Possible = true;
            revokeAction.Reason = new TextObject("{=f5Be67QF}You may grant away this title.");
            ApplyDiscounts(revokeAction);
            return revokeAction;
        }

        private TitleAction GetGrant(FeudalTitle title, Hero grantor)
        {
            var grantAction = new TitleAction(ActionType.Grant, title, grantor);
            if (title == null || grantor == null)
            {
                return null;
            }

            grantAction.Gold = 0f;
            grantAction.Renown = 0f;

            if (title.deJure != grantor)
            {
                grantAction.Possible = false;
                grantAction.Reason = new TextObject("{=CK4rr7yZ}Not legal owner.");
                return grantAction;
            }

            if (title.fief != null)
            {
                var deFacto = title.DeFacto;
                if (deFacto != grantor)
                {
                    grantAction.Possible = false;
                    grantAction.Reason = new TextObject("{=0SQXdrDP}Not actual owner of landed title.");
                    return grantAction;
                }
            }

            if (title.type > TitleType.Lordship)
            {
                var candidates = GetGrantCandidates(grantor);
                if (candidates.Count == 0)
                {
                    grantAction.Possible = false;
                    grantAction.Reason = new TextObject("{=dW8WA6PG}No valid candidates in kingdom.");
                    return grantAction;
                }
            }

            var highest = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(grantor);
            if (highest == title)
            {
                grantAction.Possible = false;
                grantAction.Reason = new TextObject("{=97ZWDaBP}Not possible to grant one's main title.");
                return grantAction;
            }

            grantAction.Possible = true;
            grantAction.Influence = GetInfluenceUsurpCost(title) * 0.33f;
            grantAction.Reason = new TextObject("{=f5Be67QF}You may grant away this title.");
            ApplyDiscounts(grantAction);
            return grantAction;
        }

        public TitleAction GetUsurp(FeudalTitle title, Hero usurper)
        {
            var usurpData = new TitleAction(ActionType.Usurp, title, usurper)
            {
                Gold = GetGoldUsurpCost(title),
                Influence = GetInfluenceUsurpCost(title),
                Renown = GetRenownUsurpCost(title)
            };
            if (title.deJure == usurper)
            {
                usurpData.Possible = false;
                usurpData.Reason = new TextObject("{=sPvMf4oj}Already legal owner.");
                return usurpData;
            }

            if (usurper.Clan == null)
            {
                usurpData.Possible = false;
                usurpData.Reason = new TextObject("{=Uw7dMzA4}No clan.");
                return usurpData;
            }

            var type = title.GetHeroClaim(usurper);
            if (type != ClaimType.None && type != ClaimType.Ongoing)
            {
                usurpData.Possible = true;
                usurpData.Reason = new TextObject("{=zMnXdAxp}You may claim this title.");

                var titleLevel = (int) title.type;
                var clanTier = usurper.Clan.Tier;
                if (clanTier < 2 || (titleLevel <= 2 && clanTier < 4))
                {
                    usurpData.Possible = false;
                    usurpData.Reason = new TextObject("{=PsLzYkAz}Clan tier is insufficient.");
                    return usurpData;
                }

                if (usurper.Gold < usurpData.Gold || usurper.Clan.Influence < usurpData.Influence)
                {
                    usurpData.Possible = false;
                    usurpData.Reason = new TextObject("{=zuKjwXH6}Missing required resources.");
                    return usurpData;
                }

                if (title.IsSovereignLevel)
                {
                    var faction = BannerKingsConfig.Instance.TitleManager.GetTitleFaction(title);
                    if (faction.Leader != usurper)
                    {
                        usurpData.Possible = false;
                        usurpData.Reason =
                            new TextObject("{=2wBj94FG}Must be faction leader to usurp highest title in hierarchy.");
                        return usurpData;
                    }
                }

                return usurpData;
            }

            usurpData.Possible = false;
            usurpData.Reason = new TextObject("{=5ysthcWa}No rightful claim.");

            ApplyDiscounts(usurpData);

            return usurpData;
        }

        protected void ApplyDiscounts(TitleAction action)
        {
            if (action.ActionTaker == null)
            {
                return;
            }

            var education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(action.ActionTaker);
            if (education.HasPerk(BKPerks.Instance.AugustDeJure))
            {
                if (action.IsHostile() || action.Type == ActionType.Found)
                {
                    if (action.Influence != 0f)
                    {
                        action.Influence *= 0.95f;
                    }

                    if (action.Gold != 0f)
                    {
                        action.Gold *= 0.95f;
                    }
                }
                else
                {
                    if (action.Influence != 0f)
                    {
                        action.Influence *= 1.05f;
                    }

                    if (action.Gold != 0f)
                    {
                        action.Gold *= 1.05f;
                    }
                }
            }
        }

        public List<Hero> GetGrantCandidates(Hero grantor)
        {
            var heroes = new List<Hero>();
            var kingdom = grantor.Clan.Kingdom;
            if (kingdom != null)
            {
                foreach (var clan in kingdom.Clans)
                {
                    if (!clan.IsUnderMercenaryService && clan != grantor.Clan)
                    {
                        heroes.Add(clan.Leader);
                    }
                }
            }

            return heroes;
        }

        public List<Hero> GetClaimants(FeudalTitle title)
        {
            var claimants = new List<Hero>();
            var deFacto = title.DeFacto;
            if (deFacto != title.deJure)
            {
                if (title.fief == null)
                {
                    if (BannerKingsConfig.Instance.TitleManager.IsHeroTitleHolder(deFacto))
                    {
                        claimants.Add(deFacto);
                    }
                }
                else
                {
                    claimants.Add(deFacto);
                }
            }

            if (title.sovereign != null && title.sovereign.deJure != title.deJure)
            {
                claimants.Add(title.sovereign.deJure);
            }

            if (title.vassals is {Count: > 0})
            {
                foreach (var vassal in title.vassals)
                {
                    if (vassal.deJure != title.deJure)
                    {
                        claimants.Add(vassal.deJure);
                    }
                }
            }

            return claimants;
        }

        private float GetInfluenceUsurpCost(FeudalTitle title)
        {
            return 500f / (float) title.type + 1f;
        }

        private float GetRenownUsurpCost(FeudalTitle title)
        {
            return 100f / (float) title.type + 1f;
        }

        public float GetGoldUsurpCost(FeudalTitle title)
        {
            var gold = 100000f / (float) title.type + 1f;
            if (title.fief != null)
            {
                var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(title.fief);
                gold += data.TotalPop / 100f;
            }

            return gold;
        }

        public int GetRelationImpact(FeudalTitle title)
        {
            var result = title.type switch
            {
                TitleType.Lordship => MBRandom.RandomInt(5, 10),
                TitleType.Barony => MBRandom.RandomInt(15, 25),
                TitleType.County => MBRandom.RandomInt(30, 40),
                TitleType.Dukedom => MBRandom.RandomInt(45, 55),
                TitleType.Kingdom => MBRandom.RandomInt(80, 90),
                _ => MBRandom.RandomInt(120, 150)
            };

            return -result;
        }

        public int GetSkillReward(TitleType title, ActionType type)
        {
            if (type == ActionType.Found)
            {
                return 2000;
            }

            var result = title switch
            {
                TitleType.Lordship => 100,
                TitleType.Barony => 200,
                TitleType.County => 300,
                TitleType.Dukedom => 500,
                TitleType.Kingdom => 1000,
                _ => 1500
            };

            return result;
        }
    }
}