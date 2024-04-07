using BannerKings.Actions;
using HarmonyLib;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace BannerKings.Patches
{
    internal class Rebelatches
    {
        [HarmonyPatch(typeof(Clan))]
        internal class ClanPatches
        {
            [HarmonyPrefix]
            [HarmonyPatch("CreateSettlementRebelClan")]
            private static bool CreateSettlementRebelClanPrefix(ref Clan __result, Settlement settlement, Hero owner, int iconMeshId = -1)
            {
                TextObject finalName;
                TextObject textObject = ClanActions.GetRandomAvailableName(settlement.Culture, settlement);
                if (textObject == null)
                {
                    textObject = settlement.Culture.ClanNameList.GetRandomElement();
                }

                TextObject origin;
                if (!textObject.GetVariableValue("ORIGIN_SETTLEMENT", out origin))
                {
                    finalName = new TextObject("{=!}{CLAN}-{SETTLEMENT}")
                        .SetTextVariable("CLAN", textObject);
                }
                else finalName = textObject.SetTextVariable("ORIGIN_SETTLEMENT", settlement.Name);

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

    }
}
