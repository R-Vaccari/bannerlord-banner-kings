using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;

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
    }
}
