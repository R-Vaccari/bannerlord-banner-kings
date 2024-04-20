using BannerKings.Settings;
using BannerKings.Utils;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using static BannerKings.Utils.PerksHelpers;

namespace BannerKings.Models.Vanilla
{
    public class BKPrisonerModel : DefaultPrisonerDonationModel
    {
        public override float CalculateRelationGainAfterHeroPrisonerDonate(PartyBase donatingParty, Hero donatedHero, Settlement donatedSettlement)
        {
            float result = 0f;
            if (donatingParty == null || donatedHero == null || donatedSettlement == null)
            {
                return result;
            }

            int num = TaleWorlds.CampaignSystem.Campaign.Current.Models.RansomValueCalculationModel.PrisonerRansomValue(donatedHero.CharacterObject, donatingParty.LeaderHero);
            int relation = donatedSettlement.OwnerClan != null ? donatedHero.GetRelation(donatedSettlement.OwnerClan.Leader) : 0;
            if (relation <= 0)
            {
                var clan = donatedHero.Clan != null ? donatedHero.Clan : donatedHero.CompanionOf;
                float num2 = 1f - relation / 200f;
                if (donatedHero.MapFaction != null && donatedHero.MapFaction.IsKingdomFaction && donatedHero.IsFactionLeader)
                {
                    result = MathF.Min(40f, MathF.Pow(num, 0.5f) * 0.5f) * num2;
                }
                else if (clan != null && clan.Leader == donatedHero)
                {
                    result = MathF.Min(30f, MathF.Pow(num, 0.5f) * 0.25f) * num2;
                }
                else
                {
                    result = MathF.Min(20f, MathF.Pow(num, 0.5f) * 0.1f) * num2;
                }
            }
            return result;
        }
        public override float CalculateInfluenceGainAfterTroopDonation(PartyBase donatingParty, CharacterObject donatedCharacter, Settlement donatedSettlement)
        {
            Hero leaderHero = donatingParty.LeaderHero;
            ExplainedNumber explainedNumber = new ExplainedNumber(donatedCharacter.GetPower() * 1.2f, false, null);

            #region DefaultPerks.Steward.Relocation
            if (BannerKingsSettings.Instance.EnableUsefulPerks && BannerKingsSettings.Instance.EnableUsefulStewardPerks && donatingParty.MobileParty != null)
            {
                DefaultPerks.Steward.Relocation.AddScaledPerkBonus(ref explainedNumber, false, donatingParty.MobileParty);
            }
            else
            {
                if (leaderHero != null && leaderHero.GetPerkValue(DefaultPerks.Steward.Relocation))
                {
                    PerkHelper.AddPerkBonusForParty(DefaultPerks.Steward.Relocation, donatingParty.MobileParty, true, ref explainedNumber);
                }
            }
            #endregion
            return explainedNumber.ResultNumber;
        }

    }
}
