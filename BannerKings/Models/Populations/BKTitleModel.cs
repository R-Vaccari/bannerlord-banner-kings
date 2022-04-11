using BannerKings.Managers.Titles;
using BannerKings.Populations;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Models
{
    public class BKTitleModel : IBannerKingsModel
    {


        public TitleAction GetAction(ActionType type, FeudalTitle title, Hero taker, Hero receiver = null)
        {
            if (type == ActionType.Usurp)
                return GetUsurp(title, taker);
            else return GetGrant(title, taker);
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
                grantAction.Reason = new TextObject("{=!}Not possible to grant one's highest title.");
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

            bool claim = title.DeFacto == Hero.MainHero;
            if (!claim)
                if (title.vassals != null && title.vassals.Count > 0)
                    foreach (FeudalTitle vassal in title.vassals)
                        if (vassal.deJure == Hero.MainHero)
                        {
                            claim = true;
                            break;
                        }

            if (claim)
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
                    usurpData.Reason = new TextObject("{=!}You do not have the required resources to obtain this title.");
                    return usurpData;
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

        private float GetGoldUsurpCost(FeudalTitle title)
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
