using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.CampaignSystem.ViewModelCollection;

namespace BannerKings.Patches
{
    internal partial class PerksAndSkillsPatches
    {
        [HarmonyPatch(typeof(CampaignUIHelper), "GetPerkRoleText")]
        class GetPerkRoleTextPatch
        {
            static void Postfix(ref TextObject __result, PerkObject perk, bool getSecondary)
            {
                TextObject textObject = null;
                var rolesText = new List<TextObject>();
                textObject = GameTexts.FindText("str_perk_one_role", null);
                if (!getSecondary && perk.PrimaryRole != SkillEffect.PerkRole.None)
                {
                    rolesText.Add(GameTexts.FindText("role", perk.PrimaryRole.ToString()));
                    if (StewardPerksData.ContainsKey(perk.StringId) && StewardPerksData[perk.StringId].PrimaryPerk?.AdditionalRoles != null)
                    {
                        rolesText.AddRange(StewardPerksData[perk.StringId].PrimaryPerk.AdditionalRoles.Select(d => GameTexts.FindText("role", d.ToString())));
                    }
                    textObject.SetTextVariable("PRIMARY_ROLE", NormalizeAdditionalRoles(perk.PrimaryRole, string.Join(" - ", rolesText)));
                }
                else if (getSecondary && perk.SecondaryRole != SkillEffect.PerkRole.None)
                {
                    rolesText.Add(GameTexts.FindText("role", perk.SecondaryRole.ToString()));
                    if (StewardPerksData.ContainsKey(perk.StringId) && StewardPerksData[perk.StringId].SecondaryPerk?.AdditionalRoles != null)
                    {
                        rolesText.AddRange(StewardPerksData[perk.StringId].SecondaryPerk.AdditionalRoles.Select(d => GameTexts.FindText("role", d.ToString())));
                    }
                    textObject.SetTextVariable("PRIMARY_ROLE", NormalizeAdditionalRoles(perk.SecondaryRole, string.Join(" - ", rolesText)));
                }

                __result = textObject;
            }
            static string NormalizeAdditionalRoles(SkillEffect.PerkRole perkRole, string text)
            {
                if (perkRole == SkillEffect.PerkRole.Governor)
                {
                    var partyOwner = GameTexts.FindText("role", SkillEffect.PerkRole.PartyOwner.ToString()).ToString();
                    var partyMember = GameTexts.FindText("role", SkillEffect.PerkRole.PartyMember.ToString()).ToString();
                    text = text.Replace(partyOwner, "Town/Castle Owner");
                    text = text.Replace(partyMember, "Town/Castle Member");
                }
                if (perkRole == SkillEffect.PerkRole.Personal || perkRole == SkillEffect.PerkRole.ClanLeader)
                {
                    var familyMember = GameTexts.FindText("role", SkillEffect.PerkRole.Captain.ToString()).ToString();
                    var clanMember = GameTexts.FindText("role", SkillEffect.PerkRole.PartyMember.ToString()).ToString();
                    text = text.Replace(familyMember, "Family Member");
                    text = text.Replace(clanMember, "Clan Member");
                }
                return text;
            }
        }
    }
}
