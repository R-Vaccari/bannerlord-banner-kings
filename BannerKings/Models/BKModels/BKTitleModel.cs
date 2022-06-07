using BannerKings.Managers.Titles;
using BannerKings.Populations;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Models.BKModels
{
    public class BKTitleModel : IBannerKingsModel
    {


        public TitleAction GetAction(ActionType type, FeudalTitle title, Hero taker, Hero receiver = null)
        {
            if (type == ActionType.Usurp)
                return GetUsurp(title, taker);
            else if (type == ActionType.Revoke)
                return GetRevoke(title, taker);
            else if (type == ActionType.Claim)
                return GetClaim(title, taker);
            else return GetGrant(title, taker);
        }

        private TitleAction GetClaim(FeudalTitle title, Hero claimant)
        {
            TitleAction claimAction = new TitleAction(ActionType.Claim, title, claimant);
            claimAction.Gold = this.GetGoldUsurpCost(title) * 0.1f;
            claimAction.Influence = this.GetInfluenceUsurpCost(title) * 0.2f;
            claimAction.Renown = this.GetRenownUsurpCost(title) * 0.2f;
            List<Hero> possibleClaimants = this.GetClaimants(title);

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

            ClaimType claimType = title.GetHeroClaim(claimant);
            if (claimType == ClaimType.Ongoing)
            {
                claimAction.Possible = false;
                claimAction.Reason = new TextObject("{=!}Already building a claim.");
                return claimAction;
            }
            else if (claimType != ClaimType.None)
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
                Kingdom kingdom = claimant.Clan.Kingdom;
                if (kingdom != null && kingdom == title.deJure.Clan.Kingdom && BannerKingsConfig.Instance.TitleManager.GetAllDeJure(title.deJure).Count == 1)
                {
                    claimAction.Possible = false;
                    claimAction.Reason = new TextObject("{=!}Not possible to claim a knight's lordship within your faction.");
                    return claimAction;
                }

                FeudalTitle boundTitle = BannerKingsConfig.Instance.TitleManager.GetTitle(title.fief.Village.Bound);
                if (claimant != boundTitle.deJure)
                {
                    claimAction.Possible = false;
                    claimAction.Reason = new TextObject("{=!}Not possible to claim lordships without owning it's suzerain title.");
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

            return claimAction;
        }

        private TitleAction GetRevoke(FeudalTitle title, Hero revoker)
        {
            TitleAction revokeAction = new TitleAction(ActionType.Revoke, title, revoker);
            if (title == null || revoker == null) return null;
            revokeAction.Influence = GetInfluenceUsurpCost(title) * 0.8f;
            revokeAction.Renown = GetRenownUsurpCost(title) * 0.6f;

            if (title.deJure == revoker)
            {
                revokeAction.Possible = false;
                revokeAction.Reason = new TextObject("{=!}Already legal owner.");
                return revokeAction;
            }

            Kingdom revokerKingdom = revoker.Clan.Kingdom;
            if (revokerKingdom == null || revokerKingdom != title.deJure.Clan.Kingdom)
            {
                revokeAction.Possible = false;
                revokeAction.Reason = new TextObject("{=!}Can not revoke a title of a lord outside your realm.");
                return revokeAction;
            }

            GovernmentType governmentType = title.contract.Government;
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

                FeudalTitle sovereign = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(revokerKingdom);
                if (revoker != sovereign.deJure)
                {
                    revokeAction.Possible = false;
                    revokeAction.Reason = new TextObject("{=!}Not de Jure faction leader.");
                    return revokeAction;
                }
            }
            else if (governmentType == GovernmentType.Imperial)
            {
                FeudalTitle sovereign = BannerKingsConfig.Instance.TitleManager.GetSovereignTitle(revokerKingdom);
                if (sovereign == null || revoker != sovereign.deJure)
                {
                    revokeAction.Possible = false;
                    revokeAction.Reason = new TextObject("{=!}Not de Jure faction leader.");
                    return revokeAction;
                }
            }
            else
            {
                List<FeudalTitle> titles = BannerKingsConfig.Instance.TitleManager.GetAllDeJure(revoker);
                bool vassal = false;
                foreach (FeudalTitle revokerTitle in titles)
                    if (revokerTitle.vassals != null)
                        foreach (FeudalTitle revokerTitleVassal in revokerTitle.vassals)
                            if (revokerTitleVassal.deJure == title.deJure)
                            {
                                vassal = true;
                                break;
                            }

                if (!vassal)
                {
                    revokeAction.Possible = false;
                    revokeAction.Reason = new TextObject("{=!}Not a direct vassal.");
                    return revokeAction;
                }
            }


            FeudalTitle revokerHighest = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(revoker);
            FeudalTitle targetHighest = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(title.deJure);

            if (targetHighest.type <= revokerHighest.type)
            {
                revokeAction.Possible = false;
                revokeAction.Reason = new TextObject("{=!}Can not revoke from a lord of superior hierarchy.");
                return revokeAction;
            }

            revokeAction.Possible = true;
            revokeAction.Reason = new TextObject("{=!}You may grant away this title.");
            return revokeAction;
        }

        private TitleAction GetGrant(FeudalTitle title, Hero grantor)
        {
            TitleAction grantAction = new TitleAction(ActionType.Grant, title, grantor);
            if (title == null || grantor == null) return null;
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
                Hero deFacto = title.DeFacto;
                if (deFacto != grantor)
                {
                    grantAction.Possible = false;
                    grantAction.Reason = new TextObject("{=!}Not actual owner of landed title.");
                    return grantAction;
                }
            }

            List<Hero> candidates = this.GetGrantCandidates(grantor);
            if (candidates.Count == 0)
            {
                grantAction.Possible = false;
                grantAction.Reason = new TextObject("{=!}No valid candidates in kingdom.");
                return grantAction;
            }


            FeudalTitle highest = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(grantor);
            if (highest == title)
            {
                grantAction.Possible = false;
                grantAction.Reason = new TextObject("{=!}Not possible to grant one's main title.");
                return grantAction;

            }

            grantAction.Possible = true;
            grantAction.Influence = this.GetInfluenceUsurpCost(title) * 0.33f;
            grantAction.Reason = new TextObject("{=!}You may grant away this title.");
            return grantAction;
        }

        public TitleAction GetUsurp(FeudalTitle title, Hero usurper)
        {
            TitleAction usurpData = new TitleAction(ActionType.Usurp, title, usurper); 
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

            ClaimType type = title.GetHeroClaim(usurper);
            if (type != ClaimType.None && type != ClaimType.Ongoing)
            {
                usurpData.Possible = true;
                usurpData.Reason = new TextObject("{=!}You may claim this title.");

                int titleLevel = (int)title.type;
                int clanTier = usurper.Clan.Tier;
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
                    Kingdom faction = BannerKingsConfig.Instance.TitleManager.GetTitleFaction(title);
                    if (faction.Leader != usurper)
                    {
                        usurpData.Possible = false;
                        usurpData.Reason = new TextObject("{=!}Must be faction leader to usurp highest title in hierarchy.");
                        return usurpData;
                    }
                }

                return usurpData;
            }

            usurpData.Possible = false;
            usurpData.Reason = new TextObject("{=!}No rightful claim.");

            return usurpData;
        }

        public List<Hero> GetGrantCandidates(Hero grantor)
        {
            List<Hero> heroes = new List<Hero>();
            Kingdom kingdom = grantor.Clan.Kingdom;
            if (kingdom != null)
                foreach (Clan clan in kingdom.Clans)
                    if (!clan.IsUnderMercenaryService && clan != grantor.Clan)
                        heroes.Add(clan.Leader);

            return heroes;
        }

        public List<Hero> GetClaimants(FeudalTitle title)
        {
            List<Hero> claimants = new List<Hero>();
            Hero deFacto = title.DeFacto;
            if (deFacto != title.deJure)
            {
                if (title.fief == null)
                {
                    if (BannerKingsConfig.Instance.TitleManager.IsHeroTitleHolder(deFacto))
                        claimants.Add(deFacto);
                }
                else claimants.Add(deFacto);
            }
            if (title.sovereign != null && title.sovereign.deJure != title.deJure) claimants.Add(title.sovereign.deJure);
            if (title.vassals != null && title.vassals.Count > 0)
                foreach (FeudalTitle vassal in title.vassals)
                    if (vassal.deJure != title.deJure)
                        claimants.Add(vassal.deJure);
            return claimants;
        }

        private float GetInfluenceUsurpCost(FeudalTitle title) => 500f / (float)title.type + 1f;

        private float GetRenownUsurpCost(FeudalTitle title) => 100f / (float)title.type + 1f;

        public float GetGoldUsurpCost(FeudalTitle title)
        {
            float gold = 100000f / (float)title.type + 1f;
            if (title.fief != null)
            {
                PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(title.fief);
                gold += (float)data.TotalPop / 100f;
            }
            return gold;
        }

        public int GetRelationImpact(FeudalTitle title)
        {
            int result;
            if (title.type == TitleType.Lordship)
                result = MBRandom.RandomInt(5, 10);
            else if (title.type == TitleType.Barony)
                result = MBRandom.RandomInt(15, 25);
            else if (title.type == TitleType.County)
                result = MBRandom.RandomInt(30, 40);
            else if (title.type == TitleType.Dukedom)
                result = MBRandom.RandomInt(45, 55);
            else if (title.type == TitleType.Kingdom)
                result = MBRandom.RandomInt(80, 90);
            else result = MBRandom.RandomInt(120, 150);

            return -result;
        }

        public ExplainedNumber CalculateEffect(Settlement settlement)
        {
            return new ExplainedNumber();
        }
    }
}
