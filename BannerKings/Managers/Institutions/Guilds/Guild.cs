using BannerKings.Managers.Institutions.Religions.Faiths;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace BannerKings.Managers.Institutions.Guilds
{
    public class Guild : LandedInstitution
    {
        private Hero guildMaster;
        protected List<Hero> members;

        public Guild(string id, Settlement settlement, Hero guildMaster, GuildTrade trade) : base(id)
        {
            this.guildMaster = guildMaster;
            members = new List<Hero>();
            GuildType = new GuildType(trade);
            base.settlement = settlement;
        }

        public int Income
        {
            get
            {
                var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
                return (int) MBMath.ClampFloat(data.GetTypeCount(PopulationManager.PopType.Craftsmen) *
                    data.EconomicData.Mercantilism.ResultNumber * Influence * 0.82f
                    * 1f + data.Autonomy, 0f, 10000f);
            }
        }

        public new float Influence
        {
            get
            {
                var power = guildMaster.Power;
                foreach (var hero in members)
                {
                    if (settlement.Notables.Contains(hero))
                    {
                        power += hero.Power;
                    }
                }

                var settlementPower = 0f;
                foreach (var hero in settlement.Notables)
                {
                    settlementPower += hero.Power;
                }

                return power / settlementPower;
            }
        }

        public int Capital => guildMaster.Gold;
        public GuildType GuildType { get; }

        public MBReadOnlyList<Hero> Members => new MBReadOnlyList<Hero>(members);

        public Hero Leader
        {
            get
            {
                if (guildMaster == null || !guildMaster.IsAlive || !guildMaster.IsActive)
                {
                    guildMaster = EvaluateNewLeader(settlement);
                }

                if (guildMaster == null)
                {
                    Destroy();
                }

                return guildMaster;
            }
        }

        public override void Destroy()
        {
            if (guildMaster != null)
            {
                KillCharacterAction.ApplyByRemove(guildMaster);
            }

            var data = BannerKingsConfig.Instance.PopulationManager.GetPopData(Settlement);
            data.EconomicData.RemoveGuild();
        }

        public void AddMemer(Hero hero)
        {
            if (!members.Contains(hero))
            {
                members.Add(hero);
            }
        }

        public void RemoveMember(Hero hero)
        {
            if (members.Contains(hero))
            {
                members.Remove(hero);
            }
        }

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
                   && notable.Power >= 400f;
        }

        public static GuildTrade GetSuitableTrade(Settlement settlement, Hero guildMaster)
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
        }
    }
}