using BannerKings.Components;
using BannerKings.Managers.Education.Lifestyles;
using BannerKings.Managers.Skills;
using BannerKings.Settings;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.Core;

namespace BannerKings.Models.Vanilla
{
    public class BKPartyConsumptionModel : DefaultMobilePartyFoodConsumptionModel
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

        public override bool DoesPartyConsumeFood(MobileParty mobileParty)
        {
            if (mobileParty.PartyComponent != null)
            {
                var type = mobileParty.PartyComponent.GetType();
                if (type != null && type.IsSubclassOf(typeof(BannerKingsComponent)))
                {
                    return false;
                }

                /*if (type != null && mobileParty.PartyComponent is BanditHeroComponent)
                {
                    return false;
                }*/
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
            PerkHelper.AddPerkBonusForParty(DefaultPerks.Steward.WarriorsDiet, party, true, ref result);
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
            if (party.Army != null)
            {
                PerkHelper.AddPerkBonusForParty(DefaultPerks.Steward.StiffUpperLip, party, true, ref result);
            }
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

            return baseConsumption;
        }
    }
}