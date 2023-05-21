using BannerKings.Components;
using BannerKings.Managers.Education.Lifestyles;
using BannerKings.Managers.Skills;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;

namespace BannerKings.Models.Vanilla
{
    public class BKPartyConsumptionModel : DefaultMobilePartyFoodConsumptionModel
    {
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

        public override ExplainedNumber CalculateDailyFoodConsumptionf(MobileParty party, ExplainedNumber baseConsumption)
        {
            var result = base.CalculateDailyFoodConsumptionf(party, baseConsumption);
            var leader = party.LeaderHero;

            if (leader != null)
            {
                var data = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(leader);
                var faceTerrainType = Campaign.Current.MapSceneWrapper.GetFaceTerrainType(party.CurrentNavigationFace);

                if (data.HasPerk(BKPerks.Instance.KheshigRaider) && faceTerrainType == TerrainType.Plain ||
                       faceTerrainType == TerrainType.Steppe)
                {
                    var cow = Game.Current.ObjectManager.GetObject<ItemObject>("cow");
                    int cattleHeads = party.ItemRoster.GetItemNumber(cow);

                    result.Add(cattleHeads * 0.06f, BKPerks.Instance.KheshigRaider.Name);
                }

                if (party.Army != null && party.SiegeEvent != null)
                {
                    var armyLeader = party.Army.LeaderParty.LeaderHero;
                    var armyEducation = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(armyLeader);
                    if (armyEducation.HasPerk(BKPerks.Instance.SiegeOverseer))
                    {
                        result.AddFactor(-0.15f, BKPerks.Instance.SiegeOverseer.Name);
                    }

                    if (faceTerrainType == TerrainType.Desert && armyEducation.Lifestyle != null &&
                        armyEducation.Lifestyle.Equals(DefaultLifestyles.Instance.Jawwal))
                    {
                        result.AddFactor(-0.3f, DefaultLifestyles.Instance.Jawwal.Name);
                    }
                }
            }


            return result;
        }
    }
}