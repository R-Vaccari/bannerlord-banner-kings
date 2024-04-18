using BannerKings.Components;
using BannerKings.Managers.Education.Lifestyles;
using BannerKings.Managers.Skills;
using BannerKings.Settings;
using BannerKings.Utils;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.Core;
using static BannerKings.Utils.PerksHelpers;

namespace BannerKings.Models.Vanilla
{
    public class BKPartyConsumptionModel : PartyFoodModel
    {
        public override int NumberOfMenOnMapToEatOneFood
        {
            get
            {
                int result = 20;
                result += (int)(BannerKingsSettings.Instance.SlowerParties * 20f);
                return result;
            }
        }

        public override float BirdFood => 0.025f;

        public override float MuleFood => 0.15f;

        public override float CattleFood => 0.175f;

        public override float PigFood => 0.1f;

        public override float HorseFood => 0.25f;

        public override float WarhorseFood => 0.5f;

        public override float SheepFood => 0.05f;

        public override float CalculateAnimalFoodNeed(MobileParty party, bool ignoreCamels)
        {
            float horses = 0f;
            float birds = 0f;
            float sheep = 0f;
            float pig = 0f;
            float warHorses = 0f;
            float cattle = 0f;
            float mules = 0f;
            foreach (var troop in party.MemberRoster.GetTroopRoster())
            {
                if (troop.Character.IsMounted) horses++;
            }

            foreach (var troop in party.PrisonRoster.GetTroopRoster())
            {
                if (troop.Character.IsMounted) horses++;
            }

            foreach (var element in party.ItemRoster)
            {
                ItemObject item = element.EquipmentElement.Item;
                if (item.HasHorseComponent)
                {
                    if (ignoreCamels && item.StringId.Contains("camel")) continue;

                    if (item.HorseComponent.IsPackAnimal) mules += element.Amount;
                    else if (item.ItemCategory == DefaultItemCategories.WarHorse) warHorses += element.Amount;
                    else if (item.ItemCategory == DefaultItemCategories.Sheep) sheep += element.Amount;
                    else if (item.ItemCategory == DefaultItemCategories.Hog) pig += element.Amount;
                    else if (item.ItemCategory == DefaultItemCategories.Cow) cattle += element.Amount;
                    else birds += element.Amount;
                }
            }

            return (horses * HorseFood) + (birds * BirdFood) + (pig * PigFood) + (mules * MuleFood) +
                (cattle * CattleFood) + (sheep * SheepFood) + (warHorses * WarhorseFood);
        }

        public override bool DoesPartyConsumeFood(MobileParty mobileParty)
        {
            if (mobileParty.PartyComponent != null)
            {
                var type = mobileParty.PartyComponent.GetType();
                if (type != null && type.IsSubclassOf(typeof(BannerKingsComponent)))
                {
                    return false;
                }
            }

            return base.DoesPartyConsumeFood(mobileParty);
        }

        private void VanillaCalculatePerkEffects(MobileParty party, ref ExplainedNumber result)
        {
            int num = 0;
            for (int i = 0; i < party.MemberRoster.Count; i++)
            {
                if (party.MemberRoster.GetCharacterAtIndex(i).Culture.IsBandit)
                {
                    num += party.MemberRoster.GetElementNumber(i);
                }
            }
            for (int j = 0; j < party.PrisonRoster.Count; j++)
            {
                if (party.PrisonRoster.GetCharacterAtIndex(j).Culture.IsBandit)
                {
                    num += party.PrisonRoster.GetElementNumber(j);
                }
            }
            if (party.LeaderHero != null && party.LeaderHero.GetPerkValue(DefaultPerks.Roguery.Promises) && num > 0)
            {
                float value = (float)num / (float)this.NumberOfMenOnMapToEatOneFood * DefaultPerks.Roguery.Promises.PrimaryBonus;
                result.Add(value, DefaultPerks.Roguery.Promises.Name, null);
            }
            PerkHelper.AddPerkBonusForParty(DefaultPerks.Athletics.Spartan, party, false, ref result);

            #region DefaultPerks.Steward.WarriorsDiet
            if (BannerKingsSettings.Instance.EnableUsefulPerks && BannerKingsSettings.Instance.EnableUsefulStewardPerks)
            {
                PerksHelpers.AddScaledPerkBonus(DefaultPerks.Steward.WarriorsDiet, ref result, false, party, DefaultSkills.Steward, (float)0, (float)15, (float)100, SkillScale.OnlyQuartermaster, minValue: (float?)-0.3f, maxValue: 0);
            }
            else
            {
                PerkHelper.AddPerkBonusForParty(DefaultPerks.Steward.WarriorsDiet, party, true, ref result);
            }
            #endregion

            if (party.EffectiveQuartermaster != null)
            {
                PerkHelper.AddEpicPerkBonusForCharacter(DefaultPerks.Steward.PriceOfLoyalty, party.EffectiveQuartermaster.CharacterObject, DefaultSkills.Steward, true, ref result, 250);
            }
            TerrainType faceTerrainType = TaleWorlds.CampaignSystem.Campaign.Current.MapSceneWrapper.GetFaceTerrainType(party.CurrentNavigationFace);
            if (faceTerrainType == TerrainType.Forest || faceTerrainType == TerrainType.Steppe)
            {
                PerkHelper.AddPerkBonusForParty(DefaultPerks.Scouting.Foragers, party, true, ref result);
            }
            if (party.IsGarrison && party.CurrentSettlement != null && party.CurrentSettlement.Town.IsUnderSiege)
            {
                PerkHelper.AddPerkBonusForTown(DefaultPerks.Athletics.StrongLegs, party.CurrentSettlement.Town, ref result);
            }

            #region DefaultPerks.Steward.StiffUpperLip
            if (party.Army != null)
            {
                if (BannerKingsSettings.Instance.EnableUsefulPerks && BannerKingsSettings.Instance.EnableUsefulStewardPerks)
                {
                    PerksHelpers.AddScaledPerkBonus(DefaultPerks.Steward.StiffUpperLip, ref result, false, party, DefaultSkills.Steward, (float)0, (float)15, (float)100, SkillScale.OnlyQuartermaster, minValue: (float?)-0.3f, maxValue: 0);
                }
                else
                {
                    PerkHelper.AddPerkBonusForParty(DefaultPerks.Steward.StiffUpperLip, party, true, ref result);
                }
            }
            #endregion
            #region DefaultPerks.Steward.WarriorsDiet
            if (BannerKingsSettings.Instance.EnableUsefulPerks && BannerKingsSettings.Instance.EnableUsefulStewardPerks)
            {
                PerksHelpers.AddScaledPerkBonus(DefaultPerks.Steward.WarriorsDiet, ref result, false, party, DefaultSkills.Steward, (float)0, (float)15, (float)100, SkillScale.OnlyQuartermaster, minValue: (float?)-0.3f, maxValue: 0);
            }
            else
            {
                PerkHelper.AddPerkBonusForParty(DefaultPerks.Steward.WarriorsDiet, party, true, ref result);
            }
            #endregion

            SiegeEvent siegeEvent = party.SiegeEvent;
            if (((siegeEvent != null) ? siegeEvent.BesiegerCamp : null) != null)
            {
                if (party.SiegeEvent.BesiegerCamp.HasInvolvedPartyForEventType(party.Party, MapEvent.BattleTypes.Siege) && party.HasPerk(DefaultPerks.Steward.SoundReserves, true))
                {
                    PerkHelper.AddPerkBonusForParty(DefaultPerks.Steward.SoundReserves, party, false, ref result);
                }
                if (party.HasPerk(DefaultPerks.Steward.MasterOfPlanning, false))
                {
                    result.AddFactor(DefaultPerks.Steward.MasterOfPlanning.PrimaryBonus, DefaultPerks.Steward.MasterOfPlanning.Name);
                }
            }
        }

        public override ExplainedNumber CalculateDailyFoodConsumptionf(MobileParty party, ExplainedNumber baseConsumption)
        {
            VanillaCalculatePerkEffects(party, ref baseConsumption);
            baseConsumption.LimitMax(0f);
            var leader = party.LeaderHero;

            if (leader != null)
            {
                var data = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(leader);
                var faceTerrainType = TaleWorlds.CampaignSystem.Campaign.Current.MapSceneWrapper.GetFaceTerrainType(party.CurrentNavigationFace);

                if (data.HasPerk(BKPerks.Instance.KheshigRaider) && faceTerrainType == TerrainType.Plain ||
                       faceTerrainType == TerrainType.Steppe)
                {
                    var cow = Game.Current.ObjectManager.GetObject<ItemObject>("cow");
                    int cattleHeads = party.ItemRoster.GetItemNumber(cow);

                    baseConsumption.Add(cattleHeads * 0.06f, BKPerks.Instance.KheshigRaider.Name);
                }

                if (party.Army != null && party.SiegeEvent != null)
                {
                    var armyLeader = party.Army.LeaderParty.LeaderHero;
                    var armyEducation = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(armyLeader);
                    if (armyEducation.HasPerk(BKPerks.Instance.SiegeOverseer))
                    {
                        baseConsumption.AddFactor(-0.15f, BKPerks.Instance.SiegeOverseer.Name);
                    }

                    if (faceTerrainType == TerrainType.Desert && armyEducation.Lifestyle != null &&
                        armyEducation.Lifestyle.Equals(DefaultLifestyles.Instance.Jawwal))
                    {
                        baseConsumption.AddFactor(-0.3f, DefaultLifestyles.Instance.Jawwal.Name);
                    }
                }
            }

            if (TaleWorlds.CampaignSystem.Campaign.Current.MapSceneWrapper.GetFaceTerrainType(party.CurrentNavigationFace) == TerrainType.Desert)
            {
                float mounts = CalculateAnimalFoodNeed(party, true);
                baseConsumption.Add(-mounts, new TaleWorlds.Localization.TextObject("{=1WT6A7nG}Carrying animals while on desert (inventory, party and prisoners)"));
            }
            else
            {
                MapWeatherModel.WeatherEvent weatherEventInPosition = TaleWorlds.CampaignSystem.Campaign.Current.Models.MapWeatherModel
                              .GetWeatherEventInPosition(party.Position2D);
                if (weatherEventInPosition == MapWeatherModel.WeatherEvent.Snowy || weatherEventInPosition == MapWeatherModel.WeatherEvent.Blizzard)
                {
                    float mounts = CalculateAnimalFoodNeed(party, false);
                    baseConsumption.Add(-mounts / 2f, new TaleWorlds.Localization.TextObject("{=BqaxcvqV}Carrying animals while in snow or blizzard (inventory, party and prisoners)"));
                }
            }

            return baseConsumption;
        }
    }
}