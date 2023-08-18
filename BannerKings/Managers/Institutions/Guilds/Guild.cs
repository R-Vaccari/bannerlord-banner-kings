using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Institutions.Guilds
{
    public class Guild : BannerKingsObject
    {
        public Guild(string id) : base(id)
        {
        }

        public Town Town { get; private set; }
        public Hero GuildMaster { get; private set; }
        public List<WorkshopType> WorkshopTypes { get; private set; }
        public List<VillageType> VillageTypes { get; private set; }

        public void Initialize(TextObject name, TextObject description, List<WorkshopType> workshopTypes,
            List<VillageType> villageTypes)
        {
            Initialize(name, description);
            WorkshopTypes = workshopTypes;
            VillageTypes = villageTypes;
        }

        public Guild MakeGuild(Town town)
        {
            return new Guild(StringId);
        }

        public int Income
        {
            get
            {
                var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(Town.Settlement);
                return (int) MBMath.ClampFloat(data.GetTypeCount(PopulationManager.PopType.Craftsmen) *
                    data.EconomicData.Mercantilism.ResultNumber * Influence * 0.82f
                    * 1f + data.Autonomy, 0f, 10000f);
            }
        }

        public new float Influence
        {
            get
            {
                var power = GuildMaster.Power;

                var settlementPower = 0f;
                foreach (var hero in Town.Settlement.Notables)
                {
                    settlementPower += hero.Power;
                }

                return power / settlementPower;
            }
        }

        public int Capital => GuildMaster.Gold;

        public static Hero EvaluateNewLeader(Settlement settlement)
        {
            Hero notable = null;
            foreach (var hero in settlement.Notables)
            {
                if (notable == null || hero.Power > notable.Power)
                {
                    if (IsSuitable(hero))
                    {
                        notable = hero;
                    }
                }
            }

            return notable;
        }

        public static bool IsSuitable(Hero notable)
        {
            return notable.Occupation is Occupation.Merchant or Occupation.Artisan
                   && notable.Power >= 200f;
        }

        /*public static GuildTrade GetSuitableTrade(Settlement settlement, Hero guildMaster)
        {
            var list = new List<(GuildTrade, float)>();
            var hasClay = settlement.BoundVillages.Any(v => v.VillageType == DefaultVillageTypes.ClayMine);
            var hasSalt = settlement.BoundVillages.Any(v => v.VillageType == DefaultVillageTypes.SaltMine);
            var hasIron = settlement.BoundVillages.Any(v => v.VillageType == DefaultVillageTypes.IronMine);
            var artisanBonus = guildMaster.Occupation == Occupation.Artisan ? 1f : 0f;
            var merchantBonus = guildMaster.Occupation == Occupation.Merchant ? 1f : 0f;
            list.Add((GuildTrade.Masons, (hasClay ? 3f : 1f) + artisanBonus));
            list.Add((GuildTrade.Merchants, (hasSalt ? 3f : 1f) + merchantBonus));
            list.Add((GuildTrade.Metalworkers, (hasIron ? 3f : 1f) + artisanBonus));
            return MBRandom.ChooseWeighted(list);
        } */
    }
}