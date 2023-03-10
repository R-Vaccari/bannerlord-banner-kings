using System;
using BannerKings.Behaviours;
using BannerKings.Behaviours.Mercenary;
using BannerKings.Extensions;
using BannerKings.Managers.CampaignStart;
using BannerKings.Managers.Court.Members;
using BannerKings.Managers.Court.Members.Tasks;
using BannerKings.Managers.Education.Lifestyles;
using BannerKings.Managers.Institutions.Religions;
using BannerKings.Managers.Institutions.Religions.Doctrines;
using BannerKings.Managers.Populations;
using BannerKings.Managers.Populations.Villages;
using BannerKings.Managers.Skills;
using BannerKings.Managers.Titles.Laws;
using BannerKings.Settings;
using BannerKings.Utils.Extensions;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using static BannerKings.Managers.PopulationManager;

namespace BannerKings.Models.Vanilla
{
    public class BKInfluenceModel : DefaultClanPoliticsModel
    {
        public float GetClanInfluencePercentage(Clan clan)
        {
            float result = 0f;

            return result;
        }
        public float GetRejectKnighthoodCost(Clan clan)
        {
            return 10f + MathF.Max(CalculateInfluenceChange(clan).ResultNumber, 5f) * 0.025f * CampaignTime.DaysInYear;
        }

        public ExplainedNumber CalculateInfluenceCap(Clan clan, bool includeDescriptions = false)
        {
            ExplainedNumber result = new ExplainedNumber(50f, includeDescriptions);
            result.Add(clan.Tier * 150f, GameTexts.FindText("str_clan_tier_bonus"));

            foreach (var fief in clan.Fiefs)
            {
                var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(fief.Settlement);
                if (data != null)
                {
                    result.Add(CalculateSettlementInfluence(fief.Settlement, data, false).ResultNumber * 20f, fief.Name);
                }
            }

            foreach (var village in clan.GetActualVillages())
            {
                var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(village.Settlement);
                if (data != null)
                {
                    result.Add(CalculateSettlementInfluence(village.Settlement, data, false).ResultNumber * 25f, village.Name);
                }
            }

            foreach (var title in BannerKingsConfig.Instance.TitleManager.GetAllDeJure(clan))
            {
                result.Add(500 / ((int)title.type * 8f), title.FullName);
            }

            if (clan.Kingdom != null)
            {
                if (clan == clan.Kingdom.RulingClan)
                {
                    result.Add(350, new TextObject("{=IcgVKFxZ}Ruler"));
                }

                if (clan.Culture != clan.Kingdom.Culture)
                {
                    result.AddFactor(-0.2f, new TextObject("{=!}Kingdom cultural difference"));
                }
            }

            BannerKingsConfig.Instance.CourtManager.ApplyCouncilEffect(ref result,
                clan.Leader,
                DefaultCouncilPositions.Instance.Chancellor,
                DefaultCouncilTasks.Instance.ArbitrateRelations,
                0.2f,
                true);

            return result;
        }

        public override ExplainedNumber CalculateInfluenceChange(Clan clan, bool includeDescriptions = false)
        {
            var baseResult = base.CalculateInfluenceChange(clan, includeDescriptions);

            if (clan == Clan.PlayerClan && Campaign.Current.GetCampaignBehavior<BKCampaignStartBehavior>().HasDebuff(DefaultStartOptions.Instance.IndebtedLord))
            {
                baseResult.Add(-2f, DefaultStartOptions.Instance.IndebtedLord.Name);
            }

            ExplainedNumber cap = CalculateInfluenceCap(clan, includeDescriptions);
            if (cap.ResultNumber < clan.Influence)
            {
                baseResult.Add((clan.Influence / cap.ResultNumber) * -2f, new TextObject("{=!}Clan Influence Limit"));
            }

            var generalSupport = 0f;
            var generalAutonomy = 0f;
            float i = 0;

            var education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(clan.Leader);
            if (clan.IsUnderMercenaryService && clan.Leader != null)
            {
                var mercenaryChange = MathF.Ceiling(clan.Influence * (1f / Campaign.Current.Models.ClanFinanceModel.RevenueSmoothenFraction()));
                if (mercenaryChange != 0)
                {
                    if (education.Lifestyle != null && education.Lifestyle.Equals(DefaultLifestyles.Instance.Mercenary))
                    {
                        baseResult.Add((float)(mercenaryChange * 0.2f), new TextObject("{=cCQO7noU}{LIFESTYLE} lifestyle")
                                        .SetTextVariable("LIFESTYLE", DefaultLifestyles.Instance.Mercenary.Name));
                    }

                    if (education.HasPerk(BKPerks.Instance.VaryagRecognizedMercenary))
                    {
                        baseResult.Add((float)(mercenaryChange * 0.1f), BKPerks.Instance.VaryagRecognizedMercenary.Name);
                    }
                }

                var career = Campaign.Current.GetCampaignBehavior<BKMercenaryCareerBehavior>().GetCareer(clan);
                if (career != null && career.HasPrivilegeCurrentKingdom(DefaultMercenaryPrivileges.Instance.IncreasedPay))
                {
                    int level = career.GetPrivilegeLevelCurrentKingdom(DefaultMercenaryPrivileges.Instance.IncreasedPay);
                    baseResult.Add((float)(mercenaryChange * level * 0.05f), DefaultMercenaryPrivileges.Instance.IncreasedPay.Name);
                }
            }

            if (BannerKingsConfig.Instance.ReligionsManager.HasBlessing(clan.Leader, DefaultDivinities.Instance.DarusosianSecondary1))
            {
                baseResult.Add(2f, DefaultDivinities.Instance.DarusosianSecondary1.Name);
            }
            else if (BannerKingsConfig.Instance.ReligionsManager.HasBlessing(clan.Leader, DefaultDivinities.Instance.VlandiaSecondary1))
            {
                baseResult.Add(2f, DefaultDivinities.Instance.VlandiaSecondary1.Name);
            }

            if (education.HasPerk(BKPerks.Instance.OutlawPlunderer))
            {
                float bandits = 0;
                if (clan.Leader.PartyBelongedTo != null && !clan.Leader.IsPrisoner)
                {
                    foreach (var element in clan.Leader.PartyBelongedTo.MemberRoster.GetTroopRoster())
                    {
                        if (element.Character.Occupation == Occupation.Bandit)
                        {
                            bandits += element.Number;
                        }
                    }
                }

                baseResult.Add(bandits * 0.1f, BKPerks.Instance.OutlawPlunderer.Name);
            }

            var council = BannerKingsConfig.Instance.CourtManager.GetCouncil(clan);
            var religion = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(clan.Leader);
            if (religion != null && clan.Settlements.Count > 0)
            {
                var spiritual = council.GetCouncilPosition(DefaultCouncilPositions.Instance.Spiritual);
                if (religion.HasDoctrine(DefaultDoctrines.Instance.Druidism) &&
                    spiritual != null && spiritual.Member == null) 
                {
                    baseResult.Add(-4f, DefaultDoctrines.Instance.Druidism.Name);
                }
            }

            foreach (var settlement in clan.Settlements)
            {
                if (!settlement.IsVillage && !settlement.IsCastle && !settlement.IsTown)
                {
                    continue;
                }

                var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
                if (data == null || settlement.Name == null)
                {
                    continue;
                }

                if (BannerKingsConfig.Instance.AI.AcceptNotableAid(clan, data))
                {
                    foreach (var notable in data.Settlement.Notables)
                    {
                        if (notable.SupporterOf == clan && notable.Gold > 5000)
                        {
                            baseResult.Add(-1f,
                                new TextObject("{=WDHTvasY}Aid from {NOTABLE}").SetTextVariable("NOTABLE", notable.Name));
                        }
                    }
                }

                var settlementResult = CalculateSettlementInfluence(settlement, data, includeDescriptions);
                if (settlement.IsVillage)
                {
                    var owner = settlement.Village.GetActualOwner();
                    if (!owner.IsClanLeader() && owner.MapFaction == settlement.MapFaction)
                    {
                        BannerKingsConfig.Instance.TitleManager.AddKnightInfluence(owner, 
                            settlementResult.ResultNumber * 0.1f * BannerKingsSettings.Instance.KnightClanCreationSpeed);
                        continue;
                    }
                }

                generalSupport += data.NotableSupport.ResultNumber - 0.5f;
                generalAutonomy += -0.5f * data.Autonomy;
                i++;

                baseResult.Add(settlementResult.ResultNumber, settlement.Name);
            }

            var position = BannerKingsConfig.Instance.CourtManager.GetHeroPosition(clan.Leader);
            if (position != null)
            {
                baseResult.Add(position.IsCorePosition(position.StringId) ? 1f : 0.5f, new TextObject("{=WvhXhUFS}Councillor role"));
            }

            float currentVassals = BannerKingsConfig.Instance.StabilityModel.CalculateCurrentVassals(clan).ResultNumber;
            float vassalLimit = BannerKingsConfig.Instance.StabilityModel.CalculateVassalLimit(clan.Leader).ResultNumber;
            if (currentVassals > vassalLimit)
            {
                float factor = vassalLimit - currentVassals;
                baseResult.Add(factor * 1.5f, new TextObject("{=EF0OTQ0p}Over Vassal Limit"));
            }

            if (i > 0)
            {
                var finalSupport = MBMath.ClampFloat(generalSupport / i, -0.5f, 0.5f);
                var finalAutonomy = MBMath.ClampFloat(generalAutonomy / i, -0.5f, 0f);
                if (finalSupport != 0f)
                {
                    baseResult.AddFactor(finalSupport, new TextObject("{=RkKAd2Yp}Overall notable support"));
                }

                if (finalAutonomy != 0f)
                {
                    baseResult.AddFactor(finalAutonomy, new TextObject("{=qJbYtZjH}Overall settlement autonomy"));
                }
            }

            return baseResult;
        }

        public float GetNoblesInfluence(PopulationData data, float nobles)
        {
            float factor = 0.01f;
            if (data.TitleData != null && data.TitleData.Title != null)
            {
                var title = data.TitleData.Title;
                if (title.contract.IsLawEnacted(DefaultDemesneLaws.Instance.NoblesLaxDuties))
                {
                    factor = 0.011f;
                }
            }

            return MathF.Max(0f, nobles * factor);
        }
        
        public ExplainedNumber CalculateSettlementInfluence(Settlement settlement, PopulationData data, bool includeDescriptions = false)
        {
            var settlementResult = new ExplainedNumber(0f, includeDescriptions);
            settlementResult.LimitMin(0f);

            float nobles = data.GetTypeCount(PopType.Nobles);
            settlementResult.Add(MBMath.ClampFloat(GetNoblesInfluence(data, nobles), 0f, 20f), new TextObject($"{{=!}}Nobles influence from {settlement.Name}"));

            var villageData = data.VillageData;
            if (villageData != null)
            {
                float manor = villageData.GetBuildingLevel(DefaultVillageBuildings.Instance.Manor);
                if (manor > 0)
                {
                    settlementResult.AddFactor(Math.Abs(manor - 3) < 0.1f ? 0.5f : manor * 0.15f, new TextObject("{=UHyznyEy}Manor"));
                }
            }

            var owner = settlement.Owner;
            if (owner != null)
            {
                if (owner.GetPerkValue(BKPerks.Instance.LordshipManorLord))
                {
                    settlementResult.Add(0.2f, BKPerks.Instance.LordshipManorLord.Name);
                }
            }

            if (data.EstateData != null)
            {
                foreach (var estate in data.EstateData.Estates)
                {
                    float proportion = estate.Acreage/ data.LandData.Acreage;
                    float estateResult = MathF.Clamp(settlementResult.ResultNumber * proportion, 0f, settlementResult.ResultNumber * 0.2f);
                    settlementResult.Add(-estateResult, estate.Name);
                    if (!includeDescriptions && estate.Owner != null && estate.Owner.IsNotable)
                    {
                        estate.Owner.AddPower(estateResult);
                    }
                }
            }

            return settlementResult;
        }
    }
}