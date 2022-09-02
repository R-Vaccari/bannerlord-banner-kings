using BannerKings.Managers.Institutions.Religions;
using BannerKings.Managers.Populations.Villages;
using BannerKings.Managers.Skills;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Settlements;

namespace BannerKings.Models.Vanilla
{
    public class BKRaidModel : DefaultRaidModel
    {
        public override float CalculateHitDamage(MapEventSide attackerSide, float settlementHitPoints)
        {
            var result = base.CalculateHitDamage(attackerSide, settlementHitPoints);
            var attacker = attackerSide.LeaderParty;
            if (attacker is {LeaderHero: { }})
            {
                var reference = result;
                var education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(attacker.LeaderHero);
                if (education.HasPerk(BKPerks.Instance.OutlawPlunderer))
                {
                    result += (reference * 1.15f - reference);
                }

                if (education.HasPerk(BKPerks.Instance.MercenaryRansacker))
                {
                    result += (reference * 1.15f - reference);
                }

                if (education.HasPerk(BKPerks.Instance.VaryagShieldBrother))
                {
                    result += (reference * 1.15f - reference);
                }

                if (education.HasPerk(BKPerks.Instance.JawwalGhazw))
                {
                    result += (reference * 1.15f - reference);
                }

                if (education.HasPerk(BKPerks.Instance.KheshigRaider))
                {
                    result *= 1.15f;
                }
            }

            var settlement = attackerSide.MapEvent.MapEventSettlement;
            if (settlement != null)
            {
                if (BannerKingsConfig.Instance.PopulationManager.IsSettlementPopulated(settlement))
                {
                    var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement).VillageData;
                    var palisade = data.GetBuildingLevel(DefaultVillageBuildings.Instance.Palisade);
                    if (palisade > 0)
                    {
                        result *= 1f - 0.12f * palisade;
                    }
                }

                if (attacker.LeaderHero != null && settlement.Culture.StringId != "battania")
                {
                    if (BannerKingsConfig.Instance.ReligionsManager.HasBlessing(attacker.LeaderHero,
                            DefaultDivinities.Instance.AmraSecondary2))
                    {
                        result *= 1.15f;
                    }
                }
            }

            return result;
        }
    }
}