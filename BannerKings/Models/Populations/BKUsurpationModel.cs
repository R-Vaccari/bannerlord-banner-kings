using BannerKings.Populations;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using static BannerKings.Managers.TitleManager;

namespace BannerKings.Models
{
    class BKUsurpationModel : IBannerKingsModel
    {

        public UsurpCosts GetUsurpationCosts(FeudalTitle title, Hero hero)
        {
            bool costsRenown = title.deJure.Clan.Kingdom == hero.Clan.Kingdom;
            float gold, influence, renown = 0f;
            influence = GetInfluenceUsurpCost(title);
            gold = GetGoldUsurpCost(title);
            if (costsRenown) renown = GetRenownUsurpCost(title);

            if (title.vassals != null && title.vassals.Count > 0)
                foreach (FeudalTitle vassal in title.vassals)
                    if (vassal.deJure != hero)
                    {
                        influence += GetInfluenceUsurpCost(vassal);
                        gold += GetGoldUsurpCost(title);
                        if (costsRenown) renown += GetRenownUsurpCost(vassal);
                    }
            return new UsurpCosts(gold, influence, renown);
        }

        public (bool, string) IsUsurpable(FeudalTitle title, Hero hero)
        {

            bool renown = false;
            bool gold = false;
            bool influence = false;
            bool type = false;
            string explanation = "";
            if (title.deJure != hero)
            {
                if ((int)title.type >= 3)
                    renown = hero.Clan.Tier >= 3;
                else if (title.type == TitleType.Dukedom)
                    renown = hero.Clan.Tier >= 4;
                else if ((int)title.type <= 2)
                    renown = hero.Clan.Tier >= 5;

                if (!renown) explanation = "Clan tier is insufficient.";
                else
                {
                    UsurpCosts costs = this.GetUsurpationCosts(title, hero);
                    gold = hero.Gold >= costs.gold;
                    influence = hero.Clan.Influence >= costs.influence;

                    if (gold == false || influence == false)
                        explanation = "You do not have the required resources to obtain this title.";
                }
            }

            if ((int)title.type >= 3)
                type = true;
            else if (title.vassals != null && title.vassals.Count > 0)
            {
                foreach (FeudalTitle vassal in title.vassals)
                    if (vassal.deJure == hero)
                        type = true;
            }

            if (type == false)
                explanation = "You are required to own one of this title's vassals before usurping it.";

            return (renown && influence && gold, explanation);
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
            throw new System.NotImplementedException();
        }
    }
}
