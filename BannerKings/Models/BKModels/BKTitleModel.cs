using System;
using System.Collections.Generic;
using System.Linq;
using BannerKings.Managers.Cultures;
using BannerKings.Managers.Skills;
using BannerKings.Managers.Titles;
using BannerKings.Managers.Titles.Governments;
using BannerKings.Models.BKModels.Abstract;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Models.BKModels
{
    public class BKTitleModel : TitleModel
    {
        public override ExplainedNumber GetSuccessionHeirScore(Hero currentLeader, Hero candidate, FeudalTitle title, bool explanations = false)
        {
            FeudalContract contract = title.Contract;
            var succession = contract.Succession;

            var result = succession.CalculateHeirScore(currentLeader, candidate, title, explanations);
            if (candidate.Culture != currentLeader.Clan.Kingdom.Culture)
            {
                result.AddFactor(-0.2f, GameTexts.FindText("str_culture"));
            }

            return result;
        }

        public override ExplainedNumber GetInheritanceHeirScore(Hero currentLeader, Hero candidate, FeudalContract contract, bool explanations = false)
        {
            var result = new ExplainedNumber(0f, explanations);
            if (contract == null)
            {
                contract = new FeudalContract(null, null,
                    DefaultGovernments.Instance.Feudal, 
                    DefaultSuccessions.Instance.FeudalElective, 
                    DefaultInheritances.Instance.Seniority, 
                    DefaultGenderLaws.Instance.Agnatic);
            }

            result.Add(candidate.Age, new TextObject("Age"));
            if (currentLeader.Children.Contains(candidate))
            {
                result.Add(contract.Inheritance.ChildrenScore, GameTexts.FindText(candidate.IsFemale ? "str_daughter" : "str_son"));
            }
            else if (currentLeader.Siblings.Contains(candidate))
            {
                result.Add(contract.Inheritance.SiblingScore, GameTexts.FindText(candidate.IsFemale ? "str_bigsister" : "str_bigbrother"));
            }
            else if (currentLeader.Spouse == candidate)
            {
                result.Add(contract.Inheritance.SpouseScore, GameTexts.FindText("str_spouse"));
            }
            else
            {
                result.Add(contract.Inheritance.RelativeScore, new TextObject("{=m6qYgCZ2}Household member"));
            }

            if (candidate.IsFemale) 
            {
                result.AddFactor(contract.GenderLaw.FemalePreference - 1f, contract.GenderLaw.Name);    
            }
            else
            {
                result.AddFactor(contract.GenderLaw.MalePreference - 1f, contract.GenderLaw.Name);
            }

            if (BannerKingsConfig.Instance.TitleManager.IsKnight(candidate))
            {
                result.AddFactor(-0.8f, new TextObject("{=7NPiKyo0}{HERO} is a {KNIGHT}")
                    .SetTextVariable("HERO", candidate.Name)
                    .SetTextVariable("KNIGHT", Utils.TextHelper.GetKnightTitle(currentLeader.Culture, candidate.IsFemale, false)));
            }

            return result;
        }

        public override ExplainedNumber GetGrantKnighthoodCost(Hero grantor)
        {
            var result = new ExplainedNumber(120f, true);
            var highest = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(grantor);
            var extra = 0f;

            if (highest is {TitleType: < TitleType.Barony})
            {
                extra = highest.TitleType switch
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

        public override TitleAction GetFoundKingdom(Kingdom faction, Hero founder)
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
            if (titles.Any(x => x.TitleType <= TitleType.Kingdom))
            {
                foundAction.Possible = false;
                foundAction.Reason = new TextObject("{=t8HXoFk7}Cannot found a kingdom while already being a de Jure sovereign.");
                return foundAction;
            }

            if (!titles.Any(x => x.TitleType <= TitleType.Dukedom))
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

        public override TitleAction GetFoundEmpire(Kingdom faction, Hero founder)
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
            if (titles.Any(x => x.TitleType == TitleType.Empire))
            {
                foundAction.Possible = false;
                foundAction.Reason = new TextObject("{=!}Cannot found an Empire-level title while already being a de Jure Empire-level title holder.");
                return foundAction;
            }

            if (titles.Count(x => x.TitleType == TitleType.Kingdom) < 2)
            {
                foundAction.Possible = false;
                foundAction.Reason = new TextObject("{=!}Cannot found an Empire-level title without holding at least 2 Kingdom-level titles.");
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
            foundAction.Reason = new TextObject("{=!}Empire can be founded.");
            ApplyDiscounts(foundAction);
            return foundAction;
        }

        public override TitleAction GetAction(ActionType type, FeudalTitle title, Hero taker, Hero receiver = null)
        {
            TitleAction action =  type switch
            {
                ActionType.Usurp => GetUsurp(title, taker),
                ActionType.Revoke => GetRevoke(title, taker),
                ActionType.Claim => GetClaim(title, taker),
                ActionType.Create => GetCreate(title, taker),
                _ => GetGrant(title, taker)
            };
            ApplyDiscounts(action);
            return action;
        }

        private TitleAction GetCreate(FeudalTitle title, Hero creator)
        {
            var createAction = new TitleAction(ActionType.Create, title, creator)
            {
                Gold = GetGoldUsurpCost(title) * 0.1f,
                Influence = GetInfluenceUsurpCost(title) * 0.2f,
                Renown = GetRenownUsurpCost(title) * 0.2f
            };

            Kingdom faction = creator.Clan.Kingdom;
            if (faction == null)
            {
                createAction.Possible = false;
                createAction.Reason = new TextObject("{=JDFpx1eN}No kingdom.");
                return createAction;
            }

            if (faction.Leader != creator)
            {
                createAction.Possible = false;
                createAction.Reason = new TextObject("{=Z2tSyHfC}Not leader of current faction.");
                return createAction;
            }

            if (creator.Gold < createAction.Gold || creator.Clan.Influence < createAction.Influence)
            {
                createAction.Possible = false;
                createAction.Reason = new TextObject("{=zuKjwXH6}Missing required resources.");
                return createAction;
            }

            List<FeudalTitle> titles = BannerKingsConfig.Instance.TitleManager.GetAllDeJure(creator);
            TitleType titleType = title.TitleType + 1;
            if (!titles.Any(x => x.TitleType <= titleType))
            {
                CulturalTitleName name1 = DefaultTitleNames.Instance.GetTitleName(creator.Culture, titleType);
                CulturalTitleName name2 = DefaultTitleNames.Instance.GetTitleName(creator.Culture, title.TitleType);
                createAction.Possible = false;
                createAction.Reason = new TextObject("{=!}You must De Jure hold at least one title of {NAME1} level or higher in order to create a {NAME2}.")
                    .SetTextVariable("NAME1", name1.Description)
                    .SetTextVariable("NAME2", name2.Description);
                return createAction;
            }

            createAction.Possible = true;
            createAction.Reason = new TextObject("{=!}You may create this title.");
            return createAction;
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

            if (title.deJure == null)
            {
                claimAction.Possible = false;
                claimAction.Reason = new TextObject("{=GpKQomFn}No de jure owner, title must be created.");
                return claimAction;
            }

            if (title.deJure == claimant)
            {
                claimAction.Possible = false;
                claimAction.Reason = new TextObject("{=sPvMf4oj}Already legal owner.");
                return claimAction;
            }

            if (!possibleClaimants.ContainsKey(claimant))
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

            if (title.TitleType == TitleType.Lordship)
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

                var boundTitle = BannerKingsConfig.Instance.TitleManager.GetTitle(title.Fief.Village.Bound);
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


            if (title.deJure == null)
            {
                revokeAction.Possible = false;
                revokeAction.Reason = new TextObject("{=GpKQomFn}No de jure owner, title must be created.");
                return revokeAction;
            }

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

            if (title.Contract.Government == DefaultGovernments.Instance.Tribal)
            {
                revokeAction.Possible = false;
                revokeAction.Reason = new TextObject("Tribal government does not allow revoking.")
                    .SetTextVariable("ASPECT", DefaultGovernments.Instance.Tribal.Name);
                return revokeAction;
            }
            else if (title.Contract.Government == DefaultGovernments.Instance.Republic && title.TitleType != TitleType.Dukedom)
            {
                revokeAction.Possible = false;
                revokeAction.Reason = new TextObject("{=RDxuKgC6}Republican government only allows revoking of dukes.")
                    .SetTextVariable("ASPECT", DefaultGovernments.Instance.Republic.Name);
                return revokeAction;
            }
            else if (title.Contract.Government == DefaultGovernments.Instance.Imperial)
            {
                var sovereign = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(revokerKingdom);
                if (sovereign == null || revoker != sovereign.deJure)
                {
                    revokeAction.Possible = false;
                    revokeAction.Reason = new TextObject("{=EecVkFHJ}Imperial government requires being de Jure faction leader.")
                        .SetTextVariable("ASPECT", DefaultGovernments.Instance.Imperial.Name);
                    return revokeAction;
                }
            }
            else
            {
                var titles = BannerKingsConfig.Instance.TitleManager.GetAllDeJure(revoker);
                var vassal = false;
                foreach (var revokerTitle in titles)
                {
                    if (revokerTitle.Vassals != null)
                    {
                        foreach (var revokerTitleVassal in revokerTitle.Vassals)
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
            }

            var revokerHighest = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(revoker);
            var targetHighest = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(title.deJure);

            if (revokerHighest != null)
            {
                if (targetHighest.TitleType <= revokerHighest.TitleType)
                {
                    revokeAction.Possible = false;
                    revokeAction.Reason = new TextObject("{=1DGBGp8e}Can not revoke from a lord of superior hierarchy.");
                    return revokeAction;
                }
            }

            revokeAction.Possible = true;
            revokeAction.Reason = new TextObject("{=f5Be67QF}You may grant away this title.");
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

            if (title.deJure == null)
            {
                grantAction.Possible = false;
                grantAction.Reason = new TextObject("{=GpKQomFn}No de jure owner, title must be created.");
                return grantAction;
            }

            if (title.deJure != grantor)
            {
                grantAction.Possible = false;
                grantAction.Reason = new TextObject("{=CK4rr7yZ}Not legal owner.");
                return grantAction;
            }

            if (title.Fief != null)
            {
                var deFacto = title.DeFacto;
                if (deFacto != grantor)
                {
                    grantAction.Possible = false;
                    grantAction.Reason = new TextObject("{=0SQXdrDP}Not actual owner of landed title.");
                    return grantAction;
                }
            }

            if (title.TitleType > TitleType.Lordship)
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

            if (title.deJure == null)
            {
                usurpData.Possible = false;
                usurpData.Reason = new TextObject("{=GpKQomFn}No de jure owner, title must be created.");
                return usurpData;
            }

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

                var titleLevel = (int) title.TitleType;
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

                if (title.deJure != null && title.deJure.MapFaction == usurper.MapFaction)
                {
                    if (BannerKingsConfig.Instance.TitleManager.CalculateAllVassals(usurper.Clan).Contains(title.deJure))
                    {
                        usurpData.Possible = false;
                        usurpData.Reason =
                            new TextObject("{=rkrK9Js5}You can not usurp from your vassal, revoke instead.");
                        return usurpData;
                    }
                }

                if (title.IsSovereignLevel)
                {
                    var faction = BannerKingsConfig.Instance.TitleManager.GetTitleFaction(title);
                    if (usurper.MapFaction != faction)
                    {
                        if (!usurper.MapFaction.IsKingdomFaction || usurper.MapFaction.Leader != usurper)
                        {
                            usurpData.Possible = false;
                            usurpData.Reason =
                                new TextObject("{=FESBxuj3}You must be the leader of a faction in order to usurp a Kingdom or Empire level title.");
                            return usurpData;
                        }

                        foreach (FeudalTitle vassal in title.Vassals)
                        {
                            if (vassal.DeFacto != usurper)
                            {
                                usurpData.Possible = false;
                                usurpData.Reason =
                                    new TextObject("{=JFcNv7no}You must be the de facto holder of {TITLE} to inherit the sovereign title {SOVEREIGN}.")
                                    .SetTextVariable("TITLE", vassal.FullName)
                                    .SetTextVariable("SOVEREIGN", title.FullName);
                                return usurpData;
                            }
                        }
                    }
                    else if (faction.Leader != usurper)
                    {
                        usurpData.Possible = false;
                        usurpData.Reason =
                            new TextObject("{=ioU78p59}As a member of {KINGDOM}, you must lead the faction to usurp its title.")
                            .SetTextVariable("KINGDOM", faction.Name);
                        return usurpData;
                    }
                }
                else if (title.TitleType == TitleType.Dukedom)
                {
                    if (title.DeFacto != usurper)
                    {
                        usurpData.Possible = false;
                        usurpData.Reason = new TextObject("{=sN05SxWb}To usurp a duchy-level title, you need to control the majority of its direct vassals.");
                        return usurpData;
                    }
                }

                return usurpData;
            }

            usurpData.Possible = false;
            usurpData.Reason = new TextObject("{=5ysthcWa}No rightful claim.");
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

        private float GetInfluenceUsurpCost(FeudalTitle title)
        {
            return 500f / ((float) title.TitleType + 1f);
        }

        private float GetRenownUsurpCost(FeudalTitle title)
        {
            return 100f / ((float) title.TitleType + 1f);
        }

        public override float GetGoldUsurpCost(FeudalTitle title)
        {
            var gold = 100000f / ((float) title.TitleType + 1f);
            if (title.Fief != null)
            {
                var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(title.Fief);
                gold += data.TotalPop / 100f;
            }

            return gold;
        }

        public override int GetRelationImpact(FeudalTitle title)
        {
            var result = title.TitleType switch
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

        public override int GetSkillReward(TitleType title, ActionType type)
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