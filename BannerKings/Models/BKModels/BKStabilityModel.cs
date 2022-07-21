using BannerKings.Managers.Titles;
using BannerKings.Populations;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Models
{
    public class BKStabilityModel : IBannerKingsModel
    {
        public ExplainedNumber CalculateAutonomyEffect(Settlement settlement, float stability, float autonomy)
        {
            ExplainedNumber result = new ExplainedNumber();
            result.LimitMin(-0.01f);
            result.LimitMax(0.01f);
            float targetAutonomy = CalculateAutonomyTarget(settlement, stability).ResultNumber;
            float random1 = 0.005f * MBRandom.RandomFloat;
            float random2 = 0.005f * MBRandom.RandomFloat;
            float change = targetAutonomy > autonomy ? 0.005f + random1 - random2 : targetAutonomy < autonomy ? -0.005f - random1 + random2 : 0f;
            result.Add(change, new TextObject());

            return result;
        }
        public ExplainedNumber CalculateEffect(Settlement settlement)
        {
            ExplainedNumber result = new ExplainedNumber();
            result.LimitMin(-0.01f);
            result.LimitMax(0.01f);
            PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
            float stability = data.Stability;
            if (settlement.Town != null)
            {
                float targetStability = this.CalculateStabilityTarget(settlement).ResultNumber;
                float random1 = 0.005f * MBRandom.RandomFloat;
                float random2 = 0.005f * MBRandom.RandomFloat;
                float change = targetStability > stability ? 0.005f + random1 - random2 : targetStability < stability ? -0.005f - random1 + random2 : 0f;
                result.Add(change, new TextObject());
            }
            else if (settlement.IsVillage && settlement.Village != null)
            {
                data.Stability = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement.Village.Bound).Stability;
            }

            return result;
        }

        public ExplainedNumber CalculateNotableSupport(Settlement settlement)
        {
            ExplainedNumber result = new ExplainedNumber(0f, true);
            result.LimitMin(0f);
            result.LimitMax(1f);
            float support = 0f;
            float totalPower = 0f;
            foreach (Hero notable in settlement.Notables)
                totalPower += notable.Power;

            foreach (Hero notable in settlement.Notables)
            {
                float powerShare = notable.Power / totalPower;
                float relation = (float)notable.GetRelation(settlement.OwnerClan.Leader) * 0.01f + 0.5f;
                result.Add(relation * powerShare, notable.Name);
            }

            return result;
        }

        public ExplainedNumber CalculateAutonomyTarget(Settlement settlement, float stability)
        {
            ExplainedNumber result = new ExplainedNumber(0f, true);
            result.LimitMin(0f);
            result.LimitMax(1f);

            result.Add(1f - stability, new TextObject("{=!}Stability"));
            if (settlement.Town != null && settlement.Town.Governor != null && settlement.Town.Governor.IsNotable)
                result.Add(0.2f, new TextObject("{=!}Notable governor"));

            if (settlement.Culture == settlement.Owner.Culture)
                result.Add(-0.1f, GameTexts.FindText("str_culture"));


            return result;
        }

        public ExplainedNumber CalculateStabilityTarget(Settlement settlement)
        {
            ExplainedNumber result = new ExplainedNumber(0f, true);
            result.LimitMin(0f);
            result.LimitMax(1f);
            PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
            float stability = data.Stability;
            if (settlement.Town != null)
            {
                Town town = settlement.Town;
                float sec = town.Security * 0.01f;
                float loyalty = town.Loyalty * 0.01f;
                float assimilation = data.CultureData.GetAssimilation(settlement.Owner.Culture);
                float[] satisfactions = data.EconomicData.Satisfactions;

                float averageSatisfaction = 0f;
                foreach (float satisfaction in satisfactions)
                    averageSatisfaction += satisfaction / 4f;

                result.Add(sec / 5f, new TextObject("Security"));
                result.Add(loyalty / 5f, new TextObject("Loyalty"));
                result.Add(assimilation / 5f, new TextObject("Cultural assimilation"));
                result.Add(averageSatisfaction / 5f, new TextObject("Produce satisfactions"));
                result.Add(data.NotableSupport.ResultNumber / 5f, new TextObject("{=!}Notable support"));

                float demesneLimit = CalculateDemesneLimit(settlement.Owner).ResultNumber;
                float currentDemesne = CalculateCurrentDemesne(settlement.OwnerClan).ResultNumber;
                if (currentDemesne > demesneLimit) result.Add((demesneLimit - currentDemesne) * 5f, new TextObject("{=!}Demesne over limit by {POINTS}")
                    .SetTextVariable("POINTS", demesneLimit - currentDemesne));

                float legitimacy = 0f;
                LegitimacyType legitimacyType = (LegitimacyType)BannerKingsConfig.Instance.Models.First(x => x.GetType() == typeof(BKLegitimacyModel))
                    .CalculateEffect(settlement).ResultNumber;
                if (legitimacyType == LegitimacyType.Lawful)
                    legitimacy = 0.1f;
                else if (legitimacyType == LegitimacyType.Lawful_Foreigner)
                    legitimacy = 0.05f;
                else if (legitimacyType == LegitimacyType.Unlawful)
                    legitimacy = -0.05f;
                else legitimacy = -0.1f;

                GovernmentType government = BannerKingsConfig.Instance.TitleManager.GetSettlementGovernment(settlement);
                if (government == GovernmentType.Feudal)
                    result.Add(0.05f, new TextObject("{=!}Government"));

                result.Add(legitimacy, new TextObject("Legitimacy"));
            }
            return result;
        }


        public ExplainedNumber CalculateCurrentUnlandedDemesne(Clan clan)
        {
            ExplainedNumber result = new ExplainedNumber(0f, true);
            result.LimitMin(0f);

            Hero leader = clan.Leader;
            foreach (FeudalTitle title in BannerKingsConfig.Instance.TitleManager.GetAllDeJure(leader))
            {
                float value = 0f;
                if (title.type == TitleType.Dukedom) value = 0.5f;
                else if (title.type <= TitleType.Kingdom) value = 1f;
                else if (title.type <= TitleType.Empire) value = 1.5f;

                if (value != 0f)
                    result.Add(value, new TextObject("{=!}{TITLE}")
                        .SetTextVariable("SETTLEMENT", title.FullName));
            }

            return result;
        }

        public ExplainedNumber CalculateUnlandedDemesneLimit(Hero hero)
        {
            ExplainedNumber result = new ExplainedNumber(0f, true);
            result.LimitMin(0f);
            result.LimitMax(5f);

            result.Add(hero.Clan.Tier / 3f, GameTexts.FindText("str_clan_tier_bonus"));
            return result;
        }

        public ExplainedNumber CalculateCurrentDemesne(Clan clan)
        {
            ExplainedNumber result = new ExplainedNumber(0f, true);
            result.LimitMin(0f);

            Hero leader = clan.Leader;
            foreach (Settlement settlement in clan.Settlements)
            {
                float value;
                if (settlement.IsTown) value = 2f;
                else if (settlement.IsCastle) value = 1f;
                else value = 0.5f;

                bool deJure = false;
                FeudalTitle title = BannerKingsConfig.Instance.TitleManager.GetTitle(settlement);
                if (title != null)
                {
                    if (title.deJure == leader)
                    {
                        deJure = true;
                        value *= 0.75f;
                    }
                    else if (title.deJure != null && title.deJure.Clan == clan) value = 0f;
                }

                if (value != 0f) 
                    result.Add(value, new TextObject("{=!}{SETTLEMENT}{DEJURE}")
                        .SetTextVariable("SETTLEMENT", settlement.Name)
                        .SetTextVariable("DEJURE", deJure ? "(de Jure)" : ""));
            }

            return result;
        }

        public ExplainedNumber CalculateCurrentVassals(Clan clan)
        {
            ExplainedNumber result = new ExplainedNumber(0f, true);
            result.LimitMin(0f);

            Hero leader = clan.Leader;
            foreach (Hero hero in BannerKingsConfig.Instance.TitleManager.CalculateVassals(leader))
                result.Add(1f, hero.Name);

            return result;
        }

        public ExplainedNumber CalculateDemesneLimit(Hero hero)
        {
            ExplainedNumber result = new ExplainedNumber(0.5f, true);
            result.LimitMin(0.5f);
            result.LimitMax(10f);

            result.Add(hero.Clan.Tier / 2f, GameTexts.FindText("str_clan_tier_bonus"));

            FeudalTitle title = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(hero);
            if (title != null)
            {
                float bonus = 0f;
                if (title.type != TitleType.Lordship)
                {
                    if (title.type == TitleType.Barony) bonus = 0.5f;
                    else if (title.type == TitleType.County) bonus = 1f;
                    else if (title.type == TitleType.Dukedom) bonus = 3f;
                    else if (title.type == TitleType.Kingdom) bonus = 6f;
                    else bonus = 10f;
                }

                if (bonus > 0f) result.Add(bonus, new TextObject("Highest title level"));
            }

            return result;
        }

        public ExplainedNumber CalculateVassalLimit(Hero hero)
        {
            ExplainedNumber result = new ExplainedNumber(0f, true);
            result.LimitMin(0f);
            result.LimitMax(50f);
            result.Add(hero.Clan.Tier, GameTexts.FindText("str_clan_tier_bonus"));

            FeudalTitle title = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(hero);
            if (title != null)
            {
                float bonus = 0f;
                if (title.type != TitleType.Lordship)
                {
                    if (title.type == TitleType.Barony) bonus = 1f;
                    else if (title.type == TitleType.County) bonus = 2f;
                    else if (title.type == TitleType.Dukedom) bonus = 4f;
                    else if (title.type == TitleType.Kingdom) bonus = 10f;
                    else bonus = 20f;
                }

                if (bonus > 0f) result.Add(bonus, new TextObject("Highest title level"));
            }

            return result;
        }
    }
}
