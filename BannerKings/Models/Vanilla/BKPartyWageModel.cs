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

namespace BannerKings.Models.Vanilla
{
    public class BKPartyWageModel : DefaultPartyWageModel
    {
        public override int GetCharacterWage(CharacterObject character)
        {
            return GetCharacterLevelWage(character);
        }

        public int GetCharacterLevelWage(CharacterObject character)
        {
            var result = character.Level switch
            {
                <= 1 => 1,
                <= 6 => 2,
                <= 11 => 6,
                <= 16 => 10,
                <= 21 => 22,
                <= 26 => 40,
                <= 31 => 60,
                _ => 80
            };

            return result;
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
            int num13 = 0;
            for (int i = 0; i < mobileParty.MemberRoster.Count; i++)
            {
                TroopRosterElement elementCopyAtIndex = mobileParty.MemberRoster.GetElementCopyAtIndex(i);
                CharacterObject character = elementCopyAtIndex.Character;
                int num14 = flag ? elementCopyAtIndex.Number : (elementCopyAtIndex.Number - elementCopyAtIndex.WoundedNumber);
                if (character.IsHero)
                {
                    Hero heroObject = elementCopyAtIndex.Character.HeroObject;
                    Clan clan = character.HeroObject.Clan;
                    if (heroObject != ((clan != null) ? clan.Leader : null))
                    {
                        if (mobileParty.LeaderHero != null && mobileParty.LeaderHero.GetPerkValue(DefaultPerks.Steward.PaidInPromise))
                        {
                            num3 += MathF.Round((float)elementCopyAtIndex.Character.TroopWage * (1f + DefaultPerks.Steward.PaidInPromise.PrimaryBonus));
                        }
                        else
                        {
                            num3 += elementCopyAtIndex.Character.TroopWage;
                        }
                    }
                }
                else
                {
                    if (character.Tier < 4)
                    {
                        if (character.Culture.IsBandit)
                        {
                            num9 += elementCopyAtIndex.Character.TroopWage * elementCopyAtIndex.Number;
                        }
                        num += elementCopyAtIndex.Character.TroopWage * num14;
                    }
                    else if (character.Tier == 4)
                    {
                        if (character.Culture.IsBandit)
                        {
                            num10 += elementCopyAtIndex.Character.TroopWage * elementCopyAtIndex.Number;
                        }
                        num2 += elementCopyAtIndex.Character.TroopWage * num14;
                    }
                    else if (character.Tier > 4)
                    {
                        if (character.Culture.IsBandit)
                        {
                            num11 += elementCopyAtIndex.Character.TroopWage * elementCopyAtIndex.Number;
                        }
                        num3 += elementCopyAtIndex.Character.TroopWage * num14;
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
                        num13 += elementCopyAtIndex.Number;
                    }
                    if (character.IsRanged)
                    {
                        num6 += num14;
                        if (character.Tier >= 4)
                        {
                            num7 += num14;
                            num8 += elementCopyAtIndex.Character.TroopWage * elementCopyAtIndex.Number;
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
                    else if (mobileParty.CurrentSettlement.IsCastle)
                    {
                        PerkHelper.AddPerkBonusForTown(DefaultPerks.Steward.StiffUpperLip, mobileParty.CurrentSettlement.Town, ref result);
                    }
                    PerkHelper.AddPerkBonusForTown(DefaultPerks.Steward.DrillSergant, mobileParty.CurrentSettlement.Town, ref result);
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
            if (mobileParty.HasPerk(DefaultPerks.Steward.Contractors, false))
            {
                float num19 = (float)num13 / (float)mobileParty.MemberRoster.TotalRegulars;
                if (num19 > 0f)
                {
                    float value3 = DefaultPerks.Steward.Contractors.PrimaryBonus * num19;
                    result.AddFactor(value3, DefaultPerks.Steward.Contractors.Name);
                }
            }
            if (mobileParty.HasPerk(DefaultPerks.Trade.MercenaryConnections, true))
            {
                float num20 = (float)num13 / (float)mobileParty.MemberRoster.TotalRegulars;
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
            if (mobileParty.HasPerk(DefaultPerks.Steward.Frugal, false))
            {
                result.AddFactor(DefaultPerks.Steward.Frugal.PrimaryBonus, DefaultPerks.Steward.Frugal.Name);
            }
            if (mobileParty.Army != null && mobileParty.HasPerk(DefaultPerks.Steward.EfficientCampaigner, true))
            {
                result.AddFactor(DefaultPerks.Steward.EfficientCampaigner.SecondaryBonus, DefaultPerks.Steward.EfficientCampaigner.Name);
            }
            if (mobileParty.SiegeEvent != null && mobileParty.SiegeEvent.BesiegerCamp.HasInvolvedPartyForEventType(mobileParty.Party, MapEvent.BattleTypes.Siege) && mobileParty.HasPerk(DefaultPerks.Steward.MasterOfWarcraft, false))
            {
                result.AddFactor(DefaultPerks.Steward.MasterOfWarcraft.PrimaryBonus, DefaultPerks.Steward.MasterOfWarcraft.Name);
            }
            if (mobileParty.EffectiveQuartermaster != null)
            {
                PerkHelper.AddEpicPerkBonusForCharacter(DefaultPerks.Steward.PriceOfLoyalty, mobileParty.EffectiveQuartermaster.CharacterObject, DefaultSkills.Steward, true, ref result, 250);
            }
            if (mobileParty.CurrentSettlement != null && mobileParty.LeaderHero != null && mobileParty.LeaderHero.GetPerkValue(DefaultPerks.Trade.ContentTrades))
            {
                result.AddFactor(DefaultPerks.Trade.ContentTrades.SecondaryBonus, DefaultPerks.Trade.ContentTrades.Name);
            }
            return result;
        }

        public override ExplainedNumber GetTotalWage(MobileParty mobileParty, bool includeDescriptions = false)
        {
            var result = GetVanillaWage(mobileParty, includeDescriptions);
            var leader = mobileParty.LeaderHero ?? mobileParty.Owner;
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

                        if (hero == Hero.MainHero)
                        {
                            Contract contract = Campaign.Current.GetCampaignBehavior<BKRetainerBehavior>().GetContract();
                            if (contract != null && contract.Contractor == leader)
                            {
                                result.Add(contract.Wage);
                            }
                        }
                        else
                        {
                            result.Add(BannerKingsConfig.Instance.CompanionModel.GetHeroWage(hero), 
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
            var result = new ExplainedNumber(base.GetTroopRecruitmentCost(troop, buyerHero, withoutItemCost) * 2.5f);
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
            
            return (int) result.ResultNumber;
        }
    }
}