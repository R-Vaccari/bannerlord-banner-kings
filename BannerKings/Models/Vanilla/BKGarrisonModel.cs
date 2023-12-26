using System.Linq;
using BannerKings.Managers.Policies;
using BannerKings.Utils;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using static BannerKings.Managers.Policies.BKGarrisonPolicy;

namespace BannerKings.Models.Vanilla
{
    public class BKGarrisonModel : DefaultSettlementGarrisonModel
    {
        public override ExplainedNumber CalculateGarrisonChange(Settlement settlement, bool includeDescriptions = false)
        {
            var baseResult = base.CalculateGarrisonChange(settlement, includeDescriptions);
            if (BannerKingsConfig.Instance.PopulationManager != null &&
                BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(settlement))
            {
                var garrison =
                    ((BKGarrisonPolicy) BannerKingsConfig.Instance.PolicyManager.GetPolicy(settlement, "garrison")).Policy;
                switch (garrison)
                {
                    case GarrisonPolicy.Dischargement:
                        baseResult.Add(-1f, new TextObject("{=DEhtngoL}Garrison policy"));
                        break;
                    case GarrisonPolicy.Enlistment:
                        baseResult.Add(1f, new TextObject("{=DEhtngoL}Garrison policy"));
                        break;
                }
            }

            return baseResult;
        }

        public override int FindNumberOfTroopsToLeaveToGarrison(MobileParty mobileParty, Settlement settlement)
        {
            var result = FindNumberTroopsVanilla(mobileParty, settlement);
            if (result > 0)
            {
                var kingdom = settlement.OwnerClan.Kingdom;
                if (kingdom != null)
                {
                    float enemies = FactionManager.GetEnemyKingdoms(kingdom).Count();
                    var strength = 0f;
                    if (settlement.Town is {GarrisonParty: { }})
                    {
                        strength = settlement.Town.GarrisonParty.MemberRoster.TotalManCount;
                    }

                    if (strength > 500)
                    {
                        return 0;
                    }

                    if (enemies == 0)
                    {
                        return 0;
                    }

                    float partyProportion = 0.025f + (enemies * 0.02f);
                    if (settlement.Town.IsOwnerUnassigned)
                    {
                        partyProportion *= 1.5f;
                    }

                    return (int) MathF.Max((result / 3f) + (mobileParty.MemberRoster.TotalManCount * partyProportion), 0f);
                }
            }

            return result;
        }

        private int FindNumberTroopsVanilla(MobileParty mobileParty, Settlement settlement)
        {
            int result = 0;
            ExceptionUtils.TryCatch(() =>
            {
                MobileParty garrisonParty = settlement.Town.GarrisonParty;
                float num = 0f;
                if (garrisonParty != null)
                {
                    num = garrisonParty.Party.TotalStrength;
                }

                float num2 = 100f;
                if (garrisonParty != null && garrisonParty.HasLimitedWage())
                {
                    num2 = (float)garrisonParty.PaymentLimit / TaleWorlds.CampaignSystem.Campaign.Current.AverageWage;
                }
                else
                {
                    num2 = FactionHelper.FindIdealGarrisonStrengthPerWalledCenter(mobileParty.MapFaction as Kingdom, settlement.OwnerClan);
                    float num3 = FactionHelper.OwnerClanEconomyEffectOnGarrisonSizeConstant(settlement.OwnerClan);
                    float num4 = FactionHelper.SettlementProsperityEffectOnGarrisonSizeConstant(settlement.Town);
                    float num5 = FactionHelper.SettlementFoodPotentialEffectOnGarrisonSizeConstant(settlement);
                    num2 *= num3;
                    num2 *= num4;
                    num2 *= num5;
                }

                if (num < num2)
                {
                    int numberOfRegularMembers = mobileParty.Party.NumberOfRegularMembers;
                    float num6 = 1f + (float)mobileParty.Party.MemberRoster.TotalWoundedRegulars / (float)mobileParty.Party.NumberOfRegularMembers;
                    int limitedPartySize = mobileParty.LimitedPartySize;
                    float num7 = MathF.Pow(MathF.Min(2f, (float)numberOfRegularMembers / (float)limitedPartySize), 1.2f) * 0.75f;
                    float num8 = (1f - num / num2) * (1f - num / num2);
                    float num9 = 1f;
                    if (mobileParty.Army != null)
                    {
                        num8 = MathF.Min(num8, 0.7f);
                        num9 = 0.3f + mobileParty.Army.TotalStrength / mobileParty.Party.TotalStrength * 0.025f;
                    }

                    float num10 = (settlement.Town.IsOwnerUnassigned ? 0.75f : 0.5f);
                    if (settlement.OwnerClan == mobileParty.LeaderHero.Clan || settlement.OwnerClan == mobileParty.Party.Owner.MapFaction.Leader.Clan)
                    {
                        num10 = 1f;
                    }

                    float num11 = MathF.Min(0.7f, num7 * num8 * num10 * num6 * num9);
                    if ((float)numberOfRegularMembers * num11 > 1f)
                    {
                        result = MBRandom.RoundRandomized((float)numberOfRegularMembers * num11);
                    }
                }
            },
            GetType().Name);

            return result;
        }
    }
}