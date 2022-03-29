using BannerKings.Populations;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using static BannerKings.Managers.TitleManager;

namespace BannerKings.Models
{
    public class BKTitleModel : IBannerKingsModel
    {

        public UsurpData IsUsurpable(FeudalTitle title, Hero hero)
        {
            UsurpData usurpData = new UsurpData(); 
            usurpData.Gold = GetGoldUsurpCost(title);
            usurpData.Influence = GetInfluenceUsurpCost(title);
            usurpData.Renown = GetRenownUsurpCost(title);
            if (title.deJure == hero)
            {
                usurpData.Usurpable = false;
                usurpData.Reason = new TextObject("{=!}Already legal owner.");
                return usurpData;
            }

            if (hero.Clan == null)
            {
                usurpData.Usurpable = false;
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
                usurpData.Usurpable = true;
                usurpData.Reason = new TextObject("{=!}You may claim this title.");

                int titleLevel = (int)title.type;
                int clanTier = hero.Clan.Tier;
                if (clanTier < 2 || (titleLevel >= 2 && clanTier < 4))
                {
                    usurpData.Usurpable = false;
                    usurpData.Reason = new TextObject("{=!}Clan tier is insufficient.");
                    return usurpData;
                }


                if (hero.Gold < usurpData.Gold || hero.Clan.Influence < usurpData.Influence)
                {
                    usurpData.Usurpable = false;
                    usurpData.Reason = new TextObject("{=!}You do not have the required resources to obtain this title.");
                    return usurpData;
                }

                return usurpData;
            }


            usurpData.Usurpable = false;
            TextObject reasonText = new TextObject("{=!}You have no claim to this title ({REASON}).");

            if (title.fief != null) reasonText.SetTextVariable("REASON", "Not de facto owner");
            else reasonText.SetTextVariable("REASON", "Not owner of 51% or more of vassals.");
            usurpData.Reason = reasonText;


            return usurpData;
        }

        private float GetInfluenceUsurpCost(FeudalTitle title) => 100f / (float)title.type + 2f;

        private float GetRenownUsurpCost(FeudalTitle title) => 10f / (float)title.type + 2f;

        private float GetGoldUsurpCost(FeudalTitle title)
        {
            float gold = 1000f / (float)title.type + 3f;
            if (title.fief != null)
            {
                PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(title.fief);
                gold += (float)data.TotalPop / 100f;
            }
            return gold;
        }

        public int GetUsurpRelationImpact(FeudalTitle title)
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
