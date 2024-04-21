using BannerKings.Managers.Education.Lifestyles;
using BannerKings.Managers.Institutions.Religions;
using BannerKings.Managers.Skills;
using BannerKings.Managers.Titles.Laws;
using BannerKings.Utils;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements.Buildings;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using BannerKings.Behaviours.Retainer;
using BannerKings.Settings;
using BannerKings.Behaviours.Mercenary;
using System;
using static BannerKings.Utils.PerksHelpers;

namespace BannerKings.Models.Vanilla
{
    public class BKPartyWageModel : DefaultPartyWageModel
    {
        int GetHeroWageAfterBerkBonus(float baseWage, Hero heroObject, MobileParty mobileParty)
        {
            ExplainedNumber paidInPromiseExplainedNumber = new ExplainedNumber(baseWage);
            Clan clan = heroObject.Clan;
            #region Steward.PaidInPromise
            if (BannerKingsSettings.Instance.EnableUsefulPerks && BannerKingsSettings.Instance.EnableUsefulStewardPerks)
            {
                if (clan?.Leader != null)
                {
                    var min = -0.4f;

                    var totalbouns = DefaultPerks.Steward.PaidInPromise.AddScaledPersonlOrClanLeaderPerkBonusWithClanAndFamilyMembers(ref paidInPromiseExplainedNumber, false, clan.Leader);
                    if (heroObject.GetPerkValue(DefaultPerks.Steward.PaidInPromise))
                    {
                        var bounsFactor = DefaultPerks.Steward.PaidInPromise.PrimaryBonus * (heroObject.GetSkillValue(DefaultSkills.Steward) / 30);

                        if (totalbouns + bounsFactor < min)
                        {
                            paidInPromiseExplainedNumber.AddFactor(-0.4f - totalbouns, DefaultPerks.Steward.PaidInPromise.Name);
                        }
                        else
                        {
                            paidInPromiseExplainedNumber.AddFactor(bounsFactor, DefaultPerks.Steward.PaidInPromise.Name);
                        }
                    }
                    return MathF.Round(paidInPromiseExplainedNumber.ResultNumber);
                }
            }
            else
            {
                if (mobileParty.LeaderHero != null && mobileParty.LeaderHero.GetPerkValue(DefaultPerks.Steward.PaidInPromise))
                {
                    return MathF.Round(baseWage * (1f + DefaultPerks.Steward.PaidInPromise.PrimaryBonus));
                }
            }
            return MathF.Round(baseWage);
            #endregion
        }
        private void CalculatePartialGarrisonWageReduction(float troopRatio, MobileParty mobileParty, PerkObject perk, ref ExplainedNumber garrisonWageReductionMultiplier, bool isSecondaryEffect)
        {
            if (troopRatio > 0f && mobileParty.CurrentSettlement.Town.Governor != null && PerkHelper.GetPerkValueForTown(perk, mobileParty.CurrentSettlement.Town))
            {
                garrisonWageReductionMultiplier.AddFactor(isSecondaryEffect ? (perk.SecondaryBonus * troopRatio) : (perk.PrimaryBonus * troopRatio), perk.Name);
            }
        }

        private ExplainedNumber GetVanillaWage(MobileParty mobileParty, bool includeDescriptions = false)
        {
            int num = 0;
            int num2 = 0;
            int num3 = 0;
            int num4 = 0;
            int num5 = 0;
            int num6 = 0;
            int num7 = 0;
            int num8 = 0;
            int num9 = 0;
            int num10 = 0;
            int num11 = 0;
            bool flag = !mobileParty.HasPerk(DefaultPerks.Steward.AidCorps, false);
            int num12 = 0;
            int mercenaryCount = 0;
            for (int i = 0; i < mobileParty.MemberRoster.Count; i++)
            {
                TroopRosterElement elementCopyAtIndex = mobileParty.MemberRoster.GetElementCopyAtIndex(i);
                CharacterObject character = elementCopyAtIndex.Character;
                if (character.IsPlayerCharacter)
                {
                    continue;
                }
                int num14 = flag ? elementCopyAtIndex.Number : (elementCopyAtIndex.Number - elementCopyAtIndex.WoundedNumber);
                int wage = (int)Math.Max(character.TroopWage * BannerKingsSettings.Instance.BaseWage, 1);

                if (character.Occupation == Occupation.Mercenary)
                {
                    MercenaryCareer career = TaleWorlds.CampaignSystem.Campaign.Current.GetCampaignBehavior<BKMercenaryCareerBehavior>()
                                   .GetCareer(Clan.PlayerClan);
                    if (career != null && career.IsTroopCustom(character)) wage = (int)(wage * 3f);
                }

                if (character.IsHero)
                {
                    //remove this to fix bug that hero wage are doubled
                    //Hero heroObject = elementCopyAtIndex.Character.HeroObject;
                    //Clan clan = character.HeroObject.Clan;
                    //if (heroObject != (clan?.Leader))
                    //{
                    //    num3 += GetHeroWageAfterBerkBonus(wage, heroObject, mobileParty);
                    //}
                    //else
                    //{
                    //    num3 += wage;
                    //}
                }
                else if (character.Culture != null)
                {
                    if (character.Tier < 4)
                    {
                        if (character.Culture.IsBandit)
                        {
                            num9 += wage * elementCopyAtIndex.Number;
                        }
                        num += wage * num14;
                    }
                    else if (character.Tier == 4)
                    {
                        if (character.Culture.IsBandit)
                        {
                            num10 += wage * elementCopyAtIndex.Number;
                        }
                        num2 += wage * num14;
                    }
                    else if (character.Tier > 4)
                    {
                        if (character.Culture.IsBandit)
                        {
                            num11 += wage * elementCopyAtIndex.Number;
                        }
                        num3 += wage * num14;
                    }
                    if (character.IsInfantry)
                    {
                        num4 += num14;
                    }
                    if (character.IsMounted)
                    {
                        num5 += num14;
                    }
                    if (character.Occupation == Occupation.CaravanGuard)
                    {
                        num12 += elementCopyAtIndex.Number;
                    }
                    if (character.Occupation == Occupation.Mercenary)
                    {
                        mercenaryCount += elementCopyAtIndex.Number;
                    }
                    if (character.IsRanged)
                    {
                        num6 += num14;
                        if (character.Tier >= 4)
                        {
                            num7 += num14;
                            num8 += wage * elementCopyAtIndex.Number;
                        }
                    }
                }
            }
            ExplainedNumber explainedNumber = new ExplainedNumber(0f, false, null);
            if (mobileParty.LeaderHero != null && mobileParty.LeaderHero.GetPerkValue(DefaultPerks.Roguery.DeepPockets))
            {
                num -= num9;
                num2 -= num10;
                num3 -= num11;
                int num15 = num9 + num10 + num11;
                explainedNumber.Add((float)num15, null, null);
                PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Roguery.DeepPockets, mobileParty.LeaderHero.CharacterObject, false, ref explainedNumber);
            }
            int num16 = num + num2 + num3;
            if (mobileParty.HasPerk(DefaultPerks.Crossbow.PickedShots, false) && num7 > 0)
            {
                float num17 = (float)num8 * DefaultPerks.Crossbow.PickedShots.PrimaryBonus;
                num16 += (int)num17;
            }
            ExplainedNumber result = new ExplainedNumber((float)num16, includeDescriptions, null);
            ExplainedNumber explainedNumber2 = new ExplainedNumber(1f, false, null);
            if (mobileParty.IsGarrison)
            {
                Settlement currentSettlement = mobileParty.CurrentSettlement;
                if (((currentSettlement != null) ? currentSettlement.Town : null) != null)
                {
                    if (mobileParty.CurrentSettlement.IsTown)
                    {
                        PerkHelper.AddPerkBonusForTown(DefaultPerks.OneHanded.MilitaryTradition, mobileParty.CurrentSettlement.Town, ref result);
                        PerkHelper.AddPerkBonusForTown(DefaultPerks.TwoHanded.Berserker, mobileParty.CurrentSettlement.Town, ref result);
                        PerkHelper.AddPerkBonusForTown(DefaultPerks.Bow.HunterClan, mobileParty.CurrentSettlement.Town, ref result);
                        float troopRatio = (float)num4 / (float)mobileParty.MemberRoster.TotalRegulars;
                        this.CalculatePartialGarrisonWageReduction(troopRatio, mobileParty, DefaultPerks.Polearm.StandardBearer, ref result, true);
                        float troopRatio2 = (float)num5 / (float)mobileParty.MemberRoster.TotalRegulars;
                        this.CalculatePartialGarrisonWageReduction(troopRatio2, mobileParty, DefaultPerks.Riding.CavalryTactics, ref result, true);
                        float troopRatio3 = (float)num6 / (float)mobileParty.MemberRoster.TotalRegulars;
                        this.CalculatePartialGarrisonWageReduction(troopRatio3, mobileParty, DefaultPerks.Crossbow.PeasantLeader, ref result, true);
                    }
                    #region Steward.StiffUpperLip
                    else if (mobileParty.CurrentSettlement.IsCastle)
                    {
                        if (BannerKingsSettings.Instance.EnableUsefulPerks && BannerKingsSettings.Instance.EnableUsefulStewardPerks)
                        {
                            DefaultPerks.Steward.StiffUpperLip.AddScaledGovernerPerkBonusForTownWithTownHeros(ref result, true, mobileParty.CurrentSettlement.Town);
                        }
                        else
                        {
                            PerkHelper.AddPerkBonusForTown(DefaultPerks.Steward.StiffUpperLip, mobileParty.CurrentSettlement.Town, ref result);
                        }
                    }
                    #endregion
                    #region Steward.DrillSergant
                    if (BannerKingsSettings.Instance.EnableUsefulPerks && BannerKingsSettings.Instance.EnableUsefulStewardPerks)
                    {
                        DefaultPerks.Steward.DrillSergant.AddScaledGovernerPerkBonusForTownWithTownHeros(ref result, true, mobileParty.CurrentSettlement.Town);
                    }
                    else
                    {
                        PerkHelper.AddPerkBonusForTown(DefaultPerks.Steward.DrillSergant, mobileParty.CurrentSettlement.Town, ref result);
                    }
                    #endregion

                    if (mobileParty.CurrentSettlement.Culture.HasFeat(DefaultCulturalFeats.EmpireGarrisonWageFeat))
                    {
                        result.AddFactor(DefaultCulturalFeats.EmpireGarrisonWageFeat.EffectBonus, GameTexts.FindText("str_culture", null));
                    }
                    foreach (Building building in mobileParty.CurrentSettlement.Town.Buildings)
                    {
                        float buildingEffectAmount = building.GetBuildingEffectAmount(BuildingEffectEnum.GarrisonWageReduce);
                        if (buildingEffectAmount > 0f)
                        {
                            explainedNumber2.AddFactor(-(buildingEffectAmount / 100f), building.Name);
                        }
                    }
                }
            }
            result.Add(explainedNumber.ResultNumber, null, null);
            float value = (mobileParty.LeaderHero != null && mobileParty.LeaderHero.Clan != null && mobileParty.LeaderHero.Clan.Kingdom != null &&
                !mobileParty.LeaderHero.Clan.IsUnderMercenaryService &&
                mobileParty.LeaderHero.Clan.Kingdom.ActivePolicies.Contains(DefaultPolicies.MilitaryCoronae)) ? 0.1f : 0f;
            if (mobileParty.HasPerk(DefaultPerks.Trade.SwordForBarter, true))
            {
                float num18 = (float)num12 / (float)mobileParty.MemberRoster.TotalRegulars;
                if (num18 > 0f)
                {
                    float value2 = DefaultPerks.Trade.SwordForBarter.SecondaryBonus * num18;
                    result.AddFactor(value2, DefaultPerks.Trade.SwordForBarter.Name);
                }
            }
            #region DefaultPerks.Steward.Contractors
            if (BannerKingsSettings.Instance.EnableUsefulPerks && BannerKingsSettings.Instance.EnableUsefulStewardPerks)
            {
                float mercenaryRatio = (float)mercenaryCount / mobileParty.MemberRoster.TotalRegulars;
                if (mercenaryRatio > 0f)
                {
                    DefaultPerks.Steward.Contractors.AddScaledPartyPerkBonus(ref result, false, mobileParty, mercenaryRatio);
                }
            }
            else
            {
                if (mobileParty.HasPerk(DefaultPerks.Steward.Contractors, false))
                {
                    float num19 = (float)mercenaryCount / (float)mobileParty.MemberRoster.TotalRegulars;
                    if (num19 > 0f)
                    {
                        float value3 = DefaultPerks.Steward.Contractors.PrimaryBonus * num19;
                        result.AddFactor(value3, DefaultPerks.Steward.Contractors.Name);
                    }
                }
            }
            #endregion
            if (mobileParty.HasPerk(DefaultPerks.Trade.MercenaryConnections, true))
            {
                float num20 = (float)mercenaryCount / (float)mobileParty.MemberRoster.TotalRegulars;
                if (num20 > 0f)
                {
                    float value4 = DefaultPerks.Trade.MercenaryConnections.SecondaryBonus * num20;
                    result.AddFactor(value4, DefaultPerks.Trade.MercenaryConnections.Name);
                }
            }
            result.AddFactor(value, DefaultPolicies.MilitaryCoronae.Name);
            result.AddFactor(explainedNumber2.ResultNumber - 1f, null);
            if (PartyBaseHelper.HasFeat(mobileParty.Party, DefaultCulturalFeats.AseraiIncreasedWageFeat))
            {
                result.AddFactor(DefaultCulturalFeats.AseraiIncreasedWageFeat.EffectBonus, null);
            }
            #region Steward.Frugal
            if (BannerKingsSettings.Instance.EnableUsefulPerks && BannerKingsSettings.Instance.EnableUsefulStewardPerks)
            {
                DefaultPerks.Steward.Frugal.AddScaledPartyPerkBonus(ref result, false, mobileParty);
            }
            else
            {
                if (mobileParty.HasPerk(DefaultPerks.Steward.Frugal, false))
                {
                    result.AddFactor(DefaultPerks.Steward.Frugal.PrimaryBonus, DefaultPerks.Steward.Frugal.Name);
                }
            }
            #endregion
            #region Steward.EfficientCampaigner
            if (mobileParty.Army != null)
            {
                if (BannerKingsSettings.Instance.EnableUsefulPerks && BannerKingsSettings.Instance.EnableUsefulStewardPerks)
                {
                    DefaultPerks.Steward.EfficientCampaigner.AddScaledPartyPerkBonus(ref result, true, mobileParty);
                }
                else
                {
                    if (mobileParty.HasPerk(DefaultPerks.Steward.EfficientCampaigner, true))
                    {
                        result.AddFactor(DefaultPerks.Steward.EfficientCampaigner.SecondaryBonus, DefaultPerks.Steward.EfficientCampaigner.Name);
                    }
                }
            }
            #endregion

            #region DefaultPerks.Steward.MasterOfWarcraft
            if (BannerKingsSettings.Instance.EnableUsefulPerks && BannerKingsSettings.Instance.EnableUsefulStewardPerks)
            {
                if (mobileParty.SiegeEvent != null && mobileParty.SiegeEvent.BesiegerCamp.HasInvolvedPartyForEventType(mobileParty.Party, MapEvent.BattleTypes.Siege))
                {
                    DefaultPerks.Steward.MasterOfWarcraft.AddScaledPartyPerkBonus(ref result, false, mobileParty);
                }
            }
            else
            {
                if (mobileParty.SiegeEvent != null && mobileParty.SiegeEvent.BesiegerCamp.HasInvolvedPartyForEventType(mobileParty.Party, MapEvent.BattleTypes.Siege) && mobileParty.HasPerk(DefaultPerks.Steward.MasterOfWarcraft, false))
                {
                    result.AddFactor(DefaultPerks.Steward.MasterOfWarcraft.PrimaryBonus, DefaultPerks.Steward.MasterOfWarcraft.Name);
                }
            }
            #endregion

            #region DefaultPerks.Steward.PriceOfLoyalty
            if (BannerKingsSettings.Instance.EnableUsefulPerks && BannerKingsSettings.Instance.EnableUsefulStewardPerks)
            {
                DefaultPerks.Steward.PriceOfLoyalty.AddScaledPartyPerkBonus(ref result, false, mobileParty);
            }
            else
            {
                if (mobileParty.EffectiveQuartermaster != null)
                {
                    PerkHelper.AddEpicPerkBonusForCharacter(DefaultPerks.Steward.PriceOfLoyalty, mobileParty.EffectiveQuartermaster.CharacterObject, DefaultSkills.Steward, true, ref result, 250);
                }
            }
            #endregion

            if (mobileParty.CurrentSettlement != null && mobileParty.LeaderHero != null && mobileParty.LeaderHero.GetPerkValue(DefaultPerks.Trade.ContentTrades))
            {
                result.AddFactor(DefaultPerks.Trade.ContentTrades.SecondaryBonus, DefaultPerks.Trade.ContentTrades.Name);
            }
            return result;
        }

        public override ExplainedNumber GetTotalWage(MobileParty mobileParty, bool includeDescriptions = false)
        {
            ExplainedNumber result = GetVanillaWage(mobileParty, includeDescriptions);

            if (mobileParty.IsLordParty && mobileParty.ActualClan.IsClanTypeMercenary)
            {
                Clan clan = mobileParty.ActualClan;
                if (clan != Clan.PlayerClan && clan.Kingdom == null)
                {
                    return new ExplainedNumber(0f);
                }
            }

            Hero leader = mobileParty.LeaderHero ?? mobileParty.Owner;
            if (leader != null)
            {
                var totalCulture = 0f;
                var mountedTroops = 0f;
                for (var i = 0; i < mobileParty.MemberRoster.Count; i++)
                {
                    var elementCopyAtIndex = mobileParty.MemberRoster.GetElementCopyAtIndex(i);
                    if (elementCopyAtIndex.Character.Culture == leader.Culture)
                    {
                        totalCulture += elementCopyAtIndex.Number;
                    }

                    if (elementCopyAtIndex.Character.HasMount())
                    {
                        mountedTroops += elementCopyAtIndex.Number;
                    }

                    if (elementCopyAtIndex.Character.IsHero)
                    {
                        Hero hero = elementCopyAtIndex.Character.HeroObject;

                        if (hero == mobileParty.LeaderHero)
                        {
                            continue;
                        }
                        //family members should be free
                        if (mobileParty.ActualClan?.Leader != null && hero.IsCloseFamily(mobileParty.ActualClan?.Leader))
                        {
                            continue;
                        }
                        if (hero == Hero.MainHero)
                        {
                            Contract contract = TaleWorlds.CampaignSystem.Campaign.Current.GetCampaignBehavior<BKRetainerBehavior>().GetContract();
                            if (contract != null && contract.Contractor == leader)
                            {
                                result.Add(contract.Wage);
                            }
                        }
                        else
                        {
                            var heroWage = BannerKingsConfig.Instance.CompanionModel.GetHeroWage(hero);
                            heroWage = GetHeroWageAfterBerkBonus(heroWage, hero, mobileParty);
                            result.Add(heroWage,
                                elementCopyAtIndex.Character.Name);
                        }
                    }
                }

                var proportion = MBMath.ClampFloat(totalCulture / mobileParty.MemberRoster.TotalManCount, 0f, 1f);
                if (proportion > 0f)
                {
                    result.AddFactor(proportion * -0.1f, GameTexts.FindText("str_culture"));
                }

                if (mobileParty.IsGarrison)
                {
                    result.Add(result.ResultNumber * -0.5f);
                }

                var education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(leader);
                float mountedProportion = mountedTroops / mobileParty.MemberRoster.Count;
                if (education.HasPerk(BKPerks.Instance.CataphractEquites) && mountedTroops > 0f)
                {
                    result.AddFactor(mountedProportion * -0.1f, BKPerks.Instance.CataphractEquites.Name);
                }

                if (mobileParty.SiegeEvent != null && education.Lifestyle != null &&
                    education.Lifestyle.Equals(DefaultLifestyles.Instance.SiegeEngineer))
                {
                    result.AddFactor(-0.3f, DefaultLifestyles.Instance.SiegeEngineer.Name);
                }
            }

            if (mobileParty.IsCaravan && mobileParty.Owner != null)
            {
                var education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(mobileParty.Owner);
                if (education.HasPerk(BKPerks.Instance.CaravaneerDealer))
                {
                    result.AddFactor(-0.1f, BKPerks.Instance.CaravaneerDealer.Name);
                }

                result.AddFactor(-0.25f, GameTexts.FindText("str_party_type", "Caravan"));
            }

            return result;
        }

        public override int GetTroopRecruitmentCost(CharacterObject troop, Hero buyerHero, bool withoutItemCost = false)
        {
            var result = new ExplainedNumber(base.GetTroopRecruitmentCost(troop, buyerHero, withoutItemCost));

            #region Steward.Frugal
            if (BannerKingsSettings.Instance.EnableUsefulPerks && BannerKingsSettings.Instance.EnableUsefulStewardPerks && buyerHero != null && buyerHero.IsPartyLeader)
            {
                if (buyerHero.GetPerkValue(DefaultPerks.Steward.Frugal))
                {
                    result.AddFactor(-DefaultPerks.Steward.Frugal.SecondaryBonus, null);
                }
                DefaultPerks.Steward.Frugal.AddScaledPartyPerkBonus(ref result, true, buyerHero.PartyBelongedTo);
            }

            #endregion
            result.LimitMin(GetCharacterWage(troop) * 10f);

            ExceptionUtils.TryCatch(() =>
            {
                if (buyerHero != null)
                {
                    if (buyerHero.CurrentSettlement != null)
                    {
                        var title = BannerKingsConfig.Instance.TitleManager.GetTitle(buyerHero.CurrentSettlement);
                        if (title != null && buyerHero.MapFaction != buyerHero.CurrentSettlement.MapFaction)
                        {
                            var contract = title.Contract;
                            if (contract.IsLawEnacted(DefaultDemesneLaws.Instance.DraftingFreeContracts))
                            {
                                result.AddFactor(1f, DefaultDemesneLaws.Instance.DraftingFreeContracts.Name);
                            }
                            else if (contract.IsLawEnacted(DefaultDemesneLaws.Instance.DraftingHidage))
                            {
                                result.AddFactor(0.5f, DefaultDemesneLaws.Instance.DraftingHidage.Name);
                            }
                        }
                    }

                    var education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(buyerHero);
                    if (troop.Occupation == Occupation.Mercenary && education.HasPerk(BKPerks.Instance.MercenaryLocalConnections))
                    {
                        result.AddFactor(-0.1f, BKPerks.Instance.MercenaryLocalConnections.Name);
                    }

                    if (troop.IsMounted && education.HasPerk(BKPerks.Instance.RitterOathbound))
                    {
                        result.AddFactor(-0.15f, BKPerks.Instance.RitterOathbound.Name);
                    }

                    if (Utils.Helpers.IsRetinueTroop(troop))
                    {
                        result.AddFactor(0.20f);
                    }

                    if (troop.Culture == buyerHero.Culture)
                    {
                        result.AddFactor(-0.05f, GameTexts.FindText("str_culture"));
                    }

                    if (education.Lifestyle != null && education.Lifestyle.Equals(DefaultLifestyles.Instance.Artisan))
                    {
                        result.AddFactor(0.15f, DefaultLifestyles.Instance.Artisan.Name);
                    }

                    if (buyerHero.Clan != null)
                    {
                        if (troop.Culture.StringId == "aserai" && BannerKingsConfig.Instance.ReligionsManager
                            .HasBlessing(buyerHero, DefaultDivinities.Instance.AseraSecondary2))
                        {
                            result.AddFactor(-0.1f);
                        }

                        var buyerKingdom = buyerHero.Clan.Kingdom;
                        if (buyerKingdom != null && troop.Culture != buyerHero.Culture)
                        {
                            result.AddFactor(0.25f, GameTexts.FindText("str_kingdom"));
                        }

                        switch (buyerHero.Clan.Tier)
                        {
                            case >= 4:
                                result.AddFactor((buyerHero.Clan.Tier - 3) * 0.05f);
                                break;
                            case <= 1:
                                result.AddFactor((buyerHero.Clan.Tier - 2) * 0.05f);
                                break;
                        }
                    }
                }
            },
            GetType().Name,
            false);

            return (int)result.ResultNumber;
        }
    }
}