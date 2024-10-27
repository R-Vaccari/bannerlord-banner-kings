using Helpers;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerKings.Actions
{
    public static class RebellionActions
    {
        public static Kingdom CreateRebelKingdom(Clan leader, List<Clan> clans, Kingdom original)
        {
            TextObject kingdomName = KingdomActions.GetKingdomName(leader);
            Campaign.Current.KingdomManager.CreateKingdom(kingdomName,
                kingdomName,
                original.Culture,
                leader,
                original.ActivePolicies.ToMBList());
            Kingdom kingdom = leader.Kingdom;
            InitializeKingdom(kingdom, kingdomName, original.Color, original.Color2);

            foreach (Clan c in clans) 
                if (c != leader)
                    ChangeKingdomAction.ApplyByJoinToKingdomByDefection(c, kingdom);

            return kingdom;
        }

        public static void InitializeKingdom(Kingdom kingdom, TextObject kingdomName, uint color1, uint color2)
        {
            Clan leader = kingdom.RulingClan;
            TextObject encyclopediaTitle = new TextObject("{=ZOEamqUd}Kingdom of {NAME}")
                .SetTextVariable("NAME", leader.Name);

            TextObject encyclopediaText = ((!leader.IsRebelClan) ? new TextObject("{=21yUheIy}The {KINGDOM_NAME} was created in {CREATION_YEAR} by {RULER.NAME}, a rising {CULTURE_ADJECTIVE} warlord.")
                :
                new TextObject("{=drZC1Frp}The {KINGDOM_NAME} was created in {CREATION_YEAR} by {RULER.NAME}, leader of a group of {CULTURE_ADJECTIVE} rebels."));

            encyclopediaText.SetTextVariable("KINGDOM_NAME", encyclopediaTitle);
            encyclopediaText.SetTextVariable("CREATION_YEAR", CampaignTime.Now.GetYear);
            encyclopediaText.SetTextVariable("CULTURE_ADJECTIVE", FactionHelper.GetAdjectiveForFactionCulture(leader.Culture));
            StringHelpers.SetCharacterProperties("RULER", leader.Leader.CharacterObject, encyclopediaText);

            kingdom.InitializeKingdom(kingdomName,
                kingdomName,
                leader.Culture,
                leader.Banner,
                color1,
                color2,
                leader.HomeSettlement,
                encyclopediaText,
                encyclopediaTitle,
                TextObject.Empty);
        }
    }
}
