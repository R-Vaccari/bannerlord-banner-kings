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
                result.Add(extra, new TextObject("{=jYrmRCM4x}Highest title level"));
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
                foundAction.Reason = new TextObject("{=qz1z1S50F}No kingdom.");
                return foundAction;
            }

            var title = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(faction);
            if (title != null)
            {
                foundAction.Possible = false;
                foundAction.Reason = new TextObject("{=urhbfnsjP}Faction sovereign title already exists.");
                return foundAction;
            }

            if (faction.Leader != founder)
            {
                foundAction.Possible = false;
                foundAction.Reason = new TextObject("{=XO6GhQowA}Not leader of current faction.");
                return foundAction;
            }

            var titles = BannerKingsConfig.Instance.TitleManager.GetAllDeJure(founder);
            if (titles.Any(x => x.type <= TitleType.Kingdom))
            {
                foundAction.Possible = false;
                foundAction.Reason = new TextObject("{=A0yfKc2im}Cannot found a kingdom while already being a de Jure sovereign.");
                return foundAction;
            }

            if (!titles.Any(x => x.type <= TitleType.Dukedom))
            {
                foundAction.Possible = false;
                foundAction.Reason = new TextObject("{=jN2YKUyxG}Cannot found a kingdom without a de Jure duke level title.");
                return foundAction;
            }

            if (faction.Clans.Count < 3)
            {
                foundAction.Possible = false;
                foundAction.Reason = new TextObject("{=xw6V1zX6b}Cannot found a kingdom for a faction with less than 3 clans.");
                return foundAction;
            }

            if (founder.Gold < foundAction.Gold || founder.Clan.Influence < foundAction.Influence)
            {
                foundAction.Possible = false;
                foundAction.Reason = new TextObject("{=CZSUuPzJU}You lack the required resources.");
                return foundAction;
            }

            foundAction.Possible = true;
            foundAction.Reason = new TextObject("{=xNv58iEMS}Kingdom can be founded.");
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
                claimAction.Reason = new TextObject("{=LrMnVe7jG}Already legal owner.");
                return claimAction;
            }

            if (!possibleClaimants.Contains(claimant))
            {
                claimAction.Possible = false;
                claimAction.Reason = new TextObject("{=L7CZoHJ86}Not a possible claimant.");
                return claimAction;
            }

            var claimType = title.GetHeroClaim(claimant);
            if (claimType == ClaimType.Ongoing)
            {
                claimAction.Possible = false;
                claimAction.Reason = new TextObject("{=jccC3zsk4}Already building a claim.");
                return claimAction;
            }

            if (claimType != ClaimType.None)
            {
                claimAction.Possible = false;
                claimAction.Reason = new TextObject("{=VmtXwjbVE}Already a claimant.");
                return claimAction;
            }

            if (title.deJure.Clan == claimant.Clan)
            {
                claimAction.Possible = false;
                claimAction.Reason = new TextObject("{=bRcMKk3FK}Owner is in same clan.");
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
                        new TextObject("{=1HXv8HEKj}Not possible to claim a knight's lordship within your faction.");
                    return claimAction;
                }

                var boundTitle = BannerKingsConfig.Instance.TitleManager.GetTitle(title.fief.Village.Bound);
                if (claimant != boundTitle.deJure)
                {
                    claimAction.Possible = false;
                    claimAction.Reason =
                        new TextObject("{=mKCz07qdW}Not possible to claim lordships without owning it's suzerain title.");
                    return claimAction;
                }
            }

            if (claimant.Gold < claimAction.Gold || claimant.Clan.Influence < claimAction.Influence)
            {
                claimAction.Possible = false;
                claimAction.Reason = new TextObject("{=dGuUaAxPP}Missing required resources.");
                return claimAction;
            }

            claimAction.Possible = true;
            claimAction.Reason = new TextObject("{=Dp469UwPN}You may claim this title.");
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
                revokeAction.Reason = new TextObject("{=LrMnVe7jG}Already legal owner.");
                return revokeAction;
            }

            var revokerKingdom = revoker.Clan.Kingdom;
            if (revokerKingdom == null || revokerKingdom != title.deJure.Clan.Kingdom)
            {
                revokeAction.Possible = false;
                revokeAction.Reason = new TextObject("{=AkRyayJZT}Can not revoke a title of a lord outside your realm.");
                return revokeAction;
            }

            var governmentType = title.contract.Government;
            switch (governmentType)
            {
                case GovernmentType.Tribal:
                    revokeAction.Possible = false;
                    revokeAction.Reason = new TextObject("{=LJbadOOtQ}Tribal government does not allow revoking.");
                    return revokeAction;
                case GovernmentType.Republic when title.type != TitleType.Dukedom:
                    revokeAction.Possible = false;
                    revokeAction.Reason = new TextObject("{=C8f2RD8om}Republics can only revoke duke titles.");
                    return revokeAction;
                case GovernmentType.Republic:
                {
                    var sovereign = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(revokerKingdom);
                    if (revoker != sovereign.deJure)
                    {
                        revokeAction.Possible = false;
                        revokeAction.Reason = new TextObject("{=Vs3Boce7k}Not de Jure faction leader.");
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
                        revokeAction.Reason = new TextObject("{=Vs3Boce7k}Not de Jure faction leader.");
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
                        revokeAction.Reason = new TextObject("{=5C7ydG5ni}Not a direct vassal.");
                        return revokeAction;
                    }

                    break;
                }
            }


            var revokerHighest = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(revoker);
            var targetHighest = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(title.deJure);

            if (targetHighest.type <= revokerHighest.type)
            {
                revokeAction.Possible = false;
                revokeAction.Reason = new TextObject("{=xAz7Sr2eG}Can not revoke from a lord of superior hierarchy.");
                return revokeAction;
            }

            revokeAction.Possible = true;
            revokeAction.Reason = new TextObject("{=Y26KtE89J}You may grant away this title.");
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
                grantAction.Reason = new TextObject("{=fmRsSi1rA}Not legal owner.");
                return grantAction;
            }

            if (title.fief != null)
            {
                var deFacto = title.DeFacto;
                if (deFacto != grantor)
                {
                    grantAction.Possible = false;
                    grantAction.Reason = new TextObject("{=cLgwyynZf}Not actual owner of landed title.");
                    return grantAction;
                }
            }

            if (title.type > TitleType.Lordship)
            {
                var candidates = GetGrantCandidates(grantor);
                if (candidates.Count == 0)
                {
                    grantAction.Possible = false;
                    grantAction.Reason = new TextObject("{=q8F3x1Y3h}No valid candidates in kingdom.");
                    return grantAction;
                }
            }

            var highest = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(grantor);
            if (highest == title)
            {
                grantAction.Possible = false;
                grantAction.Reason = new TextObject("{=5pxRru8Cr}Not possible to grant one's main title.");
                return grantAction;
            }

            grantAction.Possible = true;
            grantAction.Influence = GetInfluenceUsurpCost(title) * 0.33f;
            grantAction.Reason = new TextObject("{=Y26KtE89J}You may grant away this title.");
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
                usurpData.Reason = new TextObject("{=LrMnVe7jG}Already legal owner.");
                return usurpData;
            }

            if (usurper.Clan == null)
            {
                usurpData.Possible = false;
                usurpData.Reason = new TextObject("{=mpnfCAAvQ}No clan.");
                return usurpData;
            }

            var type = title.GetHeroClaim(usurper);
            if (type != ClaimType.None && type != ClaimType.Ongoing)
            {
                usurpData.Possible = true;
                usurpData.Reason = new TextObject("{=Dp469UwPN}You may claim this title.");

                var titleLevel = (int) title.type;
                var clanTier = usurper.Clan.Tier;
                if (clanTier < 2 || (titleLevel <= 2 && clanTier < 4))
                {
                    usurpData.Possible = false;
                    usurpData.Reason = new TextObject("{=WccGNWPV0}Clan tier is insufficient.");
                    return usurpData;
                }

                if (usurper.Gold < usurpData.Gold || usurper.Clan.Influence < usurpData.Influence)
                {
                    usurpData.Possible = false;
                    usurpData.Reason = new TextObject("{=dGuUaAxPP}Missing required resources.");
                    return usurpData;
                }

                if (title.IsSovereignLevel)
                {
                    var faction = BannerKingsConfig.Instance.TitleManager.GetTitleFaction(title);
                    if (faction.Leader != usurper)
                    {
                        usurpData.Possible = false;
                        usurpData.Reason =
                            new TextObject("{=0RxY93B1k}Must be faction leader to usurp highest title in hierarchy.");
                        return usurpData;
                    }
                }

                return usurpData;
            }

            usurpData.Possible = false;
            usurpData.Reason = new TextObject("{=cb5Hky7Fh}No rightful claim.");

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

        public int GetSkillReward(FeudalTitle title, ActionType type)
        {
            if (type == ActionType.Found)
            {
                return 2000;
            }

            var result = title.type switch
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