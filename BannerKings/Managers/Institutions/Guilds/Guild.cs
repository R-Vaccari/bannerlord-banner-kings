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
        protected Dictionary<Hero, float> members;
        public Guild(Settlement settlement, Hero guildmaster, GuildTrade trade) : base(settlement)
        {
            this.guildMaster = guildMaster;
            members = new Dictionary<Hero, float>();
            type = new GuildType(trade);
        }

        public override void Destroy()
        {
            if (guildMaster != null) KillCharacterAction.ApplyByRemove(guildMaster);
            PopulationData data = BannerKingsConfig.Instance.PopulationManager.GetPopData(Settlement);
            data.EconomicData.RemoveGuild();
        }

        public int Capital => guildMaster.Gold;
        public GuildType GuildType => type;
        public MBReadOnlyDictionary<Hero, float> Members => members.GetReadOnlyDictionary();

        public void AddMemer(Hero hero)
        {
            if (!members.ContainsKey(hero)) members.Add(hero, 0f);
        }

        public void RemoveMember(Hero hero)
        {
            if (members.ContainsKey(hero)) members.Remove(hero);
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