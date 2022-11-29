using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Models.Vanilla
{
    public class BKDiplomacyModel : DefaultDiplomacyModel
    {
        public override float GetScoreOfDeclaringWar(IFaction factionDeclaresWar, IFaction factionDeclaredWar, IFaction evaluatingClan, out TextObject warReason)
        {
            float score = 0;
            if (factionDeclaresWar.MapFaction == factionDeclaredWar.MapFaction)
            {
                return 0f;
            }

            StanceLink stanceWith = factionDeclaresWar.GetStanceWith(factionDeclaredWar);
        }



        private WarStats CalculateWarStats(IFaction faction, IFaction targetFaction)
        {
            float num = faction.TotalStrength * 0.85f;
            float num2 = 0f;
            int num3 = 0;
            foreach (Town town in faction.Fiefs)
            {
                num3 += (town.IsCastle ? 1 : 2);
            }
            if (faction.IsKingdomFaction)
            {
                foreach (Clan clan in ((Kingdom)faction).Clans)
                {
                    if (!clan.IsUnderMercenaryService)
                    {
                        int partyLimitForTier = Campaign.Current.Models.ClanTierModel.GetPartyLimitForTier(clan, clan.Tier);
                        num2 += (float)partyLimitForTier * 80f * ((clan.Leader == clan.MapFaction.Leader) ? 1.25f : 1f);
                    }
                }
            }
            num += num2;
            Clan rulingClan = faction.IsClan ? (faction as Clan) : (faction as Kingdom).RulingClan;
            float valueOfSettlements = faction.Fiefs.Sum((Town f) => (float)(f.IsTown ? 2000 : 1000) + f.Prosperity * 0.33f) * DefaultDiplomacyModel.ProsperityValueFactor;
            float num4 = 0f;
            float num5 = 0f;
            foreach (StanceLink stanceLink in faction.Stances)
            {
                if (stanceLink.IsAtWar && stanceLink.Faction1 != targetFaction && stanceLink.Faction2 != targetFaction && (!stanceLink.Faction2.IsMinorFaction || stanceLink.Faction2.Leader == Hero.MainHero))
                {
                    IFaction faction2 = (stanceLink.Faction1 == faction) ? stanceLink.Faction2 : stanceLink.Faction1;
                    if (faction2.IsKingdomFaction)
                    {
                        foreach (Clan clan2 in ((Kingdom)faction2).Clans)
                        {
                            if (!clan2.IsUnderMercenaryService)
                            {
                                num4 += (float)clan2.Tier * 80f * ((clan2.Leader == clan2.MapFaction.Leader) ? 1.5f : 1f);
                            }
                        }
                    }
                    num5 += faction2.TotalStrength;
                }
            }
            num5 += num4;
            num *= MathF.Sqrt(MathF.Sqrt((float)MathF.Min(num3 + 4, 40))) / 2.5f;
            return new DefaultDiplomacyModel.WarStats
            {
                RulingClan = rulingClan,
                Strength = num,
                ValueOfSettlements = valueOfSettlements,
                TotalStrengthOfEnemies = num5
            };
        }

        private struct WarStats
        {
            public Clan RulingClan;
            public float Strength;
            public float ValueOfSettlements;
            public float TotalStrengthOfEnemies;
        }
    }
}


