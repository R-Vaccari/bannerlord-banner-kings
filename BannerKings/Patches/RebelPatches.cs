using BannerKings.Actions;
using HarmonyLib;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Patches
{
    /*internal class Rebelatches
    {
        [HarmonyPatch(typeof(Clan))]
        internal class ClanPatches
        {
            [HarmonyPrefix]
            [HarmonyPatch("CreateSettlementRebelClan")]
            private static bool CreateSettlementRebelClanPrefix(ref Clan __result, Settlement settlement, Hero owner, int iconMeshId = -1)
            {
                

                finalName = finalName.SetTextVariable("SETTLEMENT", settlement.Name);
                Clan clan = Clan.CreateClan(settlement.StringId + "_rebel_clan_" + MBRandom.RandomInt(100), 
                    settlement, 
                    owner, 
                    settlement.MapFaction, 
                    settlement.Culture,
                    finalName, 
                    TaleWorlds.CampaignSystem.Campaign.Current.Models.ClanTierModel.RebelClanStartingTier, 
                    iconMeshId);
                clan.IsRebelClan = true;

                __result = clan;
                return false;
            }
        }
    }*/
}
