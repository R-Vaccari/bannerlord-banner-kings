using System.Linq;
using BannerKings.Managers.Skills;
using BannerKings.Managers.Titles;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Models.BKModels
{
    public class BKStabilityModel : IBannerKingsModel
    {
        public ExplainedNumber CalculateEffect(Settlement settlement)
        {
            var result = new ExplainedNumber();
            result.LimitMin(-0.01f);
            result.LimitMax(0.01f);
            var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
            var stability = data.Stability;
            if (settlement.Town != null)
            {
                var targetStability = CalculateStabilityTarget(settlement).ResultNumber;
                var random1 = 0.005f * MBRandom.RandomFloat;
                var random2 = 0.005f * MBRandom.RandomFloat;
                var change = targetStability > stability ? 0.005f + random1 - random2 :
                    targetStability < stability ? -0.005f - random1 + random2 : 0f;
                result.Add(change, new TextObject());
            }
            else if (settlement.IsVillage && settlement.Village != null)
            {
                data.Stability = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement.Village.Bound)
                    .Stability;
            }

            return result;
        }

        public bool IsHeroOverDemesneLimit(Hero hero)
        {
            return CalculateCurrentDemesne(hero.Clan).ResultNumber > CalculateDemesneLimit(hero).ResultNumber;
        }

        public bool IsHeroOverUnlandedDemesneLimit(Hero hero)
        {
            return CalculateCurrentUnlandedDemesne(hero.Clan).ResultNumber >
                   CalculateUnlandedDemesneLimit(hero).ResultNumber;
        }

        public bool IsHeroOverVassalLimit(Hero hero)
        {
            return CalculateCurrentVassals(hero.Clan).ResultNumber > CalculateVassalLimit(hero).ResultNumber;
        }

        public ExplainedNumber CalculateAutonomyEffect(Settlement settlement, float stability, float autonomy)
        {
            var result = new ExplainedNumber();
            result.LimitMin(-0.01f);
            result.LimitMax(0.01f);
            var targetAutonomy = CalculateAutonomyTarget(settlement, stability).ResultNumber;
            var random1 = 0.005f * MBRandom.RandomFloat;
            var random2 = 0.005f * MBRandom.RandomFloat;
            var change = targetAutonomy > autonomy ? 0.005f + random1 - random2 :
                targetAutonomy < autonomy ? -0.005f - random1 + random2 : 0f;
            result.Add(change, new TextObject());

            return result;
        }

        public ExplainedNumber CalculateNotableSupport(Settlement settlement)
        {
            var result = new ExplainedNumber(0f, true);
            result.LimitMin(0f);
            result.LimitMax(1f);
            var totalPower = 0f;
            foreach (var notable in settlement.Notables)
            {
                totalPower += notable.Power;
            }

            foreach (var notable in settlement.Notables)
            {
                var powerShare = notable.Power / totalPower;
                var relation = notable.GetRelation(settlement.OwnerClan.Leader) * 0.01f + 0.5f;
                result.Add(relation * powerShare, notable.Name);
            }

            return result;
        }

        public ExplainedNumber CalculateAutonomyTarget(Settlement settlement, float stability)
        {
            var result = new ExplainedNumber(0f, true);
            result.LimitMin(0f);
            result.LimitMax(1f);

            result.Add(1f - stability, new TextObject("{=!}Stability"));
            if (settlement.Town is {Governor: {IsNotable: true}})
            {
                result.Add(0.2f, new TextObject("{=!}Notable governor"));
            }

            if (settlement.Culture == settlement.Owner.Culture)
            {
                result.Add(-0.1f, GameTexts.FindText("str_culture"));
            }

            if (settlement.OwnerClan != null)
            {
                var education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(settlement.Owner);
                if (education.HasPerk(BKPerks.Instance.AugustDeFacto))
                {
                    result.Add(-0.03f, BKPerks.Instance.AugustDeFacto.Name);
                }
            }


            return result;
        }

        public ExplainedNumber CalculateStabilityTarget(Settlement settlement)
        {
            var result = new ExplainedNumber(0f, true);
            result.LimitMin(0f);
            result.LimitMax(1f);
            var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
            var stability = data.Stability;
            if (settlement.Town != null)
            {
                var town = settlement.Town;
                var sec = town.Security * 0.01f;
                var loyalty = town.Loyalty * 0.01f;
                var assimilation = data.CultureData.GetAssimilation(settlement.Owner.Culture);
                var satisfactions = data.EconomicData.Satisfactions;

                var averageSatisfaction = 0f;
                foreach (var satisfaction in satisfactions)
                {
                    averageSatisfaction += satisfaction / 4f;
                }

                result.Add(sec / 5f, new TextObject("Security"));
                result.Add(loyalty / 5f, new TextObject("Loyalty"));
                result.Add(assimilation / 5f, new TextObject("Cultural assimilation"));
                result.Add(averageSatisfaction / 5f, new TextObject("Produce satisfactions"));
                result.Add(data.NotableSupport.ResultNumber / 5f, new TextObject("{=!}Notable support"));

                var education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(town.OwnerClan.Leader);
                if (education.Perks.Contains(BKPerks.Instance.CivilOverseer))
                {
                    result.Add(0.05f, BKPerks.Instance.CivilOverseer.Name);
                }

                var demesneLimit = CalculateDemesneLimit(settlement.Owner).ResultNumber;
                var currentDemesne = CalculateCurrentDemesne(settlement.OwnerClan).ResultNumber;
                if (currentDemesne > demesneLimit)
                {
                    result.Add((demesneLimit - currentDemesne) * 0.18f, new TextObject("{=!}Demesne over limit by {POINTS}")
                        .SetTextVariable("POINTS", demesneLimit - currentDemesne));
                }

                var legitimacy = 0f;
                var legitimacyType = (LegitimacyType) BannerKingsConfig.Instance.Models
                    .First(x => x.GetType() == typeof(BKLegitimacyModel))
                    .CalculateEffect(settlement).ResultNumber;
                switch (legitimacyType)
                {
                    case LegitimacyType.Lawful:
                        legitimacy = 0.1f;
                        break;
                    case LegitimacyType.Lawful_Foreigner:
                        legitimacy = 0.05f;
                        break;
                    case LegitimacyType.Unlawful:
                        legitimacy = -0.05f;
                        break;
                    default:
                        legitimacy = -0.1f;
                        break;
                }

                var government = BannerKingsConfig.Instance.TitleManager.GetSettlementGovernment(settlement);
                if (government == GovernmentType.Feudal)
                {
                    result.Add(0.05f, new TextObject("{=!}Government"));
                }

                result.Add(legitimacy, new TextObject("Legitimacy"));
            }

            return result;
        }


        public ExplainedNumber CalculateCurrentUnlandedDemesne(Clan clan)
        {
            var result = new ExplainedNumber(0f, true);
            result.LimitMin(0f);

            var leader = clan.Leader;
            foreach (var title in BannerKingsConfig.Instance.TitleManager.GetAllDeJure(leader))
            {
                if (title.fief != null)
                {
                    continue;
                }

                var value = GetUnlandedDemesneWight(title.type);
                if (value != 0f)
                {
                    result.Add(value, new TextObject("{=!}{TITLE}")
                        .SetTextVariable("SETTLEMENT", title.FullName));
                }
            }

            return result;
        }

        public ExplainedNumber CalculateUnlandedDemesneLimit(Hero hero)
        {
            var result = new ExplainedNumber(0.5f, true);
            result.LimitMin(0f);
            result.LimitMax(5f);

            result.Add(hero.Clan.Tier / 3f, GameTexts.FindText("str_clan_tier_bonus"));

            var title = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(hero);
            if (title is {type: <= TitleType.Kingdom})
            {
                var education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(hero);
                if (education.HasPerk(BKPerks.Instance.AugustKingOfKings))
                {
                    result.Add(1f, BKPerks.Instance.AugustKingOfKings.Name);
                }
            }


            return result;
        }

        public float GetTitleScore(FeudalTitle title)
        {
            if (title.fief != null)
            {
                return GetSettlementDemesneWight(title.fief);
            }

            return GetUnlandedDemesneWight(title.type);
        }

        public float GetUnlandedDemesneWight(TitleType type)
        {
            float value;
            switch (type)
            {
                case TitleType.Dukedom:
                    value = 0.5f;
                    break;
                case <= TitleType.Kingdom:
                    value = 1f;
                    break;
                default:
                    value = 1.5f;
                    break;
            }

            return value;
        }

        public float GetSettlementDemesneWight(Settlement settlement)
        {
            float value;
            if (settlement.IsTown)
            {
                value = 2f;
            }
            else if (settlement.IsCastle)
            {
                value = 1f;
            }
            else
            {
                value = 0.5f;
            }

            return value;
        }

        public ExplainedNumber CalculateCurrentDemesne(Clan clan)
        {
            var result = new ExplainedNumber(0f, true);
            result.LimitMin(0f);

            var leader = clan.Leader;
            foreach (var settlement in clan.Settlements)
            {
                var value = GetSettlementDemesneWight(settlement);
                var deJure = false;
                var title = BannerKingsConfig.Instance.TitleManager.GetTitle(settlement);
                if (title != null)
                {
                    if (title.deJure == leader)
                    {
                        deJure = true;
                        value *= 0.75f;
                    }
                    else if (title.deJure != null && title.deJure.Clan == clan)
                    {
                        value = 0f;
                    }
                }

                if (value != 0f)
                {
                    result.Add(value, new TextObject("{=!}{SETTLEMENT}{DEJURE}")
                        .SetTextVariable("SETTLEMENT", settlement.Name)
                        .SetTextVariable("DEJURE", deJure ? "(de Jure)" : ""));
                }
            }

            return result;
        }

        public ExplainedNumber CalculateCurrentVassals(Clan clan)
        {
            var result = new ExplainedNumber(0f, true);
            result.LimitMin(0f);

            var leader = clan.Leader;
            foreach (var title in BannerKingsConfig.Instance.TitleManager.GetAllDeJure(clan))
            {
                if (title.vassals == null || title.vassals.Count == 0)
                {
                    continue;
                }

                foreach (var vassal in title.vassals)
                {
                    var deJure = vassal.deJure;
                    if (deJure == null || deJure == leader)
                    {
                        continue;
                    }

                    if (deJure.Clan == leader.Clan)
                    {
                        result.Add(0.5f, deJure.Name);
                    }
                    else
                    {
                        var suzerain = BannerKingsConfig.Instance.TitleManager.CalculateHeroSuzerain(deJure);
                        if (suzerain != null && suzerain.deJure == leader)
                        {
                            result.Add(1f, deJure.Name);
                        }
                    }
                }
            }

            return result;
        }

        public ExplainedNumber CalculateDemesneLimit(Hero hero)
        {
            var result = new ExplainedNumber(0.5f, true);
            result.LimitMin(0.5f);
            result.LimitMax(10f);

            result.Add(hero.Clan.Tier / 2f, GameTexts.FindText("str_clan_tier_bonus"));

            var title = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(hero);
            if (title != null)
            {
                var bonus = 0f;
                if (title.type != TitleType.Lordship)
                {
                    switch (title.type)
                    {
                        case TitleType.Barony:
                            bonus = 0.5f;
                            break;
                        case TitleType.County:
                            bonus = 1f;
                            break;
                        case TitleType.Dukedom:
                            bonus = 3f;
                            break;
                        case TitleType.Kingdom:
                            bonus = 6f;
                            break;
                        default:
                            bonus = 10f;
                            break;
                    }
                }

                if (bonus > 0f)
                {
                    result.Add(bonus, new TextObject("Highest title level"));
                }
            }

            var education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(hero);
            if (education.HasPerk(BKPerks.Instance.AugustDeJure))
            {
                result.Add(1f, BKPerks.Instance.AugustDeJure.Name);
            }

            return result;
        }

        public ExplainedNumber CalculateVassalLimit(Hero hero)
        {
            var result = new ExplainedNumber(0.5f, true);
            result.LimitMin(0f);
            result.LimitMax(20f);
            result.Add(hero.Clan.Tier / 2f, GameTexts.FindText("str_clan_tier_bonus"));

            var title = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(hero);
            if (title != null)
            {
                var bonus = 0f;
                if (title.type != TitleType.Lordship)
                {
                    switch (title.type)
                    {
                        case TitleType.Barony:
                            bonus = 0.5f;
                            break;
                        case TitleType.County:
                            bonus = 1f;
                            break;
                        case TitleType.Dukedom:
                            bonus = 1.5f;
                            break;
                        default:
                        {
                            if (title.type == TitleType.Kingdom)
                            {
                                bonus = 3f;
                            }
                            else
                            {
                                bonus = 4f;
                            }

                            var education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(hero);
                            if (education.HasPerk(BKPerks.Instance.AugustKingOfKings))
                            {
                                result.Add(2f, BKPerks.Instance.AugustKingOfKings.Name);
                            }

                            break;
                        }
                    }
                }

                if (bonus > 0f)
                {
                    result.Add(bonus, new TextObject("Highest title level"));
                }
            }

            return result;
        }
    }
}