using BannerKings.Managers.Education.Lifestyles;
using BannerKings.Managers.Institutions.Religions;
using BannerKings.Managers.Skills;
using BannerKings.Settings;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;

namespace BannerKings.Models.Vanilla
{
    public class BKBattleRewardModel : DefaultBattleRewardModel
    {
        public override EquipmentElement GetLootedItemFromTroop(CharacterObject character, float targetValue)
        {
            EquipmentElement result = base.GetLootedItemFromTroop(character, targetValue);
            float scale = BannerKingsSettings.Instance.LootScale;
            if (!result.Equals(default(EquipmentElement)) && scale > MBRandom.RandomFloat)
            {
                result = default(EquipmentElement);
            }

            return result;
        }

        public override ExplainedNumber CalculateInfluenceGain(PartyBase party, float influenceValueOfBattle, float contributionShare)
        {
            ExplainedNumber result = base.CalculateInfluenceGain(party, influenceValueOfBattle, contributionShare);
            var leader = party.LeaderHero;
            if (leader != null)
            {
                var education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(leader);
                if (education.HasPerk(BKPerks.Instance.CommanderWarband))
                {
                    result.AddFactor(0.25f, BKPerks.Instance.CommanderWarband.Name);
                }

                if (BannerKingsConfig.Instance.ReligionsManager.HasBlessing(leader,
                   DefaultDivinities.Instance.Wilund))
                {
                    result.AddFactor(0.3f, DefaultDivinities.Instance.Wilund.Name);
                }
            }

            return result;
        }

        public override ExplainedNumber CalculateRenownGain(PartyBase party, float renownValueOfBattle, float contributionShare)
        {
            var result = base.CalculateRenownGain(party, renownValueOfBattle, contributionShare);

            var leader = party.LeaderHero;
            if (leader != null)
            {
                var education = BannerKingsConfig.Instance.EducationManager.GetHeroEducation(leader);
                if (education.HasPerk(BKPerks.Instance.MercenaryFamousSellswords))
                {
                    result.AddFactor(0.2f, BKPerks.Instance.MercenaryFamousSellswords.Name);
                }

                if (education.Lifestyle != null && education.Lifestyle.Equals(DefaultLifestyles.Instance.Cataphract))
                {
                    result.AddFactor(0.12f, DefaultLifestyles.Instance.Cataphract.Name);
                }

                Religion rel = BannerKingsConfig.Instance.ReligionsManager.GetHeroReligion(leader); 
                if (BannerKingsConfig.Instance.ReligionsManager.HasBlessing(leader,
                    DefaultDivinities.Instance.VlandiaMain,
                    rel))
                {
                    result.AddFactor(0.15f, DefaultDivinities.Instance.VlandiaMain.Name);
                }

                if (BannerKingsConfig.Instance.ReligionsManager.HasBlessing(leader,
                    DefaultDivinities.Instance.Wilund,
                    rel))
                {
                    result.AddFactor(0.3f, DefaultDivinities.Instance.Wilund.Name);
                }

                if (BannerKingsConfig.Instance.ReligionsManager.HasBlessing(leader,
                    DefaultDivinities.Instance.GodsFate,
                    rel))
                {
                    result.AddFactor(0.6f, DefaultDivinities.Instance.GodsFate.Name);
                }

                if (BannerKingsConfig.Instance.ReligionsManager.HasBlessing(leader,
                   DefaultDivinities.Instance.WindEast,
                   rel))
                {
                    result.AddFactor(0.4f, DefaultDivinities.Instance.WindWest.Name);
                }
            }

            return result;
        }
    }
}
