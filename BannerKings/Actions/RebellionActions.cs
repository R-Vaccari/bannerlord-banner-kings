using BannerKings.Behaviours.Diplomacy.Groups;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;

namespace BannerKings.Actions
{
    public static class RebellionActions
    {
        public static Kingdom CreateRebelKingdom(RadicalGroup group, Clan leader, List<Clan> clans, Kingdom original)
        {
            Kingdom kingdom = Kingdom.CreateKingdom("rebels_" + original.StringId);
            kingdom.InitializeKingdom(group.KingdomName.SetTextVariable("KINGDOM", original.Name),
                group.KingdomName.SetTextVariable("KINGDOM", original.Name),
                original.Culture,
                original.Banner,
                original.Color2,
                original.Color,
                null,
                null,
                null,
                null);

            foreach (Clan c in clans ) ChangeKingdomAction.ApplyByJoinToKingdomByDefection(c, kingdom);
            kingdom.RulingClan = leader;

            return kingdom;
        }
    }
}
