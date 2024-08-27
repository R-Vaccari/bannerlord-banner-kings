using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;

namespace BannerKings.Managers.Dynasties
{
    public class DefaultDynasties : DefaultTypeInitializer<DefaultDynasties, Dynasty>
    {
        public Dynasty Osrick = new Dynasty("Osrick");
        public Dynasty Furqan = new Dynasty("Furqan");

        public override IEnumerable<Dynasty> All => throw new NotImplementedException();

        public override void Initialize()
        {
            Osrick.Initialize(new TextObject("{=!}Osrickin Dynasty"),
                new TextObject("{=!}The Osrickin dynasty is composed by the Wilunding who claim direct ascendency to Osrick Iron-Arm."),
                null,
                new List<Clan>()
                {
                    getClan("clan_vlandia_1"),
                    getClan("clan_vlandia_2"),
                },
                new List<Legacy>()
                {

                });
        }

        private Clan getClan(string id) => TaleWorlds.CampaignSystem.Campaign.Current.ObjectManager.GetObject<Clan>(id);
    }
}
