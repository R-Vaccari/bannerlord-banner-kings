using BannerKings.Behaviours.Diplomacy.Groups;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Library;

namespace BannerKings.Actions
{
    public static class RebellionActions
    {
        public static Kingdom CreateRebelKingdom(RadicalGroup group, Clan leader, List<Clan> clans, Kingdom original)
        {
            TaleWorlds.CampaignSystem.Campaign.Current.KingdomManager.CreateKingdom(group.KingdomName.SetTextVariable("KINGDOM", original.Name),
                group.KingdomName.SetTextVariable("KINGDOM", original.Name),
                original.Culture,
                leader,
                original.ActivePolicies.ToMBList());
            Kingdom kingdom = leader.Kingdom;

            foreach (Clan c in clans) 
                if (c != leader)
                    ChangeKingdomAction.ApplyByJoinToKingdomByDefection(c, kingdom);

            return kingdom;
        }
    }
}
