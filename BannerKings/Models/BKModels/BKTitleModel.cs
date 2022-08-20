using System.Collections.Generic;
using System.Linq;
using BannerKings.Managers.Skills;
using BannerKings.Managers.Titles;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Models.BKModels;

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

        if (highest != null && highest.type < TitleType.Barony)
        {
            if (highest.type == TitleType.County)
            {
                extra = 30f;
            }
            else if (highest.type == TitleType.Dukedom)
            {
                extra = 60f;
            }
            else if (highest.type == TitleType.Kingdom)
            {
                extra = 100f;
            }
            else
            {
                extra = 180f;
            }
        }

        if (extra != 0f)
        {
            result.Add(extra, new TextObject("{=!}Highest title level"));
        }

        return result;
    }

    public TitleAction GetFoundKingdom(Kingdom faction, Hero founder)
    {
        var foundAction = new TitleAction(ActionType.Found, null, founder);
        foundAction.Gold = 500000 +
                           BannerKingsConfig.Instance.ClanFinanceModel.CalculateClanIncome(founder.Clan).ResultNumber *
                           CampaignTime.DaysInYear;
        foundAction.Influence = 1000 + BannerKingsConfig.Instance.InfluenceModel.CalculateInfluenceChange(founder.Clan)
                .ResultNumber *
            CampaignTime.DaysInYear * 0.1f;
        foundAction.Renown = 100;

        if (faction == null)
        {
            foundAction.Possible = false;
            foundAction.Reason = new TextObject("{=!}No kingdom.");
            return foundAction;
        }

        var title = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(faction);
        if (title != null)
        {
            foundAction.Possible = false;
            foundAction.Reason = new TextObject("{=!}Faction sovereign title already exists.");
            return foundAction;
        }

        if (faction.Leader != founder)
        {
            foundAction.Possible = false;
            foundAction.Reason = new TextObject("{=!}Not leader of current faction.");
            return foundAction;
        }

        var titles = BannerKingsConfig.Instance.TitleManager.GetAllDeJure(founder);
        if (titles.Any(x => x.type <= TitleType.Kingdom))
        {
            foundAction.Possible = false;
            foundAction.Reason = new TextObject("{=!}Cannot found a kingdom while already being a de Jure sovereign.");
            return foundAction;
        }

        if (!titles.Any(x => x.type <= TitleType.Dukedom))
        {
            foundAction.Possible = false;
            foundAction.Reason = new TextObject("{=!}Cannot found a kingdom without a de Jure duke level title.");
            return foundAction;
        }

        if (faction.Clans.Count < 3)
        {
            foundAction.Possible = false;
            foundAction.Reason = new TextObject("{=!}Cannot found a kingdom for a faction with less than 3 clans.");
            return foundAction;
        }

        if (founder.Gold < foundAction.Gold || founder.Clan.Influence < foundAction.Influence)
        {
            foundAction.Possible = false;
            foundAction.Reason = new TextObject("{=!}You lack the required resources.");
            return foundAction;
        }

        foundAction.Possible = true;
        foundAction.Reason = new TextObject("{=!}Kingdom can be founded.");
        ApplyDiscounts(foundAction);
        return foundAction;
    }

    public TitleAction GetAction(ActionType type, FeudalTitle title, Hero taker, Hero receiver = null)
    {
        if (type == ActionType.Usurp)
        {
            return GetUsurp(title, taker);
        }

        if (type == ActionType.Revoke)
        {
            return GetRevoke(title, taker);
        }

        if (type == ActionType.Claim)
        {
            return GetClaim(title, taker);
        }

        return GetGrant(title, taker);
    }

    private TitleAction GetClaim(FeudalTitle title, Hero claimant)
    {
        var claimAction = new TitleAction(ActionType.Claim, title, claimant);
        claimAction.Gold = GetGoldUsurpCost(title) * 0.1f;
        claimAction.Influence = GetInfluenceUsurpCost(title) * 0.2f;
        claimAction.Renown = GetRenownUsurpCost(title) * 0.2f;
        var possibleClaimants = GetClaimants(title);

        if (title.deJure == claimant)
        {
            claimAction.Possible = false;
            claimAction.Reason = new TextObject("{=!}Already legal owner.");
            return claimAction;
        }

        if (!possibleClaimants.Contains(claimant))
        {
            claimAction.Possible = false;
            claimAction.Reason = new TextObject("{=!}Not a possible claimant.");
            return claimAction;
        }

        var claimType = title.GetHeroClaim(claimant);
        if (claimType == ClaimType.Ongoing)
        {
            claimAction.Possible = false;
            claimAction.Reason = new TextObject("{=!}Already building a claim.");
            return claimAction;
        }

        if (claimType != ClaimType.None)
        {
            claimAction.Possible = false;
            claimAction.Reason = new TextObject("{=!}Already a claimant.");
            return claimAction;
        }

        if (title.deJure.Clan == claimant.Clan)
        {
            claimAction.Possible = false;
            claimAction.Reason = new TextObject("{=!}Owner is in same clan.");
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
                    new TextObject("{=!}Not possible to claim a knight's lordship within your faction.");
                return claimAction;
            }

            var boundTitle = BannerKingsConfig.Instance.TitleManager.GetTitle(title.fief.Village.Bound);
            if (claimant != boundTitle.deJure)
            {
                claimAction.Possible = false;
                claimAction.Reason =
                    new TextObject("{=!}Not possible to claim lordships without owning it's suzerain title.");
                return claimAction;
            }
        }

        if (claimant.Gold < claimAction.Gold || claimant.Clan.Influence < claimAction.Influence)
        {
            claimAction.Possible = false;
            claimAction.Reason = new TextObject("{=!}Missing required resources.");
            return claimAction;
        }

        claimAction.Possible = true;
        claimAction.Reason = new TextObject("{=!}You may claim this title.");
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
            revokeAction.Reason = new TextObject("{=!}Already legal owner.");
            return revokeAction;
        }

        var revokerKingdom = revoker.Clan.Kingdom;
        if (revokerKingdom == null || revokerKingdom != title.deJure.Clan.Kingdom)
        {
            revokeAction.Possible = false;
            revokeAction.Reason = new TextObject("{=!}Can not revoke a title of a lord outside your realm.");
            return revokeAction;
        }

        var governmentType = title.contract.Government;
        if (governmentType == GovernmentType.Tribal)
        {
            revokeAction.Possible = false;
            revokeAction.Reason = new TextObject("{=!}Tribal government does not allow revoking.");
            return revokeAction;
        }

        if (governmentType == GovernmentType.Republic)
        {
            if (title.type != TitleType.Dukedom)
            {
                revokeAction.Possible = false;
                revokeAction.Reason = new TextObject("{=!}Republics can only revoke duke titles.");
                return revokeAction;
            }

            var sovereign = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(revokerKingdom);
            if (revoker != sovereign.deJure)
            {
                revokeAction.Possible = false;
                revokeAction.Reason = new TextObject("{=!}Not de Jure faction leader.");
                return revokeAction;
            }
        }
        else if (governmentType == GovernmentType.Imperial)
        {
            var sovereign = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(revokerKingdom);
            if (sovereign == null || revoker != sovereign.deJure)
            {
                revokeAction.Possible = false;
                revokeAction.Reason = new TextObject("{=!}Not de Jure faction leader.");
                return revokeAction;
            }
        }
        else
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
                revokeAction.Reason = new TextObject("{=!}Not a direct vassal.");
                return revokeAction;
            }
        }


        var revokerHighest = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(revoker);
        var targetHighest = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(title.deJure);

        if (targetHighest.type <= revokerHighest.type)
        {
            revokeAction.Possible = false;
            revokeAction.Reason = new TextObject("{=!}Can not revoke from a lord of superior hierarchy.");
            return revokeAction;
        }

        revokeAction.Possible = true;
        revokeAction.Reason = new TextObject("{=!}You may grant away this title.");
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
            grantAction.Reason = new TextObject("{=!}Not legal owner.");
            return grantAction;
        }

        if (title.fief != null)
        {
            var deFacto = title.DeFacto;
            if (deFacto != grantor)
            {
                grantAction.Possible = false;
                grantAction.Reason = new TextObject("{=!}Not actual owner of landed title.");
                return grantAction;
            }
        }

        if (title.type > TitleType.Lordship)
        {
            var candidates = GetGrantCandidates(grantor);
            if (candidates.Count == 0)
            {
                grantAction.Possible = false;
                grantAction.Reason = new TextObject("{=!}No valid candidates in kingdom.");
                return grantAction;
            }
        }

        var highest = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(grantor);
        if (highest == title)
        {
            grantAction.Possible = false;
            grantAction.Reason = new TextObject("{=!}Not possible to grant one's main title.");
            return grantAction;
        }

        grantAction.Possible = true;
        grantAction.Influence = GetInfluenceUsurpCost(title) * 0.33f;
        grantAction.Reason = new TextObject("{=!}You may grant away this title.");
        ApplyDiscounts(grantAction);
        return grantAction;
    }

    public TitleAction GetUsurp(FeudalTitle title, Hero usurper)
    {
        var usurpData = new TitleAction(ActionType.Usurp, title, usurper);
        usurpData.Gold = GetGoldUsurpCost(title);
        usurpData.Influence = GetInfluenceUsurpCost(title);
        usurpData.Renown = GetRenownUsurpCost(title);
        if (title.deJure == usurper)
        {
            usurpData.Possible = false;
            usurpData.Reason = new TextObject("{=!}Already legal owner.");
            return usurpData;
        }

        if (usurper.Clan == null)
        {
            usurpData.Possible = false;
            usurpData.Reason = new TextObject("{=!}No clan.");
            return usurpData;
        }

        var type = title.GetHeroClaim(usurper);
        if (type != ClaimType.None && type != ClaimType.Ongoing)
        {
            usurpData.Possible = true;
            usurpData.Reason = new TextObject("{=!}You may claim this title.");

            var titleLevel = (int) title.type;
            var clanTier = usurper.Clan.Tier;
            if (clanTier < 2 || (titleLevel <= 2 && clanTier < 4))
            {
                usurpData.Possible = false;
                usurpData.Reason = new TextObject("{=!}Clan tier is insufficient.");
                return usurpData;
            }

            if (usurper.Gold < usurpData.Gold || usurper.Clan.Influence < usurpData.Influence)
            {
                usurpData.Possible = false;
                usurpData.Reason = new TextObject("{=!}Missing required resources.");
                return usurpData;
            }

            if (title.IsSovereignLevel)
            {
                var faction = BannerKingsConfig.Instance.TitleManager.GetTitleFaction(title);
                if (faction.Leader != usurper)
                {
                    usurpData.Possible = false;
                    usurpData.Reason =
                        new TextObject("{=!}Must be faction leader to usurp highest title in hierarchy.");
                    return usurpData;
                }
            }

            return usurpData;
        }

        usurpData.Possible = false;
        usurpData.Reason = new TextObject("{=!}No rightful claim.");

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

        if (title.vassals != null && title.vassals.Count > 0)
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
        int result;
        if (title.type == TitleType.Lordship)
        {
            result = MBRandom.RandomInt(5, 10);
        }
        else if (title.type == TitleType.Barony)
        {
            result = MBRandom.RandomInt(15, 25);
        }
        else if (title.type == TitleType.County)
        {
            result = MBRandom.RandomInt(30, 40);
        }
        else if (title.type == TitleType.Dukedom)
        {
            result = MBRandom.RandomInt(45, 55);
        }
        else if (title.type == TitleType.Kingdom)
        {
            result = MBRandom.RandomInt(80, 90);
        }
        else
        {
            result = MBRandom.RandomInt(120, 150);
        }

        return -result;
    }

    public int GetSkillReward(FeudalTitle title, ActionType type)
    {
        if (type == ActionType.Found)
        {
            return 2000;
        }

        int result;
        if (title.type == TitleType.Lordship)
        {
            result = 100;
        }
        else if (title.type == TitleType.Barony)
        {
            result = 200;
        }
        else if (title.type == TitleType.County)
        {
            result = 300;
        }
        else if (title.type == TitleType.Dukedom)
        {
            result = 500;
        }
        else if (title.type == TitleType.Kingdom)
        {
            result = 1000;
        }
        else
        {
            result = 1500;
        }

        return result;
    }
}