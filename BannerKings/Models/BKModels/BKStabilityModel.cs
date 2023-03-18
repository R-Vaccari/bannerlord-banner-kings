using System.Linq;
using BannerKings.Managers.Buildings;
using BannerKings.Managers.Court;
using BannerKings.Managers.Court.Members;
using BannerKings.Managers.Court.Members.Tasks;
using BannerKings.Managers.Education.Lifestyles;
using BannerKings.Managers.Institutions.Religions;
using BannerKings.Managers.Institutions.Religions.Doctrines;
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
                result.Add(change, new TextObject("{=!}"));

                var lordshipAdaptivePerk = BKPerks.Instance.LordshipAdaptive;
                if (settlement.Owner.GetPerkValue(lordshipAdaptivePerk))
                {
                    result.AddFactor(0.04f, lordshipAdaptivePerk.Name);
                }
            }
            else if (settlement.IsVillage && settlement.Village != null)
            {
                data.Stability = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement.Village.Bound).Stability;
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
            result.Add(change);
           

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

        public ExplainedNumber CalculateAutonomyTarget(Settlement settlement, float stability, bool descriptions = false)
        {
            var result = new ExplainedNumber(0f, descriptions);
            result.LimitMin(0f);
            result.LimitMax(1f);

            result.Add(1f - stability, new TextObject("{=cVOU505N}Stability"));
            if (settlement.Town is {Governor: {IsNotable: true}})
            {
                result.Add(0.2f, new TextObject("{=2dzbDVfy}Notable governor"));
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

                if (settlement.Culture != settlement.Owner.Culture && education.Lifestyle != null &&
                    education.Lifestyle.Equals(DefaultLifestyles.Instance.SiegeEngineer))
                {
                    result.Add(0.1f, DefaultLifestyles.Instance.SiegeEngineer.Name);
                }
            }


            return result;
        }

        public ExplainedNumber CalculateStabilityTarget(Settlement settlement, bool descriptions = false)
        {
            var result = new ExplainedNumber(0f, descriptions);
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

                var courthouse = town.Buildings.FirstOrDefault(x => x.BuildingType == BKBuildings.Instance.CourtHouse);
                if (courthouse != null && courthouse.CurrentLevel > 0)
                {
                    result.Add(0.02f * courthouse.CurrentLevel, BKBuildings.Instance.CourtHouse.Name);
                }

                result.Add(sec / 5f, new TextObject("{=TfMEfR6C}Security"));
                result.Add(loyalty / 5f, new TextObject("{=B8Crp0cZ}Loyalty"));
                result.Add(assimilation / 5f, new TextObject("{=rZOM0Jit}Cultural assimilation"));
                result.Add(averageSatisfaction / 5f, new TextObject("{=9Etcg2UE}Produce satisfactions"));
                result.Add(data.NotableSupport.ResultNumber / 5f, new TextObject("{=2VCUxNNq}Notable support"));

                var education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(town.OwnerClan.Leader);
                if (education.Perks.Contains(BKPerks.Instance.CivilOverseer))
                {
                    result.Add(0.05f, BKPerks.Instance.CivilOverseer.Name);
                }

                if (education.Lifestyle != null && education.Lifestyle.Equals(DefaultLifestyles.Instance.Mercenary))
                {
                    result.Add(-0.1f, new TextObject("{=cCQO7noU}{LIFESTYLE} lifestyle")
                        .SetTextVariable("LIFESTYLE", DefaultLifestyles.Instance.Mercenary.Name));
                }

                var religion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(town.OwnerClan.Leader);
                if (BannerKingsConfig.Instance.ReligionsManager.HasBlessing(town.OwnerClan.Leader, 
                    DefaultDivinities.Instance.DarusosianSecondary2, religion))
                {
                    result.Add(0.04f, DefaultDivinities.Instance.DarusosianSecondary2.Name);
                }

                if (BannerKingsConfig.Instance.ReligionsManager.HasBlessing(town.OwnerClan.Leader,
                    DefaultDivinities.Instance.TreeloreMain, religion) && religion.FavoredCultures.Contains(town.Culture))
                {
                    result.Add(0.06f, DefaultDivinities.Instance.DarusosianSecondary2.Name);
                }

                var demesneLimit = CalculateDemesneLimit(settlement.Owner).ResultNumber;
                var currentDemesne = CalculateCurrentDemesne(settlement.OwnerClan).ResultNumber;
                if (currentDemesne > demesneLimit)
                {
                    result.Add((demesneLimit - currentDemesne) * 0.18f, new TextObject("{=XzXwGy9f}Demesne over limit by {POINTS}")
                        .SetTextVariable("POINTS", demesneLimit - currentDemesne));
                }

                var legitimacy = 0f;
                var legitimacyType = (LegitimacyType) BannerKingsConfig.Instance.LegitimacyModel
                    .CalculateEffect(settlement).ResultNumber;
                legitimacy = legitimacyType switch
                {
                    LegitimacyType.Lawful => 0.1f,
                    LegitimacyType.Lawful_Foreigner => 0.05f,
                    LegitimacyType.Unlawful => -0.05f,
                    _ => -0.1f
                };

                var government = BannerKingsConfig.Instance.TitleManager.GetSettlementGovernment(settlement);
                if (government == GovernmentType.Feudal)
                {
                    result.Add(0.05f, new TextObject("{=PSrEtF5L}Government"));
                }

                result.Add(legitimacy, new TextObject("{=UqLsS4GV}Legitimacy"));
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
                if (title.Fief != null)
                {
                    continue;
                }

                var value = GetUnlandedDemesneWight(title.TitleType);
                if (value != 0f)
                {
                    result.Add(value, new TextObject("{=kfYELUGY}{TITLE}")
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
            if (title is {TitleType: <= TitleType.Kingdom})
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
            if (title.Fief != null)
            {
                return GetSettlementDemesneWight(title.Fief);
            }

            return GetUnlandedDemesneWight(title.TitleType);
        }

        public float GetUnlandedDemesneWight(TitleType type)
        {
            var value = type switch
            {
                TitleType.Dukedom => 0.5f,
                <= TitleType.Kingdom => 1f,
                _ => 1.5f
            };

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

            var lordshipManorLord = BKPerks.Instance.LordshipManorLord;
            if (settlement.Owner.GetPerkValue(lordshipManorLord))
            {
                value -= value * 0.2f / 100;
            }

            return value;
        }

        public ExplainedNumber CalculateCurrentDemesne(Clan clan, bool descriptions = false)
        {
            var result = new ExplainedNumber(0f, descriptions);
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
                    result.Add(value, new TextObject("{=ouK35RSP}{SETTLEMENT}{DEJURE}")
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
            var list = BannerKingsConfig.Instance.TitleManager.CalculateAllVassals(clan);
            foreach (var vassal in list)
            {
                if (vassal.Clan == leader.Clan)
                {
                    result.Add(0.5f, vassal.Name);
                }
                else
                {
                    result.Add(1f, vassal.Name);
                }
            }

            return result;
        }

        public ExplainedNumber CalculateDemesneLimit(Hero hero, bool descriptions = false)
        {
            var result = new ExplainedNumber(0.5f, descriptions);
            result.LimitMin(0.5f);
            result.LimitMax(10f);

            result.Add(hero.Clan.Tier / 2f, GameTexts.FindText("str_clan_tier_bonus"));

            var title = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(hero);
            if (title != null)
            {
                var bonus = 0f;
                if (title.TitleType != TitleType.Lordship)
                {
                    bonus = title.TitleType switch
                    {
                        TitleType.Barony => 0.5f,
                        TitleType.County => 1f,
                        TitleType.Dukedom => 3f,
                        TitleType.Kingdom => 6f,
                        _ => 10f
                    };
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

            if (education.Lifestyle != null && education.Lifestyle.Equals(DefaultLifestyles.Instance.Jawwal))
            {
                result.AddFactor(-0.2f, DefaultLifestyles.Instance.Jawwal.Name);
            }

            return result;
        }

        public ExplainedNumber CalculateVassalLimit(Hero hero)
        {
            var result = new ExplainedNumber(0.5f, true);
            result.LimitMin(0f);
            result.LimitMax(20f);
            result.Add(hero.Clan.Tier / 2f, GameTexts.FindText("str_clan_tier_bonus"));

            if (hero.GetPerkValue(BKPerks.Instance.LordshipAccolade))
            {
                result.Add(1f, BKPerks.Instance.LordshipAccolade.Name);
            }

            var religion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(hero);
            if (religion != null && religion.HasDoctrine(DefaultDoctrines.Instance.Legalism))
            {
                var virtues = BannerKingsConfig.Instance.PietyModel.GetHeroVirtuesCount(hero);
                if (virtues > 0)
                {
                    result.Add(virtues * 0.5f, DefaultDoctrines.Instance.Legalism.Name);
                }
            }

            var title = BannerKingsConfig.Instance.TitleManager.GetHighestTitle(hero);
            if (title == null)
            {
                return result;
            }

            var bonus = 0f;
            if (title.TitleType != TitleType.Lordship)
            {
                switch (title.TitleType)
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
                    case TitleType.Empire:
                    case TitleType.Kingdom:
                    default:
                    {
                        bonus = title.TitleType == TitleType.Kingdom 
                            ? 3f 
                            : 4f;

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

            return result;
        }
    }
}