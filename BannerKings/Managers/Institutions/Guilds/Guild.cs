using BannerKings.Populations;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace BannerKings.Managers.Institutions.Guilds
{
    public class Guild : LandedInstitution
    {
        private GuildType type;
        private Hero guildMaster;
        protected List<Hero> members;
        public Guild(Settlement settlement, Hero guildMaster, GuildTrade trade) : base(settlement)
        {
            this.guildMaster = guildMaster;
            members = new List<Hero>();
            type = new GuildType(trade);
        }

        public override void Destroy()
        {
            if (guildMaster != null) KillCharacterAction.ApplyByRemove(guildMaster);
            PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(Settlement);
            data.EconomicData.RemoveGuild();
        }

        public int Income
        {
            get
            {
                PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(settlement);
                return (int)MBMath.ClampFloat((data.GetTypeCount(PopulationManager.PopType.Craftsmen) * data.EconomicData.Mercantilism.ResultNumber * Influence * 0.82f) 
                    * 1f + data.Autonomy, 0f, 10000f);
            }
        }

        public float Influence
        {
            get
            {
                float power = guildMaster.Power;
                foreach (Hero hero in members)
                    if (settlement.Notables.Contains(hero))
                        power += hero.Power;

                float settlementPower = 0f;
                foreach (Hero hero in settlement.Notables)
                    settlementPower += hero.Power;

                return power / settlementPower;
            }
        }

        public int Capital => guildMaster.Gold;
        public GuildType GuildType => type;
        public MBReadOnlyList<Hero> Members => members.GetReadOnlyList();

        public void AddMemer(Hero hero)
        {
            if (!members.Contains(hero)) members.Add(hero);
        }

        public void RemoveMember(Hero hero)
        {
            if (members.Contains(hero)) members.Remove(hero);
        }

        public Hero Leader
        {
            get
            {
                if (guildMaster == null || !guildMaster.IsAlive || !guildMaster.IsActive)
                    guildMaster = EvaluateNewLeader(settlement);

                if (guildMaster == null) Destroy();
                return guildMaster;
            }
        }

        public static Hero EvaluateNewLeader(Settlement settlement)
        {
            Hero notable = null;
            foreach (Hero hero in settlement.Notables)
                if (notable == null || hero.Power > notable.Power)
                    if (IsSuitable(hero))
                        notable = hero;

            return notable;
        }

        public static bool IsSuitable(Hero notable) => (notable.Occupation == Occupation.Merchant || notable.Occupation == Occupation.Artisan)
            && notable.Power >= 400f;

        public static GuildTrade GetSuitableTrade(Settlement settlement, Hero guildMaster)
        {
            List<ValueTuple<GuildTrade, float>> list = new List<(GuildTrade, float)>();
            bool hasClay = settlement.BoundVillages.Any(v => v.VillageType == DefaultVillageTypes.ClayMine);
            bool hasSalt = settlement.BoundVillages.Any(v => v.VillageType == DefaultVillageTypes.SaltMine);
            bool hasIron = settlement.BoundVillages.Any(v => v.VillageType == DefaultVillageTypes.IronMine);
            float artisanBonus = guildMaster.Occupation == Occupation.Artisan ? 1f : 0f;
            float merchantBonus = guildMaster.Occupation == Occupation.Merchant ? 1f : 0f;
            list.Add((GuildTrade.Masons, (hasClay ? 3f : 1f) + artisanBonus));
            list.Add((GuildTrade.Merchants, (hasSalt ? 3f : 1f) + merchantBonus));
            list.Add((GuildTrade.Metalworkers, (hasIron ? 3f : 1f) + artisanBonus));
            return MBRandom.ChooseWeighted(list);
        }
    }
}