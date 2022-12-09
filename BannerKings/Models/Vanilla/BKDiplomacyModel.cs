using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.Localization;

namespace BannerKings.Models.Vanilla
{
    public class BKDiplomacyModel : DefaultDiplomacyModel
    {
        public override float GetScoreOfDeclaringWar(IFaction factionDeclaresWar, IFaction factionDeclaredWar, IFaction evaluatingClan, out TextObject warReason)
        {
            float result = base.GetScoreOfDeclaringWar(factionDeclaresWar, factionDeclaredWar, evaluatingClan, out warReason);

            if (factionDeclaresWar.IsKingdomFaction)
            {
                Kingdom kingdom = (Kingdom)factionDeclaresWar;
                float wealthFactor = 0;
                foreach (var clan in kingdom.Clans)
                {
                    if (!clan.IsUnderMercenaryService)
                    {
                        wealthFactor += (clan.Gold - 25000) / 100000f;
                    }
                }

                result *= wealthFactor / kingdom.Clans.Count;
            }

            return result;
        }

    }
}


